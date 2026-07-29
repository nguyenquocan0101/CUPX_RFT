[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5100',
    [string]$EventId = "phase4-webhook-$([guid]::NewGuid().ToString('N'))",
    [switch]$ReplayOnly
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

if (-not $ReplayOnly) {
    $first = Invoke-RestMethod -Method Post `
        -Uri "$BaseUrl/api/v1/local-webhooks/trigger" `
        -Headers $headers -ContentType 'application/json' -Body $requestBody
    if (-not $first.isSuccess -or $first.isReplay) {
        throw "Initial webhook trigger did not succeed: $($first | ConvertTo-Json -Compress)"
    }
}

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

Write-Host "Local webhook persistence passed: event=$EventId state=$state replay=$($replay.isReplay)"
