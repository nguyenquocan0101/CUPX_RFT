# Phase 02 — Android toolchain alignment

**Stories:** P1 Android toolchain; P2 single Gradle module source.  
**Goal:** make the smallest evidence-based Android configuration change required by installed Flutter stable.

## Tasks

1. Inspect `android/app/build.gradle`, `android/app/build.gradle.kts`, root Gradle, settings, wrapper, plugin declarations and `android/local.properties`.
2. Determine which module DSL Gradle loads using `android\gradlew.bat -p android :app:tasks --info` and the evaluated project output; do not assume Groovy solely by convention.
3. Compare Java/SDK with AGP 8.6.0, Kotlin 2.1.0, Gradle 8.7 and Flutter guidance; do not blindly bump.
4. Apply only edits required by a reproducible build error. Keep application ID, permissions, assets and Dart unchanged. Remove/quarantine the inactive duplicate only after the task-evaluation proof; stop if both scripts are active or the result is ambiguous.

## Commands

`Get-Content android/app/build.gradle`; `Get-Content android/app/build.gradle.kts`; `Get-Content android/build.gradle`; `Get-Content android/settings.gradle`; `Get-Content android/gradle.properties`; `Get-Content android/gradle/wrapper/gradle-wrapper.properties`; `java -version`; `flutter doctor -v`; `android\gradlew.bat -p android :app:tasks --info`; `flutter build apk --release -v`.

## Acceptance

- One unambiguous active `app` Gradle configuration, proven by task evaluation; the inactive duplicate is removed/quarantined or the phase is explicitly blocked.
- Toolchain edits are justified by output and introduce no unrelated upgrades.
- No source/UI/API or kiosk permission changes beyond necessary package compatibility.

## Rollback

Restore only Gradle/wrapper/settings/gradle.properties files touched here from the pre-phase snapshot. Keep dependency fixes if valid; otherwise return to the last passing combination and record the failing pair.
