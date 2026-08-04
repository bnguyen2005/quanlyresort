#!/bin/bash

# Script test PayOs webhook với dữ liệu mẫu
# Sử dụng dữ liệu mẫu từ PayOs API documentation

# Màu sắc cho output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Webhook URL
WEBHOOK_URL="https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"

echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}🧪 TEST PAYOS WEBHOOK VỚI DỮ LIỆU MẪU${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Test 1: Dữ liệu mẫu từ PayOs API documentation
echo -e "${CYAN}📋 Test 1: Dữ liệu mẫu từ PayOs API documentation${NC}"
echo "   Description: VQRIO123"
echo ""

PAYLOAD1=$(cat <<EOF
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 123,
    "amount": 3000,
    "description": "VQRIO123",
    "accountNumber": "12345678",
    "reference": "TF230204212323",
    "transactionDateTime": "2023-02-04 18:25:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb18",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "",
    "counterAccountBankName": "",
    "counterAccountName": "",
    "counterAccountNumber": "",
    "virtualAccountName": "",
    "virtualAccountNumber": ""
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
EOF
)

RESPONSE1=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD1" \
    2>&1)

HTTP_CODE1=$(echo "$RESPONSE1" | grep "HTTP_CODE:" | cut -d: -f2)
BODY1=$(echo "$RESPONSE1" | sed '/HTTP_CODE:/d')

echo -e "${YELLOW}📥 Response:${NC}"
echo "   HTTP Code: $HTTP_CODE1"
echo "   Body: $BODY1"
echo ""

if [ "$HTTP_CODE1" == "200" ]; then
    echo -e "${GREEN}   ✅ Test 1 thành công!${NC}"
else
    echo -e "${RED}   ❌ Test 1 thất bại (HTTP $HTTP_CODE1)${NC}"
fi
echo ""

# Test 2: Dữ liệu với description = "BOOKING4"
echo -e "${CYAN}📋 Test 2: Dữ liệu với description = BOOKING4${NC}"
echo "   Description: BOOKING4"
echo ""

PAYLOAD2=$(cat <<EOF
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 40043,
    "amount": 5000,
    "description": "BOOKING4",
    "accountNumber": "0901329227",
    "reference": "TF230204212323",
    "transactionDateTime": "2025-11-13 18:25:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb18",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "",
    "counterAccountBankName": "",
    "counterAccountName": "",
    "counterAccountNumber": "",
    "virtualAccountName": "",
    "virtualAccountNumber": ""
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
EOF
)

RESPONSE2=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD2" \
    2>&1)

HTTP_CODE2=$(echo "$RESPONSE2" | grep "HTTP_CODE:" | cut -d: -f2)
BODY2=$(echo "$RESPONSE2" | sed '/HTTP_CODE:/d')

echo -e "${YELLOW}📥 Response:${NC}"
echo "   HTTP Code: $HTTP_CODE2"
echo "   Body: $BODY2"
echo ""

if [ "$HTTP_CODE2" == "200" ]; then
    echo -e "${GREEN}   ✅ Test 2 thành công!${NC}"
    
    # Kiểm tra xem có extract được booking ID không
    if echo "$BODY2" | grep -q "bookingId.*4"; then
        echo -e "${GREEN}   ✅ Đã extract được booking ID = 4${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Không thấy booking ID = 4 trong response${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 2 thất bại (HTTP $HTTP_CODE2)${NC}"
fi
echo ""

# Test 3: Dữ liệu với description = "ORDER7" (restaurant order)
echo -e "${CYAN}📋 Test 3: Dữ liệu với description = ORDER7 (restaurant order)${NC}"
echo "   Description: ORDER7"
echo ""

PAYLOAD3=$(cat <<EOF
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 20000007,
    "amount": 150000,
    "description": "ORDER7",
    "accountNumber": "0901329227",
    "reference": "TF230204212324",
    "transactionDateTime": "2025-11-13 18:30:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb19",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "",
    "counterAccountBankName": "",
    "counterAccountName": "",
    "counterAccountNumber": "",
    "virtualAccountName": "",
    "virtualAccountNumber": ""
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
EOF
)

RESPONSE3=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD3" \
    2>&1)

HTTP_CODE3=$(echo "$RESPONSE3" | grep "HTTP_CODE:" | cut -d: -f2)
BODY3=$(echo "$RESPONSE3" | sed '/HTTP_CODE:/d')

echo -e "${YELLOW}📥 Response:${NC}"
echo "   HTTP Code: $HTTP_CODE3"
echo "   Body: $BODY3"
echo ""

if [ "$HTTP_CODE3" == "200" ]; then
    echo -e "${GREEN}   ✅ Test 3 thành công!${NC}"
    
    # Kiểm tra xem có extract được order ID không
    if echo "$BODY3" | grep -q "orderId.*7\|orderNumber"; then
        echo -e "${GREEN}   ✅ Đã extract được restaurant order ID = 7${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Không thấy restaurant order ID = 7 trong response${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 3 thất bại (HTTP $HTTP_CODE3)${NC}"
fi
echo ""

# Test 4: Dữ liệu với code != "00" (lỗi)
echo -e "${CYAN}📋 Test 4: Dữ liệu với code != 00 (lỗi)${NC}"
echo "   Code: 01 (lỗi)"
echo ""

PAYLOAD4=$(cat <<EOF
{
  "code": "01",
  "desc": "Payment failed",
  "success": false,
  "data": {
    "orderCode": 123,
    "amount": 3000,
    "description": "BOOKING4",
    "accountNumber": "12345678",
    "reference": "TF230204212323",
    "transactionDateTime": "2023-02-04 18:25:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb18",
    "code": "01",
    "desc": "Thanh toán thất bại"
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
EOF
)

RESPONSE4=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD4" \
    2>&1)

HTTP_CODE4=$(echo "$RESPONSE4" | grep "HTTP_CODE:" | cut -d: -f2)
BODY4=$(echo "$RESPONSE4" | sed '/HTTP_CODE:/d')

echo -e "${YELLOW}📥 Response:${NC}"
echo "   HTTP Code: $HTTP_CODE4"
echo "   Body: $BODY4"
echo ""

if [ "$HTTP_CODE4" == "200" ]; then
    echo -e "${GREEN}   ✅ Test 4 thành công! (Webhook xử lý lỗi đúng)${NC}"
    
    # Kiểm tra xem có message về payment failed không
    if echo "$BODY4" | grep -qi "failed\|lỗi\|error"; then
        echo -e "${GREEN}   ✅ Đã xử lý lỗi đúng${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 4 thất bại (HTTP $HTTP_CODE4)${NC}"
fi
echo ""

# Test 5: Empty body (verification request)
echo -e "${CYAN}📋 Test 5: Empty body (PayOs verification request)${NC}"
echo "   Body: (empty)"
echo ""

RESPONSE5=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST "$WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "" \
    2>&1)

HTTP_CODE5=$(echo "$RESPONSE5" | grep "HTTP_CODE:" | cut -d: -f2)
BODY5=$(echo "$RESPONSE5" | sed '/HTTP_CODE:/d')

echo -e "${YELLOW}📥 Response:${NC}"
echo "   HTTP Code: $HTTP_CODE5"
echo "   Body: $BODY5"
echo ""

if [ "$HTTP_CODE5" == "200" ]; then
    echo -e "${GREEN}   ✅ Test 5 thành công! (Verification request được xử lý)${NC}"
    
    # Kiểm tra xem có status = "active" không
    if echo "$BODY5" | grep -q "active\|ready"; then
        echo -e "${GREEN}   ✅ Endpoint trả về status active${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 5 thất bại (HTTP $HTTP_CODE5)${NC}"
fi
echo ""

# Tổng kết
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}📊 TỔNG KẾT${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

PASSED=0
FAILED=0

[ "$HTTP_CODE1" == "200" ] && PASSED=$((PASSED+1)) || FAILED=$((FAILED+1))
[ "$HTTP_CODE2" == "200" ] && PASSED=$((PASSED+1)) || FAILED=$((FAILED+1))
[ "$HTTP_CODE3" == "200" ] && PASSED=$((PASSED+1)) || FAILED=$((FAILED+1))
[ "$HTTP_CODE4" == "200" ] && PASSED=$((PASSED+1)) || FAILED=$((FAILED+1))
[ "$HTTP_CODE5" == "200" ] && PASSED=$((PASSED+1)) || FAILED=$((FAILED+1))

echo -e "${GREEN}✅ Passed: $PASSED/5${NC}"
echo -e "${RED}❌ Failed: $FAILED/5${NC}"
echo ""

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}🎉 Tất cả tests đều thành công!${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠️  Một số tests thất bại. Kiểm tra lại webhook endpoint.${NC}"
    exit 1
fi
