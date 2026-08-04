#!/bin/bash

# Script test flow thanh toán tự động
# Tập trung vào chức năng chính: QR → Thanh toán → Webhook → Ẩn QR

echo "💰 TEST FLOW THANH TOÁN TỰ ĐỘNG"
echo ""

# Nhập booking ID
if [ -z "$1" ]; then
    echo "📋 Nhập Booking ID (ví dụ: 7):"
    read BOOKING_ID
else
    BOOKING_ID="$1"
fi

if [ -z "$BOOKING_ID" ]; then
    echo "❌ Booking ID không được để trống!"
    exit 1
fi

# Nhập amount
if [ -z "$2" ]; then
    echo "📋 Nhập số tiền (ví dụ: 10000):"
    read AMOUNT
else
    AMOUNT="$2"
fi

if [ -z "$AMOUNT" ]; then
    AMOUNT="10000"
fi

echo ""
echo "🧪 TEST WEBHOOK"
echo "   Booking ID: $BOOKING_ID"
echo "   Amount: $AMOUNT VND"
echo "   Content: BOOKING$BOOKING_ID"
echo ""

# Test webhook
echo "📤 Gửi webhook..."
RESPONSE=$(curl -s -X POST "http://localhost:5130/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "{\"content\": \"BOOKING$BOOKING_ID\", \"amount\": $AMOUNT}")

echo "$RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$RESPONSE"
echo ""

# Kiểm tra kết quả
if echo "$RESPONSE" | grep -q "success.*true\|Thanh toán thành công"; then
    echo "✅ Webhook thành công!"
    echo ""
    echo "📋 KIỂM TRA:"
    echo "   1. Backend logs → Sẽ thấy webhook processed"
    echo "   2. Frontend → QR sẽ tự động biến mất trong 5 giây"
    echo "   3. Booking status → 'Paid'"
    echo ""
    echo "⏰ Chờ 5 giây để frontend polling detect..."
    sleep 5
    echo ""
    echo "✅ Nếu QR không biến mất, kiểm tra:"
    echo "   - Frontend console (F12) → Xem logs polling"
    echo "   - Backend logs → Xem webhook có được xử lý không"
else
    echo "❌ Webhook thất bại!"
    echo ""
    echo "🔍 Kiểm tra:"
    echo "   - Backend có đang chạy không?"
    echo "   - Booking ID có đúng không?"
    echo "   - Booking status có phải 'Pending' không?"
fi

