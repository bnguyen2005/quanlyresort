#!/bin/bash

# Script test webhook backend
BASE_URL="http://localhost:5130"

echo "🧪 Testing Webhook Backend"
echo "=========================="
echo ""

# Test 1: PayOs Webhook
echo "1️⃣  Testing PayOs Webhook..."
TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
TRANSACTION_ID="TXN$(date +%s)"
BOOKING_ID=${1:-39}
AMOUNT=15000

PAYOS_PAYLOAD=$(cat <<EOF
{
  "code": 0,
  "desc": "Success",
  "id": "PAYOS-$(date +%s)",
  "data": {
    "transactionId": "$TRANSACTION_ID",
    "amount": $AMOUNT,
    "description": "BOOKING-$BOOKING_ID",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "transactionDateTime": "$TIMESTAMP",
    "reference": "$TRANSACTION_ID",
    "status": "PAID"
  },
  "signature": "test-signature-disabled"
}
EOF
)

echo "Payload: $PAYOS_PAYLOAD"
echo ""

RESPONSE=$(curl -s -X POST "$BASE_URL/api/payment/payos-webhook" \
  -H "Content-Type: application/json" \
  -d "$PAYOS_PAYLOAD")

echo "Response: $RESPONSE"
echo ""

# Test 2: Bank Webhook (Generic)
echo "2️⃣  Testing Generic Bank Webhook..."
BANK_PAYLOAD=$(cat <<EOF
{
  "bankName": "MB",
  "transactionId": "$TRANSACTION_ID",
  "amount": $AMOUNT,
  "content": "BOOKING-$BOOKING_ID",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "transactionDate": "$TIMESTAMP",
  "signature": null
}
EOF
)

echo "Payload: $BANK_PAYLOAD"
echo ""

RESPONSE2=$(curl -s -X POST "$BASE_URL/api/payment/bank-webhook" \
  -H "Content-Type: application/json" \
  -d "$BANK_PAYLOAD")

echo "Response: $RESPONSE2"
echo ""

# Test 3: VietQR Webhook
echo "3️⃣  Testing VietQR Webhook..."
VIETQR_PAYLOAD=$(cat <<EOF
{
  "transactionId": "$TRANSACTION_ID",
  "vietQRTransactionId": "VQR-$(date +%s)",
  "amount": $AMOUNT,
  "content": "BOOKING-$BOOKING_ID",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankCode": "MB",
  "bankName": "MBBank",
  "transactionDate": "$TIMESTAMP",
  "signature": "test-signature-disabled",
  "status": "SUCCESS"
}
EOF
)

echo "Payload: $VIETQR_PAYLOAD"
echo ""

RESPONSE3=$(curl -s -X POST "$BASE_URL/api/payment/vietqr-webhook" \
  -H "Content-Type: application/json" \
  -d "$VIETQR_PAYLOAD")

echo "Response: $RESPONSE3"
echo ""

echo "✅ Webhook test completed!"
echo ""
echo "📝 Check backend logs for:"
echo "   - 'Processing webhook from...'"
echo "   - 'Successfully processed payment for booking...'"
echo "   - 'Broadcasted via SignalR...'"

