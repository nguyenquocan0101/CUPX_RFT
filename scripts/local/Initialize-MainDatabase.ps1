[CmdletBinding()]
param(
    [string]$Server = 'tcp:127.0.0.1,1433'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$database = 'AutoBrewing_BE_Local'
$profileName = 'CUPX_LOCAL_PROFILE'
$profileValue = 'full-local-4-repos:v1'
$composeVarsPath = Join-Path $repoRoot '.local\compose-vars'
$mainVarsPath = Join-Path $repoRoot '.local\main-api-vars'
$domainsProject = Join-Path $repoRoot 'AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\Domains\Domains.csproj'
$apiProject = Join-Path $repoRoot 'AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\APIs\APIs.csproj'
$artifactDirectory = Join-Path $repoRoot 'artifacts'
$rawMigrationPath = Join-Path $artifactDirectory 'local-main-schema.raw.sql'
$migrationPath = Join-Path $artifactDirectory 'local-main-schema.sql'

function Invoke-SqlScalar {
    param(
        [string]$Query,
        [string]$Database = 'master'
    )

    $output = & sqlcmd -b -S $Server -E -C -d $Database -h -1 -W -Q "SET NOCOUNT ON; $Query"
    if ($LASTEXITCODE -ne 0) {
        throw "SQL command failed against $Server/$Database."
    }

    $value = $output | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -Last 1
    if ($null -eq $value) {
        return ''
    }

    return $value.Trim()
}

function Read-EnvironmentFile {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing local environment file: $Path"
    }

    $values = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^\s*([^#][^=]*)=(.*)$') {
            $values[$matches[1].Trim()] = $matches[2]
        }
    }

    return $values
}

function New-RandomBase64 {
    param([int]$ByteCount)

    return [Convert]::ToBase64String(
        [Security.Cryptography.RandomNumberGenerator]::GetBytes($ByteCount))
}

if (-not (Test-Path -LiteralPath $composeVarsPath -PathType Leaf)) {
    throw 'Run scripts/local/Initialize-LocalEnvironment.ps1 before database initialization.'
}

if (-not (Test-Path -LiteralPath $mainVarsPath -PathType Leaf)) {
    $encryptionKey = [Convert]::ToHexString(
        [Security.Cryptography.RandomNumberGenerator]::GetBytes(8))
    @(
        "JWT__Key=$(New-RandomBase64 -ByteCount 48)"
        "CUPX_API_KEY_ENCRYPTION_KEY=$encryptionKey"
        'LocalSeed__Enabled=true'
        'LocalSeed__AdminEmail=admin@cupx.local'
        "LocalSeed__AdminPassword=$(New-RandomBase64 -ByteCount 18)"
        "LocalSeed__KioskApiKey=$(New-RandomBase64 -ByteCount 24)"
        'LocalSeed__KioskBaseUrl=http://localhost:5160'
    ) | Set-Content -LiteralPath $mainVarsPath -Encoding utf8
    Write-Host 'Generated ignored Main API credentials in .local/main-api-vars.'
}

$databaseExists = Invoke-SqlScalar -Query "SELECT CASE WHEN DB_ID(N'$database') IS NULL THEN 0 ELSE 1 END;"

if ($databaseExists -eq '0') {
    $canCreate = Invoke-SqlScalar -Query @"
SELECT CASE
    WHEN IS_SRVROLEMEMBER(N'sysadmin') = 1
      OR HAS_PERMS_BY_NAME(NULL, N'SERVER', N'CREATE ANY DATABASE') = 1
    THEN 1 ELSE 0 END;
"@
    if ($canCreate -ne '1') {
        throw "The current Windows identity cannot create $database."
    }

    Invoke-SqlScalar -Query "CREATE DATABASE [$database]; SELECT 1;" | Out-Null
    Invoke-SqlScalar -Database $database -Query @"
EXEC sys.sp_addextendedproperty
    @name = N'$profileName',
    @value = N'$profileValue';
SELECT 1;
"@ | Out-Null
    Write-Host "Created owned database $database."
}
else {
    $marker = Invoke-SqlScalar -Database $database -Query @"
SELECT CONVERT(nvarchar(256), value)
FROM sys.extended_properties
WHERE class = 0 AND name = N'$profileName';
"@
    if ($marker -ne $profileValue) {
        throw "Refusing migration: $database does not have ownership marker $profileName=$profileValue."
    }

    & (Join-Path $PSScriptRoot 'Backup-MainDatabase.ps1') -Server $Server
}

Push-Location $repoRoot
try {
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet tool restore failed.'
    }

    $efVersionOutput = & dotnet tool run dotnet-ef --version
    $efVersionText = $efVersionOutput -join "`n"
    if ($LASTEXITCODE -ne 0 -or $efVersionText -notmatch '\b8\.0\.8\b') {
        throw 'The local dotnet-ef tool must be version 8.0.8.'
    }

    New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null

    $env:ASPNETCORE_ENVIRONMENT = 'Local'
    $env:LOCAL_MODE = 'true'
    $env:BackgroundJobs__Enabled = 'false'
    $env:ConnectionStrings__Db =
        "Server=127.0.0.1,1433;Database=$database;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"

    $composeValues = Read-EnvironmentFile -Path $composeVarsPath
    $mainValues = Read-EnvironmentFile -Path $mainVarsPath
    foreach ($entry in $mainValues.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
    $env:RabbitMQ__UserName = $composeValues['RABBITMQ_USERNAME']
    $env:RabbitMQ__Password = $composeValues['RABBITMQ_PASSWORD']

    & dotnet tool run dotnet-ef migrations script --idempotent `
        --project $domainsProject `
        --startup-project $apiProject `
        --output $rawMigrationPath
    if ($LASTEXITCODE -ne 0) {
        throw 'EF migration script generation failed.'
    }

    @"
IF DB_NAME() <> N'$database'
    THROW 51000, 'Refusing to apply CUPX migrations to an unexpected database.', 1;
GO
"@ | Set-Content -LiteralPath $migrationPath -Encoding utf8
    Get-Content -LiteralPath $rawMigrationPath | Add-Content -LiteralPath $migrationPath -Encoding utf8
    Remove-Item -LiteralPath $rawMigrationPath

    & sqlcmd -b -S $Server -E -C -d $database -i $migrationPath
    if ($LASTEXITCODE -ne 0) {
        throw 'EF migration script apply failed.'
    }

    $migrationCount = Invoke-SqlScalar -Database $database -Query @"
SELECT CASE
    WHEN DB_NAME() = N'$database'
     AND CONVERT(nvarchar(256), (
        SELECT value FROM sys.extended_properties
        WHERE class = 0 AND name = N'$profileName')) = N'$profileValue'
     AND OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
    THEN (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)
    ELSE -1 END;
"@
    if ([int]$migrationCount -le 0) {
        throw 'Database ownership or EF migration verification failed.'
    }

    $seedCounts = @()
    foreach ($run in 1..2) {
        & dotnet run --project $apiProject --launch-profile local -- --seed-only
        if ($LASTEXITCODE -ne 0) {
            throw "Local seed run $run failed."
        }

        $seedCounts += Invoke-SqlScalar -Database $database -Query @"
SELECT CONCAT(
    (SELECT COUNT(*) FROM dbo.Accounts WHERE AccountId = N'local-admin'), N'|',
    (SELECT COUNT(*) FROM dbo.Organizations WHERE OrganizationId = N'local-org'), N'|',
    (SELECT COUNT(*) FROM dbo.Kiosks WHERE KioskId = N'local-kiosk'), N'|',
    (SELECT COUNT(*) FROM dbo.Products WHERE ProductId = N'local-product'), N'|',
    (SELECT COUNT(*) FROM dbo.KioskVersionProductMappings
        WHERE KioskVersionId = N'local-kiosk-v1' AND ProductId = N'local-product'), N'|',
    (SELECT COUNT(*) FROM dbo.KioskVersionDeviceModelMappings
        WHERE KioskVersionId = N'local-kiosk-v1' AND DeviceModelId = N'local-device-model'), N'|',
    (SELECT COUNT(*) FROM dbo.Webhooks WHERE WebhookId LIKE N'local-webhook-%'));
"@
    }

    if ($seedCounts[0] -ne '1|1|1|1|1|1|2' -or $seedCounts[1] -ne $seedCounts[0]) {
        throw "Local seed is incomplete or not idempotent: $($seedCounts -join ' -> ')"
    }

    Write-Host "Main database ready: $database ($migrationCount migrations, seed $($seedCounts[1]))."
}
finally {
    Pop-Location
}
