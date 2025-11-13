# 🔄 Hướng Dẫn Trigger Deploy Thủ Công

## 🐛 Vấn Đề

Railway không tự động deploy sau khi push commit mới.

## ✅ Giải Pháp: Trigger Deploy Thủ Công

### Cách 1: Redeploy từ Railway Dashboard (Khuyên Dùng)

**Bước 1: Vào Railway Dashboard**
1. Mở https://railway.app
2. Chọn project `alluring-nourishment`
3. Chọn service `quanlyresort`

**Bước 2: Redeploy**
1. Click tab **"Deployments"**
2. Tìm deployment mới nhất (có badge "ACTIVE")
3. Click nút **"Redeploy"** (hoặc menu 3 chấm `:` → "Redeploy")
4. Xác nhận **"Deploy"**

**Bước 3: Đợi Deploy**
- Railway sẽ rebuild và deploy lại
- Thời gian: ~2-3 phút
- Xem progress trong tab "Deployments"

### Cách 2: Kiểm Tra Auto Deploy Settings

**Vào Railway Dashboard → Settings → Source:**

**Kiểm tra:**
- ✅ **Auto Deploy:** Enabled
- ✅ **Branch:** `main`
- ✅ **Repository:** `Lamm123435469898/quanlyresortt`

**Nếu Auto Deploy bị tắt:**
- Enable lại
- Hoặc deploy thủ công

### Cách 3: Kiểm Tra GitHub Webhook

**Nếu Railway không tự động detect commit:**

**Bước 1: Vào GitHub**
1. Repository: https://github.com/Lamm123435469898/quanlyresortt
2. Settings → Webhooks

**Bước 2: Kiểm Tra Webhook**
- Xem có Railway webhook không
- Xem recent deliveries có lỗi không

**Nếu webhook có vấn đề:**
- Disable và enable lại
- Hoặc tạo webhook mới trong Railway

### Cách 4: Trigger Bằng Empty Commit (Đã Thử)

**Đã push empty commit nhưng Railway không detect:**

**Có thể do:**
- Railway webhook delay
- GitHub webhook chưa trigger
- Railway đang xử lý deployment khác

**Giải pháp:**
- Đợi thêm 1-2 phút
- Hoặc trigger deploy thủ công từ Railway Dashboard

## 🔍 Kiểm Tra Deploy Status

### Bước 1: Xem Deployments Tab

**Railway Dashboard → Deployments**

**Tìm deployment mới nhất:**
- Commit: `1377047` (trigger commit) hoặc `42e8ab3` (fix commit)
- Status: "Building" → "Deploying" → "Active"
- Timestamp: Mới nhất

**Nếu không thấy:**
- Refresh trang (F5)
- Hoặc đợi thêm 1-2 phút

### Bước 2: Xem Logs Tab

**Railway Dashboard → Logs**

**Tìm build logs:**
```
Building Docker image...
Deploying service...
Service started successfully
```

**Nếu thấy build logs:**
- ✅ Railway đang deploy
- Đợi 2-3 phút

## 📋 Checklist

- [ ] Đã kiểm tra Auto Deploy settings
- [ ] Đã kiểm tra GitHub webhook
- [ ] Đã trigger deploy thủ công (nếu cần)
- [ ] Đã đợi 2-3 phút
- [ ] Đã kiểm tra deployment status
- [ ] Deployment đã hoàn tất

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Settings:** Railway Dashboard → Settings
- **GitHub Repository:** https://github.com/Lamm123435469898/quanlyresortt

## 💡 Lưu Ý

1. **Auto deploy** - Railway thường tự động deploy, nhưng có thể delay
2. **Manual deploy** - Có thể trigger deploy thủ công từ Railway Dashboard
3. **Webhook delay** - GitHub webhook có thể delay vài phút
4. **Deploy time** - Railway mất 2-3 phút để deploy

## 🎯 Khuyến Nghị

**Nếu Railway không tự động deploy:**
1. **Trigger deploy thủ công** từ Railway Dashboard (Cách 1)
2. **Kiểm tra Auto Deploy settings** (Cách 2)
3. **Kiểm tra GitHub webhook** (Cách 3)

**Sau khi deploy xong:**
- Test SePay webhook để xác nhận code mới
- Xem logs để xác nhận TransferAmount được extract

