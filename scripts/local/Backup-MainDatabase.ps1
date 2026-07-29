[CmdletBinding()]
param(
    [string]$Server = 'tcp:127.0.0.1,1433'
)

$ErrorActionPreference = 'Stop'
$database = 'AutoBrewing_BE_Local'
$profileName = 'CUPX_LOCAL_PROFILE'
$profileValue = 'full-local-4-repos:v1'

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

$marker = Invoke-SqlScalar -Database $database -Query @"
SELECT CONVERT(nvarchar(256), value)
FROM sys.extended_properties
WHERE class = 0 AND name = N'$profileName';
"@

if ($marker -ne $profileValue) {
    throw "Refusing backup: $database does not have the expected ownership marker."
}

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupName = "${database}-${timestamp}.bak"
$backupPath = Invoke-SqlScalar -Database $database -Query @"
DECLARE @directory nvarchar(4000) =
    CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'));
IF @directory IS NULL OR LEN(@directory) = 0
    THROW 51000, 'SQL Server default backup path is unavailable.', 1;
IF RIGHT(@directory, 1) NOT IN (N'\', N'/')
    SET @directory += N'\';
DECLARE @path nvarchar(4000) = @directory + N'$backupName';
BACKUP DATABASE [$database] TO DISK = @path WITH COPY_ONLY, INIT, CHECKSUM;
SELECT @path;
"@

Write-Host "Backup created: $backupPath"
