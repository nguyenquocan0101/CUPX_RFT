# Phase 05: Next.js and Flutter local configuration

**Milestone:** M3 - Next.js/Flutter local config
**P1 stories:** Web/mobile centralized local endpoints
**Dependencies:** Phase 04 API/SignalR contracts stable
**Outcome:** Web and mobile clients call only local Main API/SignalR endpoints and build with reproducible toolchains.

## Existing Files And Symbols

Next.js:

- `AutomaticBrewingCoffeeFE/next.config.mjs`
  - API rewrite and unconditional Sentry wrapping
- `AutomaticBrewingCoffeeFE/lib/axios.ts`
- `AutomaticBrewingCoffeeFE/contexts/signalR.tsx`
- `AutomaticBrewingCoffeeFE/components/common/export-button.tsx`
- `AutomaticBrewingCoffeeFE/sentry.server.config.ts`
- `AutomaticBrewingCoffeeFE/sentry.edge.config.ts`
- any direct export/download URL found by the executable URL scan

Flutter:

- `AutomaticBrewingCoffeeApp/pubspec.yaml`
  - Flutter constraint `>=3.41.0 <3.44.0`
- `lib/app/core/network/api_constants.dart`
- `lib/app/core/network/dio_client.dart`
- `lib/app/core/network/api_interceptor.dart`
- `lib/app/core/signalr/signalr_service.dart`
- `lib/main.dart`

## Planned Files

Create:

- `AutomaticBrewingCoffeeApp/.fvmrc`
- `AutomaticBrewingCoffeeApp/android/app/src/debug/AndroidManifest.xml` or equivalent debug-only network security config
- `scripts/local/Test-Clients.ps1`
- focused Flutter unit tests for URL derivation/refresh behavior
- focused FE tests only if the repository already has a test runner; otherwise use build plus scripted URL scan

Update:

- existing frontend sample configuration only when repository policy permits; the
  current legacy sample remains a tracked cleanup item and is not used by local
  startup scripts.

Do not commit `.env` or `.env.local`.

## Implementation Steps

### Next.js

1. Keep the development rewrite target configurable through `API_PROXY_TARGET`, defaulting to `http://localhost:5100`.
2. Centralize browser API usage:
   - same-origin `/api/v1` where proxying is intended
   - `NEXT_PUBLIC_NOTIFICATION_HUB_URL=http://localhost:5100/hubs/notification` for the web notification hub
3. Update `axios.ts`, `signalR.tsx`, export/download call sites and any direct URL to consume one configuration source.
4. Disable Sentry initialization/source-map upload in local mode without requiring a DSN.
5. Avoid browser-side secrets. `NEXT_PUBLIC_*` may contain only public local URLs/identifiers.

### Flutter

1. Pin Flutter `3.41.9` with FVM because host `3.44.6` violates the current constraint.
2. Do not widen the constraint as a shortcut. Evaluate that separately after tests pass.
3. Make refresh-token URL derive from `ApiConstants.baseUrl`; remove the hardcoded public domain in `api_interceptor.dart`.
4. Centralize API key header/value and SignalR URL.
   - Flutter order hub is `http://localhost:5100/hubs/order`.
   - Flutter client API key is distinct from the Kiosk inbound key and the Kiosk→Main outbound key.
5. Account for device routing:
   - Windows desktop/web: `localhost`
   - Android emulator: `10.0.2.2`
   - physical Android device, the validated primary target: `adb reverse tcp:5100 tcp:5100` and, if needed, `tcp:5160`
6. Install Android command-line tools through Android Studio SDK Manager or `sdkmanager`; accept licenses before build.
7. Allow cleartext HTTP only in the Android debug manifest/network-security configuration; release configuration must remain unchanged.
8. Ensure local runtime configuration uses placeholders and local credentials
   remain ignored. The current frontend legacy sample must be cleaned before
   declaring the client gate complete.

## Verification

```powershell
npm --prefix .\AutomaticBrewingCoffeeFE ci
npm --prefix .\AutomaticBrewingCoffeeFE run build

dart pub global activate fvm
Set-Location .\AutomaticBrewingCoffeeApp
fvm install 3.41.9
fvm use 3.41.9
fvm flutter pub get
fvm flutter analyze
fvm flutter test
Set-Location ..\

adb reverse tcp:5100 tcp:5100
adb reverse tcp:5160 tcp:5160
```

Executable URL scan:

```powershell
rg -n "https?://" .\AutomaticBrewingCoffeeFE .\AutomaticBrewingCoffeeApp\lib `
  -g "!node_modules/**" -g "!.next/**" -g "!build/**" -g "!*.md"
```

Every remaining match must be either a documented package/reference URL or an explicitly non-executable asset; no production API, refresh, SignalR or export URL may remain.

`Test-Clients.ps1` runs install/build/analyze/test/URL-scan steps fail-fast, checks every native exit code and fails if a configured filtered test selection discovers zero tests.

## Gate

- Next build succeeds with Sentry disabled and no DSN.
- Flutter analyze/test succeeds under `3.41.9`.
- Refresh request uses the configured local base URL.
- Android device can reach `5100`; no client bundle contains backend/Gemini/cloud secrets.

## Rollback

- Revert centralized configuration changes.
- Remove only the project-local FVM selection if necessary; do not uninstall the user's system Flutter SDK.
- Clear `adb reverse` mappings with explicit ports:

```powershell
adb reverse --remove tcp:5100
adb reverse --remove tcp:5160
```

## Risks

- Next.js API and SignalR have different same-origin/proxy requirements.
- Flutter runtime `.env` is bundled as an asset and therefore cannot contain secrets.
- Android SDK installation may require an interactive license acceptance.
