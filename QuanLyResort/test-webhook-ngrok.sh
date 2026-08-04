#!/bin/bash

# Script test webhook qua ngrok
# Usage: ./test-webhook-ngrok.sh <NGROK_URL> <BOOKING_ID> <AMOUNT>

NGROK_URL=${1:-"https://abc123.ngrok.io"}
BOOKING_ID=${2:-6}
AMOUNT=${3:-5000}

echo "🧪 Test Webhook Qua Ngrok"
echo "================================"
echo ""
echo "📋 Thông tin:"
echo "   Ngrok URL: $NGROK_URL"
echo "   Booking ID: $BOOKING_ID"
echo "   Amount: $AMOUNT VND"
echo ""

# Check if ngrok URL is provided
if [ "$NGROK_URL" = "https://abc123.ngrok.io" ]; then
  echo "⚠️  Cảnh báo: Đang dùng URL mẫu!"
  echo "   Hãy thay bằng URL ngrok thực tế của bạn"
  echo ""
  echo "   Cách lấy URL:"
  echo "   1. Chạy: ngrok http 5130"
  echo "   2. Copy URL từ output (ví dụ: https://abc123.ngrok.io)"
  echo "   3. Chạy lại script với URL đó"
  echo ""
  read -p "   Bạn có muốn tiếp tục với URL mẫu không? (y/n): " -n 1 -r
  echo ""
  if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Đã hủy"
    exit 1
  fi
fi

echo "🔍 Kiểm tra backend đang chạy..."
if ! curl -s http://localhost:5130/api/simplepayment/webhook-status > /dev/null 2>&1; then
  echo "❌ Backend không chạy hoặc không accessible"
  echo "   Hãy đảm bảo backend đang chạy trên port 5130"
  exit 1
fi
echo "✅ Backend đang chạy"
echo ""

echo "🔍 Kiểm tra webhook status endpoint..."
STATUS_RESPONSE=$(curl -s http://localhost:5130/api/simplepayment/webhook-status)
echo "$STATUS_RESPONSE" | jq '.' 2>/dev/null || echo "$STATUS_RESPONSE"
echo ""

echo "🚀 Gọi webhook qua ngrok..."
echo "   URL: $NGROK_URL/api/simplepayment/webhook"
echo ""

RESPONSE=$(curl -s -X POST "$NGROK_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"BOOKING-${BOOKING_ID}\",
    \"amount\": ${AMOUNT}
  }")

echo "📥 Response:"
echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
echo ""

# Check if success
if echo "$RESPONSE" | grep -q "success\|Thanh toán thành công"; then
  echo "✅ Webhook thành công!"
  echo ""
  echo "👀 Bây giờ kiểm tra:"
  echo "   1. Backend logs có hiển thị '📥 [WEBHOOK-xxxx]' không?"
  echo "   2. Frontend polling có phát hiện status = 'Paid' không?"
  echo "   3. QR có biến mất không?"
  echo ""
  echo "📊 Ngrok Dashboard:"
  echo "   Mở browser → http://localhost:4040"
  echo "   Sẽ thấy request đến /api/simplepayment/webhook"
else
  echo "⚠️  Webhook có thể không thành công"
  echo ""
  echo "Kiểm tra:"
  echo "   - Ngrok URL có đúng không?"
  echo "   - Ngrok có đang chạy không?"
  echo "   - Backend có đang chạy không?"
  echo "   - Webhook endpoint có accessible không?"
fi

echo ""
echo "✅ Hoàn tất!"

