# Phase 01 — Baseline & dependency/API recovery

**Stories:** P1 hosted dependency; P1 kiosk preservation (baseline).  
**Goal:** remove the stale local path and address only package blockers reproduced on the installed Flutter/Dart without changing app behavior.

## Tasks

1. Record `flutter --version`, `flutter doctor -v`, `git status --short`, and relevant diff; snapshot every touched tracked file plus relevant untracked paths before edits.
2. Inspect `pubspec.yaml`, lock and plugin metadata. Confirm `wakelock_plus` is hosted and locate `source: path` residue.
3. Keep hosted `wakelock_plus` (expected 1.3.2). Run the baseline build/analyzer first; upgrade `tdesign_flutter` or `google_fonts` only when a captured error names that package. Candidate versions 0.2.7/8.2.0 require solver and call-site checks, not blind pinning.
4. Run `flutter clean`, `flutter pub get`, then `flutter analyze --no-pub`; regenerate metadata and adapt only package API signatures required by the reproduced error.

## Commands

Capture analyzer files under the OS temp directory (not the repo), then run `flutter --version`; `flutter doctor -v`; `git status --short`; `git diff -- pubspec.yaml pubspec.lock android/gradle.properties`; `flutter analyze --no-pub`; `flutter clean`; `flutter pub get` with an immediate exit-code check; `rg -n "wakelock_plus|tdesign_flutter|google_fonts|source: path|path:" pubspec.yaml pubspec.lock .dart_tool/package_config.json`; `flutter analyze --no-pub`. Compare only `error` diagnostics between baseline and post-dependency captures; existing info/warning diagnostics are allowed.

## Acceptance

- `flutter pub get` exits 0 with no missing local `wakelock_plus` path.
- Hosted versions resolve and stale path metadata is gone; any package upgrade is justified by a captured compiler/analyzer error.
- Existing `WakelockPlus.enable/disable` calls and permissions remain unchanged; no new analyzer errors.

## Rollback

Restore only dependency/API edits introduced here from the pre-phase snapshot; regenerate disposable metadata with `flutter clean`/`flutter pub get`. Preserve unrelated dirty edits and untracked files.
