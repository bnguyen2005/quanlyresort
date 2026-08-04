#!/bin/bash

# Script tự động verify PayOs webhook URL
# Sử dụng PayOs API confirm-webhook endpoint

# Màu sắc cho output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# PayOs Configuration
CLIENT_ID="90ad103f-aa49-4c33-9692-76d739a68b1b"
API_KEY="acb138f1-a0f0-4a1f-9692-16d54332a580"
WEBHOOK_URL="https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"
PAYOS_API_URL="https://api-merchant.payos.vn/confirm-webhook"

echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🔍 PAYOS WEBHOOK URL VERIFICATION${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Hiển thị thông tin config
echo -e "${YELLOW}📋 Configuration:${NC}"
echo "   Client ID: ${CLIENT_ID:0:20}..."
echo "   API Key: ${API_KEY:0:20}..."
echo "   Webhook URL: $WEBHOOK_URL"
echo "   PayOs API: $PAYOS_API_URL"
echo ""

# Bước 1: Kiểm tra webhook endpoint trước
echo -e "${YELLOW}🔍 Bước 1: Kiểm tra webhook endpoint...${NC}"
WEBHOOK_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$WEBHOOK_URL" 2>&1)
WEBHOOK_HTTP_CODE=$(echo "$WEBHOOK_RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
WEBHOOK_BODY=$(echo "$WEBHOOK_RESPONSE" | sed '/HTTP_CODE:/d')

if [ "$WEBHOOK_HTTP_CODE" == "200" ]; then
    echo -e "${GREEN}   ✅ Webhook endpoint hoạt động (HTTP $WEBHOOK_HTTP_CODE)${NC}"
    echo "   Response: $WEBHOOK_BODY"
else
    echo -e "${RED}   ❌ Webhook endpoint không hoạt động (HTTP $WEBHOOK_HTTP_CODE)${NC}"
    echo "   Response: $WEBHOOK_BODY"
    echo -e "${YELLOW}   ⚠️  Không thể verify nếu endpoint không hoạt động${NC}"
    exit 1
fi
echo ""

# Bước 2: Gọi PayOs API để verify webhook URL
echo -e "${YELLOW}🔄 Bước 2: Gọi PayOs API để verify webhook URL...${NC}"
echo "   Đang gửi request đến PayOs..."

# Tạo request body
REQUEST_BODY=$(cat <<EOF
{
  "webhookUrl": "$WEBHOOK_URL"
}
EOF
)

# Gọi PayOs API
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$PAYOS_API_URL" \
    -H "Content-Type: application/json" \
    -H "x-client-id: $CLIENT_ID" \
    -H "x-api-key: $API_KEY" \
    -d "$REQUEST_BODY" \
    2>&1)

HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE:/d')

echo ""
echo -e "${BLUE}📥 Response từ PayOs API:${NC}"
echo "   HTTP Code: $HTTP_CODE"
echo "   Body: $BODY"
echo ""

# Parse response
if [ "$HTTP_CODE" == "200" ]; then
    # Kiểm tra code trong response
    CODE=$(echo "$BODY" | grep -o '"code"[[:space:]]*:[[:space:]]*[0-9]*' | grep -o '[0-9]*' | head -1)
    DESC=$(echo "$BODY" | grep -o '"desc"[[:space:]]*:[[:space:]]*"[^"]*"' | cut -d'"' -f4)
    
    if [ "$CODE" == "0" ] || [ "$CODE" == "00" ]; then
        echo -e "${GREEN}═══════════════════════════════════════════════════════════${NC}"
        echo -e "${GREEN}✅ THÀNH CÔNG! Webhook URL đã được verify${NC}"
        echo -e "${GREEN}═══════════════════════════════════════════════════════════${NC}"
        echo ""
        echo "   Code: $CODE"
        echo "   Desc: $DESC"
        echo "   Webhook URL: $WEBHOOK_URL"
        echo ""
        echo -e "${GREEN}🎉 PayOs đã chấp nhận webhook URL!${NC}"
        echo "   Bây giờ PayOs sẽ gửi webhook khi có thanh toán thành công."
        exit 0
    else
        echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
        echo -e "${YELLOW}⚠️  PayOs trả về lỗi${NC}"
        echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
        echo ""
        echo "   Code: $CODE"
        echo "   Desc: $DESC"
        echo ""
        
        # Phân tích lỗi
        if [ "$CODE" == "20" ] || [ "$DESC" == *"invalid"* ] || [ "$DESC" == *"không hợp lệ"* ]; then
            echo -e "${YELLOW}💡 Có thể PayOs chưa verify được Railway domain${NC}"
            echo "   - Đợi 10-15 phút và thử lại"
            echo "   - Hoặc liên hệ PayOs support"
        elif [ "$CODE" == "01" ] || [ "$DESC" == *"unauthorized"* ]; then
            echo -e "${RED}💡 Lỗi xác thực${NC}"
            echo "   - Kiểm tra Client ID và API Key"
        else
            echo -e "${YELLOW}💡 Lỗi không xác định${NC}"
            echo "   - Kiểm tra lại response từ PayOs"
        fi
        exit 1
    fi
else
    # Parse response để lấy code và desc
    CODE=$(echo "$BODY" | grep -o '"code"[[:space:]]*:[[:space:]]*"[0-9]*"' | grep -o '[0-9]*' | head -1)
    DESC=$(echo "$BODY" | grep -o '"desc"[[:space:]]*:[[:space:]]*"[^"]*"' | cut -d'"' -f4)
    DATA=$(echo "$BODY" | grep -o '"data"[[:space:]]*:[[:space:]]*"[^"]*"' | cut -d'"' -f4)
    
    if [ "$HTTP_CODE" == "400" ] && [ "$CODE" == "20" ]; then
        echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
        echo -e "${YELLOW}⚠️  PayOs không verify được Railway webhook URL${NC}"
        echo -e "${YELLOW}═══════════════════════════════════════════════════════════${NC}"
        echo ""
        echo "   Code: $CODE"
        echo "   Desc: $DESC"
        if [ ! -z "$DATA" ]; then
            echo "   Data: $DATA"
        fi
        echo ""
        echo -e "${YELLOW}💡 Phân tích:${NC}"
        echo "   - Webhook endpoint hoạt động bình thường (HTTP 200)"
        echo "   - PayOs không thể verify được Railway domain"
        echo "   - Có thể do PayOs firewall/network không cho phép truy cập Railway"
        echo ""
        echo -e "${YELLOW}🔧 Giải pháp:${NC}"
        echo "   1. Đợi 10-15 phút và thử lại script"
        echo "   2. Liên hệ PayOs support: support@payos.vn"
        echo "   3. Tạm thời dùng Render URL nếu cần"
        echo ""
        echo -e "${BLUE}📧 Email mẫu cho PayOs support:${NC}"
        echo "   Tiêu đề: Vấn đề verify webhook URL với Railway domain"
        echo "   Nội dung:"
        echo "   - Webhook URL: $WEBHOOK_URL"
        echo "   - Lỗi: Code 20 - Webhook url invalid"
        echo "   - Test endpoint: Đã test và trả về HTTP 200 OK"
        echo "   - Yêu cầu: Hỗ trợ verify webhook URL với Railway domain"
        exit 1
    else
        echo -e "${RED}═══════════════════════════════════════════════════════════${NC}"
        echo -e "${RED}❌ LỖI HTTP: $HTTP_CODE${NC}"
        echo -e "${RED}═══════════════════════════════════════════════════════════${NC}"
        echo ""
        echo "   Response: $BODY"
        echo ""
        
        if [ "$HTTP_CODE" == "401" ]; then
            echo -e "${RED}💡 Lỗi xác thực (401 Unauthorized)${NC}"
            echo "   - Kiểm tra Client ID và API Key"
        elif [ "$HTTP_CODE" == "404" ]; then
            echo -e "${YELLOW}💡 Endpoint không tìm thấy (404)${NC}"
            echo "   - Kiểm tra PayOs API URL"
        elif [ "$HTTP_CODE" == "500" ]; then
            echo -e "${YELLOW}💡 Lỗi server PayOs (500)${NC}"
            echo "   - Thử lại sau vài phút"
        fi
        exit 1
    fi
fi

