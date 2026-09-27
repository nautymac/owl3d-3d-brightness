# Regenerates src\app.ico from the drawing code in IconArt.cs (original artwork, no third-party logo).
# Only needed when the icon design changes; the generated app.ico is committed.
#
#   powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$ico  = Join-Path (Split-Path -Parent $here) 'src\app.ico'
Add-Type -Path (Join-Path $here 'IconArt.cs') -ReferencedAssemblies System.Drawing
[IconArt]::SaveIco('cream', $ico)   # other variants in IconArt.cs: white, sky, cool, warm
Write-Host "written: $ico ($((Get-Item $ico).Length) bytes)"
