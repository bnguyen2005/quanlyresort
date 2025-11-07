#!/bin/bash

# Script để config PayOs webhook URL qua API
# PayOs không có dashboard, phải config qua API

echo "🔧 Config PayOs Webhook URL via API"
echo ""

# Đọc config từ appsettings.json
CLIENT_ID=$(grep -A 10 '"PayOs"' appsettings.json | grep '"ClientId"' | cut -d'"' -f4)
API_KEY=$(grep -A 10 '"PayOs"' appsettings.json | grep '"ApiKey"' | cut -d'"' -f4)

# Nếu không đọc được từ file, dùng giá trị mặc định (từ code)
if [ -z "$CLIENT_ID" ]; then
    CLIENT_ID="c704495b-5984-4ad3-aa23-b2794a02aa83"
    API_KEY="f6ea421b-a8b7-46b8-92be-209eb1a9b2fb"
fi

# Nhập webhook URL từ user
if [ -z "$1" ]; then
    echo "📋 Nhập Webhook URL (ví dụ: https://abc123.ngrok.io/api/simplepayment/webhook):"
    read WEBHOOK_URL
else
    WEBHOOK_URL="$1"
fi

if [ -z "$WEBHOOK_URL" ]; then
    echo "❌ Webhook URL không được để trống!"
    exit 1
fi

echo ""
echo "📤 Đang gửi request đến PayOs API..."
echo "   Client ID: $CLIENT_ID"
echo "   API Key: $API_KEY"
echo "   Webhook URL: $WEBHOOK_URL"
echo ""

# PayOs API endpoint: https://api-merchant.payos.vn/confirm-webhook
# Method: POST
# Headers: 
#   - x-client-id: Client ID
#   - x-api-key: API Key
# Body: {"webhookUrl": "https://..."}

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: $CLIENT_ID" \
  -H "x-api-key: $API_KEY" \
  -d "{\"webhookUrl\": \"$WEBHOOK_URL\"}")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

echo "📥 Response từ PayOs:"
echo "$BODY" | jq '.' 2>/dev/null || echo "$BODY"
echo ""
echo "HTTP Status: $HTTP_CODE"
echo ""

if [ "$HTTP_CODE" = "200" ]; then
    echo "✅ Thành công! PayOs đã config webhook URL"
    echo ""
    echo "📋 Bước tiếp theo:"
    echo "   1. PayOs sẽ gửi một test webhook để verify"
    echo "   2. Kiểm tra backend logs để xem test webhook"
    echo "   3. Nếu test webhook thành công → PayOs sẽ tự động gọi webhook khi thanh toán"
    echo ""
    echo "🧪 Test webhook:"
    echo "   curl -X POST $WEBHOOK_URL \\"
    echo "     -H 'Content-Type: application/json' \\"
    echo "     -d '{\"content\": \"BOOKING-6\", \"amount\": 5000}'"
elif [ "$HTTP_CODE" = "400" ]; then
    echo "❌ Lỗi: Webhook URL không hợp lệ"
    echo "   Kiểm tra lại URL và đảm bảo URL có thể truy cập được"
elif [ "$HTTP_CODE" = "401" ]; then
    echo "❌ Lỗi: Thiếu API Key hoặc Client ID"
    echo "   Kiểm tra lại Client ID và API Key trong appsettings.json"
else
    echo "❌ Lỗi không xác định (HTTP $HTTP_CODE)"
    echo "   Response: $BODY"
fi

