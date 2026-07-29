# Plan: Kiosk Backend Docker and Cloudflare Deployment

**Date:** 2026-07-23  
**Status:** Awaiting validation  
**Mode:** Hard  
**Risk:** high-risk — touches Docker infrastructure, persistent data, external Cloudflare/Azure credentials, and physical hardware.

**Spec:** `plans/kiosk-docker-cloudflare-deployment/spec.md`

## Outcome

Deploy the Kiosk backend as a hybrid system:

- Ten Linux services are managed by one `docker compose up -d`: API, coffee controller, cup-drop controller, ice-maker controller, PostgreSQL, Adminer, CouchDB, Redis, RabbitMQ, and Cloudflare Tunnel.
- `ArmController` remains .NET Framework 4.8.1 on Windows and is supervised by WinSW.
- Only `https://kiosk-api.alpa.vn` is public, routed to `http://api:8080` inside the Compose network.
- Persistent services survive container recreation; destructive startup cleanup is disabled by default.

## Minimum Architecture

| Boundary | Components | Exposure |
|---|---|---|
| Compose network | API, 3 serial controllers, PostgreSQL, Adminer, CouchDB, Redis, RabbitMQ, cloudflared | Internal DNS by service name |
| Windows loopback | API diagnostics `17554`, Adminer `18080`, RabbitMQ AMQP for Arm `25672`, optional RabbitMQ UI `25673` | `127.0.0.1` only |
| Windows native | ArmController/WinSW, robot at `192.168.58.2` | No public listener |
| Cloudflare | `kiosk-api.alpa.vn` → `http://api:8080` | API only |

PostgreSQL and Adminer are retained at the user's request, but PostgreSQL is not reconnected to the API because its EF registration is currently disabled and doing so would expand scope.

## Phase Map

| Phase | Name | Stories | Depends on | Gate |
|---|---|---|---|---|
| 01 | Security and prerequisites | P1 public API, P1 Docker stack, P1 Arm | — | Fresh secrets, conflict-free ports, required host tools |
| 02 | Build and configuration repair | P1 Docker stack | 01 | Four application images build without embedded secrets |
| 03 | Infrastructure and persistence | P1 persistence, P2 Adminer | 02 | Four stores/broker retain test data across down/up |
| 04 | API, authentication, and safe startup | P1 Docker stack, P1 public API, P1 persistence | 03 | Local ping 200/401; API restart preserves workflow state |
| 05 | Cloudflare Tunnel | P1 public API | 04 | Healthy connector and public 200/401 for 10 minutes |
| 06 | Serial controllers | P1 Docker stack | 03 | Each physical controller stays ready for 10 minutes |
| 07 | Arm Windows service | P1 Arm | 03 | Service recovers within 10 seconds and reaches robot/broker |
| 08 | Reboot and end-to-end acceptance | All P1/P2 | 05, 06, 07 | Cold-reboot and workflow acceptance suite passes |

## Implementation Order

The order is deliberately incremental. Do not expose the hostname before local authentication and persistence gates pass. Do not bring up all hardware controllers together before each USB mapping has been proven independently.

1. [Phase 01 — Security and prerequisites](phase-01-security-prerequisites.md)
2. [Phase 02 — Build and configuration repair](phase-02-build-configuration.md)
3. [Phase 03 — Infrastructure and persistence](phase-03-infrastructure-persistence.md)
4. [Phase 04 — API and authentication](phase-04-api-auth.md)
5. [Phase 05 — Cloudflare Tunnel](phase-05-cloudflare-tunnel.md)
6. [Phase 06 — Serial controllers](phase-06-serial-controllers.md)
7. [Phase 07 — Arm Windows service](phase-07-arm-windows-service.md)
8. [Phase 08 — Reboot and end-to-end validation](phase-08-reboot-e2e.md)

## Cross-Cutting Decisions

- Use repository-root Docker build contexts so project references outside component folders are available.
- Keep a tracked, non-secret `appsettings.json`; override all environment-specific or secret values through `.env`.
- Keep real `.env`, Cloudflare token files, WinSW credentials, and Azure connection strings out of Git and images.
- Do not use `container_name`; Compose project scoping prevents collision with the backend stack already running on this machine.
- Use named volumes and explicit opt-in cleanup. `docker compose down -v` is excluded from normal operations.
- Prefer `/dev/serial/by-id/...` over `/dev/ttyUSB*`; use required environment substitutions so missing device assignments fail Compose validation.
- Use a non-`guest` RabbitMQ account because the Windows Arm process connects from outside the broker container.
- Treat “container is Up” as insufficient for hardware readiness; verify serial open/query, IoT registration, and RabbitMQ publishing.

## Test Strategy

- Static gates: Compose config, secret scan, Docker image build, port audit.
- Service gates: native health commands for PostgreSQL, CouchDB, Redis, RabbitMQ, plus authenticated API health.
- Security gates: correct API key returns 200, missing/wrong key returns 401, management services are not reachable through the public hostname.
- Durability gates: test data/message survives `docker compose down` and `up -d` without `-v`, plus an API-only restart.
- Hardware gates: test one serial controller at a time for 10 minutes, then unplug/replug and reboot recovery.
- Windows Arm gates: build, service recovery, robot TCP reachability, IoT method, and RabbitMQ update.
- Final gate: cold reboot followed by an actual safe workflow smoke test.

## Global Rollback

1. Disable or remove the Cloudflare published route first.
2. Stop application/controller services while leaving named volumes intact.
3. Restore the previous Compose/Dockerfiles from version control.
4. Restore configuration from the pre-deployment backup; never restore revoked credentials.
5. Re-enable the previous service only after its local health and authentication checks pass.

## Non-Goals

- Migrating ArmController to .NET 8/Linux.
- Wiring PostgreSQL into the API.
- Reintroducing sugar, mixer, or inhale services.
- Publishing Adminer, Swagger, database, broker, robot, or serial endpoints.
- Changing beverage workflows or device protocols beyond configuration, reliability, and readiness fixes required for deployment.

## Planning Risks Requiring Validation

- USBIP attachment may not persist after reboot; the final 120-second recovery target must be measured after a scheduled auto-attach strategy is implemented.
- Docker Desktop startup may depend on Windows login; define whether the recovery SLA begins at boot, login, or Docker Engine readiness.
- The current API deletes CouchDB/RabbitMQ state during startup; persistence cannot be claimed until this is disabled and regression-tested.
- Physical device tests can cause movement or dispensing. Phase 06 and Phase 08 require a safe test mode or operator-controlled test area.
