#!/bin/bash

# Script để kiểm tra webhook URL có hoạt động không

WEBHOOK_URL="https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"

echo "==================================================="
echo "🧪 Kiểm Tra SePay Webhook URL"
echo "==================================================="
echo "Webhook URL: $WEBHOOK_URL"
echo ""

# Test 1: Kiểm tra endpoint có accessible không (GET request)
echo "📋 Test 1: Kiểm tra endpoint có accessible không (GET request)"
echo "---------------------------------------------------"
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$WEBHOOK_URL")
echo "HTTP Status Code: $HTTP_STATUS"

if [ "$HTTP_STATUS" == "201" ] || [ "$HTTP_STATUS" == "200" ]; then
    echo "✅ Endpoint accessible (GET request)"
else
    echo "⚠️ Endpoint trả về status $HTTP_STATUS (có thể bình thường vì GET không phải method chính)"
fi
echo ""

# Test 2: Test POST request với empty body (verification request)
echo "📋 Test 2: Test POST request với empty body (verification request)"
echo "---------------------------------------------------"
RESPONSE=$(curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook-Test/1.0" \
  -d '{}' \
  -w "\nHTTP_STATUS:%{http_code}")

HTTP_STATUS=$(echo "$RESPONSE" | grep "HTTP_STATUS" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_STATUS/d')

echo "HTTP Status Code: $HTTP_STATUS"
echo "Response Body: $BODY"

if [ "$HTTP_STATUS" == "201" ] || [ "$HTTP_STATUS" == "200" ]; then
    if echo "$BODY" | grep -q "success.*true"; then
        echo "✅ Webhook endpoint hoạt động đúng!"
        echo "✅ Response có success: true"
        echo "✅ HTTP Status Code: $HTTP_STATUS"
    else
        echo "⚠️ Endpoint trả về $HTTP_STATUS nhưng response không có success: true"
    fi
else
    echo "❌ Endpoint trả về status $HTTP_STATUS (không đúng)"
fi
echo ""

# Test 3: Test POST request với SePay webhook format
echo "📋 Test 3: Test POST request với SePay webhook format (BOOKING4)"
echo "---------------------------------------------------"
WEBHOOK_PAYLOAD='{
  "id": 92704,
  "gateway": "MB",
  "transactionDate": "2023-03-25 14:02:37",
  "accountNumber": "0901329227",
  "code": null,
  "content": "BOOKING4",
  "transferType": "in",
  "transferAmount": 5000,
  "accumulated": 19077000,
  "subAccount": null,
  "referenceCode": "MBMB.3278907687",
  "description": ""
}'

RESPONSE=$(curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook/1.0" \
  -d "$WEBHOOK_PAYLOAD" \
  -w "\nHTTP_STATUS:%{http_code}")

HTTP_STATUS=$(echo "$RESPONSE" | grep "HTTP_STATUS" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_STATUS/d')

echo "HTTP Status Code: $HTTP_STATUS"
echo "Response Body: $BODY"

if [ "$HTTP_STATUS" == "201" ] || [ "$HTTP_STATUS" == "200" ]; then
    if echo "$BODY" | grep -q "success.*true"; then
        echo "✅ Webhook endpoint xử lý SePay format đúng!"
        echo "✅ Response có success: true"
        echo "✅ HTTP Status Code: $HTTP_STATUS"
    else
        echo "⚠️ Endpoint trả về $HTTP_STATUS nhưng response không có success: true"
    fi
else
    echo "❌ Endpoint trả về status $HTTP_STATUS (không đúng)"
fi
echo ""

# Test 4: Kiểm tra endpoint có trả về đúng format không
echo "📋 Test 4: Kiểm tra response format"
echo "---------------------------------------------------"
RESPONSE=$(curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook-Test/1.0" \
  -d '{}')

if echo "$RESPONSE" | grep -q "success"; then
    echo "✅ Response có field 'success'"
else
    echo "❌ Response không có field 'success'"
fi

if echo "$RESPONSE" | grep -q "true"; then
    echo "✅ Response có giá trị 'true'"
else
    echo "⚠️ Response không có giá trị 'true'"
fi

echo ""
echo "==================================================="
echo "📊 Tóm Tắt"
echo "==================================================="
echo "Webhook URL: $WEBHOOK_URL"
echo ""
echo "✅ Nếu tất cả tests đều pass:"
echo "   → Webhook endpoint hoạt động đúng"
echo "   → SePay có thể gửi webhook đến URL này"
echo ""
echo "❌ Nếu có test fail:"
echo "   → Kiểm tra Railway logs để xem lỗi"
echo "   → Kiểm tra code đã được deploy chưa"
echo "   → Kiểm tra endpoint có đúng route không"
echo ""
echo "🔗 Links:"
echo "   - Railway Dashboard: https://railway.app"
echo "   - Railway Logs: Railway Dashboard → Service → Logs"
echo "   - SePay Dashboard: https://my.sepay.vn/webhooks"
echo "==================================================="

