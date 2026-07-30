[CmdletBinding()]
param(
    [int[]]$AllowedOccupiedPorts = @()
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$failures = [System.Collections.Generic.List[string]]::new()

function Add-CheckResult {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Detail,
        [switch]$Required
    )

    $status = if ($Passed) { 'PASS' } elseif ($Required) { 'FAIL' } else { 'WARN' }
    Write-Host ('[{0}] {1}: {2}' -f $status, $Name, $Detail)

    if (-not $Passed -and $Required) {
        $failures.Add(('{0}: {1}' -f $Name, $Detail))
    }
}

function Test-ListeningPort {
    param([int]$Port)

    return $null -ne (Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue |
        Select-Object -First 1)
}

function Resolve-ToolPath {
    param([string]$Name)

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $candidates = switch ($Name) {
        'fvm' {
            @(Join-Path $env:LOCALAPPDATA 'Pub\Cache\bin\fvm.bat')
        }
        'adb' {
            @(
                if ($env:ANDROID_HOME) { Join-Path $env:ANDROID_HOME 'platform-tools\adb.exe' }
                if ($env:ANDROID_SDK_ROOT) { Join-Path $env:ANDROID_SDK_ROOT 'platform-tools\adb.exe' }
                Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
            )
        }
        default { @() }
    }

    foreach ($candidate in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and
            (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            return $candidate
        }
    }

    return $null
}

Write-Host "Repository: $repoRoot"

$dockerDesktopPath = Join-Path $env:ProgramFiles 'Docker\Docker\Docker Desktop.exe'
Add-CheckResult -Name 'Docker Desktop' -Passed (Test-Path -LiteralPath $dockerDesktopPath) `
    -Detail $dockerDesktopPath -Required

$dockerCommand = Get-Command docker -ErrorAction SilentlyContinue
Add-CheckResult -Name 'Docker CLI' -Passed ($null -ne $dockerCommand) `
    -Detail $(if ($dockerCommand) { $dockerCommand.Source } else { 'docker is not in PATH' }) -Required

if ($dockerCommand) {
    $dockerOsType = (& docker info --format '{{.OSType}}' 2>$null)
    $dockerInfoExitCode = $LASTEXITCODE
    Add-CheckResult -Name 'Docker engine' -Passed ($dockerInfoExitCode -eq 0 -and $dockerOsType -eq 'linux') `
        -Detail $(if ($dockerInfoExitCode -eq 0) { "OSType=$dockerOsType" } else { 'engine is not ready' }) -Required

    $composeVersion = (& docker compose version --short 2>$null)
    Add-CheckResult -Name 'Docker Compose' -Passed ($LASTEXITCODE -eq 0) `
        -Detail $(if ($composeVersion) { $composeVersion } else { 'compose plugin unavailable' }) -Required
}

$sqlListening = Test-ListeningPort -Port 1433
Add-CheckResult -Name 'SQL Server host' -Passed $sqlListening -Detail '127.0.0.1:1433' -Required

$requiredFreePorts = @(3000, 5100, 5160, 5672, 15672, 5984, 6379, 8025, 9000, 9001, 1025)
foreach ($port in $requiredFreePorts) {
    $isListening = Test-ListeningPort -Port $port
    $ownedByCupx = $AllowedOccupiedPorts -contains $port
    $passed = -not $isListening -or $ownedByCupx
    $detail = if ($ownedByCupx) {
        'published by the running CUPX stack'
    }
    elseif ($isListening) {
        'already listening'
    }
    else {
        'available'
    }

    Add-CheckResult -Name "Port $port" -Passed $passed -Detail $detail -Required
}

foreach ($tool in @('dotnet', 'node', 'npm', 'flutter', 'fvm', 'adb', 'sqlcmd')) {
    $toolPath = Resolve-ToolPath -Name $tool
    Add-CheckResult -Name $tool -Passed (-not [string]::IsNullOrWhiteSpace($toolPath)) `
        -Detail $(if ($toolPath) { $toolPath } else { 'not installed; required in a later phase' })
}

if ($failures.Count -gt 0) {
    throw ("Prerequisite checks failed:`n- " + ($failures -join "`n- "))
}

Write-Host 'All required Phase 01 prerequisites passed.'
