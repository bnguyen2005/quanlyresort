#!/bin/bash

# Script test QR tự động biến mất sau thanh toán
# Usage: ./test-qr-auto-hide.sh <BOOKING_ID> <AMOUNT>

BOOKING_ID=${1:-4}
AMOUNT=${2:-10000}

echo "🧪 Test QR Tự Động Biến Mất"
echo "================================"
echo ""
echo "📋 Thông tin test:"
echo "   Booking ID: $BOOKING_ID"
echo "   Amount: $AMOUNT VND"
echo ""
echo "📝 Hướng dẫn:"
echo "   1. Mở browser, đăng nhập và vào trang 'Đặt phòng của tôi'"
echo "   2. Click nút 'Thanh toán' cho booking ID $BOOKING_ID"
echo "   3. Mở Console (F12) để xem logs"
echo "   4. Chạy script này để mô phỏng thanh toán"
echo "   5. Quan sát QR code tự động biến mất và hiển thị 'Thanh toán thành công!'"
echo ""
echo "⏳ Đếm ngược 5 giây để bạn chuẩn bị..."
sleep 5

echo ""
echo "🚀 Gọi webhook để mô phỏng thanh toán..."
echo ""

RESPONSE=$(curl -s -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"BOOKING-${BOOKING_ID}\",
    \"amount\": ${AMOUNT}
  }")

echo "📥 Response từ webhook:"
echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
echo ""

# Kiểm tra response
if echo "$RESPONSE" | grep -q "success\|Đã thanh toán\|Cập nhật thành công"; then
  echo "✅ Webhook thành công!"
  echo ""
  echo "👀 Bây giờ hãy quan sát browser:"
  echo "   - QR code sẽ biến mất trong vòng 5 giây"
  echo "   - Thông báo 'Thanh toán thành công!' sẽ hiện ra"
  echo "   - Modal sẽ tự động đóng sau 2 giây"
  echo ""
  echo "📊 Kiểm tra booking status:"
  echo "   curl http://localhost:5130/api/bookings/$BOOKING_ID -H 'Authorization: Bearer YOUR_TOKEN'"
else
  echo "⚠️  Webhook có thể không thành công"
  echo "   Kiểm tra lại:"
  echo "   - Booking ID có đúng không?"
  echo "   - Backend có đang chạy không?"
  echo "   - Amount có khớp không?"
fi

echo ""
echo "✅ Test hoàn tất!"

