[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeVars = Join-Path $repoRoot '.local\compose-vars'
foreach ($line in Get-Content -LiteralPath $composeVars) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        [Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2], 'Process')
    }
}
$env:DOTNET_ENVIRONMENT = 'Local'
& dotnet run --project (Join-Path $repoRoot 'AutomaticBrewingCoffeeKioskBE\Simulator\DeviceSimulator\DeviceSimulator.csproj') -- "--journal=$(Join-Path $repoRoot '.local\runtime\device-simulator.db')"
if ($LASTEXITCODE -ne 0) { throw 'Device simulator exited with an error.' }
