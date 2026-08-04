#!/bin/bash

# Script test webhook thanh toán
# Usage: ./test-payment-webhook.sh [bookingId] [webhookType]

BASE_URL="http://localhost:5130"
BOOKING_ID=${1:-39}
WEBHOOK_TYPE=${2:-"payos"} # payos, vietqr, mbbank

echo "🧪 Testing Payment Webhook"
echo "=========================="
echo "Base URL: $BASE_URL"
echo "Booking ID: $BOOKING_ID"
echo "Webhook Type: $WEBHOOK_TYPE"
echo ""

TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
TRANSACTION_ID="TXN$(date +%s)"
AMOUNT=15000

case $WEBHOOK_TYPE in
  "payos")
    echo "📤 Testing PayOs Webhook..."
    PAYLOAD=$(cat <<EOF
{
  "code": 0,
  "desc": "Success",
  "data": {
    "orderCode": $(date +%s),
    "amount": $AMOUNT,
    "description": "BOOKING-$BOOKING_ID",
    "accountNumber": "0901329227",
    "reference": "$TRANSACTION_ID",
    "transactionDateTime": "$TIMESTAMP",
    "currency": "VND",
    "paymentLinkId": "test-link-$(date +%s)",
    "code": 0,
    "desc": "Success"
  },
  "signature": "test-signature"
}
EOF
)
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/payos-webhook" \
      -H "Content-Type: application/json" \
      -d "$PAYLOAD")
    ;;
    
  "vietqr")
    echo "📤 Testing VietQR Webhook..."
    PAYLOAD=$(cat <<EOF
{
  "transactionId": "$TRANSACTION_ID",
  "amount": $AMOUNT,
  "content": "BOOKING-$BOOKING_ID",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "transactionDate": "$TIMESTAMP",
  "signature": "test-signature"
}
EOF
)
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/vietqr-webhook" \
      -H "Content-Type: application/json" \
      -d "$PAYLOAD")
    ;;
    
  "mbbank")
    echo "📤 Testing MB Bank Webhook..."
    PAYLOAD=$(cat <<EOF
{
  "transactionId": "$TRANSACTION_ID",
  "mbTransactionId": "MB$(date +%s)",
  "amount": $AMOUNT,
  "content": "BOOKING-$BOOKING_ID",
  "accountNumber": "0901329227",
  "transactionDate": "$TIMESTAMP",
  "signature": "test-signature"
}
EOF
)
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/mbbank-webhook" \
      -H "Content-Type: application/json" \
      -d "$PAYLOAD")
    ;;
    
  *)
    echo "❌ Unknown webhook type: $WEBHOOK_TYPE"
    echo "Supported types: payos, vietqr, mbbank"
    exit 1
    ;;
esac

echo "Response: $RESPONSE"
echo ""

# Kiểm tra response
if echo "$RESPONSE" | grep -q "success\|Success"; then
    echo "✅ Webhook processed successfully!"
    echo ""
    echo "📝 Next Steps:"
    echo "   - Kiểm tra booking status đã chuyển sang 'Paid' chưa"
    echo "   - Kiểm tra payment modal trong browser có cập nhật không"
    echo "   - Kiểm tra WebSocket/SignalR có broadcast message không"
else
    echo "❌ Webhook processing failed"
    echo "Response: $RESPONSE"
fi

