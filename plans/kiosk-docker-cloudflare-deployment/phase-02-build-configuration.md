# Phase 02: Build and Configuration Repair

**Stories:** P1 Docker-managed stack  
**Depends on:** Phase 01

## Objective

Make the API and three Linux device-controller images reproducibly buildable from the repository without embedding secrets.

## Affected Files and Symbols

- `AutomaticBrewingCoffeeKioskBE\.dockerignore` (new)
- `AutomaticBrewingCoffeeKioskBE\.gitignore`
- `AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Dockerfile`
- `AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService\appsettings.json` (new baseline)
- `AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService\Kiosk.ApiService.csproj`
- `AutomaticBrewingCoffeeKioskBE\CoffeeMachineController\CoffeeMachineController\Dockerfile`
- `AutomaticBrewingCoffeeKioskBE\CupDropMachineController\CupDropMachineController\Dockerfile`
- `AutomaticBrewingCoffeeKioskBE\IceMakerMachine\IceMakerMachine\Dockerfile`
- The three controller `Program.cs` files, especially `AddOriginRabitMq(...)`, `DotEnv.Load(...)`, and coffee `cf.Connect()`

## Tasks

1. Change all four image builds to repository-root context and update Dockerfile project paths accordingly, preserving access to repository-level shared projects/assets.
2. Add a root `.dockerignore` that excludes Git metadata, `bin`, `obj`, logs, local secrets, unrelated controllers, and tests while retaining API, Shared, and the selected controllers.
3. Add a tracked non-secret `appsettings.json` because API startup loads it with `optional: false`; add a precise `.gitignore` exception for this baseline file.
4. Put only safe defaults in baseline settings: logging, production cleanup disabled, plugin folder, and non-secret endpoint structure. All credentials remain environment overrides.
5. Remove duplicate publish copies and mandatory `COPY .env` from controller Dockerfiles.
6. Change each controller to read RabbitMQ host/user/password from environment rather than `localhost/guest/guest`. Use service host `rabbitmq` in Compose.
7. Keep `DEVICE_PRIMARY_CONN_STR`, `SERIAL_PORT`, and `BAUD_RATE` environment-driven; ensure missing required values fail with a clear error.
8. Decide coffee connection behavior explicitly: enable `cf.Connect()` with bounded retry/readiness or document why the library constructor already connects. A running process without a device connection is not ready.
9. Add build-time checks that no `.env` or credential file is copied into any image layer.

## Verification

```powershell
Set-Location W:\CUPX\AutomaticBrewingCoffeeKioskBE
docker build -f ABC_Kiosk_BE\Dockerfile -t abc-kiosk-api:plan-check .
docker build -f CoffeeMachineController\CoffeeMachineController\Dockerfile -t abc-kiosk-coffee:plan-check .
docker build -f CupDropMachineController\CupDropMachineController\Dockerfile -t abc-kiosk-cup:plan-check .
docker build -f IceMakerMachine\IceMakerMachine\Dockerfile -t abc-kiosk-ice:plan-check .
docker history --no-trunc abc-kiosk-api:plan-check
```

Expected:

- All four builds exit 0.
- No build references a missing `.env`, missing solution/project, or `Shared` path.
- Image history and exported filesystem contain no local `.env` or secret value.
- Unit-level configuration tests prove controller RabbitMQ host is not forced to `localhost`.

## Rollback and Safety

- Tag plan-check images separately; do not replace running images.
- Preserve previous Dockerfiles until all four new builds pass.
- Do not connect hardware during image build tests.

## Non-Goals

- Starting the stack.
- Configuring USB passthrough.
- Migrating ArmController.
