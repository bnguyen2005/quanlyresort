#!/bin/bash

# Script test SePay webhook với dữ liệu mẫu
# Dựa trên SePay documentation và format tương tự các payment gateway khác

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
echo -e "${BLUE}🧪 TEST SEPAY WEBHOOK VỚI DỮ LIỆU MẪU${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Test 1: Format có thể của SePay (dựa trên các payment gateway khác)
echo -e "${CYAN}📋 Test 1: Format SePay có thể (với id, referenceCode, transferAmount)${NC}"
echo "   Description: BOOKING4"
echo ""

PAYLOAD1=$(cat <<EOF
{
  "id": "sepay-$(date +%s)",
  "referenceCode": "REF-$(date +%s)",
  "transferType": "IN",
  "transferAmount": 5000,
  "content": "BOOKING4",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "transactionDate": "2025-11-13T12:30:00Z",
  "description": "BOOKING4"
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
    
    # Kiểm tra xem có extract được booking ID không
    if echo "$BODY1" | grep -q "bookingId.*4\|message.*thanh toán"; then
        echo -e "${GREEN}   ✅ Đã xử lý webhook thành công${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Webhook được nhận nhưng chưa thấy booking ID = 4${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 1 thất bại (HTTP $HTTP_CODE1)${NC}"
fi
echo ""

# Test 2: Format Simple (content, amount)
echo -e "${CYAN}📋 Test 2: Format Simple (content, amount)${NC}"
echo "   Content: BOOKING4, Amount: 5000"
echo ""

PAYLOAD2=$(cat <<EOF
{
  "content": "BOOKING4",
  "amount": 5000,
  "transactionId": "SEPAY-$(date +%s)",
  "accountNumber": "0901329227",
  "transactionDate": "2025-11-13T12:30:00Z"
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
    
    if echo "$BODY2" | grep -q "bookingId.*4"; then
        echo -e "${GREEN}   ✅ Đã extract được booking ID = 4${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Không thấy booking ID = 4 trong response${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 2 thất bại (HTTP $HTTP_CODE2)${NC}"
fi
echo ""

# Test 3: Format với description (tương tự PayOs)
echo -e "${CYAN}📋 Test 3: Format với description (tương tự PayOs)${NC}"
echo "   Description: BOOKING4"
echo ""

PAYLOAD3=$(cat <<EOF
{
  "id": "sepay-$(date +%s)",
  "referenceCode": "REF-$(date +%s)",
  "transferType": "IN",
  "transferAmount": 5000,
  "description": "BOOKING4",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "transactionDate": "2025-11-13T12:30:00Z"
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
    
    if echo "$BODY3" | grep -q "bookingId.*4"; then
        echo -e "${GREEN}   ✅ Đã extract được booking ID = 4${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Không thấy booking ID = 4 trong response${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 3 thất bại (HTTP $HTTP_CODE3)${NC}"
fi
echo ""

# Test 4: Restaurant Order (ORDER7)
echo -e "${CYAN}📋 Test 4: Restaurant Order (ORDER7)${NC}"
echo "   Description: ORDER7"
echo ""

PAYLOAD4=$(cat <<EOF
{
  "id": "sepay-$(date +%s)",
  "referenceCode": "REF-$(date +%s)",
  "transferType": "IN",
  "transferAmount": 150000,
  "description": "ORDER7",
  "content": "ORDER7",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "transactionDate": "2025-11-13T12:35:00Z"
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
    echo -e "${GREEN}   ✅ Test 4 thành công!${NC}"
    
    if echo "$BODY4" | grep -q "orderId.*7\|orderNumber"; then
        echo -e "${GREEN}   ✅ Đã extract được restaurant order ID = 7${NC}"
    else
        echo -e "${YELLOW}   ⚠️  Không thấy restaurant order ID = 7 trong response${NC}"
    fi
else
    echo -e "${RED}   ❌ Test 4 thất bại (HTTP $HTTP_CODE4)${NC}"
fi
echo ""

# Test 5: Empty body (verification request)
echo -e "${CYAN}📋 Test 5: Empty body (SePay verification request)${NC}"
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
    echo ""
    echo -e "${YELLOW}💡 Lưu ý:${NC}"
    echo "   - Các test này dùng format dự đoán của SePay"
    echo "   - Cần xem SePay documentation để biết format chính xác"
    echo "   - Sau khi setup SePay webhook, test với giao dịch thật"
    exit 0
else
    echo -e "${YELLOW}⚠️  Một số tests thất bại.${NC}"
    echo ""
    echo -e "${YELLOW}💡 Lưu ý:${NC}"
    echo "   - Format webhook của SePay có thể khác"
    echo "   - Cần xem SePay documentation: https://docs.sepay.vn"
    echo "   - Cần xem SePay webhook logs để biết format thực tế"
    exit 1
fi

