[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Push-Location $repoRoot
try {
    $patterns = @(
        'BEGIN (RSA|OPENSSH|PRIVATE) KEY',
        'AccountKey=[^{$;\r\n][^;\r\n]*',
        'SharedAccessKey=[^{$;\r\n][^;\r\n]*',
        'AIza[0-9A-Za-z_-]{20,}',
        'AKIA[0-9A-Z]{16}'
    )
    $findings = @()
    foreach ($pattern in $patterns) {
        $findings += @(git grep -n -I -E $pattern -- 'AutomaticBrewingCoffeeBE/**' 'AutomaticBrewingCoffeeKioskBE/**' 'AutomaticBrewingCoffeeFE/app/**' 'AutomaticBrewingCoffeeFE/components/**' 'AutomaticBrewingCoffeeFE/services/**' 'AutomaticBrewingCoffeeFE/lib/**' 'AutomaticBrewingCoffeeApp/lib/**' 'scripts/**' 'config/**' 2>$null)
    }
    if ($findings.Count -gt 0) {
        Write-Host 'Potential tracked private material or production endpoint finding(s):'
        $findings | ForEach-Object { ($_ -split ':', 3)[0..1] -join ':' } | Sort-Object -Unique
        throw 'Source scan failed. Values are intentionally not printed.'
    }
    Write-Host 'Working-tree source scan passed.'
}
finally { Pop-Location }
