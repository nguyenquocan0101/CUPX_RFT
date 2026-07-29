[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5100'
)

$ErrorActionPreference = 'Stop'
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

$login = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/v1/auth/login" `
    -ContentType 'application/json' `
    -Body $loginBody

if (-not $login.isSuccess -or
    [string]::IsNullOrWhiteSpace($login.response.accessToken) -or
    [string]::IsNullOrWhiteSpace($login.response.refreshToken)) {
    throw 'Local login did not return both JWT tokens.'
}

$payloadPart = $login.response.accessToken.Split('.')[1]
$payloadPart = $payloadPart.Replace('-', '+').Replace('_', '/')
while ($payloadPart.Length % 4 -ne 0) {
    $payloadPart += '='
}
$payload = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payloadPart)) |
    ConvertFrom-Json

if ($payload.accountId -ne 'local-admin' -or $payload.role -ne 'Admin') {
    throw 'Local access token does not contain the expected account and role claims.'
}

$refreshBody = @{
    refreshToken = $login.response.refreshToken
} | ConvertTo-Json
$refresh = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/api/v1/auth/refresh" `
    -ContentType 'application/json' `
    -Body $refreshBody

if (-not $refresh.isSuccess -or
    [string]::IsNullOrWhiteSpace($refresh.response.accessToken) -or
    [string]::IsNullOrWhiteSpace($refresh.response.refreshToken)) {
    throw 'Local refresh did not return both JWT tokens.'
}

Write-Host 'Main API health, local login, JWT claims and refresh verified.'
