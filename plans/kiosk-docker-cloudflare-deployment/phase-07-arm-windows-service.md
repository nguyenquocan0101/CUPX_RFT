# Phase 07: ArmController Windows Service

**Stories:** P1 Windows Arm supervision  
**Depends on:** Phase 03 RabbitMQ; Phase 01 Windows prerequisites

## Objective

Build and supervise the .NET Framework 4.8.1 ArmController natively on Windows with reliable broker/robot configuration and restart behavior.

## Affected Files and Symbols

- `ArmController\ArmController2\Program.cs`
- `ArmController\ArmController2\Publisher.cs`
- `ArmController\ArmController2\ArmController2.csproj`
- `ArmController\ArmController2\packages.config`
- `ArmController\FRRobot\*`
- `AutomaticBrewingCoffeeKioskBE\deploy\windows-arm\ArmControllerService.xml` (new WinSW config)
- `deploy\windows-arm\build.ps1`, `install.ps1`, `uninstall.ps1`, `verify.ps1` (new)

## Tasks

1. Restore NuGet packages and build `ArmController2.csproj` Release with Visual Studio Build Tools/MSBuild and the .NET Framework 4.8.1 Developer Pack.
2. Configure robot IP through `ROBOT_IP`, defaulting to `192.168.58.2`, rather than hard-coding it in startup.
3. Configure RabbitMQ host `127.0.0.1`, mapped port `25672`, non-guest user/password, recovery, and retry/backoff through protected environment/config.
4. Move Publisher construction after configuration load. Static initialization must not crash before `Main`.
5. Remove the unused/null CouchDB client construction in `MonitorAsync` or supply a valid dependency if it is truly required.
6. Add explicit readiness/error logs for Azure IoT, RabbitMQ, and robot connection.
7. Package with WinSW: automatic delayed start, correct working directory for scripts/assets, rolling stdout/stderr logs, restart after failure in at most 10 seconds.
8. Add idempotent build/install/uninstall/verify scripts and ACL the service configuration/secrets.

## Verification

```powershell
Test-NetConnection 192.168.58.2 -Port 20003
Get-Service ArmController
Get-CimInstance Win32_Process -Filter "Name='ArmController2.exe'"
```

Then force-kill the process under controlled conditions and verify WinSW starts a new PID.

Expected:

- Release build exits 0.
- Service reaches Running and logs successful configuration/retry behavior without revealing secrets.
- Robot TCP port is reachable before motion tests.
- An approved IoT method results in the expected RabbitMQ update.
- Process restarts within 10 seconds after failure and service starts within 60 seconds of the defined boot readiness point.

## Rollback and Safety

- Disable robot motion and remove the Cloudflare/API trigger path during service recovery tests.
- `uninstall.ps1` must stop/remove only the named Arm service and preserve build logs/config backup.
- If robot connectivity is uncertain, keep service stopped rather than retrying motion commands.

## Non-Goals

- Running ArmController in Docker.
- Migrating to .NET 8.
- Modifying robot scripts or motion coordinates.
