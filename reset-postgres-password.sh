#!/bin/bash

# PostgreSQL Password Reset Script for Windows (Bash)
# This script helps reset PostgreSQL password to match appsettings.json

echo "========================================="
echo "PostgreSQL Password Reset Helper"
echo "========================================="
echo ""

# Find PostgreSQL installation (PostgreSQL 17 based on service check)
PG_VERSION="17"
PG_PATH="/c/Program Files/PostgreSQL/${PG_VERSION}"
PG_HBA_PATH="${PG_PATH}/data/pg_hba.conf"
PG_BIN_PATH="${PG_PATH}/bin"

# Check if pg_hba.conf exists
if [ ! -f "$PG_HBA_PATH" ]; then
    echo "ERROR: pg_hba.conf not found at: $PG_HBA_PATH"
    echo ""
    echo "Please find pg_hba.conf manually and follow these steps:"
    echo "1. Open pg_hba.conf in a text editor (as Administrator)"
    echo "2. Find line: local   all   all   scram-sha-256"
    echo "3. Change to: local   all   all   trust"
    echo "4. Save file"
    echo "5. Restart PostgreSQL service"
    echo "6. Run: \"${PG_BIN_PATH}/psql.exe\" -U postgres -c \"ALTER USER postgres WITH PASSWORD 'postgres_dev_password';\""
    echo "7. Change 'trust' back to 'scram-sha-256' in pg_hba.conf"
    echo "8. Restart PostgreSQL service again"
    exit 1
fi

echo "Found pg_hba.conf at: $PG_HBA_PATH"
echo ""

# Instructions
echo "========================================="
echo "Steps to Reset Password:"
echo "========================================="
echo ""
echo "Step 1: Edit pg_hba.conf (as Administrator)"
echo "  File: $PG_HBA_PATH"
echo "  Find: local   all   all   scram-sha-256"
echo "  Change to: local   all   all   trust"
echo ""
echo "Step 2: Restart PostgreSQL service"
echo "  Run in PowerShell (as Administrator):"
echo "    Restart-Service postgresql-x64-${PG_VERSION}"
echo "  OR"
echo "    net stop postgresql-x64-${PG_VERSION}"
echo "    net start postgresql-x64-${PG_VERSION}"
echo ""
echo "Step 3: Set new password"
echo "  Run this command:"
echo "    \"${PG_BIN_PATH}/psql.exe\" -U postgres -c \"ALTER USER postgres WITH PASSWORD 'postgres_dev_password';\""
echo ""
echo "Step 4: Restore security in pg_hba.conf"
echo "  Change 'trust' back to 'scram-sha-256'"
echo "  Restart PostgreSQL service again"
echo ""
echo "Step 5: Test connection"
echo "  Your appsettings.json already has the correct password: postgres_dev_password"
echo ""
echo "========================================="
echo "Alternative: Use Docker PostgreSQL"
echo "========================================="
echo ""
echo "If you prefer Docker (easier):"
echo "  1. Stop local PostgreSQL:"
echo "     Get-Service | Where-Object {\$_.Name -like '*postgres*'} | Stop-Service"
echo "  2. Start Docker:"
echo "     docker start pos-postgres"
echo "  3. Password is already set: postgres_dev_password"
echo ""

