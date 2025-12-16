# Manual Migration Steps (Alternative Method)

Since `dotnet-ef` tool installation is having issues, here are alternative ways to create and apply the migration:

## Option 1: Use Visual Studio Package Manager Console

If you have Visual Studio:

1. Open Visual Studio
2. Go to **Tools** → **NuGet Package Manager** → **Package Manager Console**
3. Set the **Default project** to `backend`
4. Run:
   ```powershell
   Add-Migration AddProductModificationsAndInventory -Context ApplicationDbContext
   Update-Database -Context ApplicationDbContext
   ```

## Option 2: Fix dotnet-ef Installation

Try clearing NuGet cache and reinstalling:

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Try installing with specific version
dotnet tool install --global dotnet-ef --version 8.0.0

# Or try updating existing installation
dotnet tool update --global dotnet-ef
```

## Option 3: Use Docker/Container (if available)

If you're using containers, you can run the migration inside the container.

## Option 4: Manual SQL Script (Last Resort)

If all else fails, I can generate the SQL script manually based on the model changes. Let me know if you'd like me to create this.

## Current Status

The application should work now even without the migration (it will gracefully handle missing tables), but you won't be able to use product modifications/inventory features until the migration is applied.
