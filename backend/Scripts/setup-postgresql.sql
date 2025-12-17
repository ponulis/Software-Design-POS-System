-- PostgreSQL Database Setup Script
-- This script creates the database and user for the POS System
-- Run this script as a PostgreSQL superuser (typically 'postgres')

-- Create database (if it doesn't exist)
SELECT 'CREATE DATABASE "POSSystemDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'POSSystemDb')\gexec

-- Connect to the new database
\c "POSSystemDb"

-- Create application user (optional, for production)
-- Uncomment and modify if you want a dedicated user instead of using 'postgres'
/*
CREATE USER posuser WITH PASSWORD 'your_secure_password_here';
GRANT ALL PRIVILEGES ON DATABASE "POSSystemDb" TO posuser;
ALTER USER posuser CREATEDB;
*/

-- Set timezone (optional, adjust as needed)
SET timezone = 'UTC';

-- Create extensions if needed (uncomment as needed)
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
-- CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For text search

-- Display database information
SELECT 
    datname as "Database Name",
    pg_size_pretty(pg_database_size(datname)) as "Size"
FROM pg_database 
WHERE datname = 'POSSystemDb';

-- Display current user
SELECT current_user as "Current User", current_database() as "Current Database";
