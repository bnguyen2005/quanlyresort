#!/bin/bash
# Script để test webhook cho booking 4
# Sử dụng để verify webhook endpoint hoạt động

RAILWAY_URL="${RAILWAY_URL:-https://quanlyresort-production.up.railway.app}"

echo "🧪 Testing webhook for Booking 4..."
echo "📍 Railway URL: $RAILWAY_URL"
echo ""

# Test 1: SePay format với BOOKING4
echo "📋 Test 1: SePay format với BOOKING4"
curl -X POST "$RAILWAY_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "in",
    "id": "TEST-'$(date +%s)'",
    "gateway": "MB",
    "accountNumber": "0901329227"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s | jq '.' || echo "Response (raw):"
echo ""
echo "---"
echo ""

# Test 2: SePay format với Description
echo "📋 Test 2: SePay format với Description (fallback)"
curl -X POST "$RAILWAY_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "in",
    "id": "TEST-'$(date +%s)'"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s | jq '.' || echo "Response (raw):"
echo ""
echo "---"
echo ""

echo "✅ Test completed!"
echo ""
echo "📝 Lưu ý:"
echo "1. Kiểm tra Railway logs để xem webhook có được nhận không"
echo "2. Kiểm tra booking status có được cập nhật thành 'Paid' không"
echo "3. Nếu webhook thành công, bạn sẽ thấy log: '[WEBHOOK] ✅ Booking status updated to Paid'"

