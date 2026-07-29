# Phase 01: Host preflight and Compose

**Milestone:** M1 - Infrastructure va hai backend startup
**P1 stories:** Docker infrastructure
**Dependencies:** none
**Outcome:** Nam local dependencies chay healthy va persistent, khong can cai tung service truc tiep tren Windows.

## Files And Symbols

Create:

- `compose.local.yml`
- `config/local-environment.example`
- `scripts/local/Test-Prerequisites.ps1`
- `scripts/local/Initialize-LocalEnvironment.ps1`
- `scripts/local/Start-Infra.ps1`
- `scripts/local/Stop-Infra.ps1`
- `scripts/local/Test-InfraPersistence.ps1`
- `docs/local-development.md`

Update:

- `.gitignore` only if a generated local secret/data path is not already ignored.

## Implementation Steps

1. `Test-Prerequisites.ps1` checks:
   - Docker Desktop executable exists and `docker info` reports Linux containers.
   - Compose version is available.
   - `sqlcmd`, `dotnet`, Node/npm, Flutter/FVM and Android SDK state are reported without installing or mutating anything.
   - Ports `3000`, `5100`, `5160`, `5672`, `5984`, `6379`, `8025`, `9000`, `9001` are free; `1433` is listening.
2. Add Compose services with pinned image tags:
   - Redis with append-only persistence and `redis-cli ping` health check.
   - RabbitMQ management with durable named volume and `rabbitmq-diagnostics ping`.
   - CouchDB with named volume and authenticated `/_up` health check.
   - MinIO server with data volume and `/minio/health/live`; add one-shot bucket initialization that is idempotent.
   - Mailpit with SMTP `1025`, UI `8025`, HTTP health check and named data volume.
3. Keep all containers on one named bridge network. Applications run on the host and use published `localhost` ports.
4. `config/local-environment.example` contains placeholder usernames/passwords only. `Initialize-LocalEnvironment.ps1` generates ignored `.local/compose-vars`; `Start-Infra.ps1` refuses blank or placeholder values.
5. Do not start Docker Desktop silently. The runbook tells the operator to start it, then waits on `docker info`.
6. Add named volumes. `Stop-Infra.ps1` runs `docker compose down` without `-v`.

## Windows Notes

- Docker Desktop must be in Linux-container mode.
- Main API must use launch profile port `5100`, not port `9000`, because MinIO owns `9000`.
- Container service DNS names are only for container-to-container calls. Native APIs use `localhost`.
- Do not map `/dev/ttyUSB*`; hardware stays native on Windows.

## Verification

```powershell
Set-Location W:\CUPX
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-Prerequisites.ps1
docker compose -f .\compose.local.yml --env-file .\.local\compose-vars config --quiet
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-Infra.ps1
docker compose -f .\compose.local.yml --env-file .\.local\compose-vars ps
Invoke-RestMethod http://localhost:5984/_up
Invoke-WebRequest http://localhost:9000/minio/health/live -UseBasicParsing
Invoke-WebRequest http://localhost:8025/api/v1/info -UseBasicParsing
Test-NetConnection 127.0.0.1 -Port 5672
Test-NetConnection 127.0.0.1 -Port 6379
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-InfraPersistence.ps1
```

Persistence check creates non-secret sentinel state in Redis, CouchDB and MinIO, publishes one persistent message to a durable RabbitMQ queue, sends one Mailpit message, runs `docker compose down`, starts again without `-v`, and verifies every sentinel remains.

All PowerShell verification scripts set `$ErrorActionPreference = 'Stop'`, check `$LASTEXITCODE` after native commands and fail when a filtered test command reports zero discovered tests.

## Gate

- Five services are healthy within 120 seconds.
- No PostgreSQL or SQL Server container exists in resolved Compose config.
- Named volumes and Redis/CouchDB/MinIO/RabbitMQ/Mailpit sentinels survive `down`/`up`.
- No placeholder secret is accepted by startup script.

## Rollback

- Run `powershell -ExecutionPolicy Bypass -File .\scripts\local\Stop-Infra.ps1`; it calls Compose `down` without `-v`.
- Revert Phase 01 files.
- Do not remove named volumes automatically. Volume deletion requires explicit resource names and user confirmation.

## Risks

- Docker Desktop engine may remain off even when CLI is installed.
- Existing applications may own MinIO/RabbitMQ ports.
- Health commands must not embed credentials in process output.
