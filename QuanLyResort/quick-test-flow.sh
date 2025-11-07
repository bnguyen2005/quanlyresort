#!/bin/bash

# Quick test flow: QR → Thanh toán → Webhook → Cập nhật UI
BASE_URL="http://localhost:5130"
BOOKING_ID=${1:-39}

echo "🧪 Testing Simple Payment Flow"
echo "=============================="
echo ""
echo "Booking ID: $BOOKING_ID"
echo ""

# Step 1: Check if backend is running
echo "📡 Step 1: Checking backend..."
if ! curl -s "$BASE_URL" > /dev/null 2>&1; then
  echo "❌ Backend không chạy! Hãy chạy: dotnet run"
  exit 1
fi
echo "✅ Backend đang chạy"
echo ""

# Step 2: Get booking status (before payment)
echo "📋 Step 2: Checking booking status (before payment)..."
TOKEN=${2:-""}
if [ -z "$TOKEN" ]; then
  echo "⚠️  No token provided. Skipping authenticated check."
  echo "   To get booking status, you need to login first and get token."
else
  STATUS_BEFORE=$(curl -s -X GET "$BASE_URL/api/bookings/$BOOKING_ID" \
    -H "Authorization: Bearer $TOKEN" | jq -r '.status // "Unknown"')
  echo "   Status before: $STATUS_BEFORE"
fi
echo ""

# Step 3: Simulate webhook (payment)
echo "💰 Step 3: Simulating webhook (payment)..."
TRANSACTION_ID="TEST-$(date +%s)"
PAYLOAD=$(cat <<EOF
{
  "content": "BOOKING-$BOOKING_ID",
  "amount": 15000,
  "transactionId": "$TRANSACTION_ID"
}
EOF
)

echo "   Payload:"
echo "$PAYLOAD" | jq .
echo ""

RESPONSE=$(curl -s -X POST "$BASE_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD")

echo "   Response:"
echo "$RESPONSE" | jq .
echo ""

# Check if success
if echo "$RESPONSE" | jq -e '.success == true' > /dev/null 2>&1; then
  echo "✅ Webhook thành công!"
else
  echo "❌ Webhook thất bại"
  echo "$RESPONSE" | jq .
  exit 1
fi
echo ""

# Step 4: Check booking status (after payment)
echo "📋 Step 4: Checking booking status (after payment)..."
if [ -z "$TOKEN" ]; then
  echo "⚠️  No token provided. Skipping authenticated check."
  echo "   Please check manually: http://localhost:5130/customer/my-bookings.html"
else
  sleep 2
  STATUS_AFTER=$(curl -s -X GET "$BASE_URL/api/bookings/$BOOKING_ID" \
    -H "Authorization: Bearer $TOKEN" | jq -r '.status // "Unknown"')
  echo "   Status after: $STATUS_AFTER"
  
  if [ "$STATUS_AFTER" = "Paid" ]; then
    echo "✅ Booking status đã cập nhật thành 'Paid'!"
  else
    echo "⚠️  Booking status chưa cập nhật (expected: Paid, got: $STATUS_AFTER)"
  fi
fi
echo ""

# Step 5: Instructions
echo "📱 Step 5: Frontend UI Update"
echo "=============================="
echo "1. Mở browser: http://localhost:5130/customer/my-bookings.html"
echo "2. Đăng nhập: customer1@guest.test / Guest@123"
echo "3. Nếu modal thanh toán đang mở cho booking $BOOKING_ID:"
echo "   - QR code sẽ tự động ẩn"
echo "   - Hiển thị '✅ Thanh toán thành công!'"
echo "   - Modal tự đóng sau 2 giây"
echo "4. Danh sách booking sẽ tự động reload"
echo "   - Booking $BOOKING_ID sẽ có status = 'Paid'"
echo ""

echo "✅ Test flow hoàn tất!"
echo ""
echo "💡 Tips:"
echo "   - Nếu UI không cập nhật, mở browser console (F12) để xem logs"
echo "   - Polling chạy mỗi 5 giây, có thể mất vài giây để detect"
echo "   - Nếu modal không mở, click 'Thanh toán' lại để test"

