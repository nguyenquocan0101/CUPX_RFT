# Phase 04: API, Authentication, and Safe Startup

**Stories:** P1 Docker-managed stack, P1 public API, P1 persistent state  
**Depends on:** Phase 03

## Objective

Start the Kiosk API locally with correct dependency keys, authenticated health behavior, proxy-safe HTTP handling, and no restart-time state deletion.

## Affected Files and Symbols

- `AutomaticBrewingCoffeeKioskBE\docker-compose.yml` API service
- `ABC_Kiosk_BE\Kiosk.ApiService\Program.cs`
- `ABC_Kiosk_BE\Kiosk.ApiService\Extensions\DependencyServices.cs`
- `ABC_Kiosk_BE\Kiosk.ApiService\Extensions\StartupInitializer.cs`
- `ABC_Kiosk_BE\Kiosk.ApiService\Middleware\ApiKeyAuthenticationMiddleware .cs`
- `ABC_Kiosk_BE\Kiosk.ApiService\Controllers\KioskMachineController.cs`
- `AutomaticBrewingCoffeeKioskBE\scripts\verify-api.ps1` (new)

## Tasks

1. Configure exact keys consumed by code: `ConnectionStrings__Redis`, `CouchDB__Url/Username/Pwd`, `RabbitMQ__HostName/UserName/Password`, `ApiKey`, `AzureServiceConn`, `CloudConfig__BaseUrl`, `CloudConfig__OrdersEndpoint`, `KioskId`, and `ASPNETCORE_URLS=http://+:8080`.
2. Point cloud-backend callbacks to the configurable host address, initially `http://host.docker.internal:30475`.
3. Bind the local diagnostic API port to `127.0.0.1:17554`.
4. Add dependency health ordering and application retry/backoff so broker/database restarts do not require manual API recreation.
5. Keep `StartupCleanup:Enabled` false and regression-test that API restart preserves workflow documents and durable queue state.
6. Resolve TLS termination correctly: remove origin HTTPS redirection for the private HTTP origin or configure forwarded headers before redirection. Avoid redirect loops/warnings.
7. Disable Swagger in Production or protect it explicitly; do not treat the global API key middleware as a reason to publish development tooling.
8. Implement an image-compatible authenticated health check and a PowerShell verification script.

## Verification

```powershell
docker compose up -d --build --wait --wait-timeout 120 api
curl.exe -i -H "X-API-Key: <local-test-key>" http://127.0.0.1:17554/api/v1/ping
curl.exe -i http://127.0.0.1:17554/api/v1/ping
docker compose restart api
docker compose logs --since 5m api
```

Expected:

- Correct key returns HTTP 200; missing/wrong key returns HTTP 401.
- No HTTPS redirect loop and no missing-configuration startup failure.
- After API restart, the pre-created workflow document and persistent RabbitMQ message remain.
- 100 sequential local ping requests have p95 below 500 ms.

## Rollback and Safety

- Keep Cloudflare route disabled during this phase.
- If state deletion occurs, stop API immediately and restore CouchDB/RabbitMQ backups before another start.
- Do not log API keys or Azure connection strings.

## Non-Goals

- Public DNS.
- Physical controller readiness.
- Business workflow changes.
