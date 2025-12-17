# Phase 1: PostgreSQL Migration Setup Guide

This guide walks you through Phase 1 of the PostgreSQL migration: **Preparation and Setup**.

## Prerequisites Checklist

Before starting, ensure you have:

- [ ] **PostgreSQL Server** (choose one):
  - [ ] PostgreSQL installed locally (version 12+)
  - [ ] Docker Desktop installed (recommended for development)
  - [ ] Access to remote PostgreSQL server
- [ ] **Backup of SQL Server database** (we'll create this)
- [ ] **Development environment** ready
- [ ] **Git** (to work on feature branch)

---

## Option A: Docker Setup (Recommended for Development)

### Step 1: Start PostgreSQL with Docker Compose

```bash
# Navigate to project root
cd "c:\Users\krali\OneDrive\Stalinis kompiuteris\Software-Design-POS-System"

# Start PostgreSQL and pgAdmin
docker-compose -f docker-compose.postgres.yml up -d
```

This will start:
- **PostgreSQL 15** on port `5432`
- **pgAdmin 4** on port `5050` (web UI for database management)

### Step 2: Verify PostgreSQL is Running

```bash
# Check containers are running
docker ps

# Check PostgreSQL logs
docker logs pos-postgres
```

You should see output indicating PostgreSQL is ready to accept connections.

### Step 3: Create Database

```bash
# Create database using the setup script
docker exec -i pos-postgres psql -U postgres < backend/Scripts/setup-postgresql.sql
```

Or manually:
```bash
docker exec -it pos-postgres psql -U postgres
```

Then in psql:
```sql
CREATE DATABASE "POSSystemDb";
\c "POSSystemDb"
\q
```

### Step 4: Verify Connection

```powershell
# Run verification script
cd backend/Scripts
.\verify-postgresql-connection.ps1
```

**Expected Output:**
```
=========================================
PostgreSQL Connection Verification
=========================================

Connection successful!
PostgreSQL Version: PostgreSQL 15.x ...
Database operations test: PASSED
```

### Step 5: Access pgAdmin (Optional)

1. Open browser: http://localhost:5050
2. Login:
   - Email: `admin@possystem.com`
   - Password: `admin`
3. Add server:
   - Host: `postgres` (container name)
   - Port: `5432`
   - Database: `POSSystemDb`
   - Username: `postgres`
   - Password: `postgres_dev_password`

---

## Option B: Local PostgreSQL Installation

### Step 1: Install PostgreSQL

**Windows:**
1. Download from: https://www.postgresql.org/download/windows/
2. Run installer
3. Remember the password you set for `postgres` user
4. Default port: `5432`

**Or use Chocolatey:**
```powershell
choco install postgresql
```

### Step 2: Create Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Run setup script
\i backend/Scripts/setup-postgresql.sql

# Or manually:
CREATE DATABASE "POSSystemDb";
\c "POSSystemDb"
\q
```

### Step 3: Update Connection String

Update `backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=POSSystemDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Step 4: Verify Connection

```powershell
cd backend/Scripts
.\verify-postgresql-connection.ps1 -Password "YOUR_PASSWORD"
```

---

## Step 6: Backup SQL Server Database

**IMPORTANT:** Before proceeding with migration, create a backup of your current SQL Server database.

```powershell
# Navigate to scripts directory
cd backend/Scripts

# Run backup script
.\backup-sqlserver.ps1
```

**Expected Output:**
```
=========================================
SQL Server Database Backup Script
=========================================

Server Instance: (localdb)\mssqllocaldb
Database: POSSystemDb
Backup Path: .\backups\POSSystemDb_Backup_20241217_143022.bak

Testing database connection...
Connection successful!

Creating backup...
Backup completed successfully!
Backup file: .\backups\POSSystemDb_Backup_20241217_143022.bak
Backup size: 2.45 MB
```

**Backup Location:**
- Default: `backend/Scripts/backups/`
- Files are timestamped automatically

**Verify Backup:**
- Check backup file exists
- Note the file size (should be > 0)
- Store backup in a safe location

---

## Step 7: Verify Setup Complete

### Checklist

- [ ] PostgreSQL is running and accessible
- [ ] Database `POSSystemDb` exists
- [ ] Connection test passes
- [ ] SQL Server backup created successfully
- [ ] Backup file stored safely

### Quick Verification Commands

```bash
# Check PostgreSQL is running (Docker)
docker ps | grep postgres

# Test PostgreSQL connection
docker exec -it pos-postgres psql -U postgres -d POSSystemDb -c "SELECT version();"

# Or using PowerShell script
cd backend/Scripts
.\verify-postgresql-connection.ps1
```

---

## Troubleshooting

### PostgreSQL Not Starting (Docker)

**Issue:** Container exits immediately
```bash
# Check logs
docker logs pos-postgres

# Common issues:
# - Port 5432 already in use
# - Volume permissions issue
```

**Solution:**
```bash
# Stop existing containers
docker-compose -f docker-compose.postgres.yml down

# Remove volumes if needed (WARNING: deletes data)
docker volume rm software-design-pos-system_postgres_data

# Start fresh
docker-compose -f docker-compose.postgres.yml up -d
```

### Connection Refused

**Issue:** Cannot connect to PostgreSQL

**Check:**
1. PostgreSQL is running: `docker ps` or check Windows services
2. Port 5432 is not blocked by firewall
3. Connection string is correct
4. Credentials match

**Test Connection:**
```bash
# Using psql
psql -h localhost -p 5432 -U postgres -d POSSystemDb

# Or using Docker
docker exec -it pos-postgres psql -U postgres -d POSSystemDb
```

### SQL Server Backup Fails

**Issue:** Backup script fails

**Common Causes:**
1. SQL Server LocalDB not running
2. SQL Server PowerShell module not installed
3. Database doesn't exist

**Solutions:**
```powershell
# Install SQL Server PowerShell module
Install-Module -Name SqlServer -Force -Scope CurrentUser

# Check LocalDB is running
sqllocaldb info MSSQLLocalDB

# Start LocalDB if needed
sqllocaldb start MSSQLLocalDB
```

### Permission Denied

**Issue:** Cannot create database or access PostgreSQL

**Solution:**
- Ensure you're using `postgres` superuser or have CREATEDB privilege
- Check PostgreSQL `pg_hba.conf` allows connections
- Verify user has necessary permissions

---

## Next Steps

Once Phase 1 is complete:

1. ✅ PostgreSQL is running
2. ✅ Database is created
3. ✅ Connection verified
4. ✅ SQL Server backup created

**Proceed to Phase 2:** Code Updates
- Update NuGet packages
- Update connection strings
- Fix SQL Server-specific code

See `POSTGRESQL_MIGRATION_PLAN.md` for Phase 2 details.

---

## Quick Reference

### Docker Commands

```bash
# Start PostgreSQL
docker-compose -f docker-compose.postgres.yml up -d

# Stop PostgreSQL
docker-compose -f docker-compose.postgres.yml down

# View logs
docker logs pos-postgres

# Access PostgreSQL CLI
docker exec -it pos-postgres psql -U postgres -d POSSystemDb

# Backup PostgreSQL database
docker exec pos-postgres pg_dump -U postgres POSSystemDb > backup.sql

# Restore PostgreSQL database
docker exec -i pos-postgres psql -U postgres POSSystemDb < backup.sql
```

### Connection Strings

**Docker (default):**
```
Host=localhost;Port=5432;Database=POSSystemDb;Username=postgres;Password=postgres_dev_password
```

**Local Installation:**
```
Host=localhost;Port=5432;Database=POSSystemDb;Username=postgres;Password=YOUR_PASSWORD
```

### Useful PostgreSQL Commands

```sql
-- List databases
\l

-- Connect to database
\c POSSystemDb

-- List tables
\dt

-- Describe table
\d "Businesses"

-- Exit
\q
```

---

**Phase 1 Status:** ✅ Complete when all checklist items are checked
