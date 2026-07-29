[CmdletBinding()]
param(
    [ValidateSet('simulator', 'real')]
    [string]$HardwareMode = 'simulator',
    [string]$ProfilePath
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ProfilePath)) {
    $ProfilePath = Join-Path $PSScriptRoot '..\..\config\hardware-profiles.local.example.json'
}
$profileFile = (Resolve-Path -LiteralPath $ProfilePath).Path
$profile = Get-Content -LiteralPath $profileFile -Raw | ConvertFrom-Json

if ($HardwareMode -eq 'simulator') {
    Write-Host 'Hardware profile preflight passed: simulator mode does not require COM mappings.'
    exit 0
}

if ($profile.mode -ne 'real') {
    throw "Real mode requires a profile with mode=real. Current profile mode is '$($profile.mode)'."
}

$requiredDevices = @('cupDrop', 'coffee', 'iceMaker', 'robotArm')
$mapping = @{}
foreach ($device in $requiredDevices) {
    $property = $profile.serialPorts.PSObject.Properties[$device]
    if ($null -eq $property -or [string]::IsNullOrWhiteSpace([string]$property.Value)) {
        throw "Missing serial mapping for '$device'."
    }

    $port = [string]$property.Value
    if ($port -notmatch '^COM[1-9][0-9]*$') {
        throw "Invalid serial mapping for '$device': '$port'. Use an exact COM number, never COM0."
    }
    if ($mapping.Values -contains $port) {
        throw "Serial mapping '$port' is assigned to more than one controller."
    }
    $mapping[$device] = $port
}

$inventory = @(Get-CimInstance Win32_SerialPort -ErrorAction SilentlyContinue | ForEach-Object {
    [pscustomobject]@{
        DeviceId = $_.DeviceID
        Description = $_.Description
        PnpDeviceId = $_.PNPDeviceID
        IsBluetooth = [bool](($_.Name, $_.Description, $_.PNPDeviceID -join ' ') -match '(?i)bluetooth|BTHENUM')
    }
})

$errors = [System.Collections.Generic.List[string]]::new()
foreach ($entry in $mapping.GetEnumerator()) {
    $device = $entry.Key
    $port = $entry.Value
    $match = @($inventory | Where-Object { $_.DeviceId -eq $port })
    if ($match.Count -eq 0) {
        $errors.Add("$device -> $port is not present in Win32_SerialPort.")
    } elseif ($match[0].IsBluetooth) {
        $errors.Add("$device -> $port is a Bluetooth serial port; physical controllers require a verified wired port.")
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    throw 'Hardware profile preflight failed; no controller should be started.'
}

$mapping.GetEnumerator() | Sort-Object Key | ForEach-Object { Write-Host ("{0} -> {1}" -f $_.Key, $_.Value) }
Write-Host 'Hardware profile preflight passed.'
