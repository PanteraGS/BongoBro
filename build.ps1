# Builds BongoBro.exe with the C# compiler that ships with Windows (.NET Framework 4.x).
$ErrorActionPreference = 'Stop'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$root = $PSScriptRoot
$src = Get-ChildItem (Join-Path $root 'src\*.cs') | ForEach-Object { $_.FullName }
$out = Join-Path $root 'BongoBro.exe'
$ico = Join-Path $root 'icon.ico'
$common = @('/nologo', '/target:winexe', '/optimize+', '/platform:anycpu', "/out:$out",
            '/r:System.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Web.Extensions.dll')

& $csc @common $src
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

# second pass: render the app icon with the app itself and embed it
& $out --export-icon $ico | Out-Null
Start-Sleep -Milliseconds 300
& $csc @common "/win32icon:$ico" $src
if ($LASTEXITCODE -ne 0) { throw 'Build with icon failed' }
Write-Host "OK: $out"
