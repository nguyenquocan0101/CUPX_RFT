[CmdletBinding()]
param(
    [switch]$VerifyOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

function Import-EnvironmentFile {
    param([string]$Path)

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -notmatch '^\s*([^#][^=]*)=(.*)$') {
            continue
        }

        [Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2], [EnvironmentVariableTarget]::Process)
    }
}

Import-EnvironmentFile (Join-Path $repoRoot '.local\compose-vars')

$couchAuth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($env:COUCHDB_USERNAME):$($env:COUCHDB_PASSWORD)"))
$couchHeaders = @{ Authorization = "Basic $couchAuth" }
$couchUri = 'http://localhost:5984/workflowdatas/phase3-persistence-marker'

if (-not $VerifyOnly) {
    $couchDocument = @{ _id = 'phase3-persistence-marker'; phase = 3; marker = 'preserve-on-restart' } | ConvertTo-Json
    try {
        Invoke-RestMethod -Method Put -Uri $couchUri -Headers $couchHeaders -ContentType 'application/json' -Body $couchDocument | Out-Null
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 409) {
            throw
        }
    }
}

$rabbitAuth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($env:RABBITMQ_USERNAME):$($env:RABBITMQ_PASSWORD)"))
$rabbitHeaders = @{ Authorization = "Basic $rabbitAuth" }
$queueUri = 'http://localhost:15672/api/queues/%2F/phase3-persistence'

if (-not $VerifyOnly) {
    $queueBody = @{ durable = $true; auto_delete = $false; arguments = @{} } | ConvertTo-Json
    Invoke-RestMethod -Method Put -Uri $queueUri -Headers $rabbitHeaders -ContentType 'application/json' -Body $queueBody | Out-Null
    $bindingBody = @{ routing_key = 'phase3.persistence'; arguments = @{} } | ConvertTo-Json
    Invoke-RestMethod -Method Post -Uri 'http://localhost:15672/api/bindings/%2F/e/kiosk/q/phase3-persistence' -Headers $rabbitHeaders -ContentType 'application/json' -Body $bindingBody | Out-Null
    $publishBody = @{
        routing_key = 'phase3.persistence'
        payload = 'phase3-persistence-marker'
        payload_encoding = 'string'
        properties = @{}
    } | ConvertTo-Json
    $publishResult = Invoke-RestMethod -Method Post -Uri 'http://localhost:15672/api/exchanges/%2F/kiosk/publish' -Headers $rabbitHeaders -ContentType 'application/json' -Body $publishBody
    if ($publishResult.routed -ne $true) {
        throw 'RabbitMQ persistence marker was not routed to the queue.'
    }
}

$document = Invoke-RestMethod -Uri $couchUri -Headers $couchHeaders
$queue = Invoke-RestMethod -Uri $queueUri -Headers $rabbitHeaders

if ($document.marker -ne 'preserve-on-restart') {
    throw 'CouchDB persistence marker is missing.'
}
if ($queue.name -ne 'phase3-persistence') {
    throw 'RabbitMQ persistence queue is missing.'
}

[pscustomobject]@{
    CouchDbMarker = 'preserved'
    RabbitQueue = $queue.name
    RabbitMessages = $queue.messages
}
