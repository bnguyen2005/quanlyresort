#!/bin/bash

# Script test SePay webhook cho booking 5
# Sử dụng để debug khi QR code không ẩn sau khi thanh toán

echo "🧪 Test SePay Webhook cho Booking 5"
echo "=================================="
echo ""

WEBHOOK_URL="https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"

echo "📤 Gửi webhook test với format SePay..."
echo ""

# Test 1: Format SePay chuẩn
echo "Test 1: Format SePay chuẩn (description + transferAmount)"
curl -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook-Test/1.0" \
  -d '{
    "description": "BOOKING5",
    "transferAmount": 5000,
    "transferType": "IN",
    "id": "TXN-TEST-001",
    "referenceCode": "REF-TEST-001"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=================================="
echo ""

# Test 2: Format với content thay vì description
echo "Test 2: Format với content field"
curl -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook-Test/1.0" \
  -d '{
    "content": "BOOKING5",
    "amount": 5000,
    "transferType": "IN"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=================================="
echo ""

# Test 3: Format đầy đủ
echo "Test 3: Format đầy đủ (tất cả fields)"
curl -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook-Test/1.0" \
  -d '{
    "description": "BOOKING5",
    "content": "BOOKING5",
    "transferAmount": 5000,
    "amount": 5000,
    "transferType": "IN",
    "id": "TXN-TEST-003",
    "referenceCode": "REF-TEST-003",
    "accountNumber": "0901329227",
    "bankCode": "MB"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
echo "=================================="
echo ""
echo "✅ Test hoàn tất!"
echo ""
echo "📋 Kiểm tra Railway logs để xem:"
echo "   - [WEBHOOK] 📥 Webhook received"
echo "   - [WEBHOOK] 📋 Detected Simple/SePay format"
echo "   - [WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 5"
echo "   - [WEBHOOK] ✅ Booking 5 updated to Paid successfully!"
echo ""
echo "🔗 Railway Logs: https://railway.app → Service → Logs"

