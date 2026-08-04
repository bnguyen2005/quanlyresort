#!/bin/bash

# Script để kiểm tra trạng thái deploy trên Render
# Usage: ./check-deploy-status.sh [RENDER_URL]

RENDER_URL=${1:-"https://quanlyresort-api.onrender.com"}

echo "🔍 KIỂM TRA TRẠNG THÁI DEPLOY"
echo ""

echo "📡 Testing: $RENDER_URL"
echo ""

# Test webhook status endpoint
echo "1️⃣  Test Webhook Status Endpoint:"
STATUS_RESPONSE=$(curl -s -w "\n%{http_code}" "$RENDER_URL/api/simplepayment/webhook-status" 2>&1)
HTTP_CODE=$(echo "$STATUS_RESPONSE" | tail -n1)
BODY=$(echo "$STATUS_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ Service đang chạy (HTTP 200)"
    echo "   Response: $BODY"
elif [ "$HTTP_CODE" = "503" ]; then
    echo "   ⚠️  Service đang sleep hoặc đang deploy (HTTP 503)"
    echo "   → Đợi thêm vài phút rồi thử lại"
elif [ "$HTTP_CODE" = "000" ] || [ -z "$HTTP_CODE" ]; then
    echo "   ❌ Không thể kết nối (Timeout hoặc Service chưa sẵn sàng)"
    echo "   → Service có thể đang deploy hoặc chưa start"
else
    echo "   ⚠️  HTTP $HTTP_CODE"
    echo "   Response: $BODY"
fi

echo ""

# Test root endpoint
echo "2️⃣  Test Root Endpoint:"
ROOT_RESPONSE=$(curl -s -w "\n%{http_code}" "$RENDER_URL/" 2>&1)
ROOT_CODE=$(echo "$ROOT_RESPONSE" | tail -n1)

if [ "$ROOT_CODE" = "200" ] || [ "$ROOT_CODE" = "404" ]; then
    echo "   ✅ Service đang chạy (HTTP $ROOT_CODE)"
elif [ "$ROOT_CODE" = "503" ]; then
    echo "   ⚠️  Service đang sleep (HTTP 503)"
else
    echo "   ⚠️  HTTP $ROOT_CODE"
fi

echo ""

# Summary
echo "📊 TÓM TẮT:"
if [ "$HTTP_CODE" = "200" ] || [ "$ROOT_CODE" = "200" ]; then
    echo "   ✅ DEPLOY THÀNH CÔNG!"
    echo "   → Service đang chạy bình thường"
    echo ""
    echo "🎯 Tiếp theo:"
    echo "   ./config-payos-after-deploy.sh $RENDER_URL"
elif [ "$HTTP_CODE" = "503" ] || [ "$ROOT_CODE" = "503" ]; then
    echo "   ⏳ ĐANG DEPLOY HOẶC ĐANG SLEEP"
    echo "   → Đợi thêm 2-3 phút rồi chạy lại script này"
    echo ""
    echo "   Hoặc kiểm tra trên Render Dashboard:"
    echo "   https://dashboard.render.com"
else
    echo "   ❓ TRẠNG THÁI KHÔNG RÕ"
    echo "   → Kiểm tra logs trên Render Dashboard"
    echo "   → Đảm bảo Environment Variables đã được cập nhật"
fi

