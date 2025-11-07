#!/bin/bash

# Script để test webhook thủ công và kiểm tra booking status
# Usage: ./test-webhook-manual.sh <BOOKING_ID> [AMOUNT]

echo "🧪 TEST WEBHOOK THỦ CÔNG"
echo ""

if [ -z "$1" ]; then
  echo "❌ Thiếu bookingId!"
  echo ""
  echo "Usage: ./test-webhook-manual.sh <BOOKING_ID> [AMOUNT]"
  echo "Example: ./test-webhook-manual.sh 7 5000"
  exit 1
fi

BOOKING_ID=$1
AMOUNT=${2:-5000}  # Default 5000 if not provided

echo "📋 Thông tin:"
echo "   Booking ID: $BOOKING_ID"
echo "   Amount: $AMOUNT VND"
echo ""

echo "🔄 Đang gửi webhook..."
echo ""

# Test webhook
RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "https://quanlyresort.onrender.com/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"BOOKING${BOOKING_ID}\",
    \"amount\": ${AMOUNT},
    \"transactionId\": \"TEST-$(date +%s)\"
  }")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

echo "📥 Response:"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"
echo ""
echo "📊 HTTP Status: $HTTP_CODE"
echo ""

if [ "$HTTP_CODE" = "200" ]; then
  echo "✅ Webhook thành công!"
  echo ""
  echo "🎯 Tiếp theo:"
  echo "   1. Kiểm tra browser console (F12)"
  echo "   2. Tìm: '✅ [SimplePolling] Payment detected!'"
  echo "   3. QR code sẽ tự động biến mất"
  echo ""
  echo "⏳ Chờ 2-5 giây để frontend polling detect..."
else
  echo "❌ Webhook thất bại!"
  echo ""
  echo "💡 Kiểm tra:"
  echo "   1. Booking ID có đúng không?"
  echo "   2. Booking có tồn tại không?"
  echo "   3. Amount có khớp không?"
fi

