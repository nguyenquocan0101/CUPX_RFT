[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CommandId,
    [ValidateSet('Completed', 'Failed')]
    [string]$Resolution = 'Failed',
    [string]$JournalPath = ''
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if ([string]::IsNullOrWhiteSpace($JournalPath)) {
    $JournalPath = Join-Path $repoRoot '.local\runtime\device-simulator.db'
}

& dotnet run --project (Join-Path $repoRoot 'AutomaticBrewingCoffeeKioskBE\Simulator\DeviceSimulator\DeviceSimulator.csproj') `
    --no-build -- "--journal=$JournalPath" "--reconcile=$CommandId" "--resolution=$Resolution"
if ($LASTEXITCODE -ne 0) {
    throw "Device command reconciliation failed for $CommandId."
}
