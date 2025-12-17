# PostgreSQL Connection Verification Script
# This script verifies that PostgreSQL is accessible and configured correctly

param(
    [string]$Host = "localhost",
    [int]$Port = 5432,
    [string]$Database = "POSSystemDb",
    [string]$Username = "postgres",
    [string]$Password = "postgres_dev_password"
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Connection Verification" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Npgsql is available (for .NET)
Write-Host "Checking PostgreSQL connectivity..." -ForegroundColor Yellow

# Test connection using psql if available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if ($psqlPath) {
    Write-Host "Using psql to test connection..." -ForegroundColor Yellow
    
    $env:PGPASSWORD = $Password
    $connectionString = "host=$Host port=$Port dbname=$Database user=$Username"
    
    try {
        $result = & psql -h $Host -p $Port -U $Username -d $Database -c "SELECT version();" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Connection successful!" -ForegroundColor Green
            Write-Host ""
            Write-Host "PostgreSQL Version:" -ForegroundColor Cyan
            Write-Host $result -ForegroundColor White
            
            # Test database operations
            Write-Host ""
            Write-Host "Testing database operations..." -ForegroundColor Yellow
            
            & psql -h $Host -p $Port -U $Username -d $Database -c "SELECT current_database(), current_user, version();" | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Database operations test: PASSED" -ForegroundColor Green
            }
        } else {
            Write-Host "Connection failed!" -ForegroundColor Red
            Write-Host $result -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "Error testing connection: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    } finally {
        Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    }
} else {
    Write-Host "psql not found. Testing connection using .NET..." -ForegroundColor Yellow
    
    # Try using .NET connection test
    $testScript = @"
using System;
using Npgsql;

try {
    var connString = "Host=$Host;Port=$Port;Database=$Database;Username=$Username;Password=$Password";
    using (var conn = new NpgsqlConnection(connString)) {
        conn.Open();
        Console.WriteLine("Connection successful!");
        
        using (var cmd = new NpgsqlCommand("SELECT version()", conn)) {
            var version = cmd.ExecuteScalar();
            Console.WriteLine("PostgreSQL Version: " + version);
        }
        
        using (var cmd = new NpgsqlCommand("SELECT current_database(), current_user", conn)) {
            using (var reader = cmd.ExecuteReader()) {
                if (reader.Read()) {
                    Console.WriteLine("Database: " + reader[0]);
                    Console.WriteLine("User: " + reader[1]);
                }
            }
        }
    }
} catch (Exception ex) {
    Console.WriteLine("ERROR: " + ex.Message);
    Environment.Exit(1);
}
"@
    
    try {
        $testScript | Out-File -FilePath "$env:TEMP\pgtest.cs" -Encoding UTF8
        # Note: This requires Npgsql NuGet package to be installed
        # For now, just provide instructions
        Write-Host ""
        Write-Host "To test connection, ensure:" -ForegroundColor Yellow
        Write-Host "1. PostgreSQL is running on $Host`:$Port" -ForegroundColor White
        Write-Host "2. Database '$Database' exists" -ForegroundColor White
        Write-Host "3. User '$Username' has access" -ForegroundColor White
        Write-Host ""
        Write-Host "You can test manually using:" -ForegroundColor Yellow
        Write-Host "  psql -h $Host -p $Port -U $Username -d $Database" -ForegroundColor White
    } catch {
        Write-Host "Could not create test script: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Connection String for appsettings.json:" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Host=$Host;Port=$Port;Database=$Database;Username=$Username;Password=$Password" -ForegroundColor White
Write-Host ""
