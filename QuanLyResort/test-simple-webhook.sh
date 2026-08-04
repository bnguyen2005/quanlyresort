#!/bin/bash

# Test webhook đơn giản
BASE_URL="http://localhost:5130"
BOOKING_ID=${1:-39}

echo "🧪 Testing Simple Payment Webhook"
echo "=================================="
echo ""

echo "Booking ID: $BOOKING_ID"
echo ""

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

echo "Sending webhook..."
RESPONSE=$(curl -s -X POST "$BASE_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD")

echo "Response:"
echo "$RESPONSE" | jq .
echo ""

if echo "$RESPONSE" | jq -e '.success == true' > /dev/null 2>&1; then
  echo "✅ Webhook thành công!"
  echo ""
  echo "📝 Kiểm tra booking status:"
  echo "   curl -X GET \"$BASE_URL/api/bookings/$BOOKING_ID\" -H \"Authorization: Bearer \$TOKEN\""
else
  echo "❌ Webhook thất bại"
  echo ""
  echo "📝 Kiểm tra logs backend để xem lỗi"
fi

