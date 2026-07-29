[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5100',
    [string]$ProductId = 'local-product'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$mainVarsPath = Join-Path $repoRoot '.local\main-api-vars'

if (-not (Test-Path -LiteralPath $mainVarsPath -PathType Leaf)) {
    throw 'Missing .local/main-api-vars. Run Initialize-MainDatabase.ps1 first.'
}

$values = @{}
foreach ($line in Get-Content -LiteralPath $mainVarsPath) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        $values[$matches[1].Trim()] = $matches[2]
    }
}

$loginBody = @{
    email = $values['LocalSeed__AdminEmail']
    password = $values['LocalSeed__AdminPassword']
} | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/v1/auth/login" -ContentType 'application/json' -Body $loginBody
$token = $login.response.accessToken
if ([string]::IsNullOrWhiteSpace($token)) {
    throw 'Local login did not return a JWT.'
}

$uploadedBytes = [Convert]::FromBase64String(
    'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=')
$client = [System.Net.Http.HttpClient]::new()
$client.DefaultRequestHeaders.Authorization =
    [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $token)

$form = [System.Net.Http.MultipartFormDataContent]::new()
$file = [System.Net.Http.ByteArrayContent]::new($uploadedBytes)
$file.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('image/png')
$form.Add($file, 'File', 'phase4-minio-e2e.png')

$upload = $client.PostAsync(
    "$BaseUrl/api/v1/products/$ProductId/image",
    $form).GetAwaiter().GetResult()
$uploadBody = $upload.Content.ReadAsStringAsync().GetAwaiter().GetResult()
if (-not $upload.IsSuccessStatusCode) {
    throw "Product image upload failed with HTTP $([int]$upload.StatusCode): $uploadBody"
}

$product = Invoke-RestMethod -Method Get `
    -Uri "$BaseUrl/api/v1/products/$ProductId" `
    -Headers @{ Authorization = "Bearer $token" }
$imageUrl = $product.response.imageUrl
if ([string]::IsNullOrWhiteSpace($imageUrl)) {
    throw 'Product response did not contain a MinIO image URL.'
}

$downloadedBytes = $client.GetByteArrayAsync($imageUrl).GetAwaiter().GetResult()
$sha256 = [Security.Cryptography.SHA256]::Create()
$uploadedHash = [Convert]::ToBase64String($sha256.ComputeHash($uploadedBytes))
$downloadedHash = [Convert]::ToBase64String($sha256.ComputeHash($downloadedBytes))

if ($uploadedBytes.Length -ne $downloadedBytes.Length -or $uploadedHash -ne $downloadedHash) {
    throw "MinIO round-trip mismatch. uploaded=$($uploadedBytes.Length)/$uploadedHash downloaded=$($downloadedBytes.Length)/$downloadedHash"
}

Write-Host "MinIO E2E passed: upload HTTP $([int]$upload.StatusCode), bytes=$($uploadedBytes.Length), sha256=$uploadedHash"
Write-Host "Public object URL: $imageUrl"
$client.Dispose()
