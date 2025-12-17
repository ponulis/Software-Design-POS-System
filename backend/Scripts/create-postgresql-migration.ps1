# Script to create PostgreSQL migration
# This script installs dotnet-ef tool if needed and creates the initial PostgreSQL migration

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Migration Creation Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet-ef is available
$efToolAvailable = $false
try {
    $null = dotnet ef --version 2>&1
    $efToolAvailable = $true
    Write-Host "dotnet-ef tool found" -ForegroundColor Green
} catch {
    Write-Host "dotnet-ef tool not found, attempting to install..." -ForegroundColor Yellow
}

if (-not $efToolAvailable) {
    Write-Host "Installing dotnet-ef tool..." -ForegroundColor Yellow
    try {
        dotnet tool install --global dotnet-ef --version 8.0.0
        Write-Host "dotnet-ef tool installed successfully" -ForegroundColor Green
    } catch {
        Write-Host "Failed to install dotnet-ef tool automatically" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install manually using:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install --global dotnet-ef" -ForegroundColor White
        Write-Host ""
        Write-Host "Or visit: https://learn.microsoft.com/en-us/ef/core/cli/dotnet" -ForegroundColor White
        exit 1
    }
}

# Navigate to backend directory
$backendPath = Join-Path $PSScriptRoot ".."
Set-Location $backendPath

Write-Host ""
Write-Host "Creating PostgreSQL migration..." -ForegroundColor Yellow
Write-Host "Migration name: InitialPostgreSQLMigration" -ForegroundColor White
Write-Host ""

# Create migration
try {
    dotnet ef migrations add InitialPostgreSQLMigration
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Migration created successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Review the generated migration files" -ForegroundColor White
        Write-Host "2. Ensure PostgreSQL is running" -ForegroundColor White
        Write-Host "3. Run: dotnet ef database update" -ForegroundColor White
    } else {
        Write-Host "Migration creation failed. Check errors above." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error creating migration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
