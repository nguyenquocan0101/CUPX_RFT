[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runtime = Join-Path $repoRoot '.local\runtime'
if (-not (Test-Path -LiteralPath $runtime)) { return }

function Stop-ProcessTree {
    param([int]$RootId)
    $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $RootId" -ErrorAction SilentlyContinue)
    foreach ($child in $children) { Stop-ProcessTree -RootId $child.ProcessId }
    if (Get-Process -Id $RootId -ErrorAction SilentlyContinue) {
        Stop-Process -Id $RootId -Force -ErrorAction SilentlyContinue
    }
}

Get-ChildItem -LiteralPath $runtime -Filter '*.pid' -File | ForEach-Object {
    $pidValue = [int](Get-Content -LiteralPath $_.FullName)
    $process = Get-Process -Id $pidValue -ErrorAction SilentlyContinue
    if ($process) {
        Write-Host "Stopping $($_.BaseName) (PID $pidValue)."
        Stop-ProcessTree -RootId $pidValue
    }
    Remove-Item -LiteralPath $_.FullName -Force
}

& (Join-Path $PSScriptRoot 'Stop-Infra.ps1')
