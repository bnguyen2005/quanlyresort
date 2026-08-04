#!/bin/bash
# Quick test script cho thanh toán tự động

echo "🧪 Quick Test - Thanh Toán Tự Động"
echo "═══════════════════════════════════════════════════════════"
echo ""

# Step 1: Check webhook status
echo "📋 Step 1: Kiểm tra webhook status..."
STATUS_RESPONSE=$(curl -s http://localhost:5130/api/simplepayment/webhook-status)
if echo "$STATUS_RESPONSE" | grep -q "active"; then
    echo "✅ Webhook system: ACTIVE"
else
    echo "❌ Webhook system: NOT ACTIVE"
    echo "   Response: $STATUS_RESPONSE"
    exit 1
fi
echo ""

# Step 2: Get booking ID
BOOKING_ID=${1:-4}
AMOUNT=${2:-10000}

echo "📋 Step 2: Test với Booking ID: $BOOKING_ID, Amount: $AMOUNT VND"
echo ""

# Step 3: Check booking exists
echo "📋 Step 3: Kiểm tra booking tồn tại..."
BOOKING_RESPONSE=$(curl -s "http://localhost:5130/api/bookings/$BOOKING_ID" 2>/dev/null)
if echo "$BOOKING_RESPONSE" | grep -q "BookingCode\|bookingId"; then
    BOOKING_STATUS=$(echo "$BOOKING_RESPONSE" | python3 -c "import sys, json; d=json.load(sys.stdin); print(d.get('status', 'N/A'))" 2>/dev/null || echo "N/A")
    BOOKING_CODE=$(echo "$BOOKING_RESPONSE" | python3 -c "import sys, json; d=json.load(sys.stdin); print(d.get('bookingCode', 'N/A'))" 2>/dev/null || echo "N/A")
    echo "✅ Booking found: $BOOKING_CODE - Status: $BOOKING_STATUS"
    
    if [ "$BOOKING_STATUS" = "Paid" ]; then
        echo "⚠️  Booking đã được thanh toán rồi!"
        echo "   Chọn booking khác hoặc reset booking status"
        exit 1
    fi
else
    echo "❌ Booking $BOOKING_ID không tồn tại"
    exit 1
fi
echo ""

# Step 4: Send webhook
echo "📋 Step 4: Gửi webhook (simulate payment)..."
TRANSACTION_ID="TEST-$(date +%s)"
WEBHOOK_RESPONSE=$(curl -s -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"BOOKING-$BOOKING_ID\",
    \"amount\": $AMOUNT,
    \"transactionId\": \"$TRANSACTION_ID\"
  }")

echo "$WEBHOOK_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$WEBHOOK_RESPONSE"
echo ""

# Step 5: Check result
if echo "$WEBHOOK_RESPONSE" | grep -q "success.*true"; then
    echo "✅ Webhook xử lý thành công!"
    echo ""
    echo "📋 Step 5: Kiểm tra booking status sau 2 giây..."
    sleep 2
    
    BOOKING_AFTER=$(curl -s "http://localhost:5130/api/bookings/$BOOKING_ID" 2>/dev/null)
    STATUS_AFTER=$(echo "$BOOKING_AFTER" | python3 -c "import sys, json; d=json.load(sys.stdin); print(d.get('status', 'N/A'))" 2>/dev/null || echo "N/A")
    
    if [ "$STATUS_AFTER" = "Paid" ]; then
        echo "✅ Booking status đã được update: Paid"
        echo ""
        echo "🎉 TEST THÀNH CÔNG!"
        echo ""
        echo "📝 Kiểm tra trong browser:"
        echo "   - Mở my-bookings.html"
        echo "   - Xem console logs (F12)"
        echo "   - Polling sẽ phát hiện status = Paid"
        echo "   - UI sẽ tự động update (QR biến mất, success hiện)"
    else
        echo "⚠️  Booking status: $STATUS_AFTER (chưa được update)"
    fi
else
    echo "❌ Webhook xử lý thất bại"
    echo "   Response: $WEBHOOK_RESPONSE"
fi

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "📝 Usage: ./quick-test-payment.sh [booking_id] [amount]"
echo "   Example: ./quick-test-payment.sh 4 10000"

