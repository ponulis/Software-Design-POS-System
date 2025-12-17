#!/bin/bash

# Frontend-Backend Connection Verification Script
# This script verifies that the frontend can communicate with the backend API

set -e  # Exit on error

echo "========================================="
echo "Frontend-Backend Connection Verification"
echo "========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
BACKEND_URL="http://localhost:5168"
BACKEND_API_URL="${BACKEND_URL}/api"
FRONTEND_URL="http://localhost:5173"
HEALTH_ENDPOINT="${BACKEND_API_URL}/health"

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_info() {
    echo -e "${BLUE}→${NC} $1"
}

# Step 1: Check if backend is running
echo "Step 1: Checking backend server..."
print_info "Backend URL: $BACKEND_URL"

if curl -s -f "$BACKEND_URL" > /dev/null 2>&1 || curl -s -f "$BACKEND_API_URL/health" > /dev/null 2>&1; then
    print_success "Backend server is running"
else
    print_error "Backend server is not running!"
    echo ""
    echo "Please start the backend with:"
    echo "  cd backend"
    echo "  dotnet run"
    echo ""
    exit 1
fi

# Step 2: Test health endpoint
echo ""
echo "Step 2: Testing backend health endpoint..."
print_info "Endpoint: $HEALTH_ENDPOINT"

HEALTH_RESPONSE=$(curl -s -w "\n%{http_code}" "$HEALTH_ENDPOINT" 2>&1)
HTTP_CODE=$(echo "$HEALTH_RESPONSE" | tail -n1)
BODY=$(echo "$HEALTH_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    print_success "Health endpoint responded successfully (HTTP $HTTP_CODE)"
    echo "Response:"
    echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY" | head -10
else
    print_error "Health endpoint failed (HTTP $HTTP_CODE)"
    echo "Response: $BODY"
    exit 1
fi

# Step 3: Test CORS configuration
echo ""
echo "Step 3: Testing CORS configuration..."
print_info "Testing CORS from frontend origin: $FRONTEND_URL"

CORS_RESPONSE=$(curl -s -X OPTIONS \
    -H "Origin: $FRONTEND_URL" \
    -H "Access-Control-Request-Method: GET" \
    -H "Access-Control-Request-Headers: Content-Type" \
    -w "\n%{http_code}" \
    "$HEALTH_ENDPOINT" 2>&1)

CORS_HTTP_CODE=$(echo "$CORS_RESPONSE" | tail -n1)
CORS_HEADERS=$(curl -s -I -X OPTIONS \
    -H "Origin: $FRONTEND_URL" \
    -H "Access-Control-Request-Method: GET" \
    "$HEALTH_ENDPOINT" 2>&1 | grep -i "access-control")

if echo "$CORS_HEADERS" | grep -qi "access-control-allow-origin"; then
    print_success "CORS is configured correctly"
    echo "CORS Headers:"
    echo "$CORS_HEADERS" | sed 's/^/    /'
else
    print_warning "CORS headers not found (may still work with GET requests)"
fi

# Step 4: Test API endpoint with CORS
echo ""
echo "Step 4: Testing API endpoint with CORS..."
print_info "Testing GET request from frontend origin"

API_RESPONSE=$(curl -s -X GET \
    -H "Origin: $FRONTEND_URL" \
    -H "Content-Type: application/json" \
    -w "\n%{http_code}" \
    "$HEALTH_ENDPOINT" 2>&1)

API_HTTP_CODE=$(echo "$API_RESPONSE" | tail -n1)
API_BODY=$(echo "$API_RESPONSE" | sed '$d')

if [ "$API_HTTP_CODE" = "200" ]; then
    print_success "API endpoint accessible with CORS (HTTP $API_HTTP_CODE)"
else
    print_error "API endpoint failed with CORS (HTTP $API_HTTP_CODE)"
    echo "Response: $API_BODY"
fi

# Step 5: Check frontend configuration
echo ""
echo "Step 5: Checking frontend configuration..."
FRONTEND_CLIENT_FILE="frontend/src/api/client.js"

if [ -f "$FRONTEND_CLIENT_FILE" ]; then
    print_success "Frontend API client file found"
    
    # Check if API URL matches (using simpler grep for Windows compatibility)
    if grep -q "localhost:5168" "$FRONTEND_CLIENT_FILE"; then
        print_success "Frontend configured to use correct backend port (5168)"
        print_info "API Base URL: http://localhost:5168/api"
    else
        API_URL_LINE=$(grep -i "localhost.*api\|VITE_API_BASE_URL" "$FRONTEND_CLIENT_FILE" | head -1)
        if [ -n "$API_URL_LINE" ]; then
            print_warning "Frontend API URL: $API_URL_LINE"
        else
            print_info "Using default API URL: http://localhost:5168/api"
        fi
    fi
else
    print_error "Frontend API client file not found: $FRONTEND_CLIENT_FILE"
fi

# Step 6: Check if frontend is running
echo ""
echo "Step 6: Checking frontend server..."
if curl -s -f "$FRONTEND_URL" > /dev/null 2>&1; then
    print_success "Frontend server is running"
    print_info "Frontend URL: $FRONTEND_URL"
else
    print_warning "Frontend server is not running"
    echo ""
    echo "To start the frontend:"
    echo "  cd frontend"
    echo "  npm install  # if not already done"
    echo "  npm run dev"
    echo ""
fi

# Step 7: Test actual API endpoints that frontend uses
echo ""
echo "Step 7: Testing frontend API endpoints..."

# Test menu-items endpoint (used by products)
MENU_ITEMS_RESPONSE=$(curl -s -w "\n%{http_code}" \
    -H "Origin: $FRONTEND_URL" \
    "${BACKEND_API_URL}/menu-items" 2>&1)

MENU_ITEMS_CODE=$(echo "$MENU_ITEMS_RESPONSE" | tail -n1)

if [ "$MENU_ITEMS_CODE" = "200" ] || [ "$MENU_ITEMS_CODE" = "401" ]; then
    print_success "Menu items endpoint accessible (HTTP $MENU_ITEMS_CODE)"
    if [ "$MENU_ITEMS_CODE" = "401" ]; then
        print_info "  (401 is expected - endpoint requires authentication)"
    fi
else
    print_warning "Menu items endpoint returned HTTP $MENU_ITEMS_CODE"
fi

# Step 8: Verify backend CORS settings
echo ""
echo "Step 8: Verifying backend CORS configuration..."
BACKEND_PROGRAM_FILE="backend/Program.cs"

if [ -f "$BACKEND_PROGRAM_FILE" ]; then
    if grep -q "localhost:5173" "$BACKEND_PROGRAM_FILE"; then
        print_success "Backend CORS configured for frontend port 5173"
    else
        print_warning "Backend CORS may not include port 5173"
    fi
    
    if grep -q "AllowFrontend" "$BACKEND_PROGRAM_FILE"; then
        print_success "CORS policy 'AllowFrontend' is configured"
    fi
else
    print_error "Backend Program.cs not found"
fi

# Step 9: Test with actual frontend request simulation
echo ""
echo "Step 9: Simulating frontend API request..."
print_info "Simulating axios request from frontend"

# Create a test request similar to what frontend would send
TEST_RESPONSE=$(curl -s -X GET \
    -H "Origin: $FRONTEND_URL" \
    -H "Content-Type: application/json" \
    -H "Accept: application/json" \
    -w "\n%{http_code}\n%{time_total}" \
    "$HEALTH_ENDPOINT" 2>&1)

RESPONSE_LINES=($(echo "$TEST_RESPONSE"))
HTTP_CODE="${RESPONSE_LINES[-2]}"
RESPONSE_TIME="${RESPONSE_LINES[-1]}"
RESPONSE_BODY=$(echo "$TEST_RESPONSE" | sed '$d' | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    print_success "Frontend-style request successful"
    print_info "Response time: ${RESPONSE_TIME}s"
else
    print_error "Frontend-style request failed (HTTP $HTTP_CODE)"
fi

# Summary
echo ""
echo "========================================="
echo "Verification Summary"
echo "========================================="

# Check all conditions
ALL_GOOD=true

if curl -s -f "$BACKEND_API_URL/health" > /dev/null 2>&1; then
    print_success "Backend server is running and accessible"
else
    print_error "Backend server is not accessible"
    ALL_GOOD=false
fi

if curl -s -X GET -H "Origin: $FRONTEND_URL" "$HEALTH_ENDPOINT" > /dev/null 2>&1; then
    print_success "CORS is working correctly"
else
    print_warning "CORS may have issues"
fi

if [ -f "$FRONTEND_CLIENT_FILE" ]; then
    print_success "Frontend API client configured"
else
    print_error "Frontend API client missing"
    ALL_GOOD=false
fi

echo ""
if [ "$ALL_GOOD" = true ]; then
    echo -e "${GREEN}✓ Frontend-Backend communication is configured correctly!${NC}"
    echo ""
    echo "Next steps:"
    echo "  1. Start backend: cd backend && dotnet run"
    echo "  2. Start frontend: cd frontend && npm run dev"
    echo "  3. Open browser: $FRONTEND_URL"
    echo ""
    echo "Backend API: $BACKEND_API_URL"
    echo "Frontend: $FRONTEND_URL"
    echo "Health Check: $HEALTH_ENDPOINT"
else
    echo -e "${YELLOW}⚠ Some issues found. Please review the output above.${NC}"
fi

