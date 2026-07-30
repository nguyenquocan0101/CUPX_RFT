[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$paths = @(
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
$patterns = @(
    @{ Name = 'private-key-marker'; Value = 'BEGIN (RSA|OPENSSH|PRIVATE) KEY' },
    @{ Name = 'azure-account-key'; Value = 'AccountKey=[^{$;\r\n][^;\r\n]*' },
    @{ Name = 'azure-shared-access-key'; Value = 'SharedAccessKey=[^{$;\r\n][^;\r\n]*' },
    @{ Name = 'google-api-key'; Value = 'AIza[0-9A-Za-z_-]{20,}' },
    @{ Name = 'aws-access-key'; Value = 'AKIA[0-9A-Z]{16}' }
)
$historicalMaterialPattern = 'const[[:space:]]+string[[:space:]]+[A-Za-z0-9_]*(key|secret|encryption)[A-Za-z0-9_]*[[:space:]]*=[[:space:]]*"([A-Za-z0-9+/=_-]){16,}"'
$mainVarsPath = Join-Path $repoRoot '.local\main-api-vars'
if (-not (Test-Path -LiteralPath $mainVarsPath -PathType Leaf)) {
    throw 'Missing .local/main-api-vars. Initialize the local Main API profile before running the history audit.'
}

$localValues = @{}
foreach ($line in Get-Content -LiteralPath $mainVarsPath) {
    if ($line -match '^\s*([^#][^=]*)=(.*)$') {
        $localValues[$matches[1].Trim()] = $matches[2]
    }
}
$localEncryptionKey = $localValues['CUPX_API_KEY_ENCRYPTION_KEY']
if ([string]::IsNullOrWhiteSpace($localEncryptionKey)) {
    throw 'CUPX_API_KEY_ENCRYPTION_KEY is missing from .local/main-api-vars.'
}

Push-Location $repoRoot
try {
    $commits = @(git rev-list --all)
    if ($LASTEXITCODE -ne 0 -or $commits.Count -eq 0) {
        throw 'Unable to enumerate reachable Git history.'
    }

    $findings = [System.Collections.Generic.List[string]]::new()
    $currentKeyMatchesHistorical = $false
    foreach ($commit in $commits) {
        foreach ($pattern in $patterns) {
            $files = @(git grep -i -l -I -E $pattern.Value $commit -- @paths 2>$null)
            foreach ($file in $files) {
                if (-not [string]::IsNullOrWhiteSpace($file)) {
                    $findings.Add("$($pattern.Name):$file")
                }
            }
        }

        $apiKeyLines = @(git grep -i -n -I -E $historicalMaterialPattern $commit -- 'AutomaticBrewingCoffeeBE/**/ApiKeyUtil.cs' 2>$null)
        foreach ($line in $apiKeyLines) {
            if (-not [string]::IsNullOrWhiteSpace($line)) {
                $findings.Add("hardcoded-api-key-material:$($commit):AutomaticBrewingCoffeeBE/**/ApiKeyUtil.cs")
                $literalMatch = [regex]::Match($line, '=\s*"(?<value>[A-Za-z0-9+/=_-]{16,})"')
                if ($literalMatch.Success -and $literalMatch.Groups['value'].Value -eq $localEncryptionKey) {
                    $currentKeyMatchesHistorical = $true
                }
            }
        }
    }

    $uniqueFindings = @($findings | Sort-Object -Unique)
    $nonRotationFindings = @($uniqueFindings | Where-Object { $_ -notlike 'hardcoded-api-key-material:*' })
    if ($nonRotationFindings.Count -gt 0 -or $currentKeyMatchesHistorical) {
        Write-Host 'Reachable-history findings (values are intentionally not printed):'
        $uniqueFindings | ForEach-Object { Write-Host "- $_" }
        if ($currentKeyMatchesHistorical) {
            throw 'Current local encryption key matches historical material; rotate .local/main-api-vars before continuing.'
        }
        throw 'History audit found material requiring review.'
    }

    $historicalCount = @($uniqueFindings | Where-Object { $_ -like 'hardcoded-api-key-material:*' }).Count
    Write-Host "Reachable-history audit passed: commits=$($commits.Count), historical-material=$historicalCount, current-key-rotated=true."
    $global:LASTEXITCODE = 0
}
finally { Pop-Location }
