# PostgreSQL Migration Plan

## Overview
This document outlines the comprehensive plan for migrating the POS System from SQL Server LocalDB to PostgreSQL database. The migration involves updating Entity Framework Core configuration, replacing SQL Server-specific code, migrating existing data, and ensuring compatibility.

---

## 1. Pre-Migration Assessment

### 1.1 Current System State
- **Current Database**: SQL Server LocalDB
- **EF Core Version**: 8.0.0
- **Database Provider**: `Microsoft.EntityFrameworkCore.SqlServer` v8.0.0
- **Connection String**: `Server=(localdb)\\mssqllocaldb;Database=POSSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true`
- **Migrations**: 5 existing migrations
- **Models**: 15+ entities (Business, User, Product, Service, Order, OrderItem, Appointment, Tax, Discount, Payment, GiftCard, ProductModification, InventoryItem, etc.)

### 1.2 SQL Server-Specific Code Identified
1. **Program.cs**: `UseSqlServer()` configuration
2. **DbSeeder.cs**: `ALTER TABLE ... NOCHECK CONSTRAINT ALL` (SQL Server-specific)
3. **ProductService.cs**: `Microsoft.Data.SqlClient.SqlException` exception handling
4. **HealthController.cs**: `ExecuteSqlRawAsync("SELECT 1")` (generic SQL, compatible)
5. **Migrations**: SQL Server-specific annotations (`SqlServer:Identity`, `SqlServerModelBuilderExtensions`)

### 1.3 Data Migration Considerations
- Existing migrations need to be regenerated for PostgreSQL
- Data type mappings may differ (e.g., `decimal(18,2)` → `numeric(18,2)`)
- Identity columns use different syntax (IDENTITY vs SERIAL/GENERATED ALWAYS AS IDENTITY)
- String concatenation and date functions may differ
- Index creation syntax is compatible but may need verification

---

## 2. Migration Strategy

### 2.1 Approach: Clean Migration with Data Export/Import
**Rationale**: 
- Cleaner migration path
- Ensures PostgreSQL-optimized schema
- Avoids compatibility issues from SQL Server-specific features
- Better long-term maintainability

**Alternative Considered**: Direct database migration tools (pgloader, etc.)
- **Rejected**: More complex, potential compatibility issues, harder to verify

### 2.2 Migration Phases
1. **Phase 1**: Preparation and Setup
2. **Phase 2**: Code Updates
3. **Phase 3**: Schema Migration
4. **Phase 4**: Data Migration
5. **Phase 5**: Testing and Validation
6. **Phase 6**: Deployment

---

## 3. Phase 1: Preparation and Setup

### 3.1 Prerequisites
- [ ] PostgreSQL server installed and running (version 12+ recommended)
- [ ] PostgreSQL client tools installed (psql, pgAdmin, or similar)
- [ ] Backup of current SQL Server database
- [ ] Development environment ready for testing
- [ ] Access to production database (if applicable)

### 3.2 PostgreSQL Installation
**Windows:**
```powershell
# Download PostgreSQL from https://www.postgresql.org/download/windows/
# Or use Chocolatey:
choco install postgresql
```

**Docker (Alternative):**
```bash
docker run --name pos-postgres -e POSTGRES_PASSWORD=yourpassword -e POSTGRES_DB=POSSystemDb -p 5432:5432 -d postgres:15
```

### 3.3 Database Creation
```sql
-- Connect to PostgreSQL
psql -U postgres

-- Create database
CREATE DATABASE "POSSystemDb";

-- Create user (optional, for production)
CREATE USER posuser WITH PASSWORD 'yourpassword';
GRANT ALL PRIVILEGES ON DATABASE "POSSystemDb" TO posuser;
```

### 3.4 Backup Current Database
```powershell
# Export SQL Server database schema and data
# Using SQL Server Management Studio or sqlcmd:
sqlcmd -S "(localdb)\mssqllocaldb" -d POSSystemDb -E -Q "SELECT * FROM Businesses" -o businesses_backup.csv -s "," -W
# Repeat for all tables, or use full database backup
```

---

## 4. Phase 2: Code Updates

### 4.1 Update NuGet Packages

**File**: `backend/backend.csproj`

**Changes:**
```xml
<!-- Remove SQL Server package -->
<!-- <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" /> -->

<!-- Add PostgreSQL package -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

**Action Items:**
- [ ] Remove `Microsoft.EntityFrameworkCore.SqlServer` package reference
- [ ] Add `Npgsql.EntityFrameworkCore.PostgreSQL` package reference
- [ ] Run `dotnet restore` to install new package

### 4.2 Update Database Configuration

**File**: `backend/Program.cs`

**Current Code (Line 102-103):**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**New Code:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Action Items:**
- [ ] Replace `UseSqlServer` with `UseNpgsql`
- [ ] Add `using Npgsql.EntityFrameworkCore.PostgreSQL;` if needed

### 4.3 Update Connection Strings

**File**: `backend/appsettings.json`

**Current Connection String:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=POSSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

**New Connection String:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=POSSystemDb;Username=postgres;Password=yourpassword"
}
```

**File**: `backend/appsettings.Development.json`
- [ ] Add PostgreSQL connection string for development

**File**: `backend/appsettings.Production.json`
- [ ] Add PostgreSQL connection string for production (use environment variables)

**Action Items:**
- [ ] Update `appsettings.json` with PostgreSQL connection string
- [ ] Update `appsettings.Development.json` if exists
- [ ] Update `appsettings.Production.json` if exists
- [ ] Consider using environment variables for production credentials

### 4.4 Fix SQL Server-Specific Code

#### 4.4.1 DbSeeder.cs

**File**: `backend/Data/DbSeeder.cs`

**Current Code (Lines 41, 97):**
```csharp
context.Database.ExecuteSqlRaw("ALTER TABLE Businesses NOCHECK CONSTRAINT ALL");
// ... seeding code ...
context.Database.ExecuteSqlRaw("ALTER TABLE Businesses CHECK CONSTRAINT ALL");
```

**New Code (PostgreSQL-compatible):**
```csharp
// PostgreSQL doesn't support NOCHECK CONSTRAINT in the same way
// Instead, we'll use transactions and proper ordering
using var transaction = context.Database.BeginTransaction();
try
{
    // Disable triggers temporarily (PostgreSQL approach)
    await context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica'");
    
    // ... seeding code ...
    
    // Re-enable triggers
    await context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin'");
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Alternative Approach (Simpler):**
```csharp
// Simply ensure proper order: Create Business first, then User with OwnerId
// No need to disable constraints if we seed in correct order
var business = new Business { /* ... */ };
context.Businesses.Add(business);
await context.SaveChangesAsync(); // Get Business.Id

var owner = new User 
{ 
    /* ... */ 
    BusinessId = business.Id,
    OwnerId = business.Id // Set after business is created
};
// ... rest of seeding
```

**Action Items:**
- [ ] Refactor `DbSeeder.cs` to remove SQL Server-specific constraint disabling
- [ ] Use proper transaction handling or correct seeding order
- [ ] Test seeding works correctly

#### 4.4.2 ProductService.cs

**File**: `backend/Services/ProductService.cs`

**Current Code (Line 44):**
```csharp
catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("Invalid object name"))
```

**New Code:**
```csharp
catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
// OR use generic DbUpdateException:
catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("does not exist") == true)
```

**Action Items:**
- [ ] Replace SQL Server exception handling with PostgreSQL-compatible version
- [ ] Add `using Npgsql;` if using `PostgresException`
- [ ] Test error handling scenarios

#### 4.4.3 HealthController.cs

**File**: `backend/Controllers/HealthController.cs`

**Current Code (Line 53):**
```csharp
await _context.Database.ExecuteSqlRawAsync("SELECT 1");
```

**Status**: ✅ **No changes needed** - Generic SQL, works with PostgreSQL

**Action Items:**
- [ ] Verify health check works (no changes required)

### 4.5 Update ApplicationDbContext (if needed)

**File**: `backend/Data/ApplicationDbContext.cs`

**Status**: ✅ **No changes needed** - EF Core model configuration is database-agnostic

**Note**: The `OnModelCreating` method uses EF Core fluent API which is provider-agnostic. However, verify:
- Decimal precision: `HasColumnType("decimal(18,2)")` → PostgreSQL uses `numeric(18,2)` (automatic conversion)
- String length constraints: `HasMaxLength()` works the same
- Indexes: Compatible syntax
- Relationships: Compatible

**Action Items:**
- [ ] Review `ApplicationDbContext.cs` for any SQL Server-specific code (none expected)
- [ ] Verify all data types map correctly

---

## 5. Phase 3: Schema Migration

### 5.1 Remove Old Migrations

**Strategy**: Delete existing SQL Server migrations and create fresh PostgreSQL migrations

**Action Items:**
- [ ] Backup current `Migrations/` folder (for reference)
- [ ] Delete all files in `backend/Migrations/` folder:
  - `20251212100017_InitialCreate.cs` and `.Designer.cs`
  - `20251212100051_FixProductTagsComparer.cs` and `.Designer.cs`
  - `20251214171240_AddPerformanceIndexes.cs` and `.Designer.cs`
  - `20251214214559_AddPaymentIndexes.cs` and `.Designer.cs`
  - `20251216180931_AddProductModificationsAndInventory.cs` and `.Designer.cs`
  - `20251216191155_AddPricingToProductModifications.cs` and `.Designer.cs`
  - `ApplicationDbContextModelSnapshot.cs`
- [ ] Keep a backup copy for reference

### 5.2 Create Initial PostgreSQL Migration

**Commands:**
```powershell
cd backend
dotnet ef migrations add InitialPostgreSQLMigration
```

**Action Items:**
- [ ] Run migration command
- [ ] Review generated migration file
- [ ] Verify PostgreSQL-specific syntax (should use `SERIAL` or `GENERATED ALWAYS AS IDENTITY`)
- [ ] Check decimal types are `numeric` not `decimal`

### 5.3 Review Migration Files

**Expected Changes in Migration:**
- Identity columns: `[SqlServer:Identity]` → `[NpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)]`
- Decimal types: Should remain as `decimal(18,2)` in C# but generate `numeric(18,2)` in PostgreSQL
- String types: `nvarchar(max)` → `text` or `varchar(n)`
- Indexes: Should be compatible

**Action Items:**
- [ ] Review `ApplicationDbContextModelSnapshot.cs` for PostgreSQL annotations
- [ ] Verify all migrations generate correct PostgreSQL DDL
- [ ] Check for any SQL Server-specific syntax that needs fixing

### 5.4 Apply Migration to PostgreSQL

**Commands:**
```powershell
# Ensure PostgreSQL is running and connection string is correct
dotnet ef database update
```

**Action Items:**
- [ ] Verify connection string is correct
- [ ] Run `dotnet ef database update`
- [ ] Verify database schema is created correctly
- [ ] Check all tables, indexes, and constraints exist

### 5.5 Verify Schema

**PostgreSQL Verification:**
```sql
-- Connect to database
psql -U postgres -d POSSystemDb

-- List all tables
\dt

-- Check specific table structure
\d "Businesses"
\d "Users"
\d "Orders"

-- Verify indexes
\di

-- Check constraints
SELECT conname, contype, conrelid::regclass 
FROM pg_constraint 
WHERE connamespace = 'public'::regnamespace;
```

**Action Items:**
- [ ] Verify all tables exist
- [ ] Verify column types are correct
- [ ] Verify indexes are created
- [ ] Verify foreign key constraints exist
- [ ] Verify unique constraints exist

---

## 6. Phase 4: Data Migration

### 6.1 Data Export from SQL Server

**Option A: Using EF Core (Recommended for small datasets)**
- Use existing DbContext to read from SQL Server
- Write to PostgreSQL using new DbContext
- Maintains data integrity and relationships

**Option B: Using SQL Server Export Tools**
- Export to CSV/JSON
- Import to PostgreSQL using COPY or pgAdmin

**Option C: Using Database Migration Tools**
- pgloader (Linux/Mac)
- SQL Server Migration Assistant (SSMA)

### 6.2 Data Migration Script

**Create**: `backend/Scripts/MigrateDataToPostgreSQL.cs`

**Approach**: Create a console application or script that:
1. Connects to SQL Server (old database)
2. Connects to PostgreSQL (new database)
3. Reads data from SQL Server tables
4. Writes data to PostgreSQL tables in correct order (respecting foreign keys)

**Migration Order:**
1. Businesses
2. Users (depends on Businesses)
3. Products, Services, Taxes, Discounts (depend on Businesses)
4. ProductModifications, ProductModificationValues (depend on Businesses)
5. ProductModificationAssignments (depend on Products, ProductModifications)
6. InventoryItems, InventoryModificationValues (depend on Products)
7. GiftCards (depends on Businesses)
8. Orders (depends on Businesses, Users)
9. OrderItems (depends on Orders, Products)
10. Appointments (depends on Businesses, Services, Users, Orders)
11. Payments (depends on Orders, Users)

**Action Items:**
- [ ] Create data migration script
- [ ] Test migration script on development database
- [ ] Verify data integrity after migration
- [ ] Compare record counts between databases
- [ ] Verify relationships are maintained

### 6.3 Data Validation

**Validation Checklist:**
- [ ] Record counts match for all tables
- [ ] Foreign key relationships are intact
- [ ] Decimal values are preserved correctly
- [ ] Date/time values are correct (timezone considerations)
- [ ] String values are preserved (encoding, special characters)
- [ ] Enum values are preserved
- [ ] Unique constraints are maintained
- [ ] Indexes are populated correctly

**Validation Queries:**
```sql
-- Compare record counts
SELECT 'Businesses' as TableName, COUNT(*) as Count FROM "Businesses"
UNION ALL
SELECT 'Users', COUNT(*) FROM "Users"
UNION ALL
SELECT 'Orders', COUNT(*) FROM "Orders"
-- ... repeat for all tables

-- Verify relationships
SELECT COUNT(*) FROM "Orders" o
LEFT JOIN "Businesses" b ON o."BusinessId" = b."Id"
WHERE b."Id" IS NULL; -- Should return 0

-- Check for orphaned records
SELECT COUNT(*) FROM "OrderItems" oi
LEFT JOIN "Orders" o ON oi."OrderId" = o."Id"
WHERE o."Id" IS NULL; -- Should return 0
```

---

## 7. Phase 5: Testing and Validation

### 7.1 Unit Testing

**Action Items:**
- [ ] Run all existing unit tests
- [ ] Fix any tests that fail due to database differences
- [ ] Add tests for PostgreSQL-specific scenarios if needed
- [ ] Verify exception handling works correctly

### 7.2 Integration Testing

**Test Scenarios:**
- [ ] Create new order
- [ ] Add items to order
- [ ] Process payment (cash, card, gift card)
- [ ] Split payment
- [ ] Create appointment
- [ ] Create product with modifications
- [ ] Update inventory
- [ ] Generate receipt
- [ ] Cancel order
- [ ] User authentication and authorization

**Action Items:**
- [ ] Test all CRUD operations
- [ ] Test all business logic flows
- [ ] Test error scenarios
- [ ] Test concurrent operations
- [ ] Test performance (query execution times)

### 7.3 Data Integrity Testing

**Action Items:**
- [ ] Verify foreign key constraints work
- [ ] Verify unique constraints work
- [ ] Verify cascade deletes work correctly
- [ ] Test transaction rollback scenarios
- [ ] Verify data consistency after operations

### 7.4 Performance Testing

**Action Items:**
- [ ] Compare query performance with SQL Server
- [ ] Verify indexes are being used (EXPLAIN ANALYZE)
- [ ] Test with larger datasets
- [ ] Monitor connection pooling
- [ ] Check for N+1 query problems

**PostgreSQL Query Analysis:**
```sql
-- Enable query logging
SET log_statement = 'all';
SET log_duration = on;

-- Analyze query plans
EXPLAIN ANALYZE SELECT * FROM "Orders" WHERE "BusinessId" = 1;
```

---

## 8. Phase 6: Deployment

### 8.1 Pre-Deployment Checklist

- [ ] All code changes committed
- [ ] All migrations tested in development
- [ ] Data migration tested and verified
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Rollback plan prepared
- [ ] Backup of production SQL Server database created
- [ ] PostgreSQL production database created and configured
- [ ] Connection strings updated for production
- [ ] Environment variables configured

### 8.2 Deployment Steps

**Development Environment:**
1. Update connection string
2. Run migrations: `dotnet ef database update`
3. Run data migration script
4. Verify application works
5. Run test suite

**Staging Environment:**
1. Deploy code changes
2. Create PostgreSQL database
3. Run migrations
4. Migrate data
5. Update connection strings
6. Test thoroughly
7. Get stakeholder approval

**Production Environment:**
1. **Maintenance Window**: Schedule downtime
2. **Backup**: Full backup of SQL Server database
3. **Deploy Code**: Deploy updated application code
4. **Create PostgreSQL Database**: Set up production PostgreSQL instance
5. **Run Migrations**: Apply schema to PostgreSQL
6. **Migrate Data**: Run data migration script
7. **Verify Data**: Validate data integrity
8. **Update Configuration**: Update connection strings
9. **Start Application**: Start application pointing to PostgreSQL
10. **Smoke Tests**: Verify critical functionality
11. **Monitor**: Monitor application logs and performance
12. **Rollback Plan**: Keep SQL Server available for quick rollback if needed

### 8.3 Rollback Plan

**If Migration Fails:**
1. Stop application
2. Revert connection string to SQL Server
3. Restart application
4. Verify application works with SQL Server
5. Investigate migration issues
6. Fix issues and retry migration

**Rollback Checklist:**
- [ ] SQL Server database backup available
- [ ] Connection string reversion script ready
- [ ] Application restart procedure documented
- [ ] Rollback tested in staging environment

---

## 9. Post-Migration Tasks

### 9.1 Cleanup

**Action Items:**
- [ ] Remove SQL Server package references (if not done)
- [ ] Remove old migration files (keep backup)
- [ ] Update documentation
- [ ] Remove SQL Server-specific code comments
- [ ] Archive SQL Server database backup

### 9.2 Documentation Updates

**Files to Update:**
- [ ] README.md - Update database setup instructions
- [ ] Deployment documentation
- [ ] Developer onboarding documentation
- [ ] Database schema documentation
- [ ] Connection string examples

### 9.3 Monitoring and Optimization

**Action Items:**
- [ ] Set up PostgreSQL monitoring
- [ ] Configure connection pooling (if needed)
- [ ] Review and optimize slow queries
- [ ] Set up database backups
- [ ] Configure log rotation
- [ ] Set up alerts for database issues

**PostgreSQL Configuration:**
```sql
-- Check current configuration
SHOW shared_buffers;
SHOW effective_cache_size;
SHOW maintenance_work_mem;
SHOW checkpoint_completion_target;

-- Consider tuning for production (postgresql.conf)
```

### 9.4 Team Training

**Action Items:**
- [ ] Update team on PostgreSQL differences
- [ ] Provide PostgreSQL query examples
- [ ] Document common PostgreSQL commands
- [ ] Share migration lessons learned

---

## 10. Known Differences and Considerations

### 10.1 Data Type Mappings

| SQL Server | PostgreSQL | Notes |
|------------|------------|-------|
| `decimal(18,2)` | `numeric(18,2)` | Automatic conversion |
| `nvarchar(max)` | `text` | EF Core handles this |
| `nvarchar(n)` | `varchar(n)` | Compatible |
| `datetime2` | `timestamp` | Timezone considerations |
| `bit` | `boolean` | Compatible |
| `int IDENTITY` | `SERIAL` or `GENERATED ALWAYS AS IDENTITY` | EF Core handles |

### 10.2 SQL Syntax Differences

- **String Concatenation**: SQL Server uses `+`, PostgreSQL uses `||` (EF Core handles)
- **Date Functions**: Some differences, but EF Core LINQ handles most
- **Top N**: SQL Server uses `TOP`, PostgreSQL uses `LIMIT` (EF Core handles)
- **Case Sensitivity**: PostgreSQL is case-sensitive for identifiers (use quotes)

### 10.3 Feature Differences

- **Multiple Active Result Sets (MARS)**: Not supported in PostgreSQL (not needed with EF Core)
- **Identity Columns**: Different syntax but EF Core abstracts this
- **Full-Text Search**: Different implementation (not currently used)
- **JSON Support**: Both support, but syntax differs (not currently used extensively)

### 10.4 Performance Considerations

- **Connection Pooling**: Both support, EF Core handles
- **Query Plan Caching**: Both support
- **Indexes**: Similar syntax, verify performance
- **Transactions**: Both support ACID properties

---

## 11. Risk Assessment and Mitigation

### 11.1 Identified Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Data loss during migration | Low | High | Multiple backups, test migration first |
| Performance degradation | Medium | Medium | Performance testing, query optimization |
| Application downtime | High | Medium | Maintenance window, quick rollback plan |
| Compatibility issues | Medium | Medium | Thorough testing, staging environment |
| Team learning curve | High | Low | Documentation, training |

### 11.2 Mitigation Strategies

1. **Multiple Backups**: Keep SQL Server backup until migration is verified
2. **Staged Rollout**: Test in dev → staging → production
3. **Rollback Plan**: Keep SQL Server available for quick rollback
4. **Monitoring**: Set up alerts and monitoring for PostgreSQL
5. **Documentation**: Comprehensive documentation for team

---

## 12. Timeline Estimate

### 12.1 Development Phase (1-2 weeks)
- **Day 1-2**: Setup PostgreSQL, update code
- **Day 3-4**: Create migrations, fix SQL Server-specific code
- **Day 5-7**: Data migration script development
- **Day 8-10**: Testing and bug fixes

### 12.2 Testing Phase (1 week)
- **Day 1-3**: Unit and integration testing
- **Day 4-5**: Performance testing and optimization
- **Day 6-7**: User acceptance testing

### 12.3 Deployment Phase (1-2 days)
- **Day 1**: Staging deployment and validation
- **Day 2**: Production deployment (during maintenance window)

**Total Estimated Time**: 2-3 weeks

---

## 13. Success Criteria

### 13.1 Technical Criteria
- [ ] All migrations applied successfully
- [ ] All data migrated without loss
- [ ] All tests passing
- [ ] Performance meets or exceeds SQL Server baseline
- [ ] No critical bugs introduced
- [ ] Application runs without errors

### 13.2 Business Criteria
- [ ] Zero data loss
- [ ] Minimal downtime (< 2 hours)
- [ ] All features working as before
- [ ] Team can operate system effectively
- [ ] Documentation complete

---

## 14. Checklist Summary

### Pre-Migration
- [ ] PostgreSQL installed and configured
- [ ] Backup of SQL Server database created
- [ ] Development environment ready
- [ ] Team notified of migration plan

### Code Changes
- [ ] NuGet packages updated
- [ ] Connection strings updated
- [ ] SQL Server-specific code removed/fixed
- [ ] Code reviewed and tested

### Schema Migration
- [ ] Old migrations removed
- [ ] New PostgreSQL migrations created
- [ ] Migrations applied to development database
- [ ] Schema verified

### Data Migration
- [ ] Data migration script created
- [ ] Data migrated to development PostgreSQL
- [ ] Data integrity verified
- [ ] Record counts match

### Testing
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] Performance tests passing
- [ ] User acceptance testing complete

### Deployment
- [ ] Staging deployment successful
- [ ] Production backup created
- [ ] Production migration executed
- [ ] Application verified working
- [ ] Monitoring configured

### Post-Migration
- [ ] Documentation updated
- [ ] Team trained
- [ ] Old database archived
- [ ] Monitoring and alerts configured

---

## 15. Additional Resources

### Documentation
- [EF Core PostgreSQL Provider](https://www.npgsql.org/efcore/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Migrating from SQL Server to PostgreSQL](https://www.postgresql.org/docs/current/migration.html)

### Tools
- **pgAdmin**: PostgreSQL administration tool
- **psql**: PostgreSQL command-line tool
- **pgloader**: Data migration tool (Linux/Mac)
- **DBeaver**: Universal database tool

### Support
- PostgreSQL Community: https://www.postgresql.org/community/
- Stack Overflow: Tag `postgresql` and `entity-framework-core`

---

## Conclusion

This migration plan provides a comprehensive roadmap for migrating from SQL Server LocalDB to PostgreSQL. The plan emphasizes:
- **Safety**: Multiple backups and rollback options
- **Testing**: Thorough testing at each phase
- **Documentation**: Clear documentation for team
- **Gradual Rollout**: Dev → Staging → Production

Following this plan should result in a successful migration with minimal downtime and risk.

---

**Document Version**: 1.0  
**Last Updated**: 2024-12-17  
**Author**: Migration Planning Team  
**Status**: Draft - Ready for Review
