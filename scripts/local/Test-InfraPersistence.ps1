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
    throw 'Missing local environment file. Run Initialize-LocalEnvironment.ps1 first.'
}

$settings = @{}
foreach ($line in Get-Content -LiteralPath $EnvFile) {
    if ($line -match '^\s*([^#][^=]+)=(.*)$') {
        $settings[$matches[1].Trim()] = $matches[2].Trim()
    }
}

function Invoke-Compose {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)

    & docker compose -f $composeFile --env-file $EnvFile @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose command failed: $($Arguments -join ' ')"
    }
}

function New-BasicHeader {
    param(
        [string]$Username,
        [string]$Password
    )

    $bytes = [Text.Encoding]::ASCII.GetBytes("${Username}:${Password}")
    return @{ Authorization = 'Basic ' + [Convert]::ToBase64String($bytes) }
}

$couchHeaders = New-BasicHeader -Username $settings.COUCHDB_USERNAME -Password $settings.COUCHDB_PASSWORD
$rabbitHeaders = New-BasicHeader -Username $settings.RABBITMQ_USERNAME -Password $settings.RABBITMQ_PASSWORD
$couchHeaders['Content-Type'] = 'application/json'
$rabbitHeaders['Content-Type'] = 'application/json'

$runId = [Guid]::NewGuid().ToString('N')
$marker = "phase01-$runId"
$redisKey = "cupx:phase01:persistence:$runId"
$couchDocumentId = "phase01_$runId"
$queueName = "cupx.phase01.persistence.$runId"
$minioObject = "phase01-persistence/$runId.txt"
$mailSubject = "cupx-phase01-persistence-$runId"

Invoke-Compose exec -T redis redis-cli SET $redisKey $marker | Out-Null

$couchDatabaseUri = 'http://localhost:5984/cupx_phase01_persistence'
try {
    Invoke-RestMethod -Method Put -Uri $couchDatabaseUri -Headers $couchHeaders | Out-Null
}
catch {
    if ($_.Exception.Response.StatusCode.value__ -ne 412) {
        throw
    }
}

$couchBody = @{ value = $marker } | ConvertTo-Json
Invoke-RestMethod -Method Put -Uri "$couchDatabaseUri/$couchDocumentId" `
    -Headers $couchHeaders -Body $couchBody | Out-Null

$queueUri = "http://localhost:15672/api/queues/%2F/$queueName"
Invoke-RestMethod -Method Put -Uri $queueUri -Headers $rabbitHeaders `
    -Body '{"durable":true,"auto_delete":false,"arguments":{}}' | Out-Null

$publishBody = @{
    properties = @{ delivery_mode = 2 }
    routing_key = $queueName
    payload = $marker
    payload_encoding = 'string'
} | ConvertTo-Json -Depth 4

$publishResult = Invoke-RestMethod -Method Post `
    -Uri 'http://localhost:15672/api/exchanges/%2F/amq.default/publish' `
    -Headers $rabbitHeaders -Body $publishBody
if (-not $publishResult.routed) {
    throw 'RabbitMQ persistence sentinel was not routed.'
}

$minioWrite = (@'
mc alias set local http://minio:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" >/dev/null
printf '__MARKER__' | mc pipe "local/$MINIO_BUCKET/__OBJECT__" >/dev/null
'@).Replace('__MARKER__', $marker).Replace('__OBJECT__', $minioObject)
Invoke-Compose run --rm --entrypoint /bin/sh minio-init -ec $minioWrite | Out-Null

$mail = [System.Net.Mail.MailMessage]::new(
    'phase01@cupx.local',
    'developer@cupx.local',
    $mailSubject,
    $marker
)
$smtp = [System.Net.Mail.SmtpClient]::new('localhost', 1025)
try {
    $smtp.Send($mail)
}
finally {
    $mail.Dispose()
    $smtp.Dispose()
}

& (Join-Path $PSScriptRoot 'Stop-Infra.ps1') -EnvFile $EnvFile
& (Join-Path $PSScriptRoot 'Start-Infra.ps1') -EnvFile $EnvFile

$redisValue = Invoke-Compose exec -T redis redis-cli GET $redisKey
if ($redisValue.Trim() -ne $marker) {
    throw 'Redis persistence sentinel is missing.'
}

$couchDocument = Invoke-RestMethod -Uri "$couchDatabaseUri/$couchDocumentId" -Headers $couchHeaders
if ($couchDocument.value -ne $marker) {
    throw 'CouchDB persistence sentinel is missing.'
}

$rabbitQueueState = Invoke-Compose exec -T rabbitmq rabbitmqctl list_queues name messages
$rabbitQueueText = $rabbitQueueState -join "`n"
$queuePattern = '(?m)^' + [regex]::Escape($queueName) + '\s+1\s*$'
if ($rabbitQueueText -notmatch $queuePattern) {
    throw 'RabbitMQ persistent message is missing.'
}

$minioRead = (@'
mc alias set local http://minio:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" >/dev/null
mc cat "local/$MINIO_BUCKET/__OBJECT__"
'@).Replace('__OBJECT__', $minioObject)
$minioValue = Invoke-Compose run --rm --entrypoint /bin/sh minio-init -ec $minioRead
if (($minioValue -join '').Trim() -ne $marker) {
    throw 'MinIO persistence sentinel is missing.'
}

$mailpitMessages = Invoke-RestMethod -Uri 'http://localhost:8025/api/v1/messages'
$mailpitSentinel = $mailpitMessages.messages | Where-Object Subject -eq $mailSubject
if (-not $mailpitSentinel) {
    throw 'Mailpit persistence sentinel is missing.'
}

Invoke-Compose exec -T redis redis-cli DEL $redisKey | Out-Null
Invoke-RestMethod -Method Delete `
    -Uri "$couchDatabaseUri/$couchDocumentId`?rev=$($couchDocument._rev)" `
    -Headers $couchHeaders | Out-Null
Invoke-RestMethod -Method Delete -Uri $queueUri -Headers $rabbitHeaders | Out-Null

$minioDelete = (@'
mc alias set local http://minio:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" >/dev/null
mc rm "local/$MINIO_BUCKET/__OBJECT__"
'@).Replace('__OBJECT__', $minioObject)
Invoke-Compose run --rm --entrypoint /bin/sh minio-init -ec $minioDelete | Out-Null

$mailpitDeleteBody = @{
    IDs = @($mailpitSentinel | ForEach-Object ID)
} | ConvertTo-Json
Invoke-RestMethod -Method Delete -Uri 'http://localhost:8025/api/v1/messages' `
    -ContentType 'application/json' -Body $mailpitDeleteBody | Out-Null

Write-Host 'Persistence verified for all five services; run-specific sentinels were removed.'
