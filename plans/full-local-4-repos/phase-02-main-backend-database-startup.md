# Phase 02: Main backend database and startup

**Milestone:** M1 - Infrastructure va hai backend startup
**P1 stories:** Backend local endpoints; local auth foundation
**Dependencies:** Phase 01 healthy; SQL Server host reachable on `1433`
**Outcome:** Main API boots at `5100` without cloud credentials against an isolated local SQL database.

## Existing Files And Symbols

- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/APIs/Program.cs`
  - unconditional provider registration
  - `app.MapHealthChecks("/health")`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/APIs/Extensions/ServicesDependency.cs`
  - `AddDatabase`
  - `CreateConnectionString`
  - `ApplyMigration`
  - `AddServices`
  - `AddFirebase`, `AddSupabase`, `AddVNPay`, `AddAzureIotHub`, `AddEmail`, `AddRabbitMQCap`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Domains/Migrations/*`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/Implements/AuthService.cs`
  - `Login`, JWT/refresh-token flow
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/APIs/Controllers/AuthController.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/APIs/Properties/launchSettings.json`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/Firebase/Auth/IFirebaseAuthService.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/Utils/ApiKeyUtil.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/SignalR/OrderHub.cs`

## Planned Files

Create:

- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/APIs/appsettings.Local.json`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/APIs/Extensions/LocalRuntimeDependency.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/Local/LocalDevelopmentSeeder.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/Local/LocalSeedOptions.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services/Local/DisabledFirebaseAuthService.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services.Tests/Local/LocalStartupCompositionTests.cs`
- `AutomaticBrewingCoffeeBE/AutomaticBrewingCoffee.Main/Services.Tests/Local/LocalSeedTests.cs`
- `.config/dotnet-tools.json` with `dotnet-ef` `8.0.8`
- `scripts/local/Initialize-MainDatabase.ps1`
- `scripts/local/Backup-MainDatabase.ps1`

Update the existing files/symbols above only where required.

## Implementation Steps

1. Introduce one local composition boundary selected by `LOCAL_MODE=true` or environment `Local`.
2. Keep SQL Server, Redis, CAP/RabbitMQ, JWT, SignalR, health checks and business services enabled.
3. Do not call eager cloud initializers in local mode:
   - `AddFirebase`
   - `AddSupabase`
   - `AddVNPay`
   - Azure `DeviceManager`/`HostSender`
   - MPOS and Cloudflare startup paths
   - Sentry when disabled
   - Hangfire dashboard, `RegisterRecurringJob` and any hosted job with external side effects while `BACKGROUND_JOBS_ENABLED=false`
4. Use the existing SQL/email-password `AuthService.Login` path. Register `DisabledFirebaseAuthService` only for inactive consumers that still require the interface; it returns a local-mode error without network access. Firebase login remains unavailable in local mode.
5. Add idempotent local seed behind `LOCAL_MODE` and an explicit seed flag. Seed only the minimum graph required for login/menu/order/webhook:
   - admin/test account with hashed local password
   - organization, store, kiosk and kiosk API key
   - menu, product, workflow, steps and devices
   - local `HealthCheck` and `ExecuteProduct` webhook records targeting Kiosk API
6. Health check reports SQL Server, Redis and RabbitMQ readiness without leaking connection strings.
7. Add a `local` launch profile that sets `ASPNETCORE_ENVIRONMENT=Local`, `LOCAL_MODE=true`, `BACKGROUND_JOBS_ENABLED=false` and binds `http://localhost:5100`. Do not use the existing `http` profile to verify local composition.
8. Remove the hardcoded API-key encryption material from `ApiKeyUtil`. Read a generated local encryption key from ignored secret configuration. The fresh local database is seeded only with values encrypted by the new key; any non-local database requires a separate reviewed re-encryption/rotation procedure and must not silently fall back to the exposed source key.
9. Remove/redact the plaintext API-key logging in `OrderHub`. Add a focused logging test proving API keys, JWTs and connection strings do not appear in startup/auth/SignalR logs.

## Safe SQL Workflow

Preflight:

```powershell
sqlcmd -S tcp:127.0.0.1,1433 -E -C -Q "SELECT @@SERVERNAME AS ServerName, DB_ID(N'AutoBrewing_BE_Local') AS LocalDbId"
```

Rules:

- The script creates `AutoBrewing_BE_Local` only when absent.
- Restore the repository tool manifest and require `dotnet tool run dotnet-ef --version` to report `8.0.8`.
- Verify Windows identity has `CREATE ANY DATABASE` or stop with an actionable error.
- The ownership marker is an extended property named `CUPX_LOCAL_PROFILE` with value `full-local-4-repos:v1` on `AutoBrewing_BE_Local`.
- If present without the expected EF migrations history and exact ownership marker, stop.
- If present and owned by this profile, create a `.bak` before migration.
- Generate an idempotent migration script before apply:

```powershell
dotnet tool run dotnet-ef migrations script --idempotent `
  --project .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\Domains\Domains.csproj `
  --startup-project .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\APIs\APIs.csproj `
  --output .\artifacts\local-main-schema.sql
```

- Review/apply only to `AutoBrewing_BE_Local`.
- The generated script starts with a `DB_NAME() = N'AutoBrewing_BE_Local'` guard. Apply with `sqlcmd -b -S tcp:127.0.0.1,1433 -E -C -d AutoBrewing_BE_Local -i .\artifacts\local-main-schema.sql`.
- After apply, query `DB_NAME()`, the ownership property and `__EFMigrationsHistory`; any mismatch fails the phase.
- Never execute `AutomaticBrewingCoffee_script.sql` automatically. It is data-only, large and may duplicate seeded rows.
- Automatic runtime migration must be disabled by default in local mode after explicit initialization, or guarded so it can only target the owned local database.

## Verification

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Initialize-MainDatabase.ps1
dotnet build .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\AutomaticBrewingCoffee.sln
dotnet test .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\Services.Tests\Services.Tests.csproj --filter "FullyQualifiedName~Local"
dotnet run --project .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\APIs\APIs.csproj --launch-profile local
Invoke-RestMethod http://localhost:5100/health
```

Negative startup test removes all Firebase/Supabase/Azure/VNPay/MPOS/Sentry settings, leaves background jobs disabled and expects health 200. The test creates the complete host to catch eager constructors and hosted services.

Seed test runs twice and verifies row counts and stable IDs do not change on the second run.

## Gate

- Main API health returns 200 on `5100` with no cloud credential.
- `AutoBrewing_BE_Local` is the only database changed.
- Login with the local seeded account returns JWT claims compatible with current authorization and refresh flow.
- Re-running initialization and seed is idempotent.

## Rollback

- Stop Main API.
- Revert Phase 02 code.
- If database existed before the phase, restore the recorded backup.
- If the phase created the database and it contains only marker/seed data, the reset script may remove it after explicit confirmation.
- Never drop another database or execute wildcard SQL cleanup.

## Risks

- Existing tests may assume production-like seed IDs.
- Eager constructors can still resolve cloud clients indirectly.
- Runtime `ApplyMigration` can mutate the wrong database if connection validation is incomplete.
