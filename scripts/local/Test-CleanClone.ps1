[CmdletBinding()]
param(
    [string]$RepositoryUrl = 'https://github.com/nguyenquocan0101/CUPX_RFT.git',
    [string]$Ref = 'main',
    [switch]$KeepClone
)

$ErrorActionPreference = 'Stop'
$tempRoot = (Resolve-Path ([IO.Path]::GetTempPath())).Path.TrimEnd('\')
$cloneRoot = Join-Path $tempRoot "cupx-clean-clone-$([guid]::NewGuid().ToString('N'))"

try {
    & git clone --depth 1 --branch $Ref $RepositoryUrl $cloneRoot *> $null
    if ($LASTEXITCODE -ne 0) {
        throw 'Clean clone failed.'
    }

    $requiredPaths = @(
        'AutomaticBrewingCoffeeBE',
        'AutomaticBrewingCoffeeKioskBE',
        'AutomaticBrewingCoffeeFE',
        'AutomaticBrewingCoffeeApp',
        'compose.local.yml',
        'docs/local-development.md',
        'scripts/local/Start-All.ps1'
    )
    $missing = @($requiredPaths | Where-Object {
        -not (Test-Path -LiteralPath (Join-Path $cloneRoot $_))
    })
    if ($missing.Count -gt 0) {
        throw "Clean clone is missing required paths: $($missing -join ', ')."
    }

    $tracked = @(git -C $cloneRoot ls-files)
    $forbidden = @($tracked | Where-Object {
        $_ -match '(^|/)\.env$|(^|/)\.env\.(?!example$)[^/]+$|(^|/)(\.local/|node_modules/|build/|bin/|obj/)'
    })
    if ($forbidden.Count -gt 0) {
        throw 'Clean clone contains generated or credential files in Git.'
    }

    $nestedGit = @($requiredPaths | Where-Object {
        (Test-Path -LiteralPath (Join-Path (Join-Path $cloneRoot $_) '.git'))
    })
    if ($nestedGit.Count -gt 0) {
        throw "Clean clone contains nested repositories: $($nestedGit -join ', ')."
    }

    $origin = (& git -C $cloneRoot remote get-url origin).Trim()
    if ($origin -ne $RepositoryUrl) {
        throw 'Clean clone origin does not match the requested repository.'
    }

    Write-Host "Clean clone passed: ref=$Ref projects=4 tracked=$($tracked.Count)."
    if ($KeepClone) {
        Write-Host "Clone retained at $cloneRoot."
    }
}
finally {
    if (-not $KeepClone -and (Test-Path -LiteralPath $cloneRoot)) {
        $resolvedClone = (Resolve-Path -LiteralPath $cloneRoot).Path
        $tempPrefix = "$tempRoot\"
        if (-not $resolvedClone.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Refusing to remove a clone outside the temporary directory.'
        }
        Remove-Item -LiteralPath $resolvedClone -Recurse -Force
    }
}
