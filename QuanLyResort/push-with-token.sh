#!/bin/bash

# Script để push code với token (tự động nhập credentials)
# Usage: ./push-with-token.sh

echo "🚀 PUSH CODE LÊN GITHUB VỚI TOKEN"
echo ""

cd "$(dirname "$0")/.." || exit 1

# Token mới
TOKEN="YOUR_GITHUB_PERSONAL_ACCESS_TOKEN_HERE"
USERNAME="Lamm123435469898"
REPO_URL="https://github.com/Lamm123435469898/quanlyresort.git"

echo "📊 Kiểm tra trạng thái..."
git status --short

echo ""
echo "📋 Commits sẵn sàng push:"
git log --oneline origin/main..main 2>/dev/null || git log --oneline -3

echo ""
echo "🌐 Remote hiện tại:"
git remote -v | head -1

echo ""
echo "🔄 Cấu hình remote với token..."
    git remote set-url origin "https://${USERNAME}:${TOKEN}@github.com/bnguyen2005/quanlyresortt.git"

echo ""
echo "📤 Đang push code..."
git push -u origin main

EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✅ Push thành công!"
    echo ""
    echo "🎉 Code đã lên GitHub:"
    echo "   https://github.com/Lamm123435469898/quanlyresort"
    echo ""
    echo "🔐 Đang reset remote URL (xóa token khỏi URL)..."
    git remote set-url origin "$REPO_URL"
    echo "✅ Đã reset remote URL"
    echo ""
    echo "📋 Tiếp theo: Deploy lên Render"
    echo "   Xem: QUICK-DEPLOY-RENDER.md"
else
    echo "❌ Push thất bại!"
    echo ""
    echo "💡 Nguyên nhân có thể:"
    echo "   - Repository chưa được tạo trên GitHub"
    echo "   - Token không đúng hoặc hết hạn"
    echo "   - Không có quyền truy cập repo"
    echo ""
    echo "🔍 Kiểm tra:"
    echo "   1. Repository đã tồn tại: https://github.com/Lamm123435469898/quanlyresort"
    echo "   2. Token có scope 'repo': https://github.com/settings/tokens"
    echo ""
    echo "📖 Xem hướng dẫn: HUONG-DAN-DAY-DU.md"
fi

