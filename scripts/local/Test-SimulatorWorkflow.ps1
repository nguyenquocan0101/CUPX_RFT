[CmdletBinding()]
param(
    [int]$TimeoutSeconds = 30,
    [switch]$RestartKioskAfterEnqueue
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$varsPath = Join-Path $repoRoot '.local\main-api-vars'
$composeVarsPath = Join-Path $repoRoot '.local\compose-vars'

function Read-LocalValues {
    param([string]$Path)

    $values = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^\s*([^#][^=]*)=(.*)$') {
            $values[$matches[1].Trim()] = $matches[2]
        }
    }
    return $values
}

if (-not (Test-Path -LiteralPath $varsPath -PathType Leaf)) {
    throw 'Missing local API values. Run Initialize-MainDatabase.ps1 first.'
}

$mainValues = Read-LocalValues -Path $varsPath
$composeValues = Read-LocalValues -Path $composeVarsPath
$headers = @{ 'X-API-KEY' = $mainValues['LocalSeed__KioskApiKey'] }
$workflowId = 'local-workflow'
$productId = 'local-product'
$deviceId = 'local-device'
$deviceModelId = 'local-device-model'

$syncBody = @{
    devices = @(
        @{
            deviceId = $deviceId
            deviceModelId = $deviceModelId
            serialNumber = 'LOCAL-001'
            name = 'Local Brewer'
            description = 'Local simulator device'
            dictionaryStatus = @{}
        }
    )
    workflows = @(
        @{
            workflowId = $workflowId
            productId = $productId
            name = 'Local Simulator Workflow'
            description = 'Deterministic local workflow fixture'
            type = 'Activity'
        }
    )
    steps = @(
        @{
            stepId = 'local-step'
            workflowId = $workflowId
            name = 'Simulator dispense'
            type = 'dispense'
            deviceModelId = $deviceModelId
            sequence = 1
            parameters = '{}'
        }
    )
} | ConvertTo-Json -Depth 10

$sync = Invoke-RestMethod -Method Post `
    -Uri 'http://localhost:5160/api/v1/overridden-data' `
    -Headers $headers -ContentType 'application/json' -Body $syncBody
if ($sync.statusCode -ne 202 -or -not $sync.isSuccess) {
    throw "Simulator fixture sync failed with HTTP/status $($sync.statusCode)."
}

$orderId = "local-simulator-order-$([Guid]::NewGuid().ToString('N'))"
$executeBody = @{
    orderId = $orderId
    side = 1
    products = @(@{ productId = $productId })
} | ConvertTo-Json -Depth 6

$execute = Invoke-RestMethod -Method Post `
    -Uri 'http://localhost:5160/api/v1/execute' `
    -Headers $headers -ContentType 'application/json' -Body $executeBody
if ($execute.statusCode -ne 202 -or -not $execute.isSuccess) {
    throw "Simulator workflow enqueue failed with HTTP/status $($execute.statusCode)."
}

if ($RestartKioskAfterEnqueue) {
    $kioskProcess = Get-Process -Name Kiosk.ApiService -ErrorAction SilentlyContinue
    if (-not $kioskProcess) {
        throw 'Kiosk API process was not found for restart durability test.'
    }

    $kioskProcess | Stop-Process -Force
    Start-Sleep -Seconds 2
    $launcher = Start-Process -FilePath 'powershell.exe' -WindowStyle Hidden -WorkingDirectory $repoRoot `
        -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $repoRoot 'scripts\local\Start-KioskApi.ps1')) -PassThru
    Start-Sleep -Seconds 12
}

$basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($composeValues.COUCHDB_USERNAME):$($composeValues.COUCHDB_PASSWORD)"))
$couchHeaders = @{ Authorization = "Basic $basic" }
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
$completed = $null
do {
    Start-Sleep -Milliseconds 500
    $allDocs = Invoke-RestMethod -Method Get `
        -Uri 'http://localhost:5984/workflowdatas/_all_docs?include_docs=true' `
        -Headers $couchHeaders
    $completed = @($allDocs.rows | ForEach-Object { $_.doc } | Where-Object {
        $_.workflowId -eq $workflowId -and $_.orderId -eq $orderId -and $_.workflowState -eq 4 -and $_.isCompleted -eq $true
    } | Select-Object -First 1)
} while ($completed.Count -eq 0 -and (Get-Date) -lt $deadline)

if ($completed.Count -eq 0) {
    $diagnostic = @($allDocs.rows | ForEach-Object { $_.doc } | Where-Object {
        $_.workflowId -eq $workflowId -and $_.orderId -eq $orderId
    } | Select-Object -First 1)
    if ($diagnostic.Count -gt 0) {
        $stepStates = @($diagnostic[0].steps | ForEach-Object {
            "step=$($_.step.stepId),state=$($_.state),observed=$($_.observed),executor=$($_.executor)"
        }) -join '; '
        Write-Host "Workflow diagnostic: state=$($diagnostic[0].workflowState),completed=$($diagnostic[0].isCompleted),current=$([string]::Join(',', $diagnostic[0].currentStepId)),steps=$stepStates"
    } else {
        $known = @($allDocs.rows | ForEach-Object { $_.doc } | Where-Object { $_ -ne $null } | ForEach-Object {
            "workflow=$($_.workflowId),order=$($_.orderId),state=$($_.workflowState)"
        }) -join '; '
        Write-Host "Workflow diagnostic: no CouchDB document found for the generated order. Known=$known"
    }
    throw "Simulator workflow did not reach Done within $TimeoutSeconds seconds."
}

$step = @($completed[0].steps | Where-Object { $_.step.stepId -eq 'local-step' } | Select-Object -First 1)
if ($step.Count -eq 0 -or $step[0].state -ne 1 -or $step[0].observed -ne $true) {
    throw 'Simulator workflow finished without a durable Done/Observed step state.'
}

Write-Host "Simulator workflow E2E passed: order=$orderId state=Done step=Done/Observed"
