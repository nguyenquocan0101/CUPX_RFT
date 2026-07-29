[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5160',
    [string]$ApiKey
)

$ErrorActionPreference = 'Stop'

$health = Invoke-RestMethod "$BaseUrl/health"
if ($null -eq $health) {
    throw 'Kiosk health returned an empty response.'
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw 'ApiKey is required for the authenticated ping check.'
}

$headers = @{ 'X-API-Key' = $ApiKey }
$ping = Invoke-RestMethod "$BaseUrl/api/v1/ping" -Headers $headers
if ($ping.isSuccess -ne $true) {
    throw 'Authenticated Kiosk ping did not return success.'
}

$unauthorizedStatus = $null
try {
    Invoke-WebRequest "$BaseUrl/api/v1/ping" -UseBasicParsing | Out-Null
}
catch {
    $unauthorizedStatus = $_.Exception.Response.StatusCode.value__
}

if ($unauthorizedStatus -ne 401) {
    throw "Unauthenticated Kiosk ping returned status $unauthorizedStatus instead of 401."
}

[pscustomobject]@{
    Health = 'pass'
    AuthenticatedPing = 'pass'
    UnauthenticatedPing = '401'
}
