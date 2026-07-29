# Plan: Khôi phục build APK Android trên Flutter stable mới

**Date:** 2026-07-22  
**Mode:** --hard  
**Risk:** normal — nhiều file cấu hình/toolchain và generated state, nhưng thay đổi có thể kiểm chứng và rollback; không đụng dữ liệu/auth/production signing.  
**Spec:** `plans/flutter-android-build-compatibility/spec.md`  
**Directory:** `plans/260722-flutter-android-build-compatibility/`

## Scope challenge

- **Exists?** Kiosk và `wakelock_plus` đã tồn tại; lỗi ban đầu nằm ở dependency path, còn các package/toolchain blocker phải được tái hiện trên SDK thực tế.
- **Minimum?** Hosted `wakelock_plus`, tái tạo metadata, chỉ nâng package được nêu trong lỗi thực tế, rồi chỉ chỉnh Gradle/Java/SDK khi build thực tế yêu cầu.
- **Complexity?** Hard: cần xác nhận Flutter/Dart/Java/Gradle/AGP/Kotlin/SDK và cài/chạy APK.

## Chosen approach

Giữ nguyên Dart/UI/runtime. Dùng `wakelock_plus` từ pub.dev (hosted 1.3.2), trước hết tái hiện build trên Flutter/Dart thực tế rồi chỉ nâng từng package nếu chính build/analyzer chỉ rõ blocker tương thích (các candidate `tdesign_flutter` 0.2.7 và `google_fonts` 8.2.0 chỉ là phương án có điều kiện). Không thay wakelock bằng native Android. Bảo toàn dirty changes. Xác định file Gradle active bằng task evaluation trước khi loại/quarantine DSL trùng lặp. Dùng debug signing cho release APK kiểm thử.

## User stories mapped

- **P1 — Hosted dependency:** `flutter pub get` exit 0; lock không còn `source: path` cho `wakelock_plus`.
- **P1 — Android toolchain:** `flutter build apk --release` exit 0; APK nằm trong `build/app/outputs/flutter-apk/`.
- **P1 — Kiosk screen awake:** giữ nguyên `WakelockPlus.enable/disable` và permission; không đổi UI/runtime.
- **P2 — One Gradle module source:** module `app` chỉ có một DSL/file active rõ ràng.
- **P3 — Production signing:** out of scope; để phase phát hành sau.

## Phases

1. [x] **Baseline & dependency/API recovery** — xác nhận SDK, preserve dirty state, hosted dependency, xử lý package lỗi được tái hiện và regenerate metadata.
2. [x] **Android toolchain alignment** — xác minh Gradle active và áp dụng tối thiểu các chỉnh sửa cần thiết.
3. **Build, install & kiosk verification** — analyze, build, inspect/install APK và kiểm tra màn hình sáng.

## Exact command sequence

```powershell
flutter --version
flutter doctor -v
git status --short
git diff -- android/app/build.gradle android/app/build.gradle.kts android/build.gradle android/settings.gradle android/gradle.properties android/gradle/wrapper/gradle-wrapper.properties pubspec.yaml pubspec.lock
$planTemp = Join-Path ([System.IO.Path]::GetTempPath()) "automatic-brewing-flutter-plan"
New-Item -ItemType Directory -Force $planTemp | Out-Null
flutter analyze --no-pub *> (Join-Path $planTemp "baseline-analyze.txt"); if ($LASTEXITCODE -gt 1) { throw "Unexpected analyzer failure" }
flutter clean
if ($LASTEXITCODE -ne 0) { throw "flutter clean failed" }
flutter pub get
if ($LASTEXITCODE -ne 0) { throw "flutter pub get failed; stop before analyze/build" }
rg -n "wakelock_plus|tdesign_flutter|google_fonts|source: path|path:" pubspec.yaml pubspec.lock .dart_tool/package_config.json
flutter analyze --no-pub *> (Join-Path $planTemp "post-dependency-analyze.txt"); if ($LASTEXITCODE -gt 1) { throw "Unexpected analyzer failure" }
Select-String -Path (Join-Path $planTemp "baseline-analyze.txt"),(Join-Path $planTemp "post-dependency-analyze.txt") -Pattern "\berror\b"
flutter build apk --release -v
if ($LASTEXITCODE -ne 0) { throw "APK build failed; stop and inspect the first reproducible error" }
Get-Item build/app/outputs/flutter-apk/app-release.apk
```

If Gradle fails, inspect before editing:

```powershell
Get-Content android/app/build.gradle
Get-Content android/app/build.gradle.kts
Get-Content android/build.gradle
Get-Content android/settings.gradle
Get-Content android/gradle.properties
Get-Content android/gradle/wrapper/gradle-wrapper.properties
java -version
.\android\gradlew.bat -p android :app:tasks --info
```

Install/smoke test on an attached device:

```powershell
flutter devices
adb devices -l
adb -s <serial> install -r build/app/outputs/flutter-apk/app-release.apk
adb -s <serial> shell dumpsys package com.example.abc_androidapp
```

## Acceptance criteria

- [ ] `flutter pub get` exits 0; no missing local `wakelock_plus` path.
- [ ] Lock and `.dart_tool` show hosted `wakelock_plus`; no stale path entry. Any package upgrade is tied to a captured, reproducible compiler/analyzer error and its public API remains compatible.
- [ ] `flutter analyze --no-pub` is compared with the captured baseline; no new errors or package call-site errors are introduced (existing warnings/info may remain).
- [ ] Release build exits 0; `build/app/outputs/flutter-apk/app-release.apk` exists and is >0 bytes.
- [ ] APK installs with debug signing and launches existing kiosk UI.
- [ ] Wake-lock calls/permissions remain intact and screen stays awake during kiosk flow.
- [ ] Module `app` has one unambiguous active Gradle DSL; no unrelated source/UI/API changes.

## Risks and mitigations

- **Stale generated metadata:** use `flutter clean` + `flutter pub get`; do not hand-edit generated files.
- **Package API drift:** after upgrades, inspect analyzer/build errors and make only import/signature adaptations required by the same public API; do not redesign UI.
- **Dirty worktree overwrite:** inspect status/diff first; apply surgical patches.
- **Untracked/user-owned files:** snapshot the complete status and every touched tracked file plus relevant untracked paths before edits; compare after each phase and restore only plan-owned changes.
- **AGP/Gradle/Kotlin mismatch:** do not blindly bump; use Flutter output and first reproducible error.
- **Groovy/KTS duplication:** run Gradle task evaluation with `--info`, identify the evaluated app script, then remove/quarantine only the inactive duplicate; stop if activity cannot be proven.
- **Debug signing misuse:** local testing only; never add keys/secrets.

## Rollback notes

Save status/diff before edits. Revert only files changed by this plan; never reset the whole worktree. Generated `.dart_tool`/build outputs are disposable. If `tdesign_flutter` or `google_fonts` upgrades introduce incompatible app API, restore the prior constraints and document the blocker before considering a narrower alternative.

## Red-team adjudication

- **ACCEPTED:** make package upgrades conditional on a captured compiler/analyzer error; fixed candidate versions are not prerequisites.
- **ACCEPTED:** snapshot tracked/untracked state and touched files before `pub get` or Gradle edits; compare after each phase.
- **ACCEPTED:** prove the active Gradle DSL with `:app:tasks --info` before removing/quarantining the duplicate file.
- **ACCEPTED:** compare analyzer output to a baseline and require no new errors rather than zero warnings.
- **ACCEPTED:** use the real application ID and serial-aware ADB commands; mark device smoke test blocked when no device is available.
- **NOTED:** `flutter doctor --android-licenses` is excluded from routine commands because it mutates global state interactively; run only when `doctor` reports missing licenses and the user authorizes it.
- **NOTED:** analyzer captures live under the OS temp directory, not the repository; compare only error diagnostics and allow the known baseline info/warning count.

## Handoff

After validation run `$ck-cook --hard plans/260722-flutter-android-build-compatibility/plan.md`. Production keystore/CI/CD is a separate P3 plan.

## Session Notes
<!-- Updated by cook automatically — do not edit manually -->

**Last active:** 2026-07-22 18:00
**Phase in progress:** phase-03-build-install-verify
**Status:** Project is pinned to Flutter 3.41.9 and `tdesign_flutter` compiles; release build is blocked only because the checkout lacks the required local `.env` asset.

### Decisions made this session
- Gradle `--info` proved `android/app/build.gradle` is active; `android/app/build.gradle.kts` was quarantined as `.disabled`.
- Pinned the project to Flutter 3.41.9 using `.flutter-version` and the local SDK path; kept the existing Flutter 3.44 SDK untouched.
- Regenerated dependency metadata with Dart 3.11.5; `tdesign_flutter` 0.2.7 now compiles under Flutter 3.41.9.
- Did not fabricate `.env` values or secrets; the APK build must be rerun after the user supplies the local runtime configuration.

### Next immediate action
Provide the local `.env` required by `pubspec.yaml`, then rerun `flutter build apk --release` with Flutter 3.41.9.
