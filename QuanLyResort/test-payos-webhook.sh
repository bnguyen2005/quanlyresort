#!/bin/bash

# Script để test PayOs webhook với format thực tế
# Dựa trên thông tin từ PayOs: "CSCOK68MZC1 BOOKING4"

echo "🧪 TEST PAYOS WEBHOOK"
echo "===================="
echo ""

# Lấy URL từ environment hoặc dùng default
WEBHOOK_URL="${WEBHOOK_URL:-https://quanlyresort.onrender.com/api/simplepayment/webhook}"
BOOKING_ID="${1:-4}"
ORDER_CODE="${2:-47571}"  # OrderCode từ PayOs response (có thể override)
DESCRIPTION_PREFIX="${3:-CSMJ4XFPZW3}"  # Prefix từ PayOs description (có thể override)

echo "📋 Booking ID: $BOOKING_ID"
echo "🔗 Webhook URL: $WEBHOOK_URL"
echo "📦 Order Code: $ORDER_CODE"
echo "📝 Description Prefix: $DESCRIPTION_PREFIX"
echo ""

# Format PayOs webhook (dựa trên PayOs API documentation)
# PayOs gửi webhook với format:
# {
#   "code": "00",
#   "desc": "success",
#   "data": {
#     "orderCode": 47571,
#     "amount": 5000,
#     "description": "CSMJ4XFPZW3 BOOKING4",
#     "accountNumber": "0901329227",
#     "reference": "REF123456",
#     "transactionDateTime": "2025-11-09T00:44:06Z",
#     "currency": "VND",
#     "paymentLinkId": "093bab572a0542d4a752e1f1bb22abd7"
#   },
#   "signature": "..."
# }

PAYOS_WEBHOOK_JSON=$(cat <<EOF
{
  "code": "00",
  "desc": "success",
  "data": {
    "orderCode": ${ORDER_CODE},
    "amount": 5000,
    "description": "${DESCRIPTION_PREFIX} BOOKING${BOOKING_ID}",
    "accountNumber": "0901329227",
    "accountName": "PHAM THANH LAM",
    "reference": "REF$(date +%s)",
    "transactionDateTime": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
    "currency": "VND",
    "paymentLinkId": "093bab572a0542d4a752e1f1bb22abd7"
  },
  "signature": "test-signature"
}
EOF
)

echo "📤 Sending PayOs webhook..."
echo "   Description: ${DESCRIPTION_PREFIX} BOOKING${BOOKING_ID}"
echo "   Order Code: ${ORDER_CODE}"
echo ""

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: PayOs-Webhook/1.0" \
  -d "$PAYOS_WEBHOOK_JSON")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

echo "📥 Response:"
echo "   HTTP Status: $HTTP_CODE"
echo "   Body: $BODY"
echo ""

if [ "$HTTP_CODE" = "200" ]; then
    echo "✅ Webhook processed successfully!"
    echo ""
    echo "🔍 Kiểm tra booking ${BOOKING_ID} trên website để xem status có đổi thành 'Paid' không"
else
    echo "❌ Webhook failed with status $HTTP_CODE"
    echo "   Check logs trên Render để xem chi tiết lỗi"
fi

echo ""
echo "💡 Tip: Xem logs trên Render để debug:"
echo "   https://dashboard.render.com -> Logs"
