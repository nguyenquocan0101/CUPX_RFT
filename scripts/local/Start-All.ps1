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

function Stop-ProcessTree {
    param([int]$RootId)
    $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $RootId" -ErrorAction SilentlyContinue)
    foreach ($child in $children) { Stop-ProcessTree -RootId $child.ProcessId }
    if (Get-Process -Id $RootId -ErrorAction SilentlyContinue) {
        Stop-Process -Id $RootId -Force -ErrorAction SilentlyContinue
    }
}

function Start-LocalProcess {
    param(
        [string]$Name,
        [string]$Script,
        [int]$Port = 0,
        [string]$ChildMarker
    )

    $pidFile = Join-Path $runtime "$Name.pid"
    if (Test-Path -LiteralPath $pidFile) {
        $existing = [int](Get-Content -LiteralPath $pidFile)
        $existingProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $existing" -ErrorAction SilentlyContinue
        if ($existingProcess -and $existingProcess.CommandLine -like "*$Script*") {
            $healthy = if ($Port -gt 0) {
                [bool](Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue)
            } elseif ($ChildMarker) {
                [bool](Get-CimInstance Win32_Process | Where-Object {
                    $_.CommandLine -like "*$ChildMarker*" -and $_.CommandLine -like "*$repoRoot*"
                } | Select-Object -First 1)
            } else {
                $true
            }
            if ($healthy) { return }
            Stop-ProcessTree -RootId $existingProcess.ProcessId
        }
        Remove-Item -LiteralPath $pidFile -Force -ErrorAction SilentlyContinue
    }
    $log = Join-Path $runtime "$Name.log"
    $errorLog = Join-Path $runtime "$Name.error.log"
    $process = Start-Process -FilePath 'powershell.exe' -WorkingDirectory $repoRoot -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $Script) -RedirectStandardOutput $log -RedirectStandardError $errorLog -PassThru
    $process.Id | Set-Content -LiteralPath $pidFile -Encoding ascii
    $deadline = (Get-Date).AddSeconds(60)
    do {
        $healthy = if ($Port -gt 0) {
            [bool](Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue)
        } elseif ($ChildMarker) {
            [bool](Get-CimInstance Win32_Process | Where-Object {
                $_.CommandLine -like "*$ChildMarker*" -and $_.CommandLine -like "*$repoRoot*"
            } | Select-Object -First 1)
        } else {
            $true
        }
        if ($healthy) { return }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    throw "$Name did not become healthy within 60 seconds. See $log and $errorLog."
}

if (-not (Get-NetTCPConnection -State Listen -LocalPort 5100 -ErrorAction SilentlyContinue)) {
    Start-LocalProcess -Name 'main-api' -Script (Join-Path $PSScriptRoot 'Start-MainApi.ps1') -Port 5100
}
if (-not (Get-NetTCPConnection -State Listen -LocalPort 5160 -ErrorAction SilentlyContinue)) {
    Start-LocalProcess -Name 'kiosk-api' -Script (Join-Path $PSScriptRoot 'Start-KioskApi.ps1') -Port 5160
}
if ($HardwareMode -eq 'simulator' -and -not $SkipSimulator) {
    Start-LocalProcess -Name 'device-simulator' -Script (Join-Path $PSScriptRoot 'Start-DeviceSimulator.ps1') -ChildMarker 'DeviceSimulator'
}

if (-not $SkipFrontend) {
    $frontendPath = Join-Path $repoRoot 'AutomaticBrewingCoffeeFE'
    if (-not (Get-NetTCPConnection -State Listen -LocalPort 3000 -ErrorAction SilentlyContinue)) {
        $frontendPidFile = Join-Path $runtime 'frontend.pid'
        if (Test-Path -LiteralPath $frontendPidFile) {
            $frontendPid = [int](Get-Content -LiteralPath $frontendPidFile)
            $frontendProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $frontendPid" -ErrorAction SilentlyContinue
            if ($frontendProcess -and $frontendProcess.CommandLine -like '*npm*') {
                Stop-ProcessTree -RootId $frontendProcess.ProcessId
            }
            Remove-Item -LiteralPath $frontendPidFile -Force -ErrorAction SilentlyContinue
        }
        $frontendLog = Join-Path $runtime 'frontend.log'
        $frontendErrorLog = Join-Path $runtime 'frontend.error.log'
        $process = Start-Process -FilePath 'cmd.exe' -WorkingDirectory $frontendPath -ArgumentList '/c', 'npm', 'run', 'dev' -RedirectStandardOutput $frontendLog -RedirectStandardError $frontendErrorLog -PassThru
        $process.Id | Set-Content -LiteralPath (Join-Path $runtime 'frontend.pid') -Encoding ascii
        $deadline = (Get-Date).AddSeconds(60)
        do {
            if (Get-NetTCPConnection -State Listen -LocalPort 3000 -ErrorAction SilentlyContinue) { break }
            Start-Sleep -Milliseconds 500
        } while ((Get-Date) -lt $deadline)
        if (-not (Get-NetTCPConnection -State Listen -LocalPort 3000 -ErrorAction SilentlyContinue)) {
            throw "frontend did not become healthy within 60 seconds. See $frontendLog and $frontendErrorLog."
        }

        $routeReady = $false
        $deadline = (Get-Date).AddSeconds(60)
        do {
            try {
                $response = Invoke-WebRequest -UseBasicParsing -Uri 'http://127.0.0.1:3000/login' -TimeoutSec 5
                $routeReady = $response.StatusCode -eq 200
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        } while (-not $routeReady -and (Get-Date) -lt $deadline)
        if (-not $routeReady) {
            throw "frontend /login route did not become ready within 60 seconds. See $frontendLog and $frontendErrorLog."
        }
    }
}

Write-Host "CUPX local stack started. HardwareMode=$HardwareMode"
Write-Host 'Run scripts/local/Smoke-Test.ps1 for bounded verification.'
