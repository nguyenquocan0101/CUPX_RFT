[CmdletBinding()]
param(
    [string]$EnvFile
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeFile = Join-Path $repoRoot 'compose.local.yml'

if (-not $EnvFile) {
    $EnvFile = Join-Path $repoRoot '.local\compose-vars'
}

if (-not (Test-Path -LiteralPath $EnvFile)) {
    throw 'Missing local environment file. Pass -EnvFile if a different file was used.'
}

& docker compose -f $composeFile --env-file $EnvFile down
if ($LASTEXITCODE -ne 0) {
    throw 'Docker Compose shutdown failed.'
}
