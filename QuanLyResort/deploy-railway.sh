#!/bin/bash
# Script để deploy code lên Railway
# Cách 1: Push commit mới để trigger auto deploy
# Cách 2: Dùng Railway CLI (nếu có)

set -e

echo "🚀 Railway Deploy Script"
echo "========================"
echo ""

# Màu sắc
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Kiểm tra git
if ! command -v git &> /dev/null; then
    echo -e "${RED}❌ Git không được cài đặt${NC}"
    exit 1
fi

# Kiểm tra đang ở đúng directory
# Cho phép chạy từ root hoặc từ QuanLyResort directory
if [ -f "QuanLyResort/QuanLyResort.csproj" ]; then
    # Đang ở root directory
    ROOT_DIR="."
elif [ -f "QuanLyResort.csproj" ]; then
    # Đang ở QuanLyResort directory
    ROOT_DIR=".."
else
    echo -e "${RED}❌ Không tìm thấy QuanLyResort.csproj. Đảm bảo đang ở root hoặc QuanLyResort directory.${NC}"
    exit 1
fi

# Kiểm tra git status
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo -e "${RED}❌ Không phải git repository${NC}"
    exit 1
fi

echo -e "${YELLOW}📋 Kiểm tra git status...${NC}"
git fetch origin main 2>/dev/null || true

# Kiểm tra có thay đổi chưa commit không
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo -e "${YELLOW}⚠️  Có thay đổi chưa commit. Đang commit...${NC}"
    git add -A
    git commit -m "chore: Auto commit before deploy - $(date +%Y%m%d-%H%M%S)"
fi

# Kiểm tra commit mới nhất
LATEST_COMMIT=$(git log -1 --oneline)
echo -e "${GREEN}✅ Commit mới nhất: ${LATEST_COMMIT}${NC}"

# Kiểm tra Railway CLI
if command -v railway &> /dev/null; then
    echo ""
    echo -e "${YELLOW}🔍 Tìm thấy Railway CLI${NC}"
    echo -e "${YELLOW}Chọn phương thức deploy:${NC}"
    echo "1. Push commit mới (trigger auto deploy)"
    echo "2. Dùng Railway CLI deploy"
    echo ""
    read -p "Chọn (1 hoặc 2, mặc định 1): " choice
    choice=${choice:-1}
    
    if [ "$choice" = "2" ]; then
        echo ""
        echo -e "${YELLOW}🚀 Deploy bằng Railway CLI...${NC}"
        railway up --detach
        echo -e "${GREEN}✅ Đã trigger deploy bằng Railway CLI${NC}"
        exit 0
    fi
fi

# Cách 1: Push empty commit để trigger deploy
echo ""
echo -e "${YELLOW}🚀 Trigger deploy bằng cách push empty commit...${NC}"

# Tạo empty commit
git commit --allow-empty -m "trigger: Force Railway deploy - $(date +%Y%m%d-%H%M%S)" || {
    echo -e "${RED}❌ Lỗi khi tạo commit${NC}"
    exit 1
}

# Push lên GitHub
echo -e "${YELLOW}📤 Pushing to GitHub...${NC}"
git push origin main || {
    echo -e "${RED}❌ Lỗi khi push lên GitHub${NC}"
    exit 1
}

echo ""
echo -e "${GREEN}✅ Đã push commit. Railway sẽ tự động detect và deploy.${NC}"
echo ""
echo -e "${YELLOW}📋 Các bước tiếp theo:${NC}"
echo "1. Vào Railway Dashboard: https://railway.app"
echo "2. Chọn service 'quanlyresort'"
echo "3. Tab 'Deployments' - Xem deployment mới"
echo "4. Tab 'Logs' - Xem logs deployment"
echo ""
echo -e "${YELLOW}⏳ Đợi 2-3 phút để Railway deploy xong...${NC}"
echo ""
echo -e "${GREEN}🧪 Sau khi deploy xong, test SePay webhook:${NC}"
echo "curl -X POST 'https://quanlyresort-production.up.railway.app/api/simplepayment/webhook' \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"description\": \"BOOKING4\", \"transferAmount\": 5000, \"transferType\": \"IN\"}'"
echo ""

