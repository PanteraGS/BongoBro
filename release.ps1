# Publishes a new BongoBro version to GitHub Releases. Users get an "update" button.
#   powershell -File release.ps1 -Version 1.1.0 -Notes "Новые шапки"
param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string]$Notes = ""
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$repo = 'PanteraGS/BongoBro'

if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "Версия должна быть вида 1.2.3, получено: $Version" }
$tag = "v$Version"
if (git -C $root tag --list $tag) { throw "Тег $tag уже существует: подними номер версии" }

# 1. stamp the version into the program
$versionFile = Join-Path $root 'src\Version.cs'
$text = [IO.File]::ReadAllText($versionFile)
$text = $text -replace 'AssemblyVersion\("[^"]*"\)', "AssemblyVersion(""$Version"")"
$text = $text -replace 'AssemblyFileVersion\("[^"]*"\)', "AssemblyFileVersion(""$Version"")"
$text = $text -replace 'Version = "[^"]*"', "Version = ""$Version"""
[IO.File]::WriteAllText($versionFile, $text, (New-Object Text.UTF8Encoding $false))

# 2. build
& (Join-Path $root 'build.ps1')

# 3. commit, tag, push
git -C $root add -A
if (git -C $root status --porcelain) { git -C $root commit -m "Release $tag" | Out-Null }
git -C $root tag $tag
git -C $root push origin HEAD --tags

# 4. publish the exe as a release asset (notes go through a UTF-8 file to keep Cyrillic intact)
$notesFile = Join-Path $env:TEMP "bongobro-notes-$Version.md"
if (-not $Notes) { $Notes = "BongoBro $tag" }
[IO.File]::WriteAllText($notesFile, $Notes, (New-Object Text.UTF8Encoding $false))
gh release create $tag (Join-Path $root 'BongoBro.exe') --repo $repo --title "BongoBro $tag" --notes-file $notesFile
Remove-Item $notesFile -ErrorAction SilentlyContinue
Write-Host "Опубликовано: https://github.com/$repo/releases/tag/$tag"
