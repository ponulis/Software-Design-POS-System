# Database Migration Instructions

## Problem
The error `Invalid object name 'ProductModificationAssignments'` indicates that the database migration hasn't been run yet. The new tables for product modifications and inventory don't exist in the database.

## Solution: Run the Migration

### Step 1: Navigate to the backend directory
```bash
cd backend
```

### Step 2: Create the migration
```bash
dotnet ef migrations add AddProductModificationsAndInventory --context ApplicationDbContext
```

### Step 3: Apply the migration to the database
```bash
dotnet ef database update --context ApplicationDbContext
```

### Step 4: Restart the backend server
After running the migration, restart your backend server for the changes to take effect.

## Alternative: If dotnet-ef is not installed

If you get an error that `dotnet-ef` is not found, install it first:

```bash
dotnet tool install --global dotnet-ef
```

Then run the migration commands above.

## Verification

After running the migration, you should see these new tables in your database:
- `ProductModifications`
- `ProductModificationValues`
- `ProductModificationAssignments`
- `InventoryItems`
- `InventoryModificationValues`

## Temporary Workaround

The code has been updated to handle missing tables gracefully, so the application should work even without the migration (products will just not have modifications/inventory data). However, you should run the migration to enable full functionality.
