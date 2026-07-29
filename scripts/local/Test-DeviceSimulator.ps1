[CmdletBinding()]
param(
    [int]$TimeoutSeconds = 10
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeVars = Join-Path $repoRoot '.local\compose-vars'
$values = @{}
foreach ($line in Get-Content -LiteralPath $composeVars) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') { $values[$matches[1].Trim()] = $matches[2] }
}
$basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($values.RABBITMQ_USERNAME):$($values.RABBITMQ_PASSWORD)"))
$headers = @{ Authorization = "Basic $basic"; 'Content-Type' = 'application/json' }
$commandId = "local-simulator-$([Guid]::NewGuid().ToString('N'))"
$request = @{
    CommandId = $commandId
    SchemaVersion = 1
    CorrelationId = $commandId
    WorkflowId = 'local-workflow'
    StepId = 'local-step'
    DeviceId = 'local-device'
    Method = 'dispense'
    Parameters = @{ amount = '1' }
    RequestedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    TimeoutMs = 1000
} | ConvertTo-Json -Depth 5 -Compress
$publish = @{
    properties = @{ delivery_mode = 2; content_type = 'application/json' }
    routing_key = 'device.command'
    payload = $request
    payload_encoding = 'string'
} | ConvertTo-Json -Depth 5
$result = Invoke-RestMethod -Method Post -Uri 'http://localhost:15672/api/exchanges/%2F/device-command/publish' -Headers $headers -Body $publish
if ($result.routed -ne $true) { throw 'Simulator command was not routed to RabbitMQ.' }

$queueUri = 'http://localhost:15672/api/queues/%2F/device-command'
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
do {
    Start-Sleep -Milliseconds 250
    $queue = Invoke-RestMethod -Uri $queueUri -Headers $headers
} while ($queue.messages -gt 0 -and (Get-Date) -lt $deadline)

if ($queue.messages -ne 0) { throw "Simulator did not consume the command within $TimeoutSeconds seconds." }
Write-Host "RabbitMQ simulator command consumed: $commandId"
