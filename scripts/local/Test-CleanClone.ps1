[CmdletBinding()]
param(
    [string]$RepositoryUrl = 'https://github.com/nguyenquocan0101/CUPX_RFT.git',
    [string]$Ref = 'main',
    [switch]$KeepClone
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tempRoot = (Resolve-Path ([IO.Path]::GetTempPath())).Path.TrimEnd('\')
$cloneRoot = Join-Path $tempRoot "cupx-clean-clone-$([guid]::NewGuid().ToString('N'))"

try {
    $workingTree = @(git -C $repoRoot status --porcelain)
    if ($workingTree.Count -gt 0) {
        throw 'Clean-clone verification requires a clean working tree.'
    }
    $localHead = (& git -C $repoRoot rev-parse HEAD).Trim()
    $remoteHead = ((& git ls-remote $RepositoryUrl "refs/heads/$Ref") -split '\s+')[0]
    if ([string]::IsNullOrWhiteSpace($remoteHead) -or $localHead -ne $remoteHead) {
        throw "Local HEAD is not the pushed $Ref ref. Push the verified commit first."
    }

    & git clone --depth 1 --branch $Ref $RepositoryUrl $cloneRoot *> $null
    if ($LASTEXITCODE -ne 0) {
        throw 'Clean clone failed.'
    }

    $requiredDirectories = @(
        'AutomaticBrewingCoffeeBE',
        'AutomaticBrewingCoffeeKioskBE',
        'AutomaticBrewingCoffeeFE',
        'AutomaticBrewingCoffeeApp'
    )
    $requiredFiles = @(
        'compose.local.yml',
        'docs/local-development.md',
        'scripts/local/Start-All.ps1'
    )
    $missingDirectories = @($requiredDirectories | Where-Object {
        -not (Test-Path -LiteralPath (Join-Path $cloneRoot $_) -PathType Container)
    })
    $missingFiles = @($requiredFiles | Where-Object {
        -not (Test-Path -LiteralPath (Join-Path $cloneRoot $_) -PathType Leaf)
    })
    if ($missingDirectories.Count -gt 0 -or $missingFiles.Count -gt 0) {
        $missing = @($missingDirectories + $missingFiles)
        throw "Clean clone is missing required paths: $($missing -join ', ')."
    }

    $tracked = @(git -C $cloneRoot ls-files)
    $forbidden = @($tracked | Where-Object {
        $_ -match '(^|/)\.env$|(^|/)\.env\.(?!example$)[^/]+$|(^|/)(\.local|node_modules|build|bin|obj|coverage|dist|\.next|\.dart_tool)/'
    })
    if ($forbidden.Count -gt 0) {
        throw 'Clean clone contains generated or credential files in Git.'
    }

    $sourcePaths = @(
        'AutomaticBrewingCoffeeBE/**',
        'AutomaticBrewingCoffeeKioskBE/**',
        'AutomaticBrewingCoffeeFE/**',
        'AutomaticBrewingCoffeeApp/**',
        'config/**',
        'scripts/**',
        ':(exclude)config/local-environment.example',
        ':(exclude)scripts/local/Test-SourceScan.ps1',
        ':(exclude)scripts/local/Test-GitHistoryAudit.ps1',
        ':(exclude)scripts/local/Test-CleanClone.ps1'
    )
    $credentialPatterns = @(
        'BEGIN (RSA|OPENSSH|PRIVATE) KEY',
        'AccountKey=[^{$;\r\n][^;\r\n]*',
        'SharedAccessKey=[^{$;\r\n][^;\r\n]*',
        'AIza[0-9A-Za-z_-]{20,}',
        'AKIA[0-9A-Z]{16}'
    )
    $credentialFindings = @()
    foreach ($pattern in $credentialPatterns) {
        $credentialFindings += @(git -C $cloneRoot grep -n -I -E $pattern -- @sourcePaths 2>$null)
    }
    if ($credentialFindings.Count -gt 0) {
        throw 'Clean clone contains tracked credential material.'
    }

    $rootGit = (Join-Path $cloneRoot '.git')
    $nestedGit = @(Get-ChildItem -LiteralPath $cloneRoot -Force -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq '.git' -and $_.FullName -ne $rootGit })
    if ($nestedGit.Count -gt 0) {
        throw 'Clean clone contains nested repositories.'
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
