# CUPX_RFT

CUPX is configured for a Windows full-local development profile. The four
handed-off projects remain in these top-level directories:

- `AutomaticBrewingCoffeeBE` - Main API
- `AutomaticBrewingCoffeeKioskBE` - Kiosk API, controllers and simulator
- `AutomaticBrewingCoffeeFE` - Next.js admin client
- `AutomaticBrewingCoffeeApp` - Flutter kiosk client

Start with the local runbook: [docs/local-development.md](docs/local-development.md).
The short startup path is:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Initialize-LocalEnvironment.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-Prerequisites.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-All.ps1 -HardwareMode simulator
powershell -ExecutionPolicy Bypass -File .\scripts\local\Smoke-Test.ps1 -HardwareMode simulator
```

Local infrastructure uses PostgreSQL/SQL Server as configured by the host,
Redis, RabbitMQ, CouchDB, Mailpit and MinIO. Payment is sandbox-only; no real
payment, email, webhook or cloud storage service is required. Real robot
acceptance still requires verified wired controller ports and a reachable
robot; simulator mode is the reproducible local fallback.

Run the clean-clone check before handing the repository to another machine:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-CleanClone.ps1
```
