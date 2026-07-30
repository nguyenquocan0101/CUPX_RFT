# Plan: Full-local runtime cho 4 repo Automatic Brewing Coffee

**Date:** 2026-07-29
**Status:** In Progress
**Mode:** Hard
**Risk:** high-risk - thay doi auth, database bootstrap, payment, infrastructure, hardware transport va luong E2E
**Spec:** `plans/full-local-4-repos/spec.md`
**Brainstorm:** `plans/reports/260729-full-local-4-repos-brainstorm.md`

## Outcome

Mot may Windows chay duoc:

- SQL Server host tai `127.0.0.1:1433`.
- Redis, RabbitMQ, CouchDB, MinIO va Mailpit bang mot Compose stack.
- Main API native tai `http://localhost:5100`.
- Kiosk API native tai `http://localhost:5160`.
- Next.js native tai `http://localhost:3000`.
- Flutter va cac hardware controller native tren Windows.
- Login SQL/JWT, MinIO storage, Mailpit email, sandbox success-button payment flow, webhook that giua hai backend.
- `HARDWARE_MODE=simulator` cho E2E khong can USB; `HARDWARE_MODE=real` dung COM port that.

Khong co Firebase, Supabase, Azure IoT, VNPay, MPOS, Cloudflare, Sentry hay public hostname nao bat buoc trong local profile.

## Approved Architecture And Spec Delta

Research consensus va chi dao cua user thay the mot phan spec ban dau:

| Concern | Quyết định MVP |
|---|---|
| Main database | Giu SQL Server host; tao database rieng `AutoBrewing_BE_Local` |
| PostgreSQL | Khong dung; kiosk EF/Npgsql dang bi disable |
| Docker | Redis, RabbitMQ, CouchDB, MinIO, Mailpit |
| Auth | Dung email/password + JWT hien co, seed local admin; Firebase disabled |
| Payment | Khong goi payment that trong local; Flutter sandbox co nut thanh cong de test flow |
| Webhook | Goi that Main API ↔ Kiosk API qua localhost |
| Hardware | RabbitMQ command bus; simulator hoac controller native Windows/COM |
| AI | Gemini deferred vi chua co use case |

P1 Compose story trong spec duoc chap nhan theo stack tren, khong gom SQL Server hay PostgreSQL vao Compose. Day la thay doi co chu dich, khong phai thieu sot implementation.

## Milestone Map

| Milestone user da duyet | Phase | P1 stories |
|---|---|---|
| M1. Infrastructure va hai backend startup | 01-03 | Docker infrastructure; backend local endpoints |
| M2. Auth/storage/email/payment/webhook local | 04 | Local integrations; backend local endpoints |
| M3. Next.js/Flutter local config | 05 | Web/mobile centralized local endpoints |
| M4. Hardware real/simulator va E2E | 06-08 | Hardware profiles; full simulator/real workflow |

## Phase Index And Dependencies

1. [Phase 01 - Host preflight and Compose](phase-01-host-and-compose.md)
   Dependency: none.
2. [Phase 02 - Main backend database and startup](phase-02-main-backend-database-startup.md)
   Dependency: Phase 01 healthy; SQL Server Windows authentication verified.
3. [Phase 03 - Kiosk backend startup](phase-03-kiosk-backend-startup.md)
   Dependency: Phase 01 healthy; Main API health from Phase 02.
4. [Phase 04 - Local integration providers](phase-04-local-integration-providers.md)
   Dependency: Phases 02-03.
5. [Phase 05 - Next.js and Flutter](phase-05-web-and-mobile.md)
   Dependency: Phase 04 contracts stable.
6. [Phase 06 - Command bus and simulator](phase-06-command-bus-and-simulator.md)
   Dependency: Phase 03; RabbitMQ healthy.
7. [Phase 07 - Real hardware controllers](phase-07-real-hardware.md)
   Dependency: Phase 06 command contract frozen; COM inventory available.
8. [Phase 08 - E2E, security and runbook](phase-08-e2e-security-and-runbook.md)
   Dependency: Phases 04-07.

No phase may start application changes before its dependency gate passes. Phase 05 may run in parallel with Phase 06 only after Phase 04 is complete.

## Local Port Contract

| Port | Owner |
|---:|---|
| 1433 | Existing SQL Server on Windows host |
| 3000 | Next.js |
| 5100 | Main API |
| 5160 | Kiosk API |
| 5672 / 15672 | RabbitMQ AMQP / management |
| 5984 | CouchDB |
| 6379 | Redis |
| 8025 / 1025 | Mailpit UI / SMTP |
| 9000 / 9001 | MinIO S3 / console |

Do not use the Main API launch profile that binds port `9000`; it conflicts with MinIO.

## Configuration Contract

Tracked examples contain placeholders only. Actual values live in ignored `.env` files, .NET user secrets, or process environment variables.

Main API:

```text
ASPNETCORE_ENVIRONMENT=Local
LOCAL_MODE=true
ConnectionStrings__Db
JWT__Key
JWT__Issuer
JWT__Audience
Redis__ConnectionString
Redis__DatabaseCache
RabbitMQ__HostName
RabbitMQ__Port
RabbitMQ__UserName
RabbitMQ__Password
RabbitMQ__ExchangeName
Firebase__Enabled=false
STORAGE_PROVIDER=minio
MINIO__Endpoint
MINIO__AccessKey
MINIO__SecretKey
MINIO__Bucket
MINIO__PublicBaseUrl
MINIO__UsePathStyle=true
EMAIL_PROVIDER=smtp
SmtpSettings__Host
SmtpSettings__Port
SmtpSettings__UseSsl=false
SmtpSettings__RequiresAuthentication=false
PAYMENT_MODE=sandbox-success-button
WEBHOOK__BaseUrl
WEBHOOK__SigningSecret
BACKGROUND_JOBS_ENABLED=false
SENTRY__Enabled=false
AZURE_IOTHUB__Enabled=false
VNPAY__Enabled=false
MPOS__Enabled=false
CLOUDFLARE__Enabled=false
```

Kiosk API:

```text
ASPNETCORE_ENVIRONMENT=Local
LOCAL_MODE=true
CouchDB__Url
CouchDB__Username
CouchDB__Pwd
ConnectionStrings__Redis
RabbitMQ__HostName
RabbitMQ__Port
RabbitMQ__UserName
RabbitMQ__Password
MAIN_BACKEND__BaseUrl=http://localhost:5100
MAIN_BACKEND__OutboundApiKey
ApiKey
KioskId
HARDWARE_MODE=simulator|real|azure
WORKFLOW_WORKERS_ENABLED
LOCAL_RESET_STATE=false
SerialPorts__*
```

Clients:

```text
API_PROXY_TARGET=http://localhost:5100
NEXT_PUBLIC_API_BASE_URL=/api/v1
NEXT_PUBLIC_NOTIFICATION_HUB_URL=http://localhost:5100/hubs/notification
BASE_URL
API_KEY_HEADER
API_KEY
ORDER_SIGNALR_HUB_URL=http://localhost:5100/hubs/order
KIOSK_ID
CLIENT_ID
SIDE
```

`GEMINI_API_KEY`, `GOOGLE_API_KEY`, `GEMINI_MODEL` and `AI_PROVIDER` are intentionally absent from MVP examples.

`ApiKey` protects inbound Kiosk API calls. `MAIN_BACKEND__OutboundApiKey` is used by Kiosk when calling Main API. Flutter's `API_KEY` is a separate client value. Example files must not reuse one secret for all three roles.

## Active Feature IDs

This plan owns only these five entries in the repository-wide `feature_list.json`:

- `full-local-infrastructure`
- `full-local-backend-endpoints`
- `full-local-integration-providers`
- `full-local-hardware-profiles`
- `full-local-client-endpoints`

The four pre-existing kiosk deployment entries belong to `plans/kiosk-docker-cloudflare-deployment/` and are preserved unchanged in `plans/kiosk-docker-cloudflare-deployment/feature_list.archive.json`. They are not active acceptance criteria or dependencies of this full-local plan.

## Safe Database Policy

1. Preflight connects read-only with Windows authentication and records SQL Server version/database existence.
2. Default target is `AutoBrewing_BE_Local`, never an existing production-like database name.
3. If the target already exists, initialization stops unless migration history and ownership marker identify it as local.
4. Generate/review an idempotent EF migration script before applying schema.
5. `AutomaticBrewingCoffee_script.sql` is data-only and must not run automatically or as part of normal startup.
6. Local seed is minimal and idempotent. It creates only the account, organization, store, kiosk, API key, menu/product/workflow/device and webhook records needed by smoke tests.
7. No startup path drops a SQL database, CouchDB database, RabbitMQ queue or Docker volume.
8. Reset requires an explicit command, exact local resource names and a typed confirmation. `docker compose down -v` is not a normal reset command.
9. Before changing an existing local database, take a backup. Rollback restores the backup or removes only a database created by the failed phase and proven to contain no user data.

## Cross-Phase Gates

- Infrastructure gate: all five Compose services healthy within 120 seconds and retain sentinel state across `down`/`up` without `-v`.
- Backend gate: both APIs start without cloud secrets; Main `/health` and Kiosk `/health` return 200.
- Provider gate: auth claims, object URL, captured email, payment transitions and webhook replay tests pass.
- Client gate: production URL scan is clean in executable FE/Flutter code; builds pass with pinned SDKs.
- Simulator gate: representative workflow completes under 30 seconds and duplicate command/result messages do not duplicate state changes.
- Hardware gate: COM mapping is explicit, startup cannot open the wrong port, and failures are reported as device failures.
- Release gate: clean-clone runbook passes, secret scan passes, persistence test passes, and final commit is pushed only after verification.

## Rollback Strategy

Each phase is committed independently. Roll back application changes by reverting only that phase commit. Preserve named Docker volumes and SQL backups unless the user explicitly requests data removal. Provider selection remains configuration-driven so legacy cloud providers can stay compiled but disabled. Azure transport remains a non-local compatibility option until the RabbitMQ real-hardware path has passed acceptance.

## Non-Goals / YAGNI

- No PostgreSQL or kiosk EF/Npgsql activation.
- No SQL Server container or database-provider migration.
- No Gemini/AI provider until a concrete feature exists.
- No Cloudflare Tunnel, public DNS or TLS termination.
- No local Sentry, Jaeger, OpenTelemetry or observability stack.
- No real Firebase tenant, Supabase project, SMTP delivery or payment settlement.
- No containerization of Flutter, Main API, Kiosk API or COM controllers.
- No monorepo framework migration and no rewrite of API envelopes, SignalR events or status enums.
- No Windows service installation in MVP; native processes use documented dev commands.

## Validated Decisions

Confirmed by the user on 2026-07-29:

1. Use isolated database `AutoBrewing_BE_Local` with Windows integrated SQL authentication (`-E`) and `TrustServerCertificate=True`.
2. Do not import `AutomaticBrewingCoffee_script.sql`; use the minimal idempotent local seed.
3. Prioritize a physical Android device through `adb reverse`; Android emulator remains the fallback target.
4. Use project-local FVM with Flutter `3.41.9`; do not widen `pubspec.yaml` until the app passes on `3.44.6`.
5. Generate local JWT/API keys during setup and keep them out of Git.
6. Sugar Dispenser is excluded from the representative MVP workflow.
7. Install the missing .NET Framework 4.8.1 Developer/Targeting Pack before ArmController build.
8. Phase 07 starts only after the operator records each required physical device → `COMx` mapping.

## Red-Team Notes

- **Accepted:** the spec's original PostgreSQL/Compose acceptance conflicts with the inspected runtime. The approved MVP delta is recorded above and PostgreSQL is a non-goal.
- **Accepted:** automatic EF migration and the data-only SQL dump are unsafe defaults. Phase 02 changes database initialization to an explicit, isolated and backed-up workflow.
- **Accepted:** switching all hardware at once has a large blast radius. Phase 06 freezes and tests the command contract in simulator mode before Phase 07 touches COM controllers.
- **Accepted:** an acknowledgement lost after a physical action cannot be made safe by blind retry. Real mode forbids automatic retry for unknown outcomes and deduplicates by command ID.
- **Resolved:** ArmController2 keeps the .NET Framework driver and uses a narrow RabbitMQ 6/local JSON journal adapter; no driver retargeting was required.
- **Noted:** Flutter/Android setup is host-mutating and partially interactive. It is isolated to Phase 05 and does not alter the system Flutter installation.
- **Accepted:** local launch must explicitly set environment `Local`; the generic `http` launch profile is not a valid verification path.
- **Accepted:** SQL migration uses a pinned EF tool, a concrete ownership marker, explicit database targeting and fail-fast `sqlcmd`.
- **Accepted:** Hangfire/dashboard/recurring jobs are disabled in local mode until explicitly tested.
- **Accepted:** payment and webhook replay protection is enforced by unique keys plus transactional inbox/claim behavior, including concurrent callbacks and restart replay.
- **Accepted:** hardware consumers keep a durable command journal; an interrupted `Executing` action becomes `Unknown` and requires operator reconciliation.
- **Accepted:** SignalR paths and inbound/outbound API keys are distinct configuration values; Android cleartext is debug-only.
- **Accepted:** ArmController2 builds with Visual Studio MSBuild/.NET Framework 4.8.1 tooling; Sugar Python support is explicit in Phase 07.
- **Accepted:** secret verification covers the working tree and reachable Git history; the hardcoded API-key encryption key must be externalized and any exposed credential rotated.
- **Accepted:** verification scripts fail fast and persistence coverage includes RabbitMQ and Mailpit, not only Redis/CouchDB/MinIO.

## Execution Progress

- [x] Phase 01: Host preflight and Compose
- [x] Phase 02: Main backend database and startup
- [x] Phase 03: Kiosk backend startup
- [x] Phase 04: Local integration providers (adjusted local scope; real payment deferred)
- [x] Phase 05: Next.js and Flutter local configuration (Next build, URL scan, tracked sample cleanup and FVM Flutter analyze/test pass)
- [x] Phase 06: Local command bus and simulator (RabbitMQ invoker wiring, durable simulator journal and representative workflow reaches Done/Observed under 30 seconds)
- [ ] Phase 07: Real hardware controllers (five .NET 8 controllers and ArmController2 use local RabbitMQ ingress; verified wired COM and robot mappings remain)
- [ ] Phase 08: E2E, security and runbook (all local/simulator gates pass; final real-controller gate remains blocked by physical hardware)

## Session Notes
<!-- Updated by cook automatically - do not edit manually -->

**Last active:** 2026-07-29 18:45
**Phase in progress:** phase-08-e2e-security-and-runbook
**Status:** Local smoke, simulator restart workflow, business flow through `Completed`, idempotent payment replay, ArmController build, p95, working-tree source scan, reachable-history rotation check, clean-clone and client gates pass. Real hardware remains the explicit machine-dependent gate.

### Decisions made this session

- Keep the Main API native on Windows and use integrated SQL Server authentication.
- Isolate all schema changes in marker-owned `AutoBrewing_BE_Local`; never auto-run the repository data dump.
- Disable cloud integrations and Hangfire in local composition while retaining SQL Server, Redis, RabbitMQ/CAP, JWT and SignalR.
- Store generated JWT, seed and API-key encryption values only in ignored `.local/main-api-vars`.
- Seed dependency groups separately because the existing audit interceptor queries persisted kiosk/workflow relationships during `SaveChanges`.
- Kiosk local startup uses `StartupResourceProvisioner` to ensure CouchDB databases and durable RabbitMQ topology without destructive deletes.
- Kiosk workflow workers run locally with `WORKFLOW_WORKERS_ENABLED=true`; Main backend calls use `MAIN_BACKEND__OutboundApiKey`.

### Next immediate action

Phase 04 verification completed: local auth, MinIO product image round-trip, Mailpit transport, sandbox payment UI, and durable Main -> Kiosk webhook inbox/outbox replay after Main API restart.
- Phase 05: Next.js production build, executable URL scan, tracked frontend sample localhost cleanup, FVM 3.41.9 Flutter analyze/test pass.
- Phase 06: RabbitMQ device-command publish/consume, startup topology, RabbitMQ invoker wiring, SQLite-journal simulator self-test and representative workflow Done/Observed E2E pass. Restart durability also passes with `Test-SimulatorWorkflow.ps1 -RestartKioskAfterEnqueue`: re-delivered workflow updates its CouchDB delivery tag and completes after Kiosk restart.
- Phase 07: Serial inventory confirms only Bluetooth COM17/COM18; profile preflight blocks real mode until verified wired mappings exist. Five .NET 8 native controller projects and ArmController2 now select local RabbitMQ ingress in `HARDWARE_MODE=real`; ArmController/FRRobot and ArmController2 build with installed .NET Framework 4.8.1 tooling, and Arm JSON journal reconciliation smoke passes.
- Phase 08: Start-All, Stop-All, Smoke-Test, Test-Persistence, Reset-LocalState, Test-SourceScan, Test-LocalPerformance, Test-LocalBusinessFlow and troubleshooting runbook added. Local smoke, persistence, simulator restart durability, menu/order business flow through `Completed`, idempotent sandbox callback replay, 100-request `/health` p95, working-tree source scan, reachable-history rotation check and clean clone verification pass.

## Definition Of Done

The work is complete only when all phase gates pass on this Windows machine, a clean clone can follow the runbook, simulator E2E and one representative real-hardware workflow pass, no new secret is committed, and Git records the verified commit/push result.
