# Phase 01: Security and Prerequisites

**Stories:** P1 Docker-managed stack, P1 public API, P1 Windows Arm supervision  
**Depends on:** None

## Objective

Prepare the Windows host and fresh deployment inputs without starting or exposing the new stack.

## Affected Files and External State

- `W:\CUPX\ENV_CONFIG_SUMMARY.md`
- `AutomaticBrewingCoffeeKioskBE\.env` (local, ignored)
- `AutomaticBrewingCoffeeKioskBE\.env.example` (new, tracked)
- `AutomaticBrewingCoffeeKioskBE\deploy\README.md` (new)
- Cloudflare dashboard, Azure IoT Hub credentials, Windows host prerequisites

## Tasks

1. Inventory current containers, Docker networks, bound ports, Docker Desktop Linux/WSL2 mode, .NET 8, .NET Framework 4.8.1 Developer Pack, Visual Studio Build Tools/MSBuild, NuGet, WinSW, and `usbipd-win`.
2. Reserve non-conflicting loopback ports: API `17554`, Adminer `18080`, RabbitMQ AMQP `25672`, optional RabbitMQ UI `25673`.
3. Rotate Azure credentials exposed in `ENV_CONFIG_SUMMARY.md`; redact real private keys/connection strings from documentation.
4. Create fresh random values for Kiosk API key, CouchDB/PostgreSQL/RabbitMQ credentials, and a new Cloudflare tunnel token.
5. Add `.env.example` with names and safe placeholders only. Keep the real `.env` ignored and ACL-restricted.
6. Document Cloudflare zone ownership for `alpa.vn` and the planned hostname `kiosk-api.alpa.vn`.
7. Record a pre-deployment backup and current container/port inventory.

## Verification

```powershell
docker version
docker compose version
dotnet --list-sdks
Get-NetTCPConnection -State Listen |
  Where-Object LocalPort -in 17554,18080,25672,25673
git -C W:\CUPX\AutomaticBrewingCoffeeKioskBE grep -n -E "SharedAccessKey=|BEGIN PRIVATE KEY|TUNNEL_TOKEN=.+"
```

Expected:

- Docker Engine and Compose respond successfully.
- .NET 8 is installed; MSBuild/Framework prerequisites are recorded as pass or an installation task.
- Reserved ports have no unexpected listeners.
- No real new deployment secret is tracked in the Kiosk repository.
- Old exposed Azure and invalid Cloudflare credentials are revoked before Phase 05.

## Rollback and Safety

- Rotate credentials one integration at a time and keep a short overlap only where the provider supports it.
- Do not paste tokens into commands saved in shell history; prefer protected files/environment input.
- Do not stop the existing backend stack during this phase.

## Non-Goals

- Building images.
- Starting containers.
- Connecting physical devices.
- Creating the public Cloudflare route.
