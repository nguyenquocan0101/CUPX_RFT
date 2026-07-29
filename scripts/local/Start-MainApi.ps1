[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeVarsPath = Join-Path $repoRoot '.local\compose-vars'
$mainVarsPath = Join-Path $repoRoot '.local\main-api-vars'
$projectPath = Join-Path $repoRoot 'AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\APIs\APIs.csproj'

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
$env:BackgroundJobs__Enabled = 'false'
$env:RabbitMQ__UserName = $env:RABBITMQ_USERNAME
$env:RabbitMQ__Password = $env:RABBITMQ_PASSWORD
$env:MINIO__AccessKey = $env:MINIO_ROOT_USER
$env:MINIO__SecretKey = $env:MINIO_ROOT_PASSWORD
$env:MINIO__Endpoint = "http://127.0.0.1:$($env:MINIO_API_PORT)"
$env:MINIO__Bucket = $env:MINIO_BUCKET
$env:SmtpSettings__Host = '127.0.0.1'
$env:SmtpSettings__Server = '127.0.0.1'
$env:SmtpSettings__Port = $env:MAILPIT_SMTP_PORT
$env:SmtpSettings__UseSsl = 'false'
$env:SmtpSettings__RequiresAuthentication = 'false'

& dotnet run --project $projectPath --launch-profile local
if ($LASTEXITCODE -ne 0) {
    throw 'Main API exited with an error.'
}
