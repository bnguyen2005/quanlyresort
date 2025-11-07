#!/bin/bash

# Script kiểm tra booking status và test webhook
# Usage: ./check-booking-status.sh <BOOKING_ID>

BOOKING_ID=${1:-6}

echo "🔍 Kiểm Tra Booking Status"
echo "================================"
echo ""
echo "📋 Booking ID: $BOOKING_ID"
echo ""

# Check if backend is running
if ! curl -s http://localhost:5130/api/simplepayment/webhook-status > /dev/null 2>&1; then
  echo "❌ Backend không chạy hoặc không accessible"
  echo "   Hãy đảm bảo backend đang chạy trên port 5130"
  exit 1
fi

echo "✅ Backend đang chạy"
echo ""

# Check webhook status
echo "1️⃣ Kiểm tra webhook status endpoint..."
WEBHOOK_STATUS=$(curl -s http://localhost:5130/api/simplepayment/webhook-status)
echo "$WEBHOOK_STATUS" | jq '.' 2>/dev/null || echo "$WEBHOOK_STATUS"
echo ""

# Note: Can't check booking status without token, so we'll test webhook directly
echo "2️⃣ Test webhook với booking ID $BOOKING_ID..."
echo "   (Cần token để check booking status, nhưng có thể test webhook trực tiếp)"
echo ""

read -p "   Nhập số tiền (VND): " AMOUNT
AMOUNT=${AMOUNT:-10000}

echo ""
echo "   Gọi webhook..."
RESPONSE=$(curl -s -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"BOOKING-${BOOKING_ID}\",
    \"amount\": ${AMOUNT}
  }")

echo "   Response:"
echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
echo ""

# Check if success
if echo "$RESPONSE" | grep -q "success\|Thanh toán thành công"; then
  echo "✅ Webhook thành công!"
  echo ""
  echo "👀 Bây giờ kiểm tra:"
  echo "   - Backend logs có hiển thị '📥 [WEBHOOK-xxxx]' không?"
  echo "   - Frontend polling có phát hiện status = 'Paid' không? (xem Console)"
  echo "   - QR có biến mất không?"
else
  echo "⚠️  Webhook có thể không thành công"
  echo ""
  echo "Kiểm tra:"
  echo "   - Booking ID có đúng không?"
  echo "   - Booking có tồn tại không?"
  echo "   - Amount có khớp không?"
fi

echo ""
echo "✅ Hoàn tất!"

