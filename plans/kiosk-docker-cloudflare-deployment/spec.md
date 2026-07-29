# Spec: Kiosk Backend Docker and Cloudflare Deployment

**Date:** 2026-07-23
**Status:** Ready

---

## Problem Statement

The kiosk backend cannot currently be deployed from its checked-in Compose file on the user's Windows machine because its runtime dependencies, Docker build contexts, configuration names, Linux serial-device access, and mixed Windows/Linux controller targets are inconsistent. The deployment must provide a repeatable Docker-managed stack, a separately supervised Windows robot-arm controller, and a secure public API at a stable `alpa.vn` hostname.

---

## User Stories

- **[P1]** As the kiosk operator, I want all Docker-managed backend components to start with `docker compose up -d` so that deployment and restart are repeatable.
  Accepted when: the API, three serial-device controllers, PostgreSQL, Adminer, CouchDB, Redis, RabbitMQ, and Cloudflare Tunnel are running or healthy within 120 seconds after host prerequisites and secrets are configured.

- **[P1]** As an authorized cloud client, I want to call the kiosk API through `https://kiosk-api.alpa.vn` so that the kiosk can be reached without opening inbound router ports.
  Accepted when: `GET /api/v1/ping` returns HTTP 200 with the correct `X-API-Key`, returns HTTP 401 without it, and no database or management service is published through the tunnel.

- **[P1]** As the kiosk operator, I want `ArmController` supervised on Windows so that robot control remains compatible with .NET Framework and starts again after a machine reboot.
  Accepted when: the controller is running under a Windows service wrapper within 60 seconds of boot and can reach robot controller IP `192.168.58.2`.

- **[P1]** As the kiosk operator, I want database and broker state persisted so that a container recreation does not erase operational data unintentionally.
  Accepted when: PostgreSQL, CouchDB, Redis, and RabbitMQ retain a test record/message across `docker compose down` followed by `docker compose up -d` without the `-v` option.

- **[P2]** As a local administrator, I want Adminer available only from the kiosk machine so that I can inspect PostgreSQL without exposing it publicly.
  Accepted when: Adminer is reachable through a loopback-bound port and is unreachable from another LAN or Internet host.

- **[P3]** _(out of scope — mixer, inhale controller, sugar dispenser, and sugar-dispenser protocol)_

---

## Functional Requirements

1. **FR-01:** Provide one production-oriented Compose definition for the Docker-managed portion of the kiosk.
2. **FR-02:** The Compose stack must contain exactly these application/infrastructure roles: Kiosk API, coffee controller, cup-drop controller, ice-maker controller, PostgreSQL, Adminer, CouchDB, Redis, RabbitMQ, and Cloudflare Tunnel.
3. **FR-03:** `ArmController` must not be included in the Linux Compose stack; it must be built for .NET Framework 4.8.1 and supervised on Windows with WinSW, NSSM, or an equivalent service wrapper.
4. **FR-04:** The API Docker build context must include both `ABC_Kiosk_BE` and the repository-level `Shared` project.
5. **FR-05:** The API must have a physical `appsettings.json` baseline because startup currently loads that file as mandatory; secrets must override it through environment variables.
6. **FR-06:** Add CouchDB to Compose and configure the API with `CouchDB__Url`, `CouchDB__Username`, and `CouchDB__Pwd`.
7. **FR-07:** Configure Redis using `ConnectionStrings__Redis`, matching the API's use of `GetConnectionString("Redis")`.
8. **FR-08:** Configure RabbitMQ using `RabbitMQ__HostName`, `RabbitMQ__UserName`, and `RabbitMQ__Password`, matching the properties consumed by the API.
9. **FR-09:** Modify the three Linux device controllers to read RabbitMQ host and credentials from environment variables rather than using `localhost`.
10. **FR-10:** Remove mandatory Dockerfile `COPY .env` operations; inject device and service configuration from Compose without baking secrets into images.
11. **FR-11:** Each serial controller must accept its serial path, baud rate, and Azure IoT device connection string through environment variables.
12. **FR-12:** RabbitMQ must be accessible to `ArmController` through a loopback-bound Windows host port while remaining available to containers by Compose service name.
13. **FR-13:** PostgreSQL, CouchDB, Redis, and RabbitMQ must use named volumes.
14. **FR-14:** Adminer and database management ports must bind to `127.0.0.1`; they must not be exposed by Cloudflare Tunnel.
15. **FR-15:** Configure a remotely managed Cloudflare Tunnel in Compose with `TUNNEL_TOKEN` supplied locally and route `kiosk-api.alpa.vn` to `http://api:8080`.
16. **FR-16:** Bind the API's optional local diagnostic port to loopback only; external traffic must enter through Cloudflare.
17. **FR-17:** Use unique Compose-scoped service/container names and non-conflicting host ports because another backend stack is already running on this machine.
18. **FR-18:** Configure restart policies and dependency health checks so transient database startup ordering does not permanently stop the API or controllers.
19. **FR-19:** Provide documented health checks for the API, all four infrastructure services, the tunnel connector, and the Windows arm service.
20. **FR-20:** The deployment procedure must include rotating exposed Azure IoT credentials and replacing the currently invalid Cloudflare tunnel token.

---

## Non-Functional Requirements

- **Performance:** After warm-up, the local `/api/v1/ping` endpoint must have p95 response time below 500 ms over 100 sequential requests; tunnel p95 must be below 1,500 ms from a normal Vietnam Internet connection.
- **Security:** Only ports explicitly bound to `127.0.0.1` may be published on the Windows host; the API must require an `X-API-Key` of at least 32 random bytes; no secret may be committed to Git or copied into a Docker image.
- **Availability:** Docker services must use `restart: unless-stopped`; the Windows arm service must restart after failure with a delay no greater than 10 seconds; the full system must recover within 120 seconds after a normal host reboot.
- **Data durability:** Persistent services must use named volumes and must survive container recreation without `docker compose down -v`.
- **Observability:** `docker compose logs` must identify startup failures for every Docker service, and the Windows arm controller must write a persistent rolling log.

---

## Success Criteria

- [ ] Compose startup: all 10 Docker services are running or healthy within 120 seconds.
- [ ] Public API: authenticated `GET https://kiosk-api.alpa.vn/api/v1/ping` returns HTTP 200.
- [ ] Authentication: the same request without `X-API-Key` returns HTTP 401.
- [ ] Exposure: Internet scans of the hostname cannot reach Adminer, PostgreSQL, CouchDB, Redis, or RabbitMQ management endpoints.
- [ ] Persistence: one test datum in each persistent service survives a Compose down/up cycle without volume deletion.
- [ ] Arm recovery: the Windows arm service starts within 60 seconds of reboot and restarts within 10 seconds after a forced process failure.
- [ ] Serial readiness: each of the three Linux controllers opens its assigned stable device path and stays running for at least 10 minutes.
- [ ] Tunnel health: Cloudflare shows at least one healthy connector and no `Invalid tunnel secret` errors for 10 consecutive minutes.

---

## Out of Scope

- Migrating `ArmController` from .NET Framework 4.8.1 to .NET 8/Linux.
- Sugar dispenser and `SugarDispenserProtocol`.
- `MixMachineController`.
- `InhaleController`.
- Publishing Adminer, Swagger, databases, brokers, or robot-controller ports to the public Internet.
- Changing the business workflow or beverage-making logic.
- Selecting final COM/USB mappings before the physical devices are connected.

---

## Assumptions

- Docker Desktop remains in Linux-container mode with the WSL2 backend.
- The user accepts a one-time Windows/WSL2 USB passthrough setup; subsequent application startup uses `docker compose up -d`.
- `ArmController` can reach the robot at `192.168.58.2` from the Windows host.
- The Cloudflare zone `alpa.vn` is active in the user's Cloudflare account, and `kiosk-api.alpa.vn` is an acceptable default hostname.
- The existing cloud backend remains reachable to the Kiosk API at a configurable address, currently expected to be `http://host.docker.internal:30475`.
- Required Azure IoT device/service connection strings will be supplied at deployment time.
- Serial device paths will be represented by required deployment variables and documented discovery steps; physical values do not need to be known during planning.
- Serial controllers will default to 115200 baud while allowing a per-device override.
- Production Azure IoT credentials, database passwords, Kiosk API key, and Cloudflare tunnel token are deployment-time secrets, not planning blockers.
