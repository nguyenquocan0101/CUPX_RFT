# Local Troubleshooting

## Start order

Run `Test-Prerequisites.ps1`, then `Start-All.ps1 -HardwareMode simulator`.
Native process logs and PID files are under `.local/runtime` and are ignored by
Git. `Stop-All.ps1` only stops PIDs recorded there and preserves Docker data.

## Port already in use

Use `Get-NetTCPConnection -State Listen` to identify the owning process. Stop
only the process that owns the CUPX port, or choose the documented local ports
after stopping the conflicting application. Do not kill an unrelated process by
port number alone.

## SQL Server

The local profile uses Windows authentication against `127.0.0.1,1433` and the
owned database `AutoBrewing_BE_Local`. Run `Initialize-MainDatabase.ps1` and
check that SQL Server accepts local connections. The initializer refuses an
unexpected database or missing ownership marker.

## MinIO and Mailpit

MinIO data is in the Docker named volume `cupx-local-minio`; its console is at
`http://localhost:9001`. Mailpit is at `http://localhost:8025`. Restarting
containers without `down -v` preserves both stores.

## Simulator versus real hardware

The simulator is the default and uses RabbitMQ plus a SQLite journal under
`.local/runtime`. Real mode requires identified non-Bluetooth controller ports;
run `Test-Hardware.ps1 -HardwareMode real` before enabling device workers. The
current machine inventory is not evidence that a machine controller is attached.

## Flutter

The app pins Flutter through `.fvmrc` to 3.41.9. Install FVM and that SDK before
running `fvm flutter analyze` or a device build. Android debug builds allow local
HTTP only in the debug manifest; production builds must use HTTPS.

## Source scan

Run `Test-SourceScan.ps1` before committing. It reports only file and line
locations and never prints matched values. Existing legacy cloud code may remain
for the explicitly deferred production payment/hardware profiles; it must not
be selected by the local runtime.
