# Brainstorm: Deploy Kiosk Backend with Docker and Cloudflare Tunnel

**Date:** 2026-07-23

## Ideas Explored

- **Run every component in one Docker Compose stack:** This would best match the original one-command goal, but it is not viable with the current code because `ArmController` targets .NET Framework 4.8.1/Windows containers while the API, infrastructure, and other device controllers use Linux containers.
- **Migrate `ArmController` to .NET 8/Linux:** This would allow one all-Linux Compose stack, but it adds migration and robot SDK compatibility work that the user does not want at this stage.
- **Hybrid Windows + Docker deployment:** Run `ArmController` directly on Windows and run the API, serial-device controllers, data services, and Cloudflare Tunnel in Docker Compose. This is the selected direction.
- **Temporary TryCloudflare tunnel:** Useful for a short test, but rejected as the production direction because the user owns `alpa.vn` and needs a stable public hostname.
- **Named Cloudflare Tunnel in Compose:** Use a remotely managed tunnel and map `kiosk-api.alpa.vn` only to the API service on the internal Compose network.
- **Keep only the databases actively used by the API:** Considered removing PostgreSQL/Adminer because the current API registration for PostgreSQL is commented out. The user explicitly chose to keep both.

## User's Direction

The user wants the Docker-managed portion to start with `docker compose up -d`. `ArmController` will remain a native Windows/.NET Framework process, preferably supervised as a Windows service. The Compose stack will retain:

- Kiosk API
- Coffee machine controller
- Cup-drop machine controller
- Ice-maker controller
- PostgreSQL and Adminer
- CouchDB
- Redis
- RabbitMQ
- Cloudflare Tunnel

The public hostname will default to `kiosk-api.alpa.vn`. Robot arm control remains in scope but outside Docker. Sugar dispenser, mixer, inhale controller, and related protocols are excluded.

## Open Questions

- Exact stable USB/serial device paths for the coffee, cup-drop, and ice-maker controllers are not yet known.
- Production Azure IoT connection strings, API key, database passwords, and Cloudflare tunnel token must be supplied through local secrets at deployment time.
- The assumed hostname is `kiosk-api.alpa.vn`; it can be changed before deployment without altering the architecture.

## Risks

- Docker Desktop on Windows needs one-time WSL2/USB passthrough setup before Linux containers can access physical serial devices.
- The device controllers currently hard-code RabbitMQ as `localhost`; in containers this points to the controller itself and must be changed to a configurable host such as `rabbitmq`.
- The existing Compose and Dockerfiles cannot run unchanged: CouchDB is absent, configuration names do not match the code, API build context excludes `Shared`, and multiple Dockerfiles copy missing `.env` files.
- `StartupInitializer` deletes the workflow CouchDB database and selected RabbitMQ queues on API startup; restarts may intentionally discard pending workflow state.
- A Cloudflare container already running on this machine reports `Unauthorized: Invalid tunnel secret`; a new valid tunnel token is required.
- Sensitive Azure credentials are present in `ENV_CONFIG_SUMMARY.md`; those credentials should be rotated before public deployment.
