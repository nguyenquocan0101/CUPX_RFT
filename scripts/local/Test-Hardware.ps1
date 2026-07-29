[CmdletBinding()]
param(
    [ValidateSet('simulator', 'real')]
    [string]$HardwareMode = 'simulator'
)

$ErrorActionPreference = 'Stop'
$ports = @(Get-CimInstance Win32_SerialPort -ErrorAction SilentlyContinue)
Write-Host "Detected serial ports: $($ports.Count)"
$ports | ForEach-Object { Write-Host ("{0} - {1}" -f $_.DeviceID, $_.Description) }

if ($HardwareMode -eq 'simulator') {
    Write-Host 'Simulator mode selected; physical serial ports are not required.'
    exit 0
}

$controllerPorts = @($ports | Where-Object { $_.Description -notmatch 'Bluetooth' })
if ($controllerPorts.Count -eq 0) {
    throw 'Real mode requires at least one non-Bluetooth serial controller. No hardware was detected.'
}
Write-Host 'Real mode has candidate controller ports. Confirm each mapping before enabling device workers.'
