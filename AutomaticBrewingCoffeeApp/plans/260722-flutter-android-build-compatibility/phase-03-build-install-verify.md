# Phase 03 — Build, install & kiosk verification

**Stories:** P1 Android toolchain; P1 kiosk screen awake; P3 production signing out of scope.  
**Goal:** produce and smoke-test a release APK while preserving kiosk behavior.

## Tasks

1. Run `flutter analyze --no-pub` against the captured baseline and `flutter build apk --release`; verify the build exit code is 0 and the APK at `build/app/outputs/flutter-apk/app-release.apk` is non-empty.
2. Run `flutter devices`/`adb devices -l`; if a target exists, install with `adb -s <serial> install -r` using existing debug signing. If no target exists, mark device acceptance blocked rather than claiming it passed.
3. Launch application ID `com.example.abc_androidapp`, read the device display timeout, exercise kiosk paths calling `WakelockPlus.enable/disable`, wait beyond that timeout, and verify the display remains on; retain `WAKE_LOCK` in the merged manifest.
4. Record command outputs, artifact path/size, device and residual warnings. Do not configure production keystore.

## Commands

`flutter analyze --no-pub`; `flutter build apk --release`; `Get-Item build/app/outputs/flutter-apk/app-release.apk`; `flutter devices`; `adb devices -l`; `adb -s <serial> install -r build/app/outputs/flutter-apk/app-release.apk`; `adb -s <serial> shell monkey -p com.example.abc_androidapp 1`; `adb -s <serial> shell settings get system screen_off_timeout`; `flutter logs`.

## Acceptance

- Release build exits 0 and APK is >0 bytes.
- APK installs and launches on an available target; if no target exists, installation/smoke-test status is recorded as blocked. Kiosk UI/behavior is unchanged and screen stays awake beyond the configured timeout.
- No production keys/secrets; debug signing is local-test only.

## Rollback

Uninstall only the test APK if needed. Remove generated outputs with `flutter clean`; revert changes via targeted patches, never blanket worktree reset.
