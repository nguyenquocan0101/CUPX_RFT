[CmdletBinding()]
param(
    [string]$JsonPath
)

$ErrorActionPreference = 'Stop'

$ports = @(Get-CimInstance Win32_SerialPort -ErrorAction SilentlyContinue | ForEach-Object {
    [pscustomobject]@{
        DeviceId = $_.DeviceID
        Name = $_.Name
        Description = $_.Description
        PnpDeviceId = $_.PNPDeviceID
        IsBluetooth = [bool](($_.Name, $_.Description, $_.PNPDeviceID -join ' ') -match '(?i)bluetooth|BTHENUM')
    }
})

if ($JsonPath) {
    $parent = Split-Path -Parent $JsonPath
    if ($parent) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    $ports | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $JsonPath -Encoding ascii
}

$ports | Format-Table DeviceId, IsBluetooth, Description, PnpDeviceId -AutoSize
if ($JsonPath) { Write-Host "Serial port inventory written to $JsonPath" }
