# Local Development

Application processes run natively on Windows. Shared infrastructure runs in
Docker Desktop.

## Phase 01 Infrastructure

Requirements:

- Docker Desktop in Linux-container mode.
- SQL Server listening on `127.0.0.1:1433`.
- Ports `3000`, `5100`, `5160`, `5672`, `15672`, `5984`, `6379`, `8025`,
  `9000`, `9001` and `1025` available before startup.

Generate the ignored local environment file:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Initialize-LocalEnvironment.ps1
```

The generator does not print generated values and refuses to overwrite an
existing configuration. Credential rotation with persisted broker/database data
requires the explicit reset or migration workflow from a later phase. Then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-Prerequisites.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-Infra.ps1
```

Local endpoints:

| Service | Endpoint |
|---|---|
| Redis | `localhost:6379` |
| RabbitMQ | `localhost:5672` |
| RabbitMQ management | `http://localhost:15672` |
| CouchDB | `http://localhost:5984` |
| MinIO API | `http://localhost:9000` |
| MinIO console | `http://localhost:9001` |
| Mailpit SMTP | `localhost:1025` |
| Mailpit UI | `http://localhost:8025` |

Stop containers while preserving all named volumes:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Stop-Infra.ps1
```

Verify all five stateful services survive a stop/start cycle and that native
APIs/simulator are brought back before webhook replay:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-InfraPersistence.ps1
```

Do not use `docker compose down -v` in the normal workflow. Data removal is an
explicit reset operation added in a later phase.

## Phase 02 Main API

Initialize the isolated SQL Server database and local seed:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Initialize-MainDatabase.ps1
```

The initializer:

- Uses only `AutoBrewing_BE_Local` on `127.0.0.1:1433` with Windows
  authentication.
- Requires the database ownership marker
  `CUPX_LOCAL_PROFILE=full-local-4-repos:v1`.
- Pins `dotnet-ef` to `8.0.8`, creates an idempotent migration script and
  verifies all applied migrations.
- Creates a SQL backup before changing an existing owned database.
- Generates ignored credentials in `.local/main-api-vars`.
- Runs the local seed twice and verifies stable row counts.

Start the Main API:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-MainApi.ps1
```

The API listens at `http://localhost:5100`. Local mode keeps SQL Server, Redis,
RabbitMQ, JWT, SignalR and CAP enabled while disabling Firebase, Supabase,
Cloudflare, Azure IoT Hub, VNPay, MPOS, Sentry and background jobs.

Verify health, email/password login, JWT claims and refresh:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-MainApi.ps1
```

The generated local login email and password are stored under
`LocalSeed__AdminEmail` and `LocalSeed__AdminPassword` in the ignored
`.local/main-api-vars` file. Do not commit or paste that file into logs.

To create an explicit backup without changing the schema:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Backup-MainDatabase.ps1
```

## Phase 03 Kiosk API

Start the Kiosk API with CouchDB, Redis and RabbitMQ credentials loaded from
the ignored local environment files. In local simulator mode, workflow workers
are enabled and use the RabbitMQ command bus.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-KioskApi.ps1
```

The Kiosk API listens at `http://localhost:5160`. Verify health, API-key
authentication and restart persistence:

```powershell
$key = (Get-Content .\.local\main-api-vars | Where-Object { $_ -match '^LocalSeed__KioskApiKey=(.*)$' }) -replace '^LocalSeed__KioskApiKey=', ''
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-KioskApi.ps1 -ApiKey $key
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-KioskPersistence.ps1
```

The Kiosk-to-Main outbound key is configured separately as
`MAIN_BACKEND__OutboundApiKey`; the inbound Kiosk key is `ApiKey`.

## Phase 06-07 Device workflow

Run the deterministic simulator workflow after both APIs, RabbitMQ and the
device simulator are running:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-SimulatorWorkflow.ps1
# Optional restart durability check: enqueue, restart Kiosk, then await the same order.
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-SimulatorWorkflow.ps1 -RestartKioskAfterEnqueue -TimeoutSeconds 60
```

The test creates one local workflow fixture, sends a command through RabbitMQ,
and polls CouchDB until the step and workflow are `Done`. The simulator keeps a
durable SQLite journal under `.local/runtime`; completed command IDs replay the
stored result instead of executing again. Its self-test also verifies that an
interrupted `Executing` command becomes `Unknown` after restart and cannot run
until an operator explicitly reconciles it. Use the reconciliation wrapper only
after confirming the physical outcome:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Reconcile-DeviceCommand.ps1 `
  -CommandId <command-id> -Resolution Failed
```

Before starting real controllers, inventory ports and validate an explicit
profile. The example profile intentionally fails real mode until all mappings
are replaced with verified wired controller ports:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Get-SerialPortInventory.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-HardwareProfile.ps1 -HardwareMode real
```

After replacing the profile with verified wired ports, start a native .NET 8
controller through the local RabbitMQ command bus. This launcher reads the
ignored `.local/compose-vars`, sets `HARDWARE_MODE=real`, and writes logs under
`.local/runtime`:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-HardwareController.ps1 `
  -Controller Coffee -DeviceId coffee-local -SerialPort COM7
```

Supported .NET 8 controllers are `Coffee`, `CupDrop`, `IceMaker`, `Inhale`, and
`Mix`. Do not use Bluetooth COM ports for machine controllers. ArmController2
uses its compatible .NET Framework launcher:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-ArmController.ps1 `
  -DeviceId arm-local -RobotIp 192.168.58.2
```

Reconcile an Arm command only after the physical outcome is known:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Reconcile-ArmDeviceCommand.ps1 `
  -CommandId <command-id> -Resolution Failed
```

Run the final local gates after the APIs are healthy:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-LocalPerformance.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-SourceScan.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-LocalBusinessFlow.ps1
```

The performance check warms the Main API and measures 100 sequential `/health`
requests. It fails when p95 reaches 500 ms.

The business-flow check reads the seeded kiosk menu, creates one local `RESO`
order, calls the local sandbox-success callback, verifies the order reaches
`Completed` through the Kiosk workflow, reads the order details through the
authenticated admin API, and replays the callback to verify idempotency. No
external payment provider is called; the Flutter Sandbox button uses this same
local callback.
