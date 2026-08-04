#!/bin/bash
# Script để update git remote trỏ đến repository chính

# Màu sắc cho output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔧 Update Git Remote Repository${NC}"
echo -e "===================================${NC}"
echo ""

# Repository chính
MAIN_REPO="Lamm123435469898/quanlyresort"
MAIN_REPO_URL="https://github.com/${MAIN_REPO}.git"

# Token (nếu cần)
TOKEN="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"
MAIN_REPO_URL_WITH_TOKEN="https://${TOKEN}@github.com/${MAIN_REPO}.git"

echo -e "${YELLOW}📋 Thông Tin Repository:${NC}"
echo -e "${GREEN}Repository chính: ${MAIN_REPO}${NC}"
echo -e "${YELLOW}Repository cũ (sai): quanlyresortt (2 chữ 't')${NC}"
echo ""

# Kiểm tra git remote hiện tại
CURRENT_REMOTE=$(git remote get-url origin 2>/dev/null)
echo -e "${YELLOW}📋 Git Remote Hiện Tại:${NC}"
echo -e "${CURRENT_REMOTE}"
echo ""

# Kiểm tra xem có phải repository sai không
if [[ "$CURRENT_REMOTE" == *"quanlyresortt"* ]]; then
    echo -e "${RED}❌ Git remote đang trỏ đến repository sai (quanlyresortt)${NC}"
    echo -e "${YELLOW}⚠️  Cần update để trỏ đến repository chính (quanlyresort)${NC}"
    echo ""
    
    # Xác nhận
    read -p "Bạn có muốn update git remote? (y/n): " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${YELLOW}🔄 Đang update git remote...${NC}"
        
        # Xóa remote cũ
        git remote remove origin
        
        # Thêm remote mới với repository chính
        git remote add origin "$MAIN_REPO_URL_WITH_TOKEN"
        
        echo -e "${GREEN}✅ Đã update git remote${NC}"
        echo ""
        
        # Verify
        echo -e "${YELLOW}📋 Git Remote Mới:${NC}"
        git remote -v
        echo ""
        
        # Kiểm tra branch
        CURRENT_BRANCH=$(git branch --show-current)
        echo -e "${YELLOW}📋 Branch Hiện Tại: ${CURRENT_BRANCH}${NC}"
        echo ""
        
        # Hỏi có muốn push không
        read -p "Bạn có muốn push code lên repository chính? (y/n): " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo -e "${YELLOW}📤 Đang push code lên repository chính...${NC}"
            git push -u origin "$CURRENT_BRANCH"
            
            if [ $? -eq 0 ]; then
                echo -e "${GREEN}✅ Đã push code lên repository chính${NC}"
                echo ""
                echo -e "${BLUE}💡 Bước Tiếp Theo:${NC}"
                echo "1. Kiểm tra Railway Dashboard → Deployments"
                echo "2. Xem có deployment mới không"
                echo "3. Railway sẽ tự động detect và deploy"
            else
                echo -e "${RED}❌ Lỗi khi push code${NC}"
                echo "Kiểm tra lại repository và quyền truy cập"
            fi
        else
            echo -e "${YELLOW}⚠️  Chưa push code. Bạn có thể push sau bằng:${NC}"
            echo "   git push -u origin $CURRENT_BRANCH"
        fi
    else
        echo -e "${YELLOW}⚠️  Chưa update git remote${NC}"
    fi
else
    echo -e "${GREEN}✅ Git remote đã trỏ đến repository chính${NC}"
    echo -e "${BLUE}💡 Không cần update${NC}"
fi

echo ""
echo -e "${BLUE}📋 Tóm Tắt:${NC}"
echo -e "${GREEN}Repository chính: ${MAIN_REPO}${NC}"
echo -e "${YELLOW}Railway đang connect: ${MAIN_REPO} ✅${NC}"
echo -e "${YELLOW}Git remote: (kiểm tra bằng: git remote -v)${NC}"

