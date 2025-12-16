#!/bin/bash

# Simple startup script that runs both services in background
# Usage: ./start-simple.sh

set -e  # Exit on error

echo "========================================"
echo "  Starting POS System Project"
echo "========================================"
echo ""

# Get project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Check dependencies
echo "Checking dependencies..."
if ! command -v dotnet &> /dev/null; then
    echo "✗ .NET SDK not found"
    exit 1
fi
echo "✓ .NET SDK found"

if ! command -v node &> /dev/null; then
    echo "✗ Node.js not found"
    exit 1
fi
echo "✓ Node.js found"

if ! command -v npm &> /dev/null; then
    echo "✗ npm not found"
    exit 1
fi
echo "✓ npm found"
echo ""

# Check directories
if [ ! -d "backend" ]; then
    echo "✗ Backend directory not found"
    exit 1
fi

if [ ! -d "frontend" ]; then
    echo "✗ Frontend directory not found"
    exit 1
fi

# Install frontend dependencies if needed
if [ ! -d "frontend/node_modules" ]; then
    echo "Installing frontend dependencies..."
    cd frontend
    npm install
    cd ..
    echo ""
fi

# Start backend in background
echo "Starting Backend API..."
cd backend
dotnet run > ../backend.log 2>&1 &
BACKEND_PID=$!
cd ..
echo "✓ Backend started (PID: $BACKEND_PID)"
echo "  Logs: backend.log"
echo ""

# Wait a bit for backend to start
sleep 5

# Start frontend in background
echo "Starting Frontend..."
cd frontend
npm run dev > ../frontend.log 2>&1 &
FRONTEND_PID=$!
cd ..
echo "✓ Frontend started (PID: $FRONTEND_PID)"
echo "  Logs: frontend.log"
echo ""

echo "========================================"
echo "  Project Started Successfully!"
echo "========================================"
echo ""
echo "Frontend:  http://localhost:5173"
echo "Backend:   http://localhost:5168"
echo "Swagger:   http://localhost:5168/swagger"
echo ""
echo "Process IDs:"
echo "  Backend:  $BACKEND_PID"
echo "  Frontend: $FRONTEND_PID"
echo ""
echo "To stop the services, run:"
echo "  kill $BACKEND_PID $FRONTEND_PID"
echo ""
echo "Or check logs:"
echo "  tail -f backend.log"
echo "  tail -f frontend.log"
echo ""
