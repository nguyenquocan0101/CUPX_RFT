# Spec: Full-local runtime cho 4 repo Automatic Brewing Coffee

**Date:** 2026-07-29
**Status:** Draft

---

## Problem Statement

Bốn repo được bàn giao hiện chia sẻ nhiều dependency cloud và endpoint public, khiến hệ thống khó khởi động và kiểm thử trên một máy Windows. Cần có một profile full-local có thể chạy backend chính, kiosk backend, frontend, Flutter app, infrastructure local, mock integration và hardware thật/simulator mà không phụ thuộc Firebase, Supabase, Cloudflare, Azure IoT, VNPay, MPOS hoặc SMTP thật.

Gemini là ngoại lệ được cho phép ở server-side nếu một use case AI được xác định; key không được đưa vào client.

---

## User Stories

- **[P1]** As a developer, I want one Docker Compose stack for local infrastructure so that all required data stores and brokers start with predictable service names and persistent volumes.
  Accepted when: Redis, RabbitMQ, CouchDB, MinIO and Mailpit have health checks and reach `healthy`/equivalent status within 120 seconds on a configured Windows machine; the existing host SQL Server passes a separate preflight check. PostgreSQL is not part of the MVP runtime because kiosk EF/Npgsql is disabled.

- **[P1]** As a developer, I want the main backend and kiosk backend to use local endpoints so that core API flows work with no cloud credential.
  Accepted when: API startup succeeds with cloud credentials absent, the kiosk `CloudClient` calls the local main backend, and a smoke test completes login, product/menu read, order create and order status update.

- **[P1]** As a developer, I want mock auth, email, payment and webhook integrations so that I can test business flows without external accounts or callbacks.
  Accepted when: local endpoints can create/login a test user, capture email messages, create/complete/refund a payment, trigger webhook callbacks, and preserve the existing response/event contracts.

- **[P1]** As a kiosk operator, I want to run either real hardware or simulator controllers so that workflows can be tested with or without USB devices.
  Accepted when: `HARDWARE_MODE=real` uses configured serial devices and `HARDWARE_MODE=simulator` completes a representative workflow without `/dev/ttyUSB*` access.

- **[P1]** As a web/mobile developer, I want FE and Flutter endpoint configuration to be local and centralized so that no public domain is called accidentally.
  Accepted when: Next.js uses local API and SignalR URLs, Flutter uses local API and SignalR URLs, and no production/public URL remains in executable refresh or request code.

- **[P2]** As a developer, I want optional Gemini access through a provider interface so that AI features can be enabled without coupling the application to one SDK.
  Accepted when: the key is read only by a backend environment variable, the provider is disabled cleanly when absent, and an integration test verifies the configured model/response mapping when enabled.

- **[P3]** _(out of scope — noted for future)_ Public deployment, Cloudflare Tunnel, real Firebase tenant, real Supabase project, real payment settlement and production email delivery.

---

## Functional Requirements

1. **FR-01:** Add a documented `local` runtime profile and a root/local Compose entry point for infrastructure. Services must use named volumes and service-name DNS, not hardcoded public hosts.
2. **FR-02:** Backend chính must run against local SQL Server and Redis, and CAP must run against local RabbitMQ. Startup must not require Cloudflare Tunnel, Sentry DSN, Azure IoT credentials, VNPay credentials, MPOS credentials, SMTP credentials or Supabase credentials in local mode.
3. **FR-03:** Use the existing SQL email/password login and JWT flow in local mode. Firebase registration must be disabled; if an inactive consumer still requires `IFirebaseAuthService`, inject a disabled implementation that performs no network call. JWT claims must remain compatible with current FE middleware and backend authorization.
4. **FR-04:** Replace direct Supabase Storage calls with an abstraction that supports MinIO S3-compatible storage. Configure bucket name, endpoint, access key, secret key, public base URL and path-style addressing explicitly.
5. **FR-05:** Provide local email capture through Mailpit SMTP. The implementation must persist or expose recipient, subject, HTML body and attachments for inspection without connecting to an external SMTP provider.
6. **FR-06:** Provide local payment adapters for VNPay and MPOS flows. The mock must expose deterministic state transitions and invoke the same callback/SignalR/CAP paths used by business logic.
7. **FR-07:** Provide local webhook endpoints and a replay/trigger mechanism. Callback processing must be idempotent by provider transaction/reference id.
8. **FR-08:** Refactor kiosk `CloudClient` to a named local main-backend client with configurable base URL and API key. Remove cloud-specific naming at the integration boundary while retaining compatible order complete/fail payloads.
9. **FR-09:** Add CouchDB to the kiosk local stack, configure `CouchDB:Url`, `CouchDB:Username` and `CouchDB:Pwd`, and fix the missing required `appsettings.json`/configuration bootstrap before claiming kiosk startup works.
10. **FR-10:** Add hardware profiles. Real mode maps configured serial ports; simulator mode provides the same controller commands/events without physical serial access. The API must not require real controllers for local smoke tests.
11. **FR-11:** Replace hardcoded Flutter refresh-token URL with configuration derived from the local API base URL. Align mobile API key/header and SignalR hub values with the selected local endpoints.
12. **FR-12:** Keep Next.js development proxy, API base URL and SignalR URL consistent with the local backend ports. Disable or localize Sentry initialization in the local profile.
13. **FR-13:** Define a safe local secret contract. Values belong in ignored `.env.local`/`.env` files or user secrets; examples contain placeholders only. Any credential found in workspace history/configuration must be revoked or rotated.
14. **FR-14:** Document startup order, required host prerequisites, health URLs, ports, seed/reset commands, real-vs-simulator mode, and a minimal E2E smoke test across the four repos.
15. **FR-15:** Add tests for provider selection, local auth token claims, mock payment state transitions, webhook idempotency, MinIO upload/public URL behavior, kiosk local client routing and simulator workflow.

### Required Local Configuration Keys

Names are the contract; actual values must remain local-only.

**Shared runtime**

- `ASPNETCORE_ENVIRONMENT`
- `LOCAL_MODE`
- `WEBAPP_DOMAIN`
- `PUBLIC_API_URL` (local callback base, not public tunnel)

**Backend chính**

- `ConnectionStrings__Db`
- `JWT__Key`, `JWT__Issuer`, `JWT__Audience`
- `Redis__ConnectionString`, `Redis__DatabaseCache`
- `RabbitMQ__HostName`, `RabbitMQ__Port`, `RabbitMQ__UserName`, `RabbitMQ__Password`, `RabbitMQ__ExchangeName`
- `AUTH_PROVIDER` / `Firebase__Enabled` and local mock user seed settings
- `STORAGE_PROVIDER`, `MINIO__Endpoint`, `MINIO__AccessKey`, `MINIO__SecretKey`, `MINIO__Bucket`, `MINIO__PublicBaseUrl`, `MINIO__UsePathStyle`
- `EMAIL_PROVIDER`, `LOCAL_EMAIL__Endpoint` or local capture path
- `PAYMENT_PROVIDER`, `MOCK_PAYMENT__BaseUrl`, `MOCK_PAYMENT__AutoComplete`
- `WEBHOOK__BaseUrl`, `WEBHOOK__SigningSecret`
- `SENTRY__Enabled`
- `AZURE_IOTHUB__Enabled`, `MPOS__Enabled`, `VNPAY__Enabled`, `CLOUDFLARE__Enabled`

**Kiosk backend**

- `ConnectionStrings__Db` if relational kiosk persistence is enabled
- `CouchDB__Url`, `CouchDB__Username`, `CouchDB__Pwd`
- `Redis__ConnectionString`
- `RabbitMQ__Host`, `RabbitMQ__Port`, `RabbitMQ__Username`, `RabbitMQ__Password`
- `MAIN_BACKEND__BaseUrl`, `MAIN_BACKEND__OutboundApiKey`, inbound Kiosk `ApiKey`
- `KioskId`, `HARDWARE_MODE`, `SerialPorts__*`, `DDLSourceFolder`
- `AZURE_SERVICE_ENABLED`, `AzureServiceConn` only for non-local integration

**Next.js and Flutter**

- Next.js: `API_PROXY_TARGET`, `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_NOTIFICATION_HUB_URL`, optional `NEXT_PUBLIC_SENTRY_DSN`
- Flutter: `BASE_URL`, `API_KEY_HEADER`, `API_KEY`, `ORDER_SIGNALR_HUB_URL`, `KIOSK_ID`, `CLIENT_ID`, `SIDE`

**Optional AI**

- `AI_PROVIDER=gemini`
- `GEMINI_API_KEY` or `GOOGLE_API_KEY` (backend only)
- `GEMINI_MODEL`

---

## Non-Functional Requirements

- **Performance:** local health endpoint p95 < 500 ms over 100 sequential requests after warm-up; representative order workflow completes within 30 seconds in simulator mode.
- **Security:** no real credential is required for local startup; client bundles contain no Gemini, Firebase service-account, SMTP, payment or cloud-management secret; local secrets are ignored by Git.
- **Reliability:** `docker compose down` followed by `docker compose up -d` without `-v` retains at least one test record in each stateful service; mock callbacks are safe to replay.
- **Compatibility:** existing API response envelopes, JWT role claims, SignalR hub paths/event names, and order/payment status values remain compatible unless explicitly versioned.

---

## Success Criteria

- [ ] A clean machine setup document lets a developer start all required local infrastructure with one Compose command.
- [ ] Backend chính, kiosk backend and Next.js can complete their health checks without cloud credentials.
- [ ] Flutter points only to local API/SignalR endpoints in the local profile; no hardcoded public refresh URL remains.
- [ ] Login, menu/product read, order create, mock payment success, kiosk execution, order completion and SignalR notification pass in simulator mode.
- [ ] The same representative workflow passes with configured real hardware, with hardware-specific failures reported as device errors rather than cloud/network errors.
- [ ] MinIO upload/download and local email capture are verifiable from local admin/test endpoints.
- [ ] No credential value is committed in new or modified configuration files, and previously exposed credentials are rotated.

---

## Out of Scope

- Cloudflare Tunnel and public hostname deployment.
- Real Firebase/OAuth tenant, Supabase project, SMTP provider, VNPay/MPOS settlement and Azure IoT Hub.
- Production-grade payment security/compliance or sending real money.
- Rewriting the four repos into one monorepo.
- Adding Gemini before an actual AI use case is identified.

---

## Assumptions

- Windows host has Docker Desktop, .NET SDK, Node.js/npm and Flutter SDK installed.
- Real kiosk hardware can be accessed by the host or by a supported controller runtime; simulator mode is mandatory for CI/local smoke tests without hardware.
- Existing backend SQL schema/script and kiosk CouchDB document contracts remain the starting data model.
- Gemini network access is allowed only as an explicit optional exception; all non-AI runtime dependencies remain local.

---

## Verified Host Baseline

- Docker Desktop/Compose are installed; the Linux engine must be started before Compose commands.
- SQL Server is reachable on `127.0.0.1:1433` and remains the main backend database because the code uses EF Core SQL Server.
- PostgreSQL 17 is reachable on `127.0.0.1:5432`; PostgreSQL 18 is reachable on `127.0.0.1:5433`.
- Redis, RabbitMQ, CouchDB and MinIO are not currently listening on their standard local ports.
- `psql.exe` is installed under PostgreSQL 17/18 but is not in the current `PATH`.

## Selected Local Stack

- **Host services:** existing SQL Server for the main backend. Installed PostgreSQL instances are detected but unused in the MVP.
- **Docker services:** Redis, RabbitMQ, CouchDB, MinIO and Mailpit.
- **Application processes:** main .NET API, kiosk .NET API, Next.js and Flutter run with their native dev commands.
- **Mock integrations:** local JWT auth provider, local email capture, mock VNPay/MPOS provider and local webhook replay endpoint.
- **AI:** disabled by default. `GEMINI_API_KEY` is required only after a concrete server-side AI feature is added.

---

## Delivery Repository

- **Remote:** `https://github.com/nguyenquocan0101/CUPX_RFT.git`
- **Layout:** one Git repository containing the four handed-over codebases as top-level directories: `AutomaticBrewingCoffeeApp/`, `AutomaticBrewingCoffeeBE/`, `AutomaticBrewingCoffeeFE/` and `AutomaticBrewingCoffeeKioskBE/`.
- **Before push:** remove or ignore nested `.git` metadata as appropriate, keep local `.env`/credential files out of the commit, run the secret-pattern scan, and record the final commit hash and push result.
- **Acceptance:** cloning `CUPX_RFT` followed by the documented local setup must expose the four repos and the local Compose/dev workflow without relying on the original remote repositories.
