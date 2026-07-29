# Phase 07: Real hardware controllers

**Milestone:** M4 - Hardware real/simulator va E2E
**P1 stories:** Hardware profiles
**Dependencies:** Phase 06 command contract frozen; physical device/COM inventory available
**Outcome:** Native Windows controllers consume the local RabbitMQ command contract and operate configured devices without Azure IoT.

## Existing Controller Entry Points

- `AutomaticBrewingCoffeeKioskBE/CoffeeMachineController/CoffeeMachineController/Program.cs`
- `AutomaticBrewingCoffeeKioskBE/CupDropMachineController/CupDropMachineController/Program.cs`
- `AutomaticBrewingCoffeeKioskBE/IceMakerMachine/IceMakerMachine/Program.cs`
- `AutomaticBrewingCoffeeKioskBE/InhaleController/InhaleController/Program.cs`
- `AutomaticBrewingCoffeeKioskBE/MixMachineController/MixMachineController/Program.cs`
- `AutomaticBrewingCoffeeKioskBE/ArmController/ArmController2/Program.cs`
- Sugar dispenser controller/protocol entry point selected after project inspection
- `AutomaticBrewingCoffeeKioskBE/Shared/SerialDeviceConnector/*`
- `AutomaticBrewingCoffeeKioskBE/SugarDispenserController/SugarDispenser/main/main.py`
- `AutomaticBrewingCoffeeKioskBE/SugarDispenserController/SugarDispenser/requirements.txt`

The five .NET 8 controller entry points and the .NET Framework 4.8.1 ArmController2 now select the ingress by `HARDWARE_MODE`: `real` uses a local RabbitMQ command host and `azure` keeps the existing `DeviceClient` direct methods. Existing serial/device and Fairino robot implementations remain unchanged.

## Planned Shared Files

Create where target-framework compatibility permits:

- `Shared/Shared.RabbitMqPublisher/LocalDeviceCommandHost.cs`
- RabbitMQ consumer/result publisher, command deduplication and durable SQLite command journal
- `ArmController/ArmController2/LocalArmCommandHost.cs` for the .NET Framework compatibility path
- Arm JSON journal with atomic replacement and operator reconciliation
- per-controller local settings examples with placeholders
- `scripts/local/Get-SerialPortInventory.ps1`
- `scripts/local/Start-HardwareController.ps1`
- `scripts/local/Start-ArmController.ps1`
- contract tests that replay the Phase 06 command fixtures

If `ArmController2` cannot reference the shared project due to target framework constraints, add a narrowly scoped native Windows bridge process for Arm only. Do not retarget or rewrite the robot driver as part of MVP.

## Implementation Steps

1. Install the missing .NET Framework 4.8.1 Developer/Targeting Pack through Visual Studio Installer, then verify `Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1` exists. This is an explicit operator-approved host prerequisite, not a silent script mutation.
2. Inventory devices before code execution:
   - capture `Get-CimInstance Win32_SerialPort`
   - map logical device ID/type to exact `COMx`
   - record baud/parity/data/stop bits where applicable
3. Each controller selects ingress by configuration:
   - `real`: RabbitMQ command host + real device driver
   - `azure`: existing Azure handler, retained temporarily
4. Reuse current method names/payloads (`makeDrink`, `dropCup`, `execute`, `run`, `runAll`, `runScript`, clean/status methods) so workflow data does not fork.
5. Validate method and device ID before opening a serial port.
6. Enforce one process owner per COM port. Startup fails clearly if a port is missing or already open.
7. Publish normalized Phase 06 results and device status updates.
8. Persist `Received → Executing → Completed/Failed` around the physical action. On startup, convert incomplete `Executing` rows to `Unknown`, do not ACK/retry them and require an operator reconciliation command that records the decision.
9. Add cancellation, bounded retry and safe shutdown. Do not retry non-idempotent physical actions automatically after an unknown outcome.
10. Exclude Sugar Dispenser from the validated representative MVP workflow and local seed. Its Python/RabbitMQ adaptation is deferred to a separate plan.
11. Start controllers as native processes from PowerShell; no Docker USB mapping and no Windows service installation in MVP.

## Verification

```powershell
Get-CimInstance Win32_SerialPort | Select-Object DeviceID,Name,PNPDeviceID
powershell -ExecutionPolicy Bypass -File .\scripts\local\Get-SerialPortInventory.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-HardwareProfile.ps1 -HardwareMode real -ProfilePath .\config\hardware-profiles.local.example.json
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-HardwareController.ps1 -Controller Coffee -DeviceId coffee-local -SerialPort COM7
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-ArmController.ps1 -DeviceId arm-local -RobotIp 192.168.58.2
dotnet build .\AutomaticBrewingCoffeeKioskBE\CoffeeMachineController\CoffeeMachineController\CoffeeMachineController.csproj
dotnet build .\AutomaticBrewingCoffeeKioskBE\CupDropMachineController\CupDropMachineController\CupDropMachineController.csproj
dotnet build .\AutomaticBrewingCoffeeKioskBE\IceMakerMachine\IceMakerMachine\IceMakerMachine.csproj
dotnet build .\AutomaticBrewingCoffeeKioskBE\InhaleController\InhaleController\InhaleController.csproj
dotnet build .\AutomaticBrewingCoffeeKioskBE\MixMachineController\MixMachineController\MixMachineController.csproj
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
& $msbuild .\AutomaticBrewingCoffeeKioskBE\ArmController\ArmController.sln /t:Restore,Build /p:Configuration=Debug
if ($LASTEXITCODE -ne 0) { throw "ArmController MSBuild failed" }
```

Preflight verifies the .NET Framework 4.8.1 targeting pack, Visual Studio MSBuild, `packages.config` restore and required native robot driver. All five .NET 8 native controller projects plus ArmController/FRRobot and ArmController2 build on this host. The shared .NET 8 ingress host creates one durable queue and SQLite journal per controller; ArmController2 uses a compatible RabbitMQ 6 host and atomic JSON journal because it cannot reference the `net8.0` shared project. Both paths convert interrupted `Executing` rows to `Unknown` and do not automatically retry physical actions. Coffee local smoke created `device-command.local-coffee-smoke` and its SQLite journal without Azure credentials; Arm journal reconciliation smoke changed an `Unknown` record to operator-resolved `Failed` without starting the robot. The current machine has only Bluetooth COM17/COM18, so the example profile intentionally fails real mode. Sugar is excluded from the representative MVP workflow.

Hardware acceptance uses a dry-run/status command first, then one operator-observed representative workflow. It also kills one controller after the journal reaches `Executing` and verifies restart reports `Unknown` without repeating movement. Record command IDs, device IDs, COM mappings and results without recording connection strings/secrets.

For an interrupted command, reconcile only after the physical outcome is known:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Reconcile-DeviceCommand.ps1 `
  -CommandId <command-id> -Resolution Failed
```

The journal records the operator decision in `DeviceCommandReconciliations`; it never retries an `Unknown` physical action automatically.

## Gate

- The five .NET 8 controllers and ArmController2 build with local RabbitMQ ingress paths that do not require Azure credentials. **Not yet met:** no verified wired COM mapping or reachable Fairino robot is present on this host.
- Only configured COM ports are opened.
- Status/dry-run commands pass before any movement/dispense command.
- One representative real-hardware workflow completes; failures surface as device errors, not cloud/network errors.
- Replaying a completed command ID does not repeat a physical action.

## Rollback

- Stop controller processes and return to `HARDWARE_MODE=simulator`.
- Preserve the frozen RabbitMQ contract.
- Azure ingress may be reselected for legacy operation, but local E2E acceptance remains blocked until real mode passes.
- Do not change device firmware or erase controller settings as rollback.

## Risks

- Arm target framework/package compatibility may require the bridge fallback.
- A retry after lost acknowledgement can duplicate a physical action.
- Incorrect COM mapping can control the wrong device; mapping validation is a hard gate.
- Physical safety procedures and operator supervision are required for movement/dispense tests.
