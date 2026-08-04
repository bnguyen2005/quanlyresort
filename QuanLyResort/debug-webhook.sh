#!/bin/bash

# Debug webhook - kiểm tra webhook có hoạt động không
BASE_URL="http://localhost:5130"
BOOKING_ID=${1:-39}

echo "🔍 Debug Webhook"
echo "================"
echo ""

# Step 1: Check if backend is running
echo "1️⃣  Checking backend..."
if ! curl -s "$BASE_URL" > /dev/null 2>&1; then
  echo "❌ Backend không chạy!"
  echo "   Hãy chạy: cd QuanLyResort && dotnet run"
  exit 1
fi
echo "✅ Backend đang chạy"
echo ""

# Step 2: Test webhook endpoint
echo "2️⃣  Testing webhook endpoint..."
PAYLOAD=$(cat <<EOF
{
  "content": "BOOKING-$BOOKING_ID",
  "amount": 15000,
  "transactionId": "TEST-$(date +%s)"
}
EOF
)

echo "Payload:"
echo "$PAYLOAD" | jq .
echo ""

RESPONSE=$(curl -s -w "\nHTTP_STATUS:%{http_code}" -X POST "$BASE_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD")

HTTP_STATUS=$(echo "$RESPONSE" | grep "HTTP_STATUS" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | grep -v "HTTP_STATUS")

echo "Response Status: $HTTP_STATUS"
echo "Response Body:"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"
echo ""

if [ "$HTTP_STATUS" = "200" ]; then
  echo "✅ Webhook endpoint hoạt động!"
  
  # Check if success
  if echo "$BODY" | jq -e '.success == true' > /dev/null 2>&1; then
    echo "✅ Webhook xử lý thành công!"
  else
    echo "⚠️  Webhook có response nhưng không success"
    echo "   Check message: $(echo "$BODY" | jq -r '.message // "N/A"')"
  fi
else
  echo "❌ Webhook endpoint trả về lỗi: $HTTP_STATUS"
  echo ""
  echo "Possible issues:"
  echo "  1. Endpoint không tồn tại"
  echo "  2. CORS issue"
  echo "  3. Authentication required (should not be)"
  echo "  4. Server error"
fi
echo ""

# Step 3: Check booking status (if token provided)
if [ -n "$2" ]; then
  TOKEN=$2
  echo "3️⃣  Checking booking status..."
  STATUS=$(curl -s -X GET "$BASE_URL/api/bookings/$BOOKING_ID" \
    -H "Authorization: Bearer $TOKEN" | jq -r '.status // "Unknown"')
  echo "   Booking Status: $STATUS"
  
  if [ "$STATUS" = "Paid" ]; then
    echo "✅ Booking đã được cập nhật thành Paid!"
  else
    echo "⚠️  Booking status: $STATUS (expected: Paid)"
  fi
fi
echo ""

echo "📝 Next steps:"
echo "  1. Check backend logs để xem webhook có được nhận không"
echo "  2. Nếu PayOs gửi webhook, đảm bảo webhook URL đúng"
echo "  3. Test với real payment để xem PayOs có gửi webhook không"

