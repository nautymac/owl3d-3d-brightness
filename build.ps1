# Builds dist\Owl3DBrightness.exe with the C# compiler that ships with Windows (.NET Framework 4).
# The icon is src\app.ico (original artwork, regenerate with tools\make-icon.ps1).
#
#   powershell -ExecutionPolicy Bypass -File build.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src  = Join-Path $root 'src\Owl3DBrightness.cs'
$ico  = Join-Path $root 'src\app.ico'
$dist = Join-Path $root 'dist'
$out  = Join-Path $dist 'Owl3DBrightness.exe'
$csc  = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw "csc.exe not found: $csc" }
if (-not (Test-Path $ico)) { throw "icon not found: $ico (run tools\make-icon.ps1)" }
New-Item -ItemType Directory -Force $dist | Out-Null

& $csc /nologo /nowarn:0219 /target:winexe /codepage:65001 /optimize+ "/win32icon:$ico" "/out:$out" $src
if ($LASTEXITCODE -ne 0) { throw "build failed ($LASTEXITCODE)" }
Write-Host "built: $out ($((Get-Item $out).Length) bytes)"
