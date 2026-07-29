[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Coffee', 'CupDrop', 'IceMaker', 'Inhale', 'Mix')]
    [string]$Controller,
    [Parameter(Mandatory = $true)]
    [string]$DeviceId,
    [Parameter(Mandatory = $true)]
    [string]$SerialPort,
    [int]$BaudRate = 115200
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

$projects = @{
    Coffee = 'AutomaticBrewingCoffeeKioskBE\CoffeeMachineController\CoffeeMachineController\CoffeeMachineController.csproj'
    CupDrop = 'AutomaticBrewingCoffeeKioskBE\CupDropMachineController\CupDropMachineController\CupDropMachineController.csproj'
    IceMaker = 'AutomaticBrewingCoffeeKioskBE\IceMakerMachine\IceMakerMachine\IceMakerMachine.csproj'
    Inhale = 'AutomaticBrewingCoffeeKioskBE\InhaleController\InhaleController\InhaleController.csproj'
    Mix = 'AutomaticBrewingCoffeeKioskBE\MixMachineController\MixMachineController\MixMachineController.csproj'
}

$env:HARDWARE_MODE = 'real'
$env:DEVICE_ID = $DeviceId
$env:SERIAL_PORT = $SerialPort
$env:BAUD_RATE = $BaudRate.ToString()
$env:DOTNET_ENVIRONMENT = 'Local'

$runtimeDir = Join-Path $repoRoot '.local\runtime'
New-Item -ItemType Directory -Force -Path $runtimeDir | Out-Null
$safeName = $Controller.ToLowerInvariant()
$stdout = Join-Path $runtimeDir "controller-$safeName.out.log"
$stderr = Join-Path $runtimeDir "controller-$safeName.err.log"
$project = Join-Path $repoRoot $projects[$Controller]

$process = Start-Process -FilePath 'dotnet' `
    -ArgumentList @('run', '--no-build', '--project', $project) `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput $stdout `
    -RedirectStandardError $stderr `
    -PassThru

Write-Host "Started $Controller controller: PID=$($process.Id) device=$DeviceId serial=$SerialPort"
Write-Host "stdout: $stdout"
Write-Host "stderr: $stderr"
