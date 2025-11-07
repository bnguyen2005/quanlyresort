#!/bin/bash

# Script test chức năng thanh toán
# Usage: ./test-payment.sh [bookingId] [token]

BASE_URL="http://localhost:5130"
BOOKING_ID=${1:-39}
TOKEN=${2:-""}

if [ -z "$TOKEN" ]; then
    echo "❌ Vui lòng cung cấp JWT token"
    echo "Usage: ./test-payment.sh [bookingId] [token]"
    echo "Example: ./test-payment.sh 39 eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    exit 1
fi

echo "🧪 Testing Payment Functionality"
echo "================================"
echo "Base URL: $BASE_URL"
echo "Booking ID: $BOOKING_ID"
echo ""

# Test 1: Tạo Payment Session
echo "1️⃣  Testing Payment Session Creation..."
SESSION_RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/session/create" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"bookingId\": $BOOKING_ID, \"amount\": 15000}")

echo "Response: $SESSION_RESPONSE"
SESSION_ID=$(echo $SESSION_RESPONSE | grep -o '"sessionId":"[^"]*' | cut -d'"' -f4)

if [ -z "$SESSION_ID" ]; then
    echo "❌ Failed to create payment session"
    exit 1
fi

echo "✅ Payment session created: $SESSION_ID"
echo ""

# Test 2: Kiểm tra Payment Status
echo "2️⃣  Checking Payment Status..."
STATUS_RESPONSE=$(curl -s -X GET "$BASE_URL/api/payment/status/$SESSION_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Response: $STATUS_RESPONSE"
echo ""

# Test 3: Test Database Check
echo "3️⃣  Testing Database Check..."
DB_CHECK_RESPONSE=$(curl -s -X GET "$BASE_URL/api/payment/test/db-check?bookingId=$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Response: $DB_CHECK_RESPONSE"
echo ""

# Test 4: Simulate Test Payment (nếu có quyền)
echo "4️⃣  Simulating Test Payment..."
TEST_PAYMENT_RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/test/$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Response: $TEST_PAYMENT_RESPONSE"
echo ""

# Test 5: Kiểm tra lại Database sau khi test payment
echo "5️⃣  Checking Database After Test Payment..."
sleep 2
DB_CHECK_AFTER=$(curl -s -X GET "$BASE_URL/api/payment/test/db-check?bookingId=$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Response: $DB_CHECK_AFTER"
echo ""

echo "✅ Test completed!"
echo ""
echo "📝 Next Steps:"
echo "   - Kiểm tra payment modal trong browser có cập nhật không"
echo "   - Kiểm tra WebSocket/SignalR connection trong browser console"
echo "   - Kiểm tra booking status đã chuyển sang 'Paid' chưa"

