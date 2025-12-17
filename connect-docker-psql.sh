#!/bin/bash

# Script to connect to PostgreSQL in Docker
# Usage: ./connect-docker-psql.sh

echo "========================================="
echo "Connecting to PostgreSQL in Docker"
echo "========================================="
echo ""

CONTAINER_NAME="pos-postgres"
DB_NAME="POSSystemDb"
DB_USER="postgres"
DB_PASSWORD="postgres_dev_password"

# Check if container exists
if ! docker ps -a | grep -q "$CONTAINER_NAME"; then
    echo "ERROR: Container $CONTAINER_NAME not found!"
    echo "Start it with: docker-compose -f docker-compose.postgres.yml up -d"
    exit 1
fi

# Check if container is running
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    echo "Container $CONTAINER_NAME is not running."
    echo ""
    echo "Attempting to start container..."
    
    # Try to start the container
    if docker start "$CONTAINER_NAME" 2>&1 | grep -q "Ports are not available"; then
        echo ""
        echo "ERROR: Port 5432 is already in use by local PostgreSQL!"
        echo ""
        echo "You have two options:"
        echo ""
        echo "OPTION 1: Stop local PostgreSQL (requires Administrator):"
        echo "  Open PowerShell as Administrator and run:"
        echo "    Stop-Service postgresql-x64-17"
        echo "  Then run this script again."
        echo ""
        echo "OPTION 2: Connect to local PostgreSQL instead:"
        echo "  \"/c/Program Files/PostgreSQL/17/bin/psql.exe\" -U postgres -d $DB_NAME"
        echo ""
        exit 1
    fi
    
    echo "Waiting for container to be ready..."
    sleep 3
fi

echo "Connecting to psql in Docker container..."
echo ""
echo "Container: $CONTAINER_NAME"
echo "Database: $DB_NAME"
echo "User: $DB_USER"
echo ""
echo "========================================="
echo ""

# Connect to psql
docker exec -it "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME"

