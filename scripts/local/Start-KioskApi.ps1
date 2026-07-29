[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeVarsPath = Join-Path $repoRoot '.local\compose-vars'
$mainVarsPath = Join-Path $repoRoot '.local\main-api-vars'
$projectPath = Join-Path $repoRoot 'AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService\Kiosk.ApiService.csproj'

function Import-EnvironmentFile {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing local environment file: $Path"
    }

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -notmatch '^\s*([^#][^=]*)=(.*)$') {
            continue
        }

        [Environment]::SetEnvironmentVariable(
            $matches[1].Trim(),
            $matches[2],
            [EnvironmentVariableTarget]::Process)
    }
}

Import-EnvironmentFile -Path $composeVarsPath
Import-EnvironmentFile -Path $mainVarsPath

$env:ASPNETCORE_ENVIRONMENT = 'Local'
$env:LOCAL_MODE = 'true'
$env:WORKFLOW_WORKERS_ENABLED = 'true'
$env:CouchDB__Username = $env:COUCHDB_USERNAME
$env:CouchDB__Pwd = $env:COUCHDB_PASSWORD
$env:RabbitMQ__UserName = $env:RABBITMQ_USERNAME
$env:RabbitMQ__Password = $env:RABBITMQ_PASSWORD
$env:ApiKey = $env:LocalSeed__KioskApiKey
$env:MAIN_BACKEND__OutboundApiKey = $env:LocalSeed__KioskApiKey
$env:MAIN_BACKEND__BaseUrl = 'http://localhost:5100'

Push-Location (Split-Path -Parent $projectPath)
try {
    & dotnet run --no-build --project $projectPath --launch-profile local
}
finally {
    Pop-Location
}
if ($LASTEXITCODE -ne 0) {
    throw 'Kiosk API exited with an error.'
}
