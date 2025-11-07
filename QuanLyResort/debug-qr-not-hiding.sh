#!/bin/bash

# Script debug QR không biến mất sau thanh toán
# Usage: ./debug-qr-not-hiding.sh <BOOKING_ID>

BOOKING_ID=${1:-4}

echo "🔍 Debug QR Không Biến Mất"
echo "================================"
echo ""
echo "📋 Booking ID: $BOOKING_ID"
echo ""

# Step 1: Check booking status
echo "1️⃣ Kiểm tra booking status hiện tại..."
echo "   curl http://localhost:5130/api/bookings/$BOOKING_ID"
echo ""

# Step 2: Check webhook logs
echo "2️⃣ Kiểm tra webhook logs trong backend..."
echo "   Xem terminal backend hoặc logs để tìm:"
echo "   📥 [WEBHOOK-xxxx] Webhook received"
echo ""

# Step 3: Manual trigger webhook
echo "3️⃣ Test manual webhook (mô phỏng thanh toán)..."
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

# Step 4: Check booking status again
echo "4️⃣ Kiểm tra booking status sau webhook..."
echo "   Đợi 2 giây..."
sleep 2

echo ""
echo "✅ Kiểm tra xong!"
echo ""
echo "📝 Checklist:"
echo "   [ ] Backend có nhận webhook không? (xem logs)"
echo "   [ ] Booking status đã thành 'Paid' chưa?"
echo "   [ ] Frontend polling có chạy không? (mở Console F12)"
echo "   [ ] Console có log '[SimplePolling] Booking status: Paid' không?"
echo "   [ ] Console có log '[showPaymentSuccess]' không?"
echo ""
echo "🔧 Nếu vẫn không hoạt động:"
echo "   1. Mở Console (F12) và kiểm tra logs"
echo "   2. Kiểm tra booking status: GET /api/bookings/$BOOKING_ID"
echo "   3. Kiểm tra webhook endpoint: POST /api/simplepayment/webhook"
echo "   4. Kiểm tra modal có đúng ID 'simplePaymentModal' không"
echo "   5. Kiểm tra elements có đúng ID không (spQRImage, spSuccess, etc.)"

