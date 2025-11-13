# 🚀 Hướng Dẫn Setup Auto Deploy trên Railway

## 📺 Tham Khảo Video

Video hướng dẫn: https://www.youtube.com/watch?v=_dZXZSmmw2g

## ✅ Các Bước Setup Auto Deploy

### Bước 1: Connect GitHub Repository

**Railway Dashboard → New Project → Deploy from GitHub repo**

1. Chọn repository: `Lamm123435469898/quanlyresortt`
2. Chọn branch: `main`
3. Railway sẽ tự động connect và tạo webhook

### Bước 2: Kiểm Tra Auto Deploy Settings

**Railway Dashboard → Service → Settings → Source**

**Kiểm tra:**
- ✅ **Source Repo:** `Lamm123435469898/quanlyresortt`
- ✅ **Branch:** `main`
- ✅ **Auto Deploy:** Enabled (quan trọng!)

**Nếu Auto Deploy chưa được bật:**
- Enable nó
- Railway sẽ tự động deploy khi có commit mới

### Bước 3: Kiểm Tra GitHub Webhook

**GitHub Repository → Settings → Webhooks**

**Tìm Railway webhook:**
- URL: `https://railway.app/webhook/...`
- Events: `push`, `deployment`, etc.
- Recent deliveries: Không có lỗi

**Nếu không thấy webhook:**
- Railway sẽ tự động tạo khi connect repository
- Hoặc tạo thủ công trong Railway Settings

### Bước 4: Test Auto Deploy

**Push commit mới:**
```bash
git commit --allow-empty -m "test: Auto deploy"
git push origin main
```

**Railway sẽ tự động:**
1. Detect commit mới
2. Trigger deployment
3. Build và deploy service

## 🔍 Kiểm Tra Auto Deploy Hoạt Động

### Dấu Hiệu Auto Deploy Hoạt Động

**Railway Dashboard → Deployments**

**Sau khi push commit mới:**
- ✅ Xuất hiện deployment mới trong vòng 1-2 phút
- ✅ Status: "Building" → "Deploying" → "Active"
- ✅ Commit: Commit mới nhất bạn vừa push

**Railway Dashboard → Activity**

**Sẽ thấy:**
- ✅ "1 change in quanlyresort" - vài phút trước
- ✅ "quanlyresort Deployment successful" - sau khi deploy xong

### Dấu Hiệu Auto Deploy Không Hoạt Động

**Nếu không thấy deployment mới:**
- ❌ Auto Deploy chưa được bật
- ❌ GitHub webhook chưa hoạt động
- ❌ Railway chưa detect commit mới

## 🔧 Fix Nếu Auto Deploy Không Hoạt Động

### Fix 1: Enable Auto Deploy

**Railway Dashboard → Settings → Source**
1. Tìm "Auto Deploy" hoặc "Automatic Deployments"
2. Enable nó
3. Save changes

### Fix 2: Reconnect Repository

**Railway Dashboard → Settings → Source**
1. Click "Disconnect"
2. Click "Connect" lại
3. Chọn repository và branch
4. Railway sẽ tạo webhook mới

### Fix 3: Kiểm Tra GitHub Webhook

**GitHub Repository → Settings → Webhooks**
1. Tìm Railway webhook
2. Xem recent deliveries có lỗi không
3. Nếu có lỗi → Disable và enable lại

## 📋 Checklist Setup Auto Deploy

- [ ] Railway App đã được cài đặt trên GitHub
- [ ] Repository đã được connect trong Railway
- [ ] Branch `main` đã được chọn
- [ ] Auto Deploy đã được bật
- [ ] GitHub webhook đã được tạo
- [ ] Đã test push commit mới
- [ ] Railway tự động detect và deploy

## 🔗 Links

- **Video hướng dẫn:** https://www.youtube.com/watch?v=_dZXZSmmw2g
- **Railway Dashboard:** https://railway.app
- **Service Settings:** Railway Dashboard → Settings
- **GitHub Webhooks:** GitHub Repository → Settings → Webhooks

## 💡 Lưu Ý

1. **Auto Deploy** - Phải được bật trong Settings → Source
2. **GitHub webhook** - Railway tự động tạo khi connect repository
3. **Deploy time** - Railway mất 1-2 phút để detect và deploy
4. **Activity log** - Có thể delay vài phút

## 🎯 Kết Luận

**Để Railway tự động deploy:**
1. ✅ Connect GitHub repository
2. ✅ Enable Auto Deploy trong Settings
3. ✅ Push commit mới lên GitHub
4. ✅ Railway tự động detect và deploy

**Không cần:**
- ❌ Pre-deploy Command
- ❌ Start Command
- ❌ Manual trigger (trừ khi cần)

**Railway sẽ tự động deploy khi có commit mới trên branch đã connect!**

