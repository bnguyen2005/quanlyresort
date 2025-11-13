# ⚙️ Railway Source Settings - Hướng Dẫn

## 📋 Settings Hiện Tại

### Source Repo
- **Repository:** `Lamm123435469898/quanlyresortt` ✅
- **Branch:** `main` ✅
- **Root Directory:** Để trống ✅

### Wait for CI
- **Status:** OFF (chưa bật) ✅

## 🔍 Giải Thích "Wait for CI"

### "Wait for CI" Là Gì?

**"Wait for CI"** có nghĩa là:
- Railway sẽ đợi GitHub Actions hoàn thành trước khi deploy
- Nếu GitHub Actions fail, Railway sẽ không deploy
- Nếu GitHub Actions pass, Railway sẽ tự động deploy

### Khi Nào Cần Bật "Wait for CI"?

**Bật nếu:**
- ✅ Bạn có GitHub Actions workflow (`.github/workflows/*.yml`)
- ✅ Bạn muốn đảm bảo tests/builds pass trước khi deploy
- ✅ Bạn muốn CI/CD pipeline đầy đủ

**Không cần bật nếu:**
- ❌ Không có GitHub Actions
- ❌ Muốn deploy ngay khi push code
- ❌ Không cần chạy tests trước khi deploy

## ✅ Khuyến Nghị Cho Project Này

**Không cần bật "Wait for CI" vì:**
- Project này không có GitHub Actions workflow
- Muốn deploy ngay khi push code
- Railway sẽ tự động build và deploy

## 🔧 Các Settings Quan Trọng Khác

### Auto Deploy (Quan Trọng!)

**Kiểm tra:**
- **Auto Deploy:** Phải là **Enabled** ✅
- Nếu bị tắt, Railway sẽ không tự động deploy

**Cách kiểm tra:**
1. Railway Dashboard → Settings → Source
2. Tìm "Auto Deploy" hoặc "Automatic Deployments"
3. Đảm bảo nó được bật

### Branch Connected

**Kiểm tra:**
- **Branch:** `main` ✅
- Đảm bảo branch đúng

### Root Directory

**Kiểm tra:**
- **Root Directory:** Để trống ✅ (hoặc `QuanLyResort` nếu cần)
- Đảm bảo Dockerfile path đúng

## 🐛 Nếu Railway Không Tự Động Deploy

### Nguyên Nhân Có Thể

1. **Auto Deploy bị tắt**
   - Fix: Enable Auto Deploy trong Settings

2. **GitHub webhook không hoạt động**
   - Fix: Kiểm tra GitHub webhook trong Settings → Webhooks

3. **Railway đang xử lý deployment khác**
   - Fix: Đợi deployment hiện tại hoàn tất

4. **Commit chưa được push lên GitHub**
   - Fix: Kiểm tra `git log origin/main`

### Giải Pháp

**Option 1: Enable Auto Deploy**
1. Railway Dashboard → Settings → Source
2. Tìm "Auto Deploy" hoặc "Automatic Deployments"
3. Enable nó

**Option 2: Trigger Deploy Thủ Công**
1. Railway Dashboard → Deployments
2. Click "Redeploy" trên deployment mới nhất
3. Đợi 2-3 phút

**Option 3: Kiểm Tra GitHub Webhook**
1. GitHub Repository → Settings → Webhooks
2. Kiểm tra Railway webhook có active không
3. Xem recent deliveries có lỗi không

## 📋 Checklist

- [ ] "Wait for CI": OFF (đúng, không cần bật)
- [ ] Auto Deploy: Enabled (quan trọng!)
- [ ] Branch: `main` (đúng)
- [ ] Root Directory: Để trống (đúng)
- [ ] GitHub webhook: Active (nếu có)

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Settings:** Railway Dashboard → Settings
- **Service Deployments:** Railway Dashboard → Deployments
- **GitHub Repository:** https://github.com/Lamm123435469898/quanlyresortt

## 💡 Lưu Ý

1. **"Wait for CI"** - Không cần bật nếu không có GitHub Actions
2. **Auto Deploy** - Phải được bật để Railway tự động deploy
3. **GitHub webhook** - Phải hoạt động để Railway detect commit mới
4. **Manual deploy** - Có thể trigger deploy thủ công nếu cần

## 🎯 Kết Luận

**Settings hiện tại:**
- ✅ "Wait for CI": OFF (đúng, không cần bật)
- ⚠️ Cần kiểm tra Auto Deploy có được bật không

**Bước tiếp theo:**
1. Kiểm tra Auto Deploy có được bật không
2. Nếu chưa bật → Enable nó
3. Hoặc trigger deploy thủ công từ Railway Dashboard

