Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\scripts\seed-monthly-revenue.ps1 --environment <environment>"
}

function Get-EnvironmentArgument {
    param(
        [string[]] $Arguments
    )

    for ($index = 0; $index -lt $Arguments.Count; $index++) {
        $argument = $Arguments[$index]

        if ($argument -ieq "--environment" -or $argument -ieq "-e") {
            if ($index + 1 -ge $Arguments.Count) {
                throw "Missing value for --environment."
            }

            return $Arguments[$index + 1]
        }

        if ($argument.StartsWith("--environment=", [System.StringComparison]::OrdinalIgnoreCase)) {
            return $argument.Substring("--environment=".Length)
        }

        if ($argument.StartsWith("-e=", [System.StringComparison]::OrdinalIgnoreCase)) {
            return $argument.Substring("-e=".Length)
        }
    }

    return $null
}

function Get-DefaultConnectionString {
    param(
        [object] $Settings
    )

    if ($null -eq $Settings) {
        return $null
    }

    $connectionStrings = $Settings.PSObject.Properties["ConnectionStrings"]
    if ($null -eq $connectionStrings) {
        return $null
    }

    $defaultConnection = $connectionStrings.Value.PSObject.Properties["DefaultConnection"]
    if ($null -eq $defaultConnection) {
        return $null
    }

    return [string] $defaultConnection.Value
}

$environmentName = Get-EnvironmentArgument -Arguments $args
if ([string]::IsNullOrWhiteSpace($environmentName)) {
    Show-Usage
    exit 1
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$backendRoot = Join-Path $repoRoot "backend"
$baseSettingsPath = Join-Path $backendRoot "appsettings.json"
$environmentSettingsPath = Join-Path $backendRoot "appsettings.$environmentName.json"
$seedSqlPath = Join-Path $backendRoot "db\seed\monthly-revenue-sample.sql"

if (-not (Test-Path -LiteralPath $environmentSettingsPath)) {
    throw "App settings file was not found: $environmentSettingsPath"
}

if (-not (Test-Path -LiteralPath $seedSqlPath)) {
    throw "Seed SQL file was not found: $seedSqlPath"
}

$baseSettings = $null
if (Test-Path -LiteralPath $baseSettingsPath) {
    $baseSettings = Get-Content -LiteralPath $baseSettingsPath -Raw | ConvertFrom-Json
}

$environmentSettings = Get-Content -LiteralPath $environmentSettingsPath -Raw | ConvertFrom-Json
$connectionString = Get-DefaultConnectionString -Settings $environmentSettings

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    $connectionString = Get-DefaultConnectionString -Settings $baseSettings
}

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw "ConnectionStrings:DefaultConnection was not found in $environmentSettingsPath or $baseSettingsPath."
}

$sql = Get-Content -LiteralPath $seedSqlPath -Raw

Write-Host "Environment: $environmentName"
Write-Host "App settings: $environmentSettingsPath"
Write-Host "Seed SQL: $seedSqlPath"

$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString

try {
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $command.CommandTimeout = 180
    [void] $command.ExecuteNonQuery()

    Write-Host "Monthly revenue seed applied successfully."
}
finally {
    if ($null -ne $connection) {
        $connection.Dispose()
    }
}