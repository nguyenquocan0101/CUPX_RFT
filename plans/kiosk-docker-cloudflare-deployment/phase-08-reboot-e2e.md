# Phase 08: Reboot and End-to-End Acceptance

**Stories:** All P1 stories and P2 local Adminer  
**Depends on:** Phases 05, 06, and 07

## Objective

Prove the hybrid deployment recovers from a cold reboot, preserves state, remains securely exposed, and can execute a safe end-to-end kiosk workflow.

## Affected Files and External State

- `AutomaticBrewingCoffeeKioskBE\scripts\verify-deployment.ps1` (new)
- `AutomaticBrewingCoffeeKioskBE\DEPLOYMENT.md` (new)
- Docker Desktop startup configuration
- Windows USB auto-attach scheduled task
- WinSW Arm service
- Cloudflare connector and public route

## Tasks

1. Create one verification script that checks Docker Engine readiness, 10 Compose services, named volumes, API 200/401, Cloudflare connector logs, public 200/401, USB mappings/controllers, and Arm service/process.
2. Document initial deploy, normal start/stop, log collection, backup/restore, token rotation, device remapping, rollback, and the prohibition on normal `down -v`.
3. Define the recovery SLA start point explicitly: Windows boot, user login, or Docker Engine ready.
4. Perform a cold reboot with all services configured for automatic recovery.
5. Confirm persistent test data/messages and workflow state remain.
6. Run a safe operator-approved workflow that exercises cup, coffee, ice, and arm in controlled conditions.
7. Record timestamps, logs, p95 measurements, connector health, device identity, and pass/fail evidence in the deployment report.

## Verification

```powershell
W:\CUPX\AutomaticBrewingCoffeeKioskBE\scripts\verify-deployment.ps1
docker compose ps
Get-Service ArmController
```

Expected:

- All 10 Compose services are running/healthy within 120 seconds of the agreed readiness start point.
- Arm service is running within 60 seconds and recovers a killed process within 10 seconds.
- Local and public API authentication returns 200/401 as specified.
- Cloudflare remains healthy with no invalid-token errors for 10 minutes.
- Four persistent services retain their test state.
- Each serial controller stays ready for 10 minutes with the correct stable device mapping.
- Adminer is local-only.
- The safe end-to-end workflow completes with expected state transitions and no unintended actuation.

## Rollback and Safety

1. Disable Cloudflare route.
2. Stop Arm and hardware controllers.
3. Preserve and back up named volumes/logs.
4. Restore previous application images/config.
5. Re-enable components one at a time only after local gates pass.

## Non-Goals

- Load testing physical equipment.
- Destructive failure injection into databases.
- Production approval without an operator-reviewed hardware smoke test.
