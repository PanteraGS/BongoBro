// BongoBro leaderboard: Cloudflare Worker + D1.
//
//   POST /submit       { id, token, name, keys, char }  -> save my total key presses
//   GET  /leaderboard?id=...                          -> last published ranking (+ my place)
//
// The ranking is recomputed by a cron trigger every 12 hours, so everybody sees the
// same table until the next refresh.

const TOP = 50;
const MIN_SUBMIT_GAP_MS = 30 * 1000;        // per player
const MAX_KEYS_PER_SEC = 25;                // fastest humans stay well below this
const MAX_FIRST_KEYS = 20000000;            // first report can carry old progress
const NAME_RE = /^[\p{L}\p{N} _.\-]{2,16}$/u;

export default {
  async fetch(req, env) {
    const url = new URL(req.url);
    try {
      if (req.method === "POST" && url.pathname === "/submit") return await submit(req, env);
      if (req.method === "GET" && url.pathname === "/leaderboard") return await leaderboard(url, env);
      if (req.method === "GET" && url.pathname === "/") return json({ ok: true, service: "bongobro-leaderboard" });
      return json({ error: "not_found" }, 404);
    } catch (e) {
      return json({ error: "server_error", detail: String(e && e.message || e) }, 500);
    }
  },

  async scheduled(event, env, ctx) {
    ctx.waitUntil(buildSnapshot(env));
  },
};

function json(body, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json; charset=utf-8", "cache-control": "no-store" },
  });
}

async function sha256(text) {
  const buf = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(text));
  return [...new Uint8Array(buf)].map(b => b.toString(16).padStart(2, "0")).join("");
}

async function submit(req, env) {
  let b;
  try { b = await req.json(); } catch { return json({ error: "bad_json" }, 400); }
  const id = String(b.id || ""), token = String(b.token || "");
  const name = String(b.name || "").trim().replace(/\s+/g, " ");
  const keys = Math.floor(Number(b.keys));
  const ch = b.char === "vlad" ? "vlad" : "yura";
  if (!/^[0-9a-f]{32}$/.test(id) || !/^[0-9a-f]{64}$/.test(token)) return json({ error: "bad_id" }, 400);
  if (!NAME_RE.test(name)) return json({ error: "bad_name", got: name }, 400);
  if (!Number.isFinite(keys) || keys < 0) return json({ error: "bad_keys" }, 400);

  const now = Date.now();
  const hash = await sha256(token);
  const nameKey = name.toLowerCase();
  const me = await env.DB.prepare("SELECT * FROM players WHERE id = ?").bind(id).first();

  // a nickname belongs to whoever took it first
  const owner = await env.DB.prepare("SELECT id FROM players WHERE name_key = ?").bind(nameKey).first();
  if (owner && owner.id !== id) return json({ error: "name_taken" }, 409);

  if (!me) {
    if (keys > MAX_FIRST_KEYS) return json({ error: "too_many_keys" }, 422);
    await env.DB.prepare(
      "INSERT INTO players (id, token_hash, name, name_key, char, keys, created_at, updated_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?)"
    ).bind(id, hash, name, nameKey, ch, keys, now, now).run();
    return json({ ok: true, keys });
  }

  if (me.token_hash !== hash) return json({ error: "bad_token" }, 403);
  if (now - me.updated_at < MIN_SUBMIT_GAP_MS && me.name === name) return json({ error: "too_often" }, 429);
  // keys only grow, and not faster than a human could type since the last report
  let accepted = Math.max(me.keys, keys);
  const allowed = me.keys + Math.ceil((now - me.updated_at) / 1000 * MAX_KEYS_PER_SEC);
  const clamped = accepted > allowed;
  if (clamped) accepted = allowed;
  await env.DB.prepare("UPDATE players SET name = ?, name_key = ?, char = ?, keys = ?, updated_at = ? WHERE id = ?")
    .bind(name, nameKey, ch, accepted, now, id).run();
  return json({ ok: true, keys: accepted, clamped });
}

async function buildSnapshot(env) {
  const now = Date.now();
  const { results } = await env.DB.prepare(
    "SELECT id, name, char, keys FROM players WHERE keys > 0 ORDER BY keys DESC, updated_at ASC"
  ).all();
  const ranked = results.map((r, i) => ({ rank: i + 1, id: r.id, name: r.name, char: r.char, keys: r.keys }));
  const data = JSON.stringify({ created: now, ranked });
  await env.DB.prepare(
    "INSERT INTO snapshot (k, data, created_at) VALUES (1, ?, ?) ON CONFLICT(k) DO UPDATE SET data = excluded.data, created_at = excluded.created_at"
  ).bind(data, now).run();
  return { created: now, ranked };
}

async function leaderboard(url, env) {
  const row = await env.DB.prepare("SELECT data FROM snapshot WHERE k = 1").first();
  // first ever request (or the cron has not run yet): build one now
  let snap = row ? JSON.parse(row.data) : await buildSnapshot(env);
  // an empty table helps nobody: publish as soon as the first players show up
  if (snap.ranked.length === 0) {
    const n = await env.DB.prepare("SELECT COUNT(*) AS n FROM players WHERE keys > 0").first();
    if (n && n.n > 0) snap = await buildSnapshot(env);
  }
  const id = url.searchParams.get("id");
  const meEntry = id ? snap.ranked.find(r => r.id === id) : null;
  const strip = r => ({ rank: r.rank, name: r.name, char: r.char, keys: r.keys, me: !!id && r.id === id });
  return json({
    updated: snap.created,
    next: nextRefresh(snap.created),
    players: snap.ranked.length,
    top: snap.ranked.slice(0, TOP).map(strip),
    me: meEntry ? strip(meEntry) : null,
  });
}

// cron runs at 00:00 and 12:00 UTC
function nextRefresh(from) {
  const d = new Date(Math.max(from, Date.now()));
  const next = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), d.getUTCHours() < 12 ? 12 : 24));
  return next.getTime();
}
