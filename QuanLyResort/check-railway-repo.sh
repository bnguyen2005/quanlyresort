#!/bin/bash
# Script để kiểm tra Railway đang connect với repository nào

# Màu sắc cho output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔍 Kiểm Tra Railway Repository Connection${NC}"
echo -e "==========================================${NC}"
echo ""

# Kiểm tra git remote
echo -e "${YELLOW}📋 Git Remote Repository:${NC}"
GIT_REPO=$(git remote get-url origin 2>/dev/null | sed -E 's|.*github.com[:/]([^/]+/[^/]+)\.git.*|\1|')
if [ -n "$GIT_REPO" ]; then
    echo -e "${GREEN}✅ Git Remote: ${GIT_REPO}${NC}"
else
    echo -e "${RED}❌ Không tìm thấy git remote${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}📋 Cách Kiểm Tra Railway Repository:${NC}"
echo ""
echo -e "${BLUE}1. Railway Dashboard → Settings → Source${NC}"
echo "   - Xem 'Source Repo' field"
echo "   - Repository name sẽ hiển thị ở đó"
echo ""
echo -e "${BLUE}2. GitHub Repository → Settings → Webhooks${NC}"
echo "   - Tìm webhook có URL: https://railway.app/webhook/..."
echo "   - Xem 'Recent deliveries' để xác nhận webhook hoạt động"
echo ""
echo -e "${BLUE}3. Railway Dashboard → Deployments${NC}"
echo "   - Xem deployment mới nhất"
echo "   - Commit message sẽ cho biết repository nào"
echo ""

# Kiểm tra GitHub webhooks (nếu có GitHub CLI)
if command -v gh &> /dev/null; then
    echo -e "${YELLOW}📋 GitHub Webhooks (sử dụng GitHub CLI):${NC}"
    gh api repos/${GIT_REPO}/hooks --jq '.[] | select(.config.url | contains("railway.app")) | {id: .id, url: .config.url, active: .active}' 2>/dev/null
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Tìm thấy Railway webhook${NC}"
    else
        echo -e "${YELLOW}⚠️  Không tìm thấy Railway webhook hoặc chưa cài GitHub CLI${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  GitHub CLI chưa được cài đặt${NC}"
    echo "   Cài đặt: brew install gh"
    echo "   Hoặc kiểm tra thủ công: GitHub Repository → Settings → Webhooks"
fi

echo ""
echo -e "${YELLOW}📋 So Sánh Repository Names:${NC}"
echo -e "${GREEN}Git Remote: ${GIT_REPO}${NC}"
echo -e "${YELLOW}Railway Repo: (kiểm tra trong Railway Dashboard)${NC}"
echo ""
echo -e "${BLUE}💡 Lưu Ý:${NC}"
echo "   - Repository name phải khớp chính xác"
echo "   - Nếu không khớp → Railway sẽ không detect commit mới"
echo "   - Nếu không khớp → Disconnect và connect lại với repository đúng"
echo ""

