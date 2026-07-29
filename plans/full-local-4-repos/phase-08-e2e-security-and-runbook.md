# Phase 08: E2E, security and runbook

**Milestone:** M4 - Hardware real/simulator va E2E
**P1 stories:** All five P1 stories
**Dependencies:** Phases 04-07
**Outcome:** One documented command sequence starts the local system, verifies simulator and real-hardware paths, and produces reproducible evidence without leaking secrets.

## Planned Files

Create:

- `scripts/local/Start-All.ps1`
- `scripts/local/Stop-All.ps1`
- `scripts/local/Smoke-Test.ps1`
- `scripts/local/Reset-LocalState.ps1`
- `scripts/local/Test-SourceScan.ps1`
- `scripts/local/Test-LocalPerformance.ps1`
- `scripts/local/Test-LocalBusinessFlow.ps1`
- `scripts/local/Test-Persistence.ps1`
- `docs/local-development.md` final runbook updates
- `docs/local-troubleshooting.md`
- deterministic smoke-test fixtures under `tests/local-e2e/`

Update:

- root `README.md` if present; otherwise add a short root local-development pointer.
- `.gitignore` only for newly discovered generated secret/runtime paths.

## Startup Order

1. Run read-only prerequisite and port checks.
2. Start Docker Desktop manually if needed.
3. Start Compose and wait for five healthy dependencies.
4. Verify/initialize `AutoBrewing_BE_Local`.
5. Start Main API on `5100`; wait for `/health`.
6. Start Kiosk API on `5160`; wait for `/health` and authenticated ping.
7. Start simulator or real controllers according to `HARDWARE_MODE`.
8. Start Next.js on `3000`.
9. Configure/start Flutter using the target-specific local address.

`Start-All.ps1` records native process IDs in a local ignored runtime directory and refuses to kill unrelated processes already using a required port.

## Deterministic Simulator Smoke Test

1. Assert no cloud credential is configured or required.
2. Login seeded local account and capture JWT.
3. Read menu/product.
4. Create order with stable fixture inputs.
5. Create mock payment, transition to success and replay callback.
6. Verify one payment/order transition and SignalR notification.
7. Verify Main API calls Kiosk execute webhook with local API key.
8. Simulator consumes device commands and completes workflow.
9. Kiosk calls Main complete endpoint.
10. Verify final order/workflow state and MinIO/Mailpit evidence.

All generated IDs are captured from responses; tests do not depend on random hardcoded database IDs. Timestamps are compared using ranges, not exact wall-clock values.

## Reliability And Performance

- Run 100 sequential warmed `/health` requests and calculate p95; target `<500 ms`.
- Representative simulator workflow target `<30 s`.
- Run `Test-Persistence.ps1`:
  - create named sentinels in stateful services
  - stop/start Compose without `-v`
  - restart both APIs
  - verify sentinels and workflow state remain
- Run concurrent payment/webhook callbacks, replay after API restart, replay command messages and reconcile one simulated `Unknown` command.

## Secret And Public URL Checks

`Test-SourceScan.ps1` scans executable tracked source and reports file/line locations without printing matched values. Check:

- private keys and service-account markers
- Azure connection strings/shared access keys
- JWT/payment/storage secrets
- `.env` and credential files accidentally tracked
- public production API/SignalR/refresh/export URLs in executable code
- runtime logs containing API keys, JWTs or connection strings

Use `git ls-files` for the working-tree scan. A separate history audit is still required before release and must identify the hardcoded encryption material currently present in `Services/Utils/ApiKeyUtil.cs` without printing its value.

Any credential ever committed or pushed is rotated even if Git history is later rewritten. History rewriting is a separate explicit operation and is never performed automatically.

## Safe Reset

`Reset-LocalState.ps1`:

- requires the exact profile name and typed confirmation
- refuses non-local SQL database names
- creates a SQL backup before reset when the database exists
- deletes only named local CouchDB databases/queues/buckets/Redis keys
- never runs `docker compose down -v` internally
- prints the exact resources before mutation

Normal `Stop-All.ps1` preserves all data.

## Verification Commands

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-All.ps1 -HardwareMode simulator
powershell -ExecutionPolicy Bypass -File .\scripts\local\Smoke-Test.ps1 -HardwareMode simulator
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-Persistence.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-SourceScan.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-LocalPerformance.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-LocalBusinessFlow.ps1

$env:LOCAL_MODE = 'true'
dotnet test .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\AutomaticBrewingCoffee.sln
$env:LOCAL_MODE = $null
dotnet test .\AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService.Tests\Kiosk.ApiService.Tests.csproj
npm --prefix .\AutomaticBrewingCoffeeFE run build
Push-Location .\AutomaticBrewingCoffeeApp
fvm flutter analyze
fvm flutter test
Pop-Location
```

Clean clone validation uses a new temporary directory, follows only the runbook and confirms all four top-level projects are present. It must not copy the original workspace `.env`, database files or build outputs.

## Gate

- Simulator smoke test passes from clean local state.
- Operator-observed real-hardware representative workflow from Phase 07 is recorded.
- Persistence, p95 and idempotency tests pass.
- Full builds/tests and working-tree public-URL scans pass.
- The current working tree has no secret finding; every reachable-history finding has documented rotation evidence.
- Clean clone follows the runbook without original remotes or cloud credentials.
- Final commit/push occurs only after all evidence is recorded.

Current evidence: `/health` p95 is 10.48 ms over 100 warmed requests; working-tree source scan passes; local business flow verifies menu read, order creation, sandbox-success callback, Preparing -> Completed workflow, order/detail reads and idempotent callback replay; MinIO upload/read is byte-for-byte (`HTTP 202`, 68 bytes, matching SHA-256); durable webhook inbox/outbox replay passes after a real Main API process restart with `Succeeded|Succeeded|1|1|GET`; infrastructure, Kiosk CouchDB/RabbitMQ and webhook persistence pass; Main local tests pass `35/35` and Kiosk tests pass `4/4`; Next.js production build, Flutter tests, Flutter analyze with non-fatal legacy lints, simulator workflow restart, all native controller builds and a depth-one clean clone from `origin/main` pass. Device simulator self-test now covers restart-to-`Unknown`, refusal to retry, explicit operator reconciliation and idempotent result replay. Reachable history still contains the legacy public host in the import commit; this is a public URL finding, not a credential, and current `ApiKeyUtil` reads its encryption key from process configuration with no current source finding. Real hardware remains the only machine-dependent acceptance gate: this host exposes Bluetooth-only `COM17`/`COM18`, and real-mode preflight correctly refuses the simulator profile.

## Rollback

- `Stop-All.ps1` stops only PIDs it started and preserves data.
- Revert Phase 08 scripts/docs independently.
- Restore SQL backup if reset validation changed data.
- Do not delete Docker volumes or local device configuration automatically.

## Risks

- A process supervisor script can stop the wrong PID if ownership metadata is weak.
- Smoke tests can become flaky if they depend on fixed sleeps; poll bounded health/state endpoints instead.
- Secret scanners can leak matched values; output must redact values by design.
