[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$frontend = Join-Path $repoRoot 'AutomaticBrewingCoffeeFE'
$mobile = Join-Path $repoRoot 'AutomaticBrewingCoffeeApp'

Push-Location $frontend
try {
    $env:LOCAL_MODE = 'true'
    $env:API_PROXY_TARGET = 'http://localhost:5100'
    $env:NEXT_PUBLIC_API_BASE_URL = '/api/v1'
    $env:NEXT_PUBLIC_SIGNALR_URL = '/hubs/notification'
    & npm run build
    if ($LASTEXITCODE -ne 0) { throw 'Next.js local build failed.' }
}
finally { Pop-Location }

$fvm = Get-Command fvm -ErrorAction SilentlyContinue
$fvmPath = if ($fvm) { $fvm.Source } else { Join-Path $env:LOCALAPPDATA 'Pub\Cache\bin\fvm.bat' }
$hasFvm = Test-Path -LiteralPath $fvmPath
Push-Location $mobile
try {
    if ($hasFvm) {
        & $fvmPath flutter analyze --no-fatal-infos --no-fatal-warnings
    }
    else {
        & flutter analyze --no-fatal-infos --no-fatal-warnings
    }
    if ($LASTEXITCODE -ne 0) { throw 'Flutter analyze failed.' }
    if ($hasFvm) { & $fvmPath flutter test } else { & flutter test }
    if ($LASTEXITCODE -ne 0) { throw 'Flutter tests failed.' }
}
finally { Pop-Location }

$frontendMatches = @(git -C $repoRoot grep -n -I -E 'https://[^ ]+/(api/|hubs/|auth/|orders|kiosks|products)' -- 'AutomaticBrewingCoffeeFE/app/**' 'AutomaticBrewingCoffeeFE/components/**' 'AutomaticBrewingCoffeeFE/services/**' 'AutomaticBrewingCoffeeFE/lib/**' 2>$null)
if ($frontendMatches.Count -gt 0) { throw 'Frontend executable source contains a production API-style URL.' }

$mobileMatches = @(git -C $repoRoot grep -n -I -E 'https://[^ ]+/(api/|hubs/|auth/|orders|kiosks|products)' -- 'AutomaticBrewingCoffeeApp/lib/**' 2>$null)
if ($mobileMatches.Count -gt 0) { throw 'Flutter executable source contains a production API-style URL.' }

if (-not (Test-Path -LiteralPath (Join-Path $frontend '.next\BUILD_ID'))) {
    Write-Warning 'Frontend build output is missing; run npm run build with local environment values.'
}
if (-not (Test-Path -LiteralPath (Join-Path $mobile '.fvmrc'))) { throw 'Flutter FVM pin is missing.' }

Write-Host 'Client endpoint scan passed for executable frontend and Flutter source.'
