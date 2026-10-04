# Briefly applies a mild correction to the Shift, checks every other output,
# then restores the exact gamma/saturation snapshots even if a check fails.
# Exit the tray app before running this test.
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $repo 'dist\Owl3DBrightness.exe'))
$flags = [Reflection.BindingFlags]'Static,Public,NonPublic'
$native = $assembly.GetType('Native')
$rampType = $assembly.GetType('Ramp')
$nvType = $assembly.GetType('Nv')
$device = $assembly.GetType('ShiftPower').GetMethod('FindDeviceName', $flags).Invoke($null, @())
if (-not $device) { throw 'Exactly one Shift must be connected.' }
if (Get-Process Owl3DBrightness -ErrorAction SilentlyContinue) { throw 'Exit the tray app first.' }
Add-Type -AssemblyName System.Windows.Forms

function Read-Ramp([string]$display) {
    $dc = $native.GetMethod('CreateDC').Invoke($null, @($display, $display, $null, [IntPtr]::Zero))
    if ($dc -eq [IntPtr]::Zero) { throw "Cannot open $display" }
    try {
        [UInt16[]]$values = [UInt16[]]::new(768)
        if (-not $native.GetMethod('GetDeviceGammaRamp').Invoke($null, @($dc, $values))) { throw "Cannot read $display" }
        return ,$values
    } finally { $null = $native.GetMethod('DeleteDC').Invoke($null, @($dc)) }
}
function Write-Ramp([string]$display, [UInt16[]]$values) {
    $dc = $native.GetMethod('CreateDC').Invoke($null, @($display, $display, $null, [IntPtr]::Zero))
    if ($dc -eq [IntPtr]::Zero) { throw "Cannot open $display" }
    try {
        if (-not $native.GetMethod('SetDeviceGammaRamp').Invoke($null, @($dc, $values))) { throw "Cannot restore $display" }
    } finally { $null = $native.GetMethod('DeleteDC').Invoke($null, @($dc)) }
}
function Assert-Same($before, $after, [string]$label) {
    if (($before -join ',') -cne ($after -join ',')) { throw "$label changed unexpectedly" }
}

$snapshots = @{}
foreach ($screen in [Windows.Forms.Screen]::AllScreens) { $snapshots[$screen.DeviceName] = Read-Ramp $screen.DeviceName }
$nvType.GetMethod('Init', $flags).Invoke($null, @())
$nvAvailable = $nvType.GetField('Available', $flags).GetValue($null)
$nvSnapshots = @()
if ($nvAvailable) {
    $enum = $nvType.GetField('en', $flags).GetValue($null)
    $name = $nvType.GetField('name', $flags).GetValue($null)
    $get = $nvType.GetField('get', $flags).GetValue($null)
    for ($i = 0; $i -lt 64; $i++) {
        $enumArgs = [object[]]@($i, [IntPtr]::Zero)
        if ($enum.DynamicInvoke($enumArgs) -ne 0) { break }
        $handle = $enumArgs[1]
        $text = [Text.StringBuilder]::new(64)
        if ($name.DynamicInvoke([object[]]@($handle, $text)) -ne 0) { throw 'Cannot identify NVIDIA output' }
        $info = $nvType.GetMethod('New', $flags).Invoke($null, @())
        $getArgs = [object[]]@($handle, 0, $info.PSObject.BaseObject)
        if ($get.DynamicInvoke($getArgs) -ne 0) { throw 'Cannot read NVIDIA saturation' }
        $nvSnapshots += [pscustomobject]@{ Device = $text.ToString(); Handle = $handle; Level = $getArgs[2].cur }
    }
}
$originalSat = $nvType.GetMethod('Get', $flags).Invoke($null, @())
try {
    [UInt16[]]$testRamp = $rampType.GetMethod('Build', $flags).Invoke($null, @([double]1.05, [double]1.0, [double]1.0))
    if (-not $rampType.GetMethod('Apply', $flags).Invoke($null, @(,$testRamp))) { throw 'Shift gamma correction rejected' }
    Start-Sleep -Milliseconds 200
    Assert-Same $testRamp (Read-Ramp $device) 'Shift gamma readback'
    foreach ($display in $snapshots.Keys) {
        if ($display -ne $device) { Assert-Same $snapshots[$display] (Read-Ramp $display) "$display gamma" }
    }
    if ($nvAvailable -and $originalSat -ge 0) {
        $testSat = if ($originalSat -ge 95) { $originalSat - 5 } else { $originalSat + 5 }
        if (-not $nvType.GetMethod('Set', $flags).Invoke($null, @([int]$testSat))) { throw 'Shift saturation correction rejected' }
        foreach ($snapshot in $nvSnapshots) {
            $info = $nvType.GetMethod('New', $flags).Invoke($null, @())
            $getArgs = [object[]]@($snapshot.Handle, 0, $info.PSObject.BaseObject)
            if ($get.DynamicInvoke($getArgs) -ne 0) { throw 'Cannot verify NVIDIA saturation' }
            $expected = if ($snapshot.Device -eq $device) { $testSat } else { $snapshot.Level }
            if ($getArgs[2].cur -ne $expected) { throw "$($snapshot.Device) saturation mismatch" }
        }
    }
    Write-Host "PASS: gamma and saturation target $device; other outputs unchanged."
} finally {
    Write-Ramp $device $snapshots[$device]
    if ($nvAvailable -and $originalSat -ge 0) {
        if (-not $nvType.GetMethod('Set', $flags).Invoke($null, @([int]$originalSat))) { throw 'Failed to restore Shift saturation' }
        if ($nvType.GetMethod('Get', $flags).Invoke($null, @()) -ne $originalSat) { throw 'Restored saturation mismatch' }
    }
    Assert-Same $snapshots[$device] (Read-Ramp $device) 'Restored Shift gamma'
    Write-Host 'PASS: original Shift settings restored.'
}
