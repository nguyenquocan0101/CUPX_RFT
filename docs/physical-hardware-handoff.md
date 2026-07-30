# Physical Hardware Handoff

**Status:** local software and simulator gates pass on commit `95630f3`.
Real hardware has not been accepted yet. This note is the runbook for the
company Windows machine that has the wired controllers and Fairino robot.

## Before Power-On

1. Pull the pushed tree and verify it is clean:

```powershell
git pull origin main
git status --short
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-Prerequisites.ps1
```

2. Generate the machine-local configuration. Never copy `.local` or any
   credential file from another machine:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Initialize-LocalEnvironment.ps1
```

3. Start infrastructure and APIs without starting a simulator:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-All.ps1 -HardwareMode real
```

4. Inventory ports. Only use verified wired controller ports; do not use
   Bluetooth ports:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Get-SerialPortInventory.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-Hardware.ps1 -HardwareMode real
```

## Configure Mappings

Copy the tracked example to the ignored local profile and replace every
`COM0`/placeholder with the exact Device Manager mapping. Set the Fairino IP
to the robot on the isolated machine network.

```powershell
Copy-Item .\config\hardware-profiles.local.example.json .\config\hardware-profiles.local.json
# Edit config\hardware-profiles.local.json locally; do not commit it.
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-HardwareProfile.ps1 `
  -HardwareMode real -ProfilePath .\config\hardware-profiles.local.json
```

The profile check must pass before any controller is started. Confirm baud
rate, parity and stop bits against the device documentation. Keep one process
owner per COM port.

## Start Safely

Build the native controllers once, then start one device at a time. Use the
actual device ID from the local workflow configuration:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-HardwareController.ps1 `
  -Controller Coffee -DeviceId coffee-local -SerialPort COM7

powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-ArmController.ps1 `
  -DeviceId arm-local -RobotIp 192.168.58.2
```

Replace `COM7`, device IDs and the robot IP with the verified company-machine
values. The five native controller names are `Coffee`, `CupDrop`, `IceMaker`,
`Inhale` and `Mix`. ArmController2 requires its previously verified
.NET Framework build.

Start with a status/dry-run command and operator supervision. Do not run a
dispense or robot movement command until the physical area is clear.

## Acceptance Evidence

Record only non-secret values: commit hash, controller name, device ID, COM
mapping, robot IP, command ID and final result.

- Every wired COM mapping passes `Test-HardwareProfile.ps1`.
- A status/dry-run command returns a device result without a cloud error.
- One supervised representative workflow completes through the local RabbitMQ
  command bus and reaches `Completed`.
- Replaying the completed command ID does not repeat the physical action.
- Killing a controller after `Executing` and restarting it produces `Unknown`;
  it does not retry automatically.
- Reconcile the command only after the operator knows the physical outcome:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Reconcile-DeviceCommand.ps1 `
  -CommandId <command-id> -Resolution Completed
```

For an unsuccessful physical outcome, use `-Resolution Failed`. Do not put
connection strings, API keys, passwords or screenshots containing secrets in
the repository.

## Rollback

Stop native controllers, preserve the journal, and return to simulator mode:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\local\Stop-All.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Start-All.ps1 -HardwareMode simulator
powershell -ExecutionPolicy Bypass -File .\scripts\local\Smoke-Test.ps1 -HardwareMode simulator
```

The real-hardware gate is complete only after the supervised workflow and
journal/replay checks above are recorded.
