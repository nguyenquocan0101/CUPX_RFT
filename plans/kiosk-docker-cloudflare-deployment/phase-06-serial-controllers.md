# Phase 06: Serial Device Controllers

**Stories:** P1 Docker-managed stack  
**Depends on:** Phase 03; Phase 02 images

## Objective

Attach and validate the coffee, cup-drop, and ice-maker controllers one at a time using stable Linux serial paths.

## Affected Files and External State

- `AutomaticBrewingCoffeeKioskBE\docker-compose.yml` coffee/cup/ice services
- `.env.example` and local `.env` serial/Azure variables
- The three controller `Program.cs` files and Dockerfiles from Phase 02
- `AutomaticBrewingCoffeeKioskBE\deploy\windows-usb\attach-devices.ps1` (new)
- `AutomaticBrewingCoffeeKioskBE\deploy\windows-usb\verify-devices.ps1` (new)
- Windows `usbipd-win`, WSL2, Docker Desktop device visibility

## Tasks

1. Install/configure `usbipd-win`; inventory each adapter by VID/PID and serial number.
2. Bind/attach devices to WSL2 and prove the paths are visible from a disposable Linux container.
3. Prefer `/dev/serial/by-id/...`; if unavailable, create a documented stable udev/USB-port strategy. Do not reuse one host device for multiple services.
4. Add required Compose substitutions for each device path, baud rate, Azure IoT device connection string, and RabbitMQ credentials.
5. Implement a scheduled auto-attach task for reboot/replug, with clear logs and idempotent behavior.
6. Start and validate coffee alone, then cup alone, then ice alone. Require serial open/query, RabbitMQ channel, and IoT handler readiness.
7. Make failures exit nonzero or report unhealthy with actionable logs. Do not accept process/container “Up” as ready.
8. After individual gates pass, start all three and check mapping remains unique.

## Verification

For each device:

```powershell
usbipd list
docker compose up -d <coffee|cup|ice>
docker compose logs --since 10m <coffee|cup|ice>
docker inspect <container> --format '{{json .State}}'
```

Expected:

- The intended stable device path exists inside only its assigned container.
- Controller opens and queries the correct device, registers IoT/RabbitMQ functionality, and remains ready for 10 minutes.
- Unplug/replug restores the same logical mapping.
- A reboot restores attachment and controller readiness without manual path editing.
- All three can run simultaneously without sharing a device node.

## Rollback and Safety

- Test with actuators disabled, empty, or in an operator-approved safe state.
- Stop a controller before detaching its USB device.
- Keep each controller independently stoppable; a failed device must not require database volume removal or stack recreation.

## Non-Goals

- Changing serial protocols.
- Guaranteeing final paths before devices are physically connected.
- ArmController deployment.
