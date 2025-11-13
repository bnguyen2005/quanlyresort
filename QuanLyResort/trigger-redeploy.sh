#!/bin/bash
# Script để trigger redeploy trên Railway bằng cách push empty commit

echo "🔄 Triggering Railway redeploy..."
echo ""

# Kiểm tra git status
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "⚠️  Có thay đổi chưa commit. Đang commit..."
    git add -A
    git commit -m "chore: Trigger redeploy"
fi

# Push empty commit để trigger redeploy
echo "📤 Pushing empty commit to trigger Railway redeploy..."
git commit --allow-empty -m "trigger: Force Railway redeploy - $(date +%Y%m%d-%H%M%S)"
git push origin main

echo ""
echo "✅ Đã push commit. Railway sẽ tự động detect và deploy."
echo ""
echo "📋 Các bước tiếp theo:"
echo "1. Vào Railway Dashboard: https://railway.app"
echo "2. Chọn service 'quanlyresort'"
echo "3. Tab 'Deployments' - Xem deployment mới"
echo "4. Tab 'Logs' - Xem logs deployment"
echo ""
echo "⏳ Đợi 2-3 phút để Railway deploy xong..."

