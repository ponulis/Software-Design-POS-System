# PostgreSQL Migration Scripts

This directory contains scripts and utilities for migrating from SQL Server to PostgreSQL.

## Scripts Overview

### Setup Scripts

#### `setup-postgresql.sql`
PostgreSQL database setup script. Creates the database and optional user.

**Usage:**
```bash
psql -U postgres -f setup-postgresql.sql
```

Or using Docker:
```bash
docker exec -i pos-postgres psql -U postgres < setup-postgresql.sql
```

### Backup Scripts

#### `backup-sqlserver.ps1`
Creates a backup of the SQL Server LocalDB database before migration.

**Usage:**
```powershell
.\backup-sqlserver.ps1
```

**Parameters:**
- `-ServerInstance`: SQL Server instance (default: "(localdb)\mssqllocaldb")
- `-Database`: Database name (default: "POSSystemDb")
- `-BackupPath`: Backup directory (default: ".\backups")
- `-BackupFileName`: Backup file name (default: auto-generated with timestamp)

**Example:**
```powershell
.\backup-sqlserver.ps1 -BackupPath "C:\Backups" -BackupFileName "MyBackup.bak"
```

### Verification Scripts

#### `verify-postgresql-connection.ps1`
Verifies PostgreSQL connection and configuration.

**Usage:**
```powershell
.\verify-postgresql-connection.ps1
```

**Parameters:**
- `-Host`: PostgreSQL host (default: "localhost")
- `-Port`: PostgreSQL port (default: 5432)
- `-Database`: Database name (default: "POSSystemDb")
- `-Username`: PostgreSQL username (default: "postgres")
- `-Password`: PostgreSQL password (default: "postgres_dev_password")

**Example:**
```powershell
.\verify-postgresql-connection.ps1 -Host "localhost" -Port 5432 -Database "POSSystemDb" -Username "postgres" -Password "mypassword"
```

## Docker Setup

Use the `docker-compose.postgres.yml` file in the project root to quickly set up PostgreSQL:

```bash
docker-compose -f docker-compose.postgres.yml up -d
```

This will start:
- PostgreSQL 15 on port 5432
- pgAdmin 4 on port 5050 (optional database management UI)

**Default Credentials:**
- Database: `POSSystemDb`
- Username: `postgres`
- Password: `postgres_dev_password`
- pgAdmin: `admin@possystem.com` / `admin`

## Prerequisites

### For PowerShell Scripts
- PowerShell 5.1+ or PowerShell Core 7+
- SQL Server PowerShell module (for backup script): `Install-Module -Name SqlServer`

### For SQL Scripts
- PostgreSQL client (`psql`) or access to PostgreSQL server
- PostgreSQL superuser privileges (for database creation)

### For Docker
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose

## Quick Start

1. **Start PostgreSQL (Docker):**
   ```bash
   docker-compose -f docker-compose.postgres.yml up -d
   ```

2. **Create Database:**
   ```bash
   docker exec -i pos-postgres psql -U postgres < backend/Scripts/setup-postgresql.sql
   ```

3. **Verify Connection:**
   ```powershell
   cd backend/Scripts
   .\verify-postgresql-connection.ps1
   ```

4. **Backup SQL Server (before migration):**
   ```powershell
   cd backend/Scripts
   .\backup-sqlserver.ps1
   ```

## Troubleshooting

### PostgreSQL Connection Issues
- Ensure PostgreSQL is running: `docker ps` (if using Docker)
- Check port 5432 is not blocked
- Verify credentials match docker-compose.yml or your PostgreSQL setup

### SQL Server Backup Issues
- Ensure SQL Server LocalDB is running
- Install SQL Server PowerShell module: `Install-Module -Name SqlServer`
- Check backup directory permissions

### Docker Issues
- Ensure Docker Desktop is running
- Check ports 5432 and 5050 are not in use
- View logs: `docker-compose -f docker-compose.postgres.yml logs`

## Next Steps

After completing Phase 1 setup:
1. Proceed to Phase 2: Code Updates
2. Update NuGet packages
3. Update connection strings
4. Create PostgreSQL migrations

See `POSTGRESQL_MIGRATION_PLAN.md` for complete migration guide.
