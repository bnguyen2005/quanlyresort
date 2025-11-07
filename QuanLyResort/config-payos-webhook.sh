#!/bin/bash

# Script để cấu hình PayOs webhook sau khi deploy lên Render
# Usage: ./config-payos-webhook.sh

echo "🔧 CẤU HÌNH PAYOS WEBHOOK"
echo ""

# PayOs credentials
CLIENT_ID="c704495b-5984-4ad3-aa23-b2794a02aa83"
API_KEY="f6ea421b-a8b7-46b8-92be-209eb1a9b2fb"

# Webhook URL (Render domain)
WEBHOOK_URL="https://quanlyresort.onrender.com/api/simplepayment/webhook"

echo "📋 Thông tin:"
echo "   Client ID: $CLIENT_ID"
echo "   Webhook URL: $WEBHOOK_URL"
echo ""

echo "🔄 Đang cấu hình webhook..."
echo ""

# Call PayOs API to configure webhook
RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "https://api.payos.vn/v2/webhook-url" \
  -H "Content-Type: application/json" \
  -H "x-client-id: $CLIENT_ID" \
  -H "x-api-key: $API_KEY" \
  -d "{
    \"webhookUrl\": \"$WEBHOOK_URL\"
  }")

# Extract HTTP status code (last line)
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
# Extract response body (all lines except last)
BODY=$(echo "$RESPONSE" | sed '$d')

echo "📥 Response:"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"
echo ""
echo "📊 HTTP Status: $HTTP_CODE"
echo ""

if [ "$HTTP_CODE" = "200" ]; then
  echo "✅ Webhook đã được cấu hình thành công!"
  echo ""
  echo "🎯 Tiếp theo:"
  echo "   1. Tạo booking mới"
  echo "   2. Click 'Thanh toán'"
  echo "   3. Quét QR code và thanh toán"
  echo "   4. Kiểm tra logs trên Render"
  echo "   5. QR code sẽ tự động biến mất"
else
  echo "❌ Cấu hình webhook thất bại!"
  echo ""
  echo "💡 Nguyên nhân có thể:"
  echo "   - URL không hợp lệ"
  echo "   - PayOs chưa verify domain"
  echo "   - Credentials không đúng"
  echo ""
  echo "🔍 Kiểm tra:"
  echo "   1. Webhook URL có thể truy cập: curl $WEBHOOK_URL"
  echo "   2. PayOs credentials có đúng không"
  echo "   3. Xem logs trên Render để debug"
fi
