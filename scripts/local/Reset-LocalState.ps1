[CmdletBinding()]
param(
    [string]$Profile = '',
    [string]$Confirmation = ''
)

$ErrorActionPreference = 'Stop'
if ($Profile -ne 'full-local-4-repos:v1') { throw 'Reset requires -Profile full-local-4-repos:v1.' }
if ($Confirmation -ne 'RESET CUPX LOCAL') { throw 'Reset requires -Confirmation "RESET CUPX LOCAL".' }

Write-Host 'Resources to reset: AutoBrewing_BE_Local and cupx-local-* named volumes.'
$answer = Read-Host 'Type RESET CUPX LOCAL again to continue'
if ($answer -ne 'RESET CUPX LOCAL') { throw 'Reset cancelled.' }

& (Join-Path $PSScriptRoot 'Backup-MainDatabase.ps1')
& (Join-Path $PSScriptRoot 'Stop-Infra.ps1')
& docker volume rm cupx-local-redis cupx-local-rabbitmq cupx-local-couchdb cupx-local-minio cupx-local-mailpit 2>$null
if ($LASTEXITCODE -ne 0) { Write-Warning 'Some named volumes were absent or still in use.' }
Write-Host 'Named local infrastructure volumes reset. Re-run Initialize-MainDatabase.ps1 and Start-Infra.ps1.'
