#!/bin/bash
# Script để test webhook thanh toán tự động

BOOKING_ID=${1:-39}
AMOUNT=${2:-15000}

echo "🧪 Testing Webhook Payment System"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "📋 Test Parameters:"
echo "   Booking ID: $BOOKING_ID"
echo "   Amount: $AMOUNT VND"
echo "   Content: BOOKING-$BOOKING_ID"
echo ""
echo "🔍 Step 1: Check webhook status..."
curl -s http://localhost:5130/api/simplepayment/webhook-status | python3 -m json.tool
echo ""
echo ""
echo "🔍 Step 2: Get current booking status..."
curl -s "http://localhost:5130/api/bookings/$BOOKING_ID" \
  -H "Authorization: Bearer $(cat ~/.resort-token 2>/dev/null || echo '')" | \
  python3 -c "import sys, json; d=json.load(sys.stdin); print(f\"   Status: {d.get('status', 'N/A')}\"); print(f\"   Amount: {d.get('estimatedTotalAmount', 0):,} VND\")" 2>/dev/null || echo "   (Need authentication)"
echo ""
echo ""
echo "📥 Step 3: Sending webhook request..."
RESPONSE=$(curl -s -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"BOOKING-$BOOKING_ID\",
    \"amount\": $AMOUNT,
    \"transactionId\": \"TEST-$(date +%s)\"
  }")

echo "$RESPONSE" | python3 -m json.tool
echo ""
echo ""
echo "🔍 Step 4: Check booking status after webhook..."
sleep 1
curl -s "http://localhost:5130/api/bookings/$BOOKING_ID" \
  -H "Authorization: Bearer $(cat ~/.resort-token 2>/dev/null || echo '')" | \
  python3 -c "import sys, json; d=json.load(sys.stdin); print(f\"   Status: {d.get('status', 'N/A')}\"); print(f\"   Invoice: {d.get('invoice', {}).get('invoiceNumber', 'N/A')}\")" 2>/dev/null || echo "   (Need authentication)"
echo ""
echo "═══════════════════════════════════════════════════════════"
echo "✅ Test completed!"
echo ""
echo "📝 Usage:"
echo "   ./test-webhook.sh [booking_id] [amount]"
echo "   Example: ./test-webhook.sh 39 15000"

