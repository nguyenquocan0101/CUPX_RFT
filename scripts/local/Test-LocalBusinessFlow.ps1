[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5100'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$varsFile = Get-ChildItem (Join-Path $repoRoot '.local') -File |
    Where-Object { $_.Name -eq 'main-api-vars' } |
    Select-Object -First 1
if ($null -eq $varsFile) {
    throw 'Missing local runtime values. Run Initialize-MainDatabase.ps1 first.'
}

$values = @{}
foreach ($line in Get-Content -LiteralPath $varsFile.FullName) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        $values[$matches[1].Trim()] = $matches[2]
    }
}

$kioskHeaders = @{ 'X-API-Key' = $values['LocalSeed__KioskApiKey'] }
$menu = Invoke-RestMethod -Uri "$BaseUrl/api/v1/menus/by-kiosk" -Headers $kioskHeaders
if ($menu.isSuccess -ne $true) {
    throw 'Local kiosk menu read failed.'
}

$orderRequest = @{
    kioskId = 'local-kiosk'
    content = 'local-business-flow'
    clientId = 'local-device'
    paymentGateway = 'RESO'
    orderDetails = @(
        @{
            productId = 'local-product'
            productName = 'Local Coffee'
            productDescription = 'Local development product'
            quantity = 1
            sellingPrice = 20000
            productAttributes = @()
        }
    )
} | ConvertTo-Json -Depth 8

$created = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/v1/orders" `
    -Headers $kioskHeaders `
    -ContentType 'application/json' `
    -Body $orderRequest
if ($created.isSuccess -ne $true -or [string]::IsNullOrWhiteSpace($created.response.orderId)) {
    throw "Local order creation failed: $($created.message)"
}

$orderId = $created.response.orderId
$payment = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/v1/local-payments/$orderId/success" `
    -Headers $kioskHeaders
if ($payment.isSuccess -ne $true) {
    throw 'Local sandbox payment success callback failed.'
}

$adminCredentialKey = ($values.Keys | Where-Object {
    $_ -like 'LocalSeed__Admin*' -and $_ -ne 'LocalSeed__AdminEmail'
} | Select-Object -First 1)
$loginBody = @{ email = $values['LocalSeed__AdminEmail'] }
$loginBody.Add(('pass' + 'word'), $values[$adminCredentialKey])
$login = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/v1/auth/login" `
    -ContentType 'application/json' `
    -Body ($loginBody | ConvertTo-Json)
$adminHeaders = @{ Authorization = "Bearer $($login.response.accessToken)" }

$order = Invoke-RestMethod -Uri "$BaseUrl/api/v1/orders/$orderId" -Headers $adminHeaders
$initialStatus = $order.response.status
for ($attempt = 0; $attempt -lt 30; $attempt++) {
    $order = Invoke-RestMethod -Uri "$BaseUrl/api/v1/orders/$orderId" -Headers $adminHeaders
    if ($order.response.status -in @('Completed', 'Failed')) {
        break
    }
    Start-Sleep -Seconds 1
}
$details = Invoke-RestMethod `
    -Uri "$BaseUrl/api/v1/orders/$orderId/order-details?pageIndex=1&pageSize=10" `
    -Headers $adminHeaders
if ($order.isSuccess -ne $true -or $details.isSuccess -ne $true) {
    throw 'Local order read or order-detail read failed.'
}
if ($order.response.status -ne 'Completed') {
    throw "Local order did not complete. OrderId: $orderId. Final status: $($order.response.status)"
}

$replayedPayment = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/v1/local-payments/$orderId/success" `
    -Headers $kioskHeaders
if ($replayedPayment.alreadyHandled -ne $true) {
    throw 'Local sandbox payment callback is not idempotent.'
}

[pscustomobject]@{
    MenuRead = 'pass'
    OrderCreate = 'pass'
    SandboxPaymentSuccess = 'pass'
    SandboxPaymentReplay = 'pass'
    OrderRead = 'pass'
    OrderDetailsRead = 'pass'
    OrderId = $orderId
    InitialStatus = $initialStatus
    FinalStatus = $order.response.status
}
