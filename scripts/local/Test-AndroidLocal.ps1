[CmdletBinding()]
param(
    [string]$ApplicationId = 'com.example.abc_androidapp',
    [int]$StartupTimeoutSeconds = 20
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$mobile = Join-Path $repoRoot 'AutomaticBrewingCoffeeApp'
$varsPath = Join-Path $repoRoot '.local\main-api-vars'
$runtime = Join-Path $repoRoot '.local\runtime'
$adb = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
$fvm = Join-Path $env:LOCALAPPDATA 'Pub\Cache\bin\fvm.bat'
$apk = Join-Path $mobile 'build\app\outputs\flutter-apk\app-debug.apk'

if (-not (Test-Path -LiteralPath $varsPath -PathType Leaf)) {
    throw 'Missing .local/main-api-vars. Initialize the local Main API profile first.'
}
if (-not (Test-Path -LiteralPath $adb -PathType Leaf)) {
    throw "ADB was not found at $adb."
}
if (-not (Test-Path -LiteralPath $fvm -PathType Leaf)) {
    throw "FVM was not found at $fvm."
}

$values = @{}
foreach ($line in Get-Content -LiteralPath $varsPath) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        $values[$matches[1].Trim()] = $matches[2]
    }
}
$apiKey = $values['LocalSeed__KioskApiKey']
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw 'LocalSeed__KioskApiKey is missing from .local/main-api-vars.'
}

New-Item -ItemType Directory -Force -Path $runtime | Out-Null
$buildLog = Join-Path $runtime 'android-local-build.log'
$buildErrorLog = Join-Path $runtime 'android-local-build.error.log'

Push-Location $mobile
try {
    $build = Start-Process -FilePath $fvm `
        -ArgumentList @(
            'flutter', 'build', 'apk', '--debug',
            '--dart-define=CUPX_LOCAL_MODE=true',
            '--dart-define=CUPX_API_BASE_URL=http://10.0.2.2:5100/api/v1',
            '--dart-define=CUPX_SIGNALR_URL=http://10.0.2.2:5100/hubs/notification',
            "--dart-define=CUPX_API_KEY=$apiKey",
            '--dart-define=CUPX_KIOSK_ID=local-kiosk',
            '--dart-define=CUPX_CLIENT_ID=local-client',
            '--dart-define=CUPX_SIDE=left'
        ) `
        -WorkingDirectory $mobile `
        -RedirectStandardOutput $buildLog `
        -RedirectStandardError $buildErrorLog `
        -PassThru -Wait
    if ($build.ExitCode -ne 0) {
        throw "Android debug APK build failed. See $buildErrorLog."
    }
}
finally { Pop-Location }

if (-not (Test-Path -LiteralPath $apk -PathType Leaf)) {
    throw "Android APK was not produced at $apk."
}

& $adb install -r $apk *> $null
if ($LASTEXITCODE -ne 0) { throw 'Android APK installation failed.' }
& $adb logcat -c
& $adb shell am force-stop $ApplicationId
& $adb shell am start -n "$ApplicationId/.MainActivity" *> $null
if ($LASTEXITCODE -ne 0) { throw 'Android MainActivity did not start.' }

$deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
$appPid = ''
$organizationSuccess = 0
$fatal = 0
do {
    Start-Sleep -Seconds 1
    $appPid = ((& $adb shell pidof $ApplicationId 2>$null) -join '').Trim()
    $log = @(& $adb logcat -d -t 1600)
    $organizationSuccess = @($log | Select-String 'Organization: Fetched and cached successfully').Count
    $fatal = @($log | Select-String 'FATAL EXCEPTION|NotInitializedError').Count
} while (([string]::IsNullOrWhiteSpace($appPid) -or $organizationSuccess -lt 1) -and (Get-Date) -lt $deadline)

if ([string]::IsNullOrWhiteSpace($appPid)) {
    throw "Android application did not remain running within $StartupTimeoutSeconds seconds."
}

$screen = Join-Path $runtime 'android-emulator-screen.png'
& $adb exec-out screencap -p > $screen

if ($fatal -gt 0) {
    throw 'Android local smoke found a fatal exception. Inspect logcat on the emulator.'
}
if ($organizationSuccess -lt 1) {
    throw 'Android local smoke did not preload organization data from the local API.'
}

Write-Host "Android local smoke passed: app=$ApplicationId pid=$appPid organizationPreload=$organizationSuccess."
