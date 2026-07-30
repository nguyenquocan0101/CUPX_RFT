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
    ':(exclude)scripts/local/Test-GitHistoryAudit.ps1'
)
$patterns = @(
    @{ Name = 'private-key-marker'; Value = 'BEGIN (RSA|OPENSSH|PRIVATE) KEY' },
    @{ Name = 'azure-account-key'; Value = 'AccountKey=[^{$;\r\n][^;\r\n]*' },
    @{ Name = 'azure-shared-access-key'; Value = 'SharedAccessKey=[^{$;\r\n][^;\r\n]*' },
    @{ Name = 'google-api-key'; Value = 'AIza[0-9A-Za-z_-]{20,}' },
    @{ Name = 'aws-access-key'; Value = 'AKIA[0-9A-Z]{16}' }
)

Push-Location $repoRoot
try {
    $commits = @(git rev-list --all)
    if ($LASTEXITCODE -ne 0 -or $commits.Count -eq 0) {
        throw 'Unable to enumerate reachable Git history.'
    }

    $findings = [System.Collections.Generic.List[string]]::new()
    foreach ($commit in $commits) {
        foreach ($pattern in $patterns) {
            $files = @(git grep -i -l -I -E $pattern.Value $commit -- @paths 2>$null)
            foreach ($file in $files) {
                if (-not [string]::IsNullOrWhiteSpace($file)) {
                    $findings.Add("$($pattern.Name):$file")
                }
            }
        }

        $apiKeyFiles = @(git grep -i -l -I -E 'const[[:space:]]+string[[:space:]]+[A-Za-z0-9_]*(key|secret|encryption)[A-Za-z0-9_]*[[:space:]]*=[[:space:]]*"([A-Za-z0-9+/=_-]){16,}"' $commit -- 'AutomaticBrewingCoffeeBE/**/ApiKeyUtil.cs' 2>$null)
        foreach ($file in $apiKeyFiles) {
            if (-not [string]::IsNullOrWhiteSpace($file)) {
                $findings.Add("hardcoded-api-key-material:$file")
            }
        }
    }

    if ($findings.Count -gt 0) {
        Write-Host 'Reachable-history findings (values are intentionally not printed):'
        $findings | Sort-Object -Unique | ForEach-Object { Write-Host "- $_" }
        throw 'History audit found material requiring rotation review.'
    }

    Write-Host "Reachable-history audit passed: commits=$($commits.Count), findings=0."
}
finally { Pop-Location }
