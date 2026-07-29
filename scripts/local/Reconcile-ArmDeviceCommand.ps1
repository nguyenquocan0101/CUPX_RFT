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
    $JournalPath = Join-Path $repoRoot '.local\runtime\controller-arm.json'
}

$executable = Join-Path $repoRoot 'AutomaticBrewingCoffeeKioskBE\ArmController\ArmController2\bin\Debug\ArmController2.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    throw "ArmController2 is not built: $executable"
}

& $executable "--reconcile=$CommandId" "--resolution=$Resolution" "--journal=$JournalPath"
if ($LASTEXITCODE -ne 0) {
    throw "Arm command reconciliation failed for $CommandId."
}
