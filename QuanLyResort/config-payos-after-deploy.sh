#!/bin/bash

# Script để config PayOs webhook sau khi deploy lên server thật
# Usage: ./config-payos-after-deploy.sh <YOUR_DOMAIN>
# Example: ./config-payos-after-deploy.sh https://quanlyresort-api.onrender.com

echo "🔧 Config PayOs Webhook Sau Khi Deploy"
echo ""

# Nhập domain từ user
if [ -z "$1" ]; then
    echo "📋 Nhập domain của bạn (ví dụ: https://quanlyresort-api.onrender.com):"
    read DOMAIN
else
    DOMAIN="$1"
fi

# Remove trailing slash
DOMAIN=$(echo "$DOMAIN" | sed 's/\/$//')

# Construct webhook URL
WEBHOOK_URL="${DOMAIN}/api/simplepayment/webhook"

echo ""
echo "📤 Đang config PayOs webhook..."
echo "   Webhook URL: $WEBHOOK_URL"
echo ""

# Đọc config từ appsettings.json hoặc dùng giá trị mặc định
CLIENT_ID=$(grep -A 10 '"PayOs"' appsettings.json 2>/dev/null | grep '"ClientId"' | cut -d'"' -f4)
API_KEY=$(grep -A 10 '"PayOs"' appsettings.json 2>/dev/null | grep '"ApiKey"' | cut -d'"' -f4)

# Nếu không đọc được, dùng giá trị mặc định
if [ -z "$CLIENT_ID" ]; then
    CLIENT_ID="c704495b-5984-4ad3-aa23-b2794a02aa83"
    API_KEY="f6ea421b-a8b7-46b8-92be-209eb1a9b2fb"
fi

echo "   Client ID: $CLIENT_ID"
echo "   API Key: $API_KEY"
echo ""

# Gọi PayOs API
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
    echo "🎉 Bây giờ PayOs sẽ tự động gọi webhook khi thanh toán thành công!"
    echo ""
    echo "📋 Test ngay:"
    echo "   1. Mở: ${DOMAIN}/customer/my-bookings.html"
    echo "   2. Click 'Thanh toán' cho booking pending"
    echo "   3. Quét QR và thanh toán"
    echo "   4. Webhook sẽ tự động được gọi → QR tự động biến mất!"
elif [ "$HTTP_CODE" = "400" ]; then
    echo "❌ Lỗi: Webhook URL không hợp lệ"
    echo "   Kiểm tra lại URL và đảm bảo:"
    echo "   - URL phải là HTTPS"
    echo "   - URL phải accessible từ internet"
    echo "   - Endpoint phải trả về 200 OK"
    echo ""
    echo "🧪 Test endpoint:"
    echo "   curl ${WEBHOOK_URL%-webhook}/webhook-status"
elif [ "$HTTP_CODE" = "401" ]; then
    echo "❌ Lỗi: Thiếu API Key hoặc Client ID"
    echo "   Kiểm tra lại Client ID và API Key trong appsettings.json"
else
    echo "❌ Lỗi không xác định (HTTP $HTTP_CODE)"
    echo "   Response: $BODY"
fi

