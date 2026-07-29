[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5100',
    [int]$WarmupRequests = 10,
    [int]$Requests = 100,
    [int]$TargetP95Milliseconds = 500
)

$ErrorActionPreference = 'Stop'

if ($WarmupRequests -lt 0 -or $Requests -lt 1) {
    throw 'WarmupRequests must be zero or greater and Requests must be at least one.'
}

$healthUri = "$BaseUrl/health"
for ($index = 0; $index -lt $WarmupRequests; $index++) {
    $response = Invoke-WebRequest -UseBasicParsing -Uri $healthUri -TimeoutSec 10
    if ($response.StatusCode -ne 200) {
        throw "Health warmup request returned HTTP $($response.StatusCode)."
    }
}

$samples = [System.Collections.Generic.List[double]]::new()
for ($index = 0; $index -lt $Requests; $index++) {
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-WebRequest -UseBasicParsing -Uri $healthUri -TimeoutSec 10
    $timer.Stop()

    if ($response.StatusCode -ne 200) {
        throw "Health request $($index + 1) returned HTTP $($response.StatusCode)."
    }

    $samples.Add($timer.Elapsed.TotalMilliseconds)
}

$ordered = @($samples | Sort-Object)
$p95Index = [Math]::Max(0, [int][Math]::Ceiling($ordered.Count * 0.95) - 1)
$p95 = [Math]::Round($ordered[$p95Index], 2)
$median = [Math]::Round($ordered[[int][Math]::Ceiling($ordered.Count * 0.5) - 1], 2)
$minimum = [Math]::Round($ordered[0], 2)
$maximum = [Math]::Round($ordered[$ordered.Count - 1], 2)

Write-Host ('Local /health performance: n={0}, min={1}ms, median={2}ms, p95={3}ms, max={4}ms' -f `
    $ordered.Count, $minimum, $median, $p95, $maximum)

if ($p95 -ge $TargetP95Milliseconds) {
    throw "Local /health p95 ${p95}ms is over the target ${TargetP95Milliseconds}ms."
}

Write-Host "Local /health p95 target passed (<${TargetP95Milliseconds}ms)."
