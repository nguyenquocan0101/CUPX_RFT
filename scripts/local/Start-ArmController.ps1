[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$DeviceId,
    [string]$RobotIp = '192.168.58.2'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composeVars = Join-Path $repoRoot '.local\compose-vars'
if (-not (Test-Path -LiteralPath $composeVars)) {
    throw "Missing local service configuration: $composeVars"
}

foreach ($line in Get-Content -LiteralPath $composeVars) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        [Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2], 'Process')
    }
}

$env:HARDWARE_MODE = 'real'
$env:DEVICE_ID = $DeviceId
$env:ARM_ROBOT_IP = $RobotIp
$env:DOTNET_ENVIRONMENT = 'Local'
$runtimeDir = Join-Path $repoRoot '.local\runtime'
New-Item -ItemType Directory -Force -Path $runtimeDir | Out-Null
$journalPath = Join-Path $runtimeDir 'controller-arm.json'
$env:LOCAL_COMMAND_JOURNAL = $journalPath
$controllerDir = Join-Path $repoRoot 'AutomaticBrewingCoffeeKioskBE\ArmController\ArmController2'
$executable = Join-Path $controllerDir 'bin\Debug\ArmController2.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    throw "ArmController2 is not built: $executable"
}

$stdout = Join-Path $runtimeDir 'controller-arm.out.log'
$stderr = Join-Path $runtimeDir 'controller-arm.err.log'
$process = Start-Process -FilePath $executable `
    -ArgumentList @() `
    -WorkingDirectory $controllerDir `
    -RedirectStandardOutput $stdout `
    -RedirectStandardError $stderr `
    -PassThru

Write-Host "Started Arm controller: PID=$($process.Id) device=$DeviceId robot=$RobotIp"
Write-Host "stdout: $stdout"
Write-Host "stderr: $stderr"
