#!/bin/bash
# Script để test SePay webhook trên Railway production

# Màu sắc cho output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Railway production URL
WEBHOOK_URL="https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"

echo -e "${BLUE}🧪 Test SePay Webhook trên Railway Production${NC}"
echo -e "==============================================${NC}"
echo ""
echo -e "${YELLOW}Webhook URL: ${WEBHOOK_URL}${NC}"
echo ""

# Test 1: Empty body (verification request)
echo -e "${BLUE}Test 1: Empty Body (Verification Request)${NC}"
echo -e "${YELLOW}Expected: 200 OK với status='active'${NC}"
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '')
HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE/d')

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ Test 1 PASSED - HTTP $HTTP_CODE${NC}"
    echo -e "${GREEN}Response: $BODY${NC}"
else
    echo -e "${RED}❌ Test 1 FAILED - HTTP $HTTP_CODE${NC}"
    echo -e "${RED}Response: $BODY${NC}"
fi
echo ""

# Test 2: SePay format với Description và TransferAmount
echo -e "${BLUE}Test 2: SePay Format - Description + TransferAmount${NC}"
echo -e "${YELLOW}Expected: 200 OK, booking ID được extract${NC}"
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 150000,
    "transferType": "IN",
    "id": "TXN123456",
    "referenceCode": "REF123456"
  }')
HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE/d')

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ Test 2 PASSED - HTTP $HTTP_CODE${NC}"
    echo -e "${GREEN}Response: $BODY${NC}"
    if echo "$BODY" | grep -q "BOOKING4\|booking.*4"; then
        echo -e "${GREEN}✅ Booking ID được extract thành công${NC}"
    fi
else
    echo -e "${RED}❌ Test 2 FAILED - HTTP $HTTP_CODE${NC}"
    echo -e "${RED}Response: $BODY${NC}"
fi
echo ""

# Test 3: SePay format với Content và Amount
echo -e "${BLUE}Test 3: SePay Format - Content + Amount${NC}"
echo -e "${YELLOW}Expected: 200 OK, booking ID được extract${NC}"
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING5",
    "amount": 200000,
    "transactionId": "TXN789012"
  }')
HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE/d')

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ Test 3 PASSED - HTTP $HTTP_CODE${NC}"
    echo -e "${GREEN}Response: $BODY${NC}"
    if echo "$BODY" | grep -q "BOOKING5\|booking.*5"; then
        echo -e "${GREEN}✅ Booking ID được extract thành công${NC}"
    fi
else
    echo -e "${RED}❌ Test 3 FAILED - HTTP $HTTP_CODE${NC}"
    echo -e "${RED}Response: $BODY${NC}"
fi
echo ""

# Test 4: SePay format với camelCase (transferAmount)
echo -e "${BLUE}Test 4: SePay Format - camelCase (transferAmount)${NC}"
echo -e "${YELLOW}Expected: 200 OK, TransferAmount được extract${NC}"
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING6",
    "transferAmount": 300000,
    "transferType": "IN"
  }')
HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE/d')

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ Test 4 PASSED - HTTP $HTTP_CODE${NC}"
    echo -e "${GREEN}Response: $BODY${NC}"
    if echo "$BODY" | grep -q "300000\|300,000"; then
        echo -e "${GREEN}✅ TransferAmount được extract thành công${NC}"
    fi
else
    echo -e "${RED}❌ Test 4 FAILED - HTTP $HTTP_CODE${NC}"
    echo -e "${RED}Response: $BODY${NC}"
fi
echo ""

# Test 5: Restaurant Order format
echo -e "${BLUE}Test 5: Restaurant Order Format (ORDER7)${NC}"
echo -e "${YELLOW}Expected: 200 OK, restaurant order ID được extract${NC}"
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "ORDER7",
    "transferAmount": 50000,
    "transferType": "IN"
  }')
HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE/d')

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ Test 5 PASSED - HTTP $HTTP_CODE${NC}"
    echo -e "${GREEN}Response: $BODY${NC}"
    if echo "$BODY" | grep -q "ORDER7\|order.*7"; then
        echo -e "${GREEN}✅ Restaurant order ID được extract thành công${NC}"
    fi
else
    echo -e "${RED}❌ Test 5 FAILED - HTTP $HTTP_CODE${NC}"
    echo -e "${RED}Response: $BODY${NC}"
fi
echo ""

# Summary
echo -e "${BLUE}📋 Tóm Tắt Kết Quả:${NC}"
echo -e "==================${NC}"
echo ""
echo -e "${YELLOW}💡 Lưu Ý:${NC}"
echo "1. Kiểm tra Railway logs để xem chi tiết webhook processing"
echo "2. Railway Dashboard → Logs → Tìm '[WEBHOOK]'"
echo "3. Xem có thấy booking ID được extract không"
echo ""
echo -e "${YELLOW}🔍 Kiểm Tra Logs:${NC}"
echo "Railway Dashboard → Service → Logs"
echo "Tìm: [WEBHOOK] hoặc 'SePay' hoặc 'Simple/SePay format'"
echo ""
echo -e "${YELLOW}📋 Next Steps:${NC}"
echo "1. Setup SePay webhook trong dashboard: https://my.sepay.vn/webhooks"
echo "2. URL: $WEBHOOK_URL"
echo "3. Test với giao dịch thật từ SePay"
echo "4. Kiểm tra Railway logs sau khi có giao dịch"
echo ""

