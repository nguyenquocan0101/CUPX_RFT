# Phase 03: Kiosk backend startup

**Status:** Implementation verified and code review APPROVED; awaiting Hard-mode human approval.

**Milestone:** M1 - Infrastructure va hai backend startup
**P1 stories:** Backend local endpoints
**Dependencies:** Phase 01 healthy; Main API health from Phase 02
**Outcome:** Kiosk API boots at `5160`, preserves CouchDB/RabbitMQ state and calls Main API through a configurable local client.

## Existing Files And Symbols

- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService/Program.cs`
  - mandatory `AddJsonFile("appsettings.json", optional: false)`
  - `AddDatabase`, `AddAppServices`, `AddWorkflowWorker`, `AddRabbitMQ`
- `Kiosk.ApiService/Extensions/DependencyServices.cs`
  - `AddDatabase`
  - `AddAppServices`
  - `AddWorkflowWorker`
  - `AddRabbitMQ`
  - `AddRedisCache`
- `Kiosk.ApiService/Extensions/StartupInitializer.cs`
  - `InitializeAsync`
  - `DeleteCouchDbDatabase`
  - `DeleteRabbitMqQueue`
- `Services/ExternalClients/CloudClient.cs`
- `Services/Implements/OrderCacheService.cs`
- `Kiosk.ApiService/Constants/ConstantValue.cs`
- `Kiosk.ApiService/Properties/launchSettings.json`

## Planned Files

Create:

- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService/appsettings.json`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService/appsettings.Local.json`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Services/ExternalClients/MainBackendClient.cs`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Services/ExternalClients/IMainBackendClient.cs`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService.Tests/Kiosk.ApiService.Tests.csproj`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService.Tests/LocalStartupTests.cs`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService.Tests/StartupInitializerTests.cs`
- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Kiosk.ApiService.Tests/MainBackendClientTests.cs`

Remove `CloudClient.cs` only after all consumers compile against `IMainBackendClient`; do not leave two active client registrations.

## Implementation Steps

1. Add non-secret base `appsettings.json` so configuration bootstrap can succeed.
2. Stop rebuilding a second configuration object when `builder.Configuration` can own JSON + environment layering.
3. Map `CouchDB`, `ConnectionStrings:Redis`, `RabbitMQ`, `MAIN_BACKEND`, API key and local mode settings. Keep inbound Kiosk `ApiKey` distinct from `MAIN_BACKEND__OutboundApiKey`, which authenticates Kiosk→Main calls.
4. Replace `CloudConfig:BaseUrl`/`CloudClient` registration with one typed `MainBackendClient`:
   - base URL `http://localhost:5100`
   - `X-API-Key` from `MAIN_BACKEND__OutboundApiKey`
   - preserve order complete/fail request and response envelopes
5. Make `StartupInitializer.InitializeAsync` non-destructive:
   - get-or-create required CouchDB databases
   - declare durable exchanges/queues idempotently
   - never delete workflow database or queues during normal startup
   - destructive reset exists only in the explicit reset script from Phase 08
6. Add `/health` outside API-key middleware or allowlist exactly that path. Keep `/api/v1/ping` authenticated for API-key verification.
7. Until Phase 06 supplies local device invocation, `WORKFLOW_WORKERS_ENABLED=false` prevents Azure `ServiceClient` construction. The API/read/sync/callback paths must still boot.
8. Keep kiosk EF/Npgsql disabled. Do not use `ConnectionStrings__Db`.
9. Add a `local` launch profile that explicitly sets `ASPNETCORE_ENVIRONMENT=Local`, `LOCAL_MODE=true`, `WORKFLOW_WORKERS_ENABLED=false` and binds `http://localhost:5160`.

## Verification

```powershell
dotnet build .\AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\ABC_Kiosk_BE.sln
dotnet test .\AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService.Tests\Kiosk.ApiService.Tests.csproj
dotnet run --project .\AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService\Kiosk.ApiService.csproj --launch-profile local
Invoke-RestMethod http://localhost:5160/health
Invoke-RestMethod http://localhost:5160/api/v1/ping -Headers @{"X-API-Key"=$env:LOCAL_KIOSK_API_KEY}
```

Persistence test:

1. Insert a known CouchDB workflow document and publish a durable RabbitMQ test message.
2. Restart Kiosk API twice.
3. Verify the document/queue were not deleted.

Negative test omits `AzureServiceConn` and all cloud settings and expects Kiosk health 200.

## Gate

- Kiosk API health returns 200 on `5160` without Azure credentials.
- Authenticated ping returns 200; missing/invalid API key returns 401.
- Restart does not delete CouchDB databases or RabbitMQ queues.
- `MainBackendClient` routes complete/fail calls to `localhost:5100`.

## Rollback

- Stop Kiosk API.
- Revert Phase 03 code and restore previous client registration.
- Preserve CouchDB/RabbitMQ state.
- Do not re-enable destructive startup behavior; if a compatibility rollback needs queue cleanup, invoke the explicit reset command manually.

## Risks

- Middleware order can accidentally protect `/health`.
- Worker constructors may create Azure clients before `WORKFLOW_WORKERS_ENABLED` is evaluated.
- Existing queue declarations may conflict if durability/type differs.
