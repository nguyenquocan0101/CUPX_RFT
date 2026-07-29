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
    throw 'Missing local environment file. Copy config/local-environment.example to .local/compose-vars.'
}

$unsafeLines = Get-Content -LiteralPath $EnvFile |
    Where-Object { $_ -match '^\s*[^#][^=]*=\s*$' -or $_ -match 'change-me' }

if ($unsafeLines) {
    throw 'The environment file still contains blank or change-me values.'
}

& docker compose -f $composeFile --env-file $EnvFile config --quiet
if ($LASTEXITCODE -ne 0) {
    throw 'Docker Compose configuration validation failed.'
}

$allowedOccupiedPorts = [System.Collections.Generic.List[int]]::new()
$portMappings = @(
    @{ Service = 'redis'; ContainerPort = 6379 },
    @{ Service = 'rabbitmq'; ContainerPort = 5672 },
    @{ Service = 'rabbitmq'; ContainerPort = 15672 },
    @{ Service = 'couchdb'; ContainerPort = 5984 },
    @{ Service = 'minio'; ContainerPort = 9000 },
    @{ Service = 'minio'; ContainerPort = 9001 },
    @{ Service = 'mailpit'; ContainerPort = 1025 },
    @{ Service = 'mailpit'; ContainerPort = 8025 }
)

$runningServices = @(
    & docker compose -f $composeFile --env-file $EnvFile ps --status running --services
)
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to inspect running CUPX infrastructure services.'
}

foreach ($mapping in $portMappings) {
    if ($runningServices -notcontains $mapping.Service) {
        continue
    }

    $published = & docker compose -f $composeFile --env-file $EnvFile `
        port $mapping.Service $mapping.ContainerPort 2>$null
    if ($LASTEXITCODE -eq 0 -and $published -match ':(\d+)$') {
        $allowedOccupiedPorts.Add([int]$matches[1])
    }
}

foreach ($port in @(3000, 5100, 5160)) {
    $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $listener) { continue }
    $processInfo = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)" -ErrorAction SilentlyContinue
    if ($processInfo.CommandLine -like "*$repoRoot*") { $allowedOccupiedPorts.Add($port) }
}

& (Join-Path $PSScriptRoot 'Test-Prerequisites.ps1') `
    -AllowedOccupiedPorts $allowedOccupiedPorts.ToArray()

& docker compose -f $composeFile --env-file $EnvFile up -d --wait --wait-timeout 120 `
    redis rabbitmq couchdb minio mailpit
if ($LASTEXITCODE -ne 0) {
    throw 'Local infrastructure did not become healthy within 120 seconds.'
}

& docker compose -f $composeFile --env-file $EnvFile rm -f -s minio-init
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to clear the previous MinIO initialization container.'
}

& docker compose -f $composeFile --env-file $EnvFile run --rm minio-init
if ($LASTEXITCODE -ne 0) {
    throw 'MinIO bucket initialization failed.'
}

& docker compose -f $composeFile --env-file $EnvFile ps
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to read local infrastructure status.'
}
