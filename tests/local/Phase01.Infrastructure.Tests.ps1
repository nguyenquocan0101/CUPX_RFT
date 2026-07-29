$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$composePath = Join-Path $repoRoot 'compose.local.yml'
$startScriptPath = Join-Path $repoRoot 'scripts\local\Start-Infra.ps1'
$stopScriptPath = Join-Path $repoRoot 'scripts\local\Stop-Infra.ps1'

function Read-TextOrEmpty {
    param([string]$Path)

    if (Test-Path -LiteralPath $Path) {
        return Get-Content -Raw -LiteralPath $Path
    }

    return ''
}

Describe 'Phase 01 local infrastructure contract' {
    $requiredFiles = @(
        'compose.local.yml',
        'config\local-environment.example',
        'scripts\local\Initialize-LocalEnvironment.ps1',
        'scripts\local\Test-Prerequisites.ps1',
        'scripts\local\Start-Infra.ps1',
        'scripts\local\Stop-Infra.ps1',
        'scripts\local\Test-InfraPersistence.ps1',
        'docs\local-development.md'
    )

    foreach ($relativePath in $requiredFiles) {
        It "contains $relativePath" {
            Test-Path -LiteralPath (Join-Path $repoRoot $relativePath) | Should Be $true
        }
    }

    $compose = Read-TextOrEmpty $composePath

    foreach ($service in @('redis', 'rabbitmq', 'couchdb', 'minio', 'mailpit')) {
        It "defines the $service service" {
            $compose | Should Match "(?m)^  ${service}:\s*$"
        }
    }

    foreach ($excludedService in @('postgres', 'postgresql', 'sqlserver', 'mssql')) {
        It "does not define a $excludedService service" {
            $compose | Should Not Match "(?m)^  ${excludedService}:\s*$"
        }
    }

    It 'does not use latest image tags' {
        $compose | Should Not Match '(?m)^\s+image:\s+\S+:latest\s*$'
    }

    It 'declares persistent named volumes' {
        foreach ($volume in @('redis-data', 'rabbitmq-data', 'couchdb-data', 'minio-data', 'mailpit-data')) {
            $compose | Should Match "(?m)^  ${volume}:\s*$"
        }
    }

    It 'uses a stable RabbitMQ node name for volume recovery' {
        $compose | Should Match '(?m)^\s+hostname:\s+rabbitmq\s*$'
        $compose | Should Match '(?m)^\s+RABBITMQ_NODENAME:\s+rabbit@rabbitmq\s*$'
    }

    It 'publishes infrastructure ports on loopback only' {
        foreach ($port in @(5672, 15672, 5984, 6379, 8025, 9000, 9001, 1025)) {
            $compose | Should Match "127\.0\.0\.1:${port}:${port}"
        }
    }

    It 'stops infrastructure without deleting volumes' {
        $stopScript = Read-TextOrEmpty $stopScriptPath
        $stopScript | Should Match 'docker compose'
        $stopScript | Should Match '\bdown\b'
        $stopScript | Should Not Match '(?m)(^|\s)(-v|--volumes)(\s|$)'
    }

    It 'rejects unchanged example secrets before startup' {
        $startScript = Read-TextOrEmpty $startScriptPath
        $startScript | Should Match 'change-me'
        $startScript | Should Match '\bthrow\b'
    }
}
