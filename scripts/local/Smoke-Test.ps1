[CmdletBinding()]
param(
    [ValidateSet('simulator', 'real')]
    [string]$HardwareMode = 'simulator'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$vars = Join-Path $repoRoot '.local\main-api-vars'
if (-not (Test-Path -LiteralPath $vars)) { throw 'Local credentials are missing. Run Initialize-MainDatabase.ps1.' }
$values = @{}
foreach ($line in Get-Content -LiteralPath $vars) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') { $values[$matches[1].Trim()] = $matches[2] }
}

& (Join-Path $PSScriptRoot 'Test-MainApi.ps1')
& (Join-Path $PSScriptRoot 'Test-KioskApi.ps1') -ApiKey $values['LocalSeed__KioskApiKey']
& (Join-Path $PSScriptRoot 'Test-MinioObjectStorage.ps1')
& (Join-Path $PSScriptRoot 'Test-LocalWebhookPersistence.ps1')
if ($HardwareMode -eq 'simulator') {
    & dotnet run --no-build --project (Join-Path $repoRoot 'AutomaticBrewingCoffeeKioskBE\Simulator\DeviceSimulator\DeviceSimulator.csproj') -- --self-test
    if ($LASTEXITCODE -ne 0) { throw 'Device simulator journal self-test failed.' }
    & (Join-Path $PSScriptRoot 'Test-DeviceSimulator.ps1')
    & (Join-Path $PSScriptRoot 'Test-SimulatorWorkflow.ps1')
}
Write-Host "Local smoke test passed. HardwareMode=$HardwareMode"
