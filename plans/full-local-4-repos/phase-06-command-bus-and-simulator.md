# Phase 06: Local command bus and simulator

**Milestone:** M4 - Hardware real/simulator va E2E
**P1 stories:** Hardware profiles; backend local endpoints
**Dependencies:** Phase 03; RabbitMQ healthy; provider contracts from Phase 04
**Outcome:** Kiosk workflow invokes devices through one abstraction and completes a representative workflow in simulator mode without Azure or serial hardware.

## Existing Files And Symbols

- `AutomaticBrewingCoffeeKioskBE/ABC_Kiosk_BE/Services/Background/WorkflowObserverWorker.cs`
  - constructor creates Azure `ServiceClient`
  - `InvokeDeviceMethodAsync` calls in normal/callback paths
- `Services/Background/CleanWorkflowObserverWorker.cs`
- `Services/WorkflowEngine/WorkflowExecutor.cs`
- `Kiosk.ApiService/Extensions/DependencyServices.cs`
  - `AddWorkflowWorker`
  - `AddRabbitMQ`
- `Shared/Shared.MessageStore/QueueConstants.cs`
- `Shared/Shared.MessageStore/InformationMessages.cs`
- `Shared/Shared.RabbitMqPublisher/Publisher.cs`

## Planned Files And Symbols

Create:

- `ABC_Kiosk_BE/Services/Interfaces/IDeviceMethodInvoker.cs`
- `ABC_Kiosk_BE/Services/DeviceCommands/RabbitMqDeviceMethodInvoker.cs`
- `ABC_Kiosk_BE/Services/DeviceCommands/RabbitMqDeviceMethodInvoker.cs`
- `ABC_Kiosk_BE/Services/DeviceCommands/AzureDeviceMethodInvoker.cs`
- `Shared/Shared.MessageStore/DeviceCommandMessages.cs`
- `AutomaticBrewingCoffeeKioskBE/Simulator/DeviceSimulator/DeviceSimulator.csproj`
- `AutomaticBrewingCoffeeKioskBE/Simulator/DeviceSimulator/Program.cs`
- `scripts/local/Test-SimulatorWorkflow.ps1`
- automated tests under `ABC_Kiosk_BE/Kiosk.ApiService.Tests/DeviceCommands/`

## Command Contract

One request envelope:

```text
CommandId
SchemaVersion
CorrelationId
WorkflowId
StepId
DeviceId
Method
Parameters
RequestedAtUtc
TimeoutMs
```

One result envelope:

```text
CommandId
SchemaVersion
CorrelationId
DeviceId
Status
Payload
ErrorCode
ErrorMessage
CompletedAtUtc
```

Requirements:

- JSON shape/version is explicit and deterministic.
- `CommandId` is the idempotency key.
- Routing key contains logical device ID/type, never a COM port.
- Publisher confirm, durable queue and bounded timeout are required.
- Each consumer persists a durable command journal keyed by `CommandId` with states `Received`, `Executing`, `Completed`, `Failed` and `Unknown`.
- A duplicate `Completed`/`Failed` request returns the stored result; it must not execute twice.
- On restart, a journal entry left in `Executing` becomes `Unknown` and cannot be retried until an operator explicitly reconciles it.
- A message is acknowledged only after the result/journal state is durable. Invalid or repeatedly failing messages go to a named dead-letter queue.
- Late/unknown result is logged and discarded without mutating another workflow.

## Implementation Steps

1. Replace direct Azure calls in both workflow observers and `WorkflowExecutor` with `IDeviceMethodInvoker`.
2. Register invoker by mode:
   - `simulator` and `real`: RabbitMQ invoker
   - `azure`: Azure wrapper for legacy compatibility
3. Remove eager `ServiceClient` construction from workers.
4. Add simulator as one native .NET process that consumes the same command queues as real controllers. Use a local SQLite journal so restart and replay behavior are testable before hardware work.
5. Simulator supports the deterministic local workflow fixture used by the E2E
   test and persists command results in SQLite.
6. Publish results through the existing workflow/device update paths so state
   machine and CouchDB state are exercised.
7. Enable workflow workers after invoker health is ready.

## Verification

```powershell
dotnet build .\AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\ABC_Kiosk_BE.sln
dotnet build .\AutomaticBrewingCoffeeKioskBE\Simulator\DeviceSimulator\DeviceSimulator.csproj
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-SimulatorWorkflow.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-SimulatorWorkflow.ps1 -RestartKioskAfterEnqueue -TimeoutSeconds 60
```

Simulator scenarios:

1. Happy path completes the representative workflow under 30 seconds.
2. Duplicate command request executes once and returns one stable result.
3. Timeout marks the device/step failed with a device error.
4. Explicit simulator failure drives existing callback/reset behavior.
5. Kiosk API and simulator restart do not delete workflow state or queues; the restart durability script verifies re-delivery with a fresh RabbitMQ delivery tag.
6. Crash simulation after `Executing` produces `Unknown`; no automatic physical/action replay occurs, and explicit reconciliation is required.
7. Malformed command reaches the dead-letter queue without blocking valid commands.

## Gate

- `HARDWARE_MODE=simulator` starts without `AzureServiceConn` or COM access.
- Representative workflow reaches completed state within 30 seconds.
- Duplicate, timeout and failure tests are deterministic.
- The workflow fixture reaches `Done` and its step is observed as `Done`.

## Rollback

- Select `HARDWARE_MODE=azure` only for legacy compatibility.
- Revert worker injection changes as one phase commit if needed.
- Preserve RabbitMQ/CouchDB state for diagnosis; do not purge queues automatically.

## Risks

- Existing workers invoke Azure in more than one normal/callback path.
- Result correlation can update the wrong step if identifiers are incomplete.
- Simulator behavior may become too detailed; implement only methods needed by accepted workflows.
