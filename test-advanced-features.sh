#!/bin/bash

# Test script cho các tính năng nâng cao
# Usage: ./test-advanced-features.sh [base-url] [token]

BASE_URL="${1:-http://localhost:5130}"
TOKEN="${2:-}"

echo "🧪 Testing Advanced Features"
echo "Base URL: $BASE_URL"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test function
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo -n "Testing: $description... "
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "\n%{http_code}" "$BASE_URL$endpoint" \
            ${TOKEN:+-H "Authorization: Bearer $TOKEN"})
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$BASE_URL$endpoint" \
            ${TOKEN:+-H "Authorization: Bearer $TOKEN"} \
            -H "Content-Type: application/json" \
            ${data:+-d "$data"})
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✅ OK (${http_code})${NC}"
        echo "$body" | jq . 2>/dev/null || echo "$body"
    else
        echo -e "${RED}❌ FAILED (${http_code})${NC}"
        echo "$body"
    fi
    echo ""
}

# 1. Test Localization
echo -e "${YELLOW}=== 1. Testing Multi-language Support ===${NC}"
test_endpoint "GET" "/api/localization/current" "" "Get current language"
test_endpoint "GET" "/api/localization/strings?lang=vi" "" "Get Vietnamese translations"
test_endpoint "GET" "/api/localization/strings?lang=en" "" "Get English translations"
test_endpoint "GET" "/api/localization/supported" "" "Get supported languages"

if [ -n "$TOKEN" ]; then
    test_endpoint "POST" "/api/localization/set-language" '{"language":"en"}' "Set language to English"
fi

# 2. Test 2FA (requires authentication)
if [ -n "$TOKEN" ]; then
    echo -e "${YELLOW}=== 2. Testing 2FA Authentication ===${NC}"
    test_endpoint "GET" "/api/auth/2fa/status" "" "Get 2FA status"
    test_endpoint "POST" "/api/auth/2fa/generate" "" "Generate 2FA secret"
    echo -e "${YELLOW}Note: To enable 2FA, you need to scan QR code and enter the code${NC}"
    echo ""
fi

# 3. Test Notifications (requires authentication)
if [ -n "$TOKEN" ]; then
    echo -e "${YELLOW}=== 3. Testing Notifications ===${NC}"
    test_endpoint "GET" "/api/notifications/unread-count" "" "Get unread count"
    test_endpoint "GET" "/api/notifications" "" "Get all notifications"
    test_endpoint "GET" "/api/notifications?unreadOnly=true" "" "Get unread notifications"
fi

# 4. Test Email (Contact form - no auth required)
echo -e "${YELLOW}=== 4. Testing Email Service ===${NC}"
test_endpoint "POST" "/api/contact" '{
    "name": "Test User",
    "email": "test@example.com",
    "subject": "Test Email",
    "message": "This is a test message"
}' "Send contact email"

echo -e "${GREEN}✅ Testing completed!${NC}"
echo ""
echo "📝 Next steps:"
echo "1. Check email inbox for contact form test"
echo "2. Test 2FA by generating secret and scanning QR code"
echo "3. Test notifications by creating a booking or payment"
echo "4. Test language switching on frontend"

