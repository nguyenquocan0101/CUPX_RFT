[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Write-Host 'Checking infrastructure persistence...'
& (Join-Path $PSScriptRoot 'Test-InfraPersistence.ps1')
Write-Host 'Infrastructure persistence passed; restoring native local processes...'
& (Join-Path $PSScriptRoot 'Start-All.ps1') -HardwareMode simulator -SkipDatabase -SkipFrontend -SkipSimulator
Start-Sleep -Seconds 5
Write-Host 'Checking Kiosk CouchDB/RabbitMQ persistence...'
& (Join-Path $PSScriptRoot 'Test-KioskPersistence.ps1')
Write-Host 'Checking webhook inbox/outbox replay after infrastructure restart...'
& (Join-Path $PSScriptRoot 'Test-LocalWebhookPersistence.ps1')
Write-Host 'Local persistence checks passed.'
