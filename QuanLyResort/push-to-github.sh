#!/bin/bash

# Script để push code lên GitHub
# Usage: ./push-to-github.sh

echo "🚀 PUSH CODE LÊN GITHUB"
echo ""

cd "$(dirname "$0")/.." || exit 1

# Kiểm tra git status
echo "📊 Kiểm tra trạng thái..."
git status --short

echo ""
echo "📋 Commits sẵn sàng push:"
git log --oneline origin/main..main 2>/dev/null || git log --oneline -3

echo ""
echo "🌐 Remote:"
git remote -v | head -1

echo ""
echo "⚠️  LƯU Ý:"
echo "   Bạn cần Personal Access Token (PAT) để push"
echo ""
echo "📝 Nếu chưa có PAT:"
echo "   1. Vào: https://github.com/settings/tokens"
echo "   2. Generate new token (classic)"
echo "   3. Chọn scope: repo (full control)"
echo "   4. Copy token"
echo ""

read -p "Nhấn Enter để tiếp tục push (hoặc Ctrl+C để hủy)..."

echo ""
echo "🔄 Đang push..."
git push -u origin main

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Push thành công!"
    echo ""
    echo "🎉 Code đã lên GitHub:"
    echo "   https://github.com/Lamm123435469898/quanlyresort"
    echo ""
    echo "📋 Tiếp theo: Deploy lên Render"
    echo "   Xem: QUICK-DEPLOY-RENDER.md"
else
    echo ""
    echo "❌ Push thất bại!"
    echo ""
    echo "💡 Nguyên nhân có thể:"
    echo "   - Chưa có PAT token"
    echo "   - PAT token không đúng"
    echo "   - Không có quyền truy cập repo"
    echo ""
    echo "📖 Xem hướng dẫn: PUSH-CODE-TO-GITHUB.md"
fi

