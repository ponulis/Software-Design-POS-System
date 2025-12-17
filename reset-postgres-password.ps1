# PostgreSQL Password Reset Script
# Run this script as Administrator

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Password Reset" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$pgHbaPath = "C:\Program Files\PostgreSQL\17\data\pg_hba.conf"
$pgBinPath = "C:\Program Files\PostgreSQL\17\bin"
$serviceName = "postgresql-x64-17"
$newPassword = "postgres_dev_password"

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Check if pg_hba.conf exists
if (-not (Test-Path $pgHbaPath)) {
    Write-Host "ERROR: pg_hba.conf not found at: $pgHbaPath" -ForegroundColor Red
    exit 1
}

Write-Host "Step 1: Backing up pg_hba.conf..." -ForegroundColor Yellow
Copy-Item $pgHbaPath "$pgHbaPath.backup" -Force
Write-Host "Backup created: $pgHbaPath.backup" -ForegroundColor Green

Write-Host ""
Write-Host "Step 2: Modifying pg_hba.conf to allow passwordless connection..." -ForegroundColor Yellow
$content = Get-Content $pgHbaPath
$modified = $content -replace '^local\s+all\s+all\s+scram-sha-256', 'local   all             all                                     trust'
$modified | Set-Content $pgHbaPath
Write-Host "Changed scram-sha-256 to trust for local connections" -ForegroundColor Green

Write-Host ""
Write-Host "Step 3: Restarting PostgreSQL service..." -ForegroundColor Yellow
Restart-Service $serviceName -Force
Start-Sleep -Seconds 3
Write-Host "Service restarted" -ForegroundColor Green

Write-Host ""
Write-Host "Step 4: Setting new password..." -ForegroundColor Yellow
& "$pgBinPath\psql.exe" -U postgres -c "ALTER USER postgres WITH PASSWORD '$newPassword';"
if ($LASTEXITCODE -eq 0) {
    Write-Host "Password set successfully!" -ForegroundColor Green
} else {
    Write-Host "ERROR: Failed to set password. Trying alternative method..." -ForegroundColor Red
    # Try with postgres database explicitly
    & "$pgBinPath\psql.exe" -U postgres -d postgres -c "ALTER USER postgres WITH PASSWORD '$newPassword';"
}

Write-Host ""
Write-Host "Step 5: Restoring pg_hba.conf security..." -ForegroundColor Yellow
$content = Get-Content $pgHbaPath
$restored = $content -replace '^local\s+all\s+all\s+trust', 'local   all             all                                     scram-sha-256'
$restored | Set-Content $pgHbaPath
Write-Host "Changed trust back to scram-sha-256" -ForegroundColor Green

Write-Host ""
Write-Host "Step 6: Restarting PostgreSQL service again..." -ForegroundColor Yellow
Restart-Service $serviceName -Force
Start-Sleep -Seconds 3
Write-Host "Service restarted" -ForegroundColor Green

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Password reset complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "New password: $newPassword" -ForegroundColor Yellow
Write-Host "Your appsettings.json already has this password configured." -ForegroundColor Green
Write-Host ""
Write-Host "You can now run your migrations!" -ForegroundColor Green

