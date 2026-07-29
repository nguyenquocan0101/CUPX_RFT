[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5100',
    [string]$EventId = "phase4-webhook-$([guid]::NewGuid().ToString('N'))",
    [switch]$ReplayOnly,
    [switch]$RestartMainApi,
    [int]$RestartTimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$mainVarsPath = Join-Path $repoRoot '.local\main-api-vars'
$database = 'AutoBrewing_BE_Local'

if (-not (Test-Path -LiteralPath $mainVarsPath -PathType Leaf)) {
    throw 'Missing .local/main-api-vars. Run Initialize-MainDatabase.ps1 first.'
}

$values = @{}
foreach ($line in Get-Content -LiteralPath $mainVarsPath) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        $values[$matches[1].Trim()] = $matches[2]
    }
}

$requestBody = @{
    source = 'local-e2e'
    eventType = 'HealthCheck'
    eventId = $EventId
    path = '/api/v1/ping'
    httpMethod = 'GET'
    payload = @{ marker = 'phase4' }
} | ConvertTo-Json -Depth 5
$headers = @{ 'X-API-Key' = $values['LocalSeed__KioskApiKey'] }

if ($ReplayOnly -and $RestartMainApi) {
    throw '-ReplayOnly and -RestartMainApi cannot be combined.'
}

function Wait-MainApi {
    param([int]$TimeoutSeconds)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $health = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/health" -TimeoutSec 3
            if ($health.StatusCode -eq 200) { return }
        }
        catch {
            Start-Sleep -Seconds 1
        }
    } while ((Get-Date) -lt $deadline)

    throw "Main API did not become healthy within $TimeoutSeconds seconds."
}

function Stop-ProcessTree {
    param([int]$RootId)

    $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $RootId" -ErrorAction SilentlyContinue)
    foreach ($child in $children) { Stop-ProcessTree -RootId $child.ProcessId }
    if (Get-Process -Id $RootId -ErrorAction SilentlyContinue) {
        Stop-Process -Id $RootId -Force -ErrorAction Stop
    }
}

function Restart-MainApi {
    $listener = Get-NetTCPConnection -State Listen -LocalPort 5100 -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if (-not $listener) {
        throw 'Cannot restart Main API because port 5100 has no listener.'
    }

    $process = Get-Process -Id $listener.OwningProcess -ErrorAction Stop
    $processPath = $process.Path
    $repoRootPrefix = $repoRoot.TrimEnd('\') + '\'
    if ([string]::IsNullOrWhiteSpace($processPath) -or
        -not $processPath.StartsWith($repoRootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to stop non-repo process $($process.Id) on port 5100."
    }

    Write-Host "Restarting Main API process $($process.Id)."
    Stop-ProcessTree -RootId $process.Id
    $deadline = (Get-Date).AddSeconds(15)
    do {
        $stillListening = Get-NetTCPConnection -State Listen -LocalPort 5100 -ErrorAction SilentlyContinue
        if (-not $stillListening) { break }
        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)
    if ($stillListening) { throw 'Main API port 5100 did not release after stopping the process.' }

    $runtime = Join-Path $repoRoot '.local\runtime'
    New-Item -ItemType Directory -Path $runtime -Force | Out-Null
    $startScript = Join-Path $PSScriptRoot 'Start-MainApi.ps1'
    $log = Join-Path $runtime 'main-api-webhook-restart.log'
    $errorLog = Join-Path $runtime 'main-api-webhook-restart.error.log'
    $started = Start-Process -FilePath 'powershell.exe' `
        -WorkingDirectory $repoRoot `
        -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $startScript) `
        -RedirectStandardOutput $log `
        -RedirectStandardError $errorLog `
        -PassThru
    $started.Id | Set-Content -LiteralPath (Join-Path $runtime 'main-api.pid') -Encoding ascii
    Wait-MainApi -TimeoutSeconds $RestartTimeoutSeconds
}

if (-not $ReplayOnly) {
    $first = Invoke-RestMethod -Method Post `
        -Uri "$BaseUrl/api/v1/local-webhooks/trigger" `
        -Headers $headers -ContentType 'application/json' -Body $requestBody
    if (-not $first.isSuccess -or $first.isReplay) {
        throw "Initial webhook trigger did not succeed: $($first | ConvertTo-Json -Compress)"
    }
}

if ($RestartMainApi) { Restart-MainApi }

$replay = Invoke-RestMethod -Method Post `
    -Uri "$BaseUrl/api/v1/local-webhooks/trigger" `
    -Headers $headers -ContentType 'application/json' -Body $requestBody
if (-not $replay.isSuccess -or -not $replay.isReplay) {
    throw "Webhook replay was not served from durable state: $($replay | ConvertTo-Json -Compress)"
}

$eventIdSql = $EventId.Replace("'", "''")
$query = @"
SELECT CONCAT(
    i.Status, N'|', o.Status, N'|', i.AttemptCount, N'|', o.AttemptCount, N'|', o.HttpMethod)
FROM dbo.LocalWebhookInboxes i
JOIN dbo.LocalWebhookOutboxes o ON o.InboxId = i.InboxId
WHERE i.EventId = N'$eventIdSql';
"@
$state = (& sqlcmd -S 'tcp:127.0.0.1,1433' -E -C -d $database -h -1 -W -Q "SET NOCOUNT ON; $query" |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1).Trim()
if ($state -ne 'Succeeded|Succeeded|1|1|GET') {
    throw "Unexpected durable webhook state: $state"
}

Write-Host "Local webhook persistence passed: event=$EventId state=$state replay=$($replay.isReplay) restarted=$($RestartMainApi.IsPresent)"
