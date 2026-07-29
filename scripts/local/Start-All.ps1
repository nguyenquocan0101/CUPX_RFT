[CmdletBinding()]
param(
    [ValidateSet('simulator', 'real')]
    [string]$HardwareMode = 'simulator',
    [switch]$SkipDatabase,
    [switch]$SkipFrontend,
    [switch]$SkipSimulator
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtime = Join-Path $repoRoot '.local\runtime'
New-Item -ItemType Directory -Path $runtime -Force | Out-Null

$ownedPorts = [System.Collections.Generic.List[int]]::new()
foreach ($port in @(3000, 5100, 5160)) {
    $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($listener) {
        $processInfo = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)" -ErrorAction SilentlyContinue
        if ($processInfo.CommandLine -like "*$repoRoot*") { $ownedPorts.Add($port) }
    }
}
$composeFile = Join-Path $repoRoot 'compose.local.yml'
$composeEnv = Join-Path $repoRoot '.local\compose-vars'
$runningServices = @(& docker compose -f $composeFile --env-file $composeEnv ps --status running --services 2>$null)
if ($runningServices.Count -gt 0) {
    foreach ($port in @(5672, 15672, 5984, 6379, 8025, 9000, 9001, 1025)) {
        if (Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue) { $ownedPorts.Add($port) }
    }
}
& (Join-Path $PSScriptRoot 'Test-Prerequisites.ps1') -AllowedOccupiedPorts $ownedPorts.ToArray()
if (-not $SkipDatabase) { & (Join-Path $PSScriptRoot 'Initialize-MainDatabase.ps1') }

& (Join-Path $PSScriptRoot 'Start-Infra.ps1')

function Start-LocalProcess {
    param([string]$Name, [string]$Script)
    $pidFile = Join-Path $runtime "$Name.pid"
    if (Test-Path -LiteralPath $pidFile) {
        $existing = [int](Get-Content -LiteralPath $pidFile)
        $existingProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $existing" -ErrorAction SilentlyContinue
        if ($existingProcess -and $existingProcess.CommandLine -like "*$Script*") { return }
        Remove-Item -LiteralPath $pidFile -Force -ErrorAction SilentlyContinue
    }
    $log = Join-Path $runtime "$Name.log"
    $errorLog = Join-Path $runtime "$Name.error.log"
    $process = Start-Process -FilePath 'powershell.exe' -WorkingDirectory $repoRoot -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $Script) -RedirectStandardOutput $log -RedirectStandardError $errorLog -PassThru
    $process.Id | Set-Content -LiteralPath $pidFile -Encoding ascii
    Start-Sleep -Seconds 2
}

if (-not (Get-NetTCPConnection -State Listen -LocalPort 5100 -ErrorAction SilentlyContinue)) {
    Start-LocalProcess -Name 'main-api' -Script (Join-Path $PSScriptRoot 'Start-MainApi.ps1')
}
if (-not (Get-NetTCPConnection -State Listen -LocalPort 5160 -ErrorAction SilentlyContinue)) {
    Start-LocalProcess -Name 'kiosk-api' -Script (Join-Path $PSScriptRoot 'Start-KioskApi.ps1')
}
if ($HardwareMode -eq 'simulator' -and -not $SkipSimulator) {
    Start-LocalProcess -Name 'device-simulator' -Script (Join-Path $PSScriptRoot 'Start-DeviceSimulator.ps1')
}

if (-not $SkipFrontend) {
    $frontendPath = Join-Path $repoRoot 'AutomaticBrewingCoffeeFE'
    if (-not (Get-NetTCPConnection -State Listen -LocalPort 3000 -ErrorAction SilentlyContinue)) {
        $frontendLog = Join-Path $runtime 'frontend.log'
        $frontendErrorLog = Join-Path $runtime 'frontend.error.log'
        $process = Start-Process -FilePath 'cmd.exe' -WorkingDirectory $frontendPath -ArgumentList '/c', 'npm', 'run', 'dev' -RedirectStandardOutput $frontendLog -RedirectStandardError $frontendErrorLog -PassThru
        $process.Id | Set-Content -LiteralPath (Join-Path $runtime 'frontend.pid') -Encoding ascii
    }
}

Write-Host "CUPX local stack started. HardwareMode=$HardwareMode"
Write-Host 'Run scripts/local/Smoke-Test.ps1 for bounded verification.'
