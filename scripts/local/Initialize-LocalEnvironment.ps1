[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtimeDirectory = Join-Path $repoRoot '.local'
$outputPath = Join-Path $runtimeDirectory 'compose-vars'

if (Test-Path -LiteralPath $outputPath) {
    throw 'Local configuration already exists. Credential rotation requires the explicit reset/migration workflow.'
}

function New-RandomValue {
    $bytes = New-Object byte[] 32
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $generator.GetBytes($bytes)
    }
    finally {
        $generator.Dispose()
    }

    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

New-Item -ItemType Directory -Force -Path $runtimeDirectory | Out-Null

$values = @(
    'RABBITMQ_USERNAME=cupx',
    ('RABBITMQ_PASSWORD=' + (New-RandomValue)),
    'COUCHDB_USERNAME=cupx',
    ('COUCHDB_PASSWORD=' + (New-RandomValue)),
    ('COUCHDB_SECRET=' + (New-RandomValue)),
    'MINIO_ROOT_USER=cupx',
    ('MINIO_ROOT_PASSWORD=' + (New-RandomValue)),
    'MINIO_BUCKET=cupx-local'
)

Set-Content -LiteralPath $outputPath -Value $values -Encoding ascii
Write-Host "Generated ignored local configuration at $outputPath."
