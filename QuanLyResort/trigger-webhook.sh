#!/bin/bash

# Script để trigger webhook ngay sau khi thanh toán
# Sử dụng khi PayOs không gọi webhook tự động

echo "🔔 Trigger Webhook Manual"
echo ""

# Nhập booking ID và amount từ user
if [ -z "$1" ]; then
    echo "📋 Nhập Booking ID (ví dụ: 10):"
    read BOOKING_ID
else
    BOOKING_ID=$1
fi

if [ -z "$2" ]; then
    echo "📋 Nhập Amount (VND, ví dụ: 5000):"
    read AMOUNT
else
    AMOUNT=$2
fi

# Lấy ngrok URL từ ngrok API
NGROK_URL=$(curl -s http://localhost:4040/api/tunnels 2>/dev/null | grep -o '"public_url":"https://[^"]*"' | head -1 | cut -d'"' -f4)

if [ -z "$NGROK_URL" ]; then
    echo "❌ Không tìm thấy ngrok URL. Đảm bảo ngrok đang chạy!"
    echo "   Chạy: ngrok http 5130"
    exit 1
fi

WEBHOOK_URL="${NGROK_URL}/api/simplepayment/webhook"

echo ""
echo "📤 Đang gửi webhook..."
echo "   Booking ID: $BOOKING_ID"
echo "   Amount: $AMOUNT VND"
echo "   Webhook URL: $WEBHOOK_URL"
echo ""

# Gửi webhook
RESPONSE=$(curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d "{\"content\": \"BOOKING$BOOKING_ID\", \"amount\": $AMOUNT}")

echo "📥 Response:"
echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
echo ""

# Kiểm tra kết quả
if echo "$RESPONSE" | grep -q "\"success\":true"; then
    echo "✅ Webhook thành công! Booking $BOOKING_ID đã được update thành 'Paid'"
    echo ""
    echo "🔄 Frontend sẽ tự động cập nhật trong vòng 5 giây:"
    echo "   - QR code sẽ biến mất"
    echo "   - Hiện '✅ Thanh toán thành công!'"
    echo "   - Modal tự động đóng"
elif echo "$RESPONSE" | grep -q "Đã thanh toán rồi"; then
    echo "ℹ️ Booking $BOOKING_ID đã được thanh toán trước đó"
else
    echo "❌ Webhook thất bại. Kiểm tra backend logs để biết chi tiết."
fi

