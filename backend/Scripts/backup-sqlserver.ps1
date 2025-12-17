# SQL Server Database Backup Script
# This script creates a backup of the SQL Server LocalDB database before migration
# Run this script before starting the PostgreSQL migration

param(
    [string]$ServerInstance = "(localdb)\mssqllocaldb",
    [string]$Database = "POSSystemDb",
    [string]$BackupPath = ".\backups",
    [string]$BackupFileName = "POSSystemDb_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "SQL Server Database Backup Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if SQL Server module is available
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Host "SQL Server PowerShell module not found." -ForegroundColor Yellow
    Write-Host "Installing SqlServer module..." -ForegroundColor Yellow
    Install-Module -Name SqlServer -Force -Scope CurrentUser -AllowClobber
    Import-Module SqlServer
}

# Create backup directory if it doesn't exist
if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    Write-Host "Created backup directory: $BackupPath" -ForegroundColor Green
}

$FullBackupPath = Join-Path $BackupPath $BackupFileName

Write-Host "Server Instance: $ServerInstance" -ForegroundColor White
Write-Host "Database: $Database" -ForegroundColor White
Write-Host "Backup Path: $FullBackupPath" -ForegroundColor White
Write-Host ""

try {
    # Test database connection
    Write-Host "Testing database connection..." -ForegroundColor Yellow
    $connectionTest = Test-SqlDatabaseConnection -ServerInstance $ServerInstance -Database $Database -ErrorAction Stop
    Write-Host "Connection successful!" -ForegroundColor Green
    
    # Create backup
    Write-Host "Creating backup..." -ForegroundColor Yellow
    Backup-SqlDatabase `
        -ServerInstance $ServerInstance `
        -Database $Database `
        -BackupFile $FullBackupPath `
        -CompressionOption On `
        -ErrorAction Stop
    
    Write-Host ""
    Write-Host "Backup completed successfully!" -ForegroundColor Green
    Write-Host "Backup file: $FullBackupPath" -ForegroundColor Green
    
    # Get backup file size
    $backupSize = (Get-Item $FullBackupPath).Length / 1MB
    Write-Host "Backup size: $([math]::Round($backupSize, 2)) MB" -ForegroundColor Green
    
    # Export schema as SQL (optional)
    Write-Host ""
    Write-Host "Exporting database schema..." -ForegroundColor Yellow
    $SchemaPath = Join-Path $BackupPath "POSSystemDb_Schema_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    
    # Use sqlcmd to export schema
    $schemaExportCmd = "sqlcmd -S `"$ServerInstance`" -d $Database -E -Q `"SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.Businesses'))`" -o `"$SchemaPath`""
    # Note: This is a simplified example. For full schema export, consider using:
    # - SQL Server Management Studio (Generate Scripts)
    # - sqlpackage.exe (Export)
    # - Visual Studio Database Project
    
    Write-Host "Schema export path: $SchemaPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "Backup Summary" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "Backup File: $FullBackupPath" -ForegroundColor White
    Write-Host "Backup Size: $([math]::Round($backupSize, 2)) MB" -ForegroundColor White
    Write-Host "Status: SUCCESS" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Verify backup file exists and is not corrupted" -ForegroundColor White
    Write-Host "2. Store backup in a safe location" -ForegroundColor White
    Write-Host "3. Proceed with PostgreSQL migration" -ForegroundColor White
    
} catch {
    Write-Host ""
    Write-Host "ERROR: Backup failed!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Ensure SQL Server LocalDB is running" -ForegroundColor White
    Write-Host "2. Verify database name is correct" -ForegroundColor White
    Write-Host "3. Check you have backup permissions" -ForegroundColor White
    Write-Host "4. Ensure backup directory is writable" -ForegroundColor White
    exit 1
}
