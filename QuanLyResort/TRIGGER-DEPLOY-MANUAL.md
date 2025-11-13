# 🔄 Hướng Dẫn Trigger Deploy Thủ Công

## 🐛 Vấn Đề

Commit mới `42e8ab3` - "fix: Add JsonPropertyName attributes for SePay fields..." không xuất hiện trong Railway Dashboard.

**Nguyên nhân có thể:**
1. Railway chưa detect commit mới từ GitHub
2. GitHub webhook chưa trigger
3. Railway đang build nhưng chưa hiển thị

## ✅ Giải Pháp

### Cách 1: Kiểm Tra Commit Đã Push Chưa

**Kiểm tra local:**
```bash
git log --oneline -5
```

**Kiểm tra remote (GitHub):**
```bash
git log origin/main --oneline -5
```

**Nếu commit chưa có trên GitHub:**
```bash
git push origin main
```

### Cách 2: Trigger Deploy Thủ Công

**Option A: Redeploy từ Railway Dashboard**

1. Vào Railway Dashboard: https://railway.app
2. Chọn service `quanlyresort`
3. Tab **"Deployments"**
4. Click nút **"Redeploy"** (hoặc menu 3 chấm `:` → "Redeploy")
5. Chọn **"Deploy"**
6. Đợi 2-3 phút

**Option B: Trigger bằng Empty Commit**

```bash
cd QuanLyResort
./trigger-redeploy.sh
```

Hoặc thủ công:
```bash
git commit --allow-empty -m "trigger: Force Railway redeploy - $(date +%Y%m%d-%H%M%S)"
git push origin main
```

### Cách 3: Kiểm Tra GitHub Webhook

**Nếu Railway không tự động deploy:**

1. Vào GitHub repository: https://github.com/Lamm123435469898/quanlyresortt
2. Settings → Webhooks
3. Kiểm tra Railway webhook có active không
4. Xem recent deliveries có lỗi không

**Nếu webhook có vấn đề:**
- Disable và enable lại
- Hoặc tạo webhook mới trong Railway

### Cách 4: Kiểm Tra Railway Source Settings

**Vào Railway Dashboard → Settings → Source:**

1. **Repository:** `Lamm123435469898/quanlyresortt`
2. **Branch:** `main`
3. **Root Directory:** Để trống (hoặc `QuanLyResort` nếu cần)
4. **Auto Deploy:** ✅ Enabled

**Nếu Auto Deploy bị tắt:**
- Enable lại
- Hoặc deploy thủ công

## 🔍 Kiểm Tra Deploy Status

### Bước 1: Xem Deployments Tab

**Railway Dashboard → Deployments**

**Tìm deployment mới nhất:**
- Commit: `42e8ab3` - "fix: Add JsonPropertyName attributes..."
- Status: "Building" → "Deploying" → "Active"
- Timestamp: Mới nhất

**Nếu không thấy:**
- Railway chưa detect commit mới
- Cần trigger deploy thủ công

### Bước 2: Xem Logs Tab

**Railway Dashboard → Logs**

**Tìm build logs:**
```
Building Docker image...
Deploying service...
Service started successfully
```

**Nếu thấy build logs:**
- Railway đang deploy
- Đợi 2-3 phút

**Nếu không thấy:**
- Railway chưa trigger deploy
- Cần trigger thủ công

## 📋 Checklist

- [ ] Đã kiểm tra commit trên GitHub
- [ ] Đã kiểm tra Railway webhook
- [ ] Đã kiểm tra Auto Deploy settings
- [ ] Đã trigger deploy thủ công (nếu cần)
- [ ] Đã đợi 2-3 phút
- [ ] Đã kiểm tra deployment mới trong Railway

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **GitHub Repository:** https://github.com/Lamm123435469898/quanlyresortt
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Logs:** Railway Dashboard → Logs

## 💡 Lưu Ý

1. **Deploy time** - Railway mất 2-3 phút để deploy
2. **Webhook delay** - GitHub webhook có thể delay vài phút
3. **Manual trigger** - Nếu auto deploy không hoạt động, trigger thủ công
4. **Check logs** - Xem logs để xác nhận deploy đang chạy

## 🎯 Bước Tiếp Theo

1. **Kiểm tra commit trên GitHub** - Xác nhận commit đã push
2. **Trigger deploy thủ công** - Nếu Railway chưa detect
3. **Đợi 2-3 phút** - Để Railway deploy xong
4. **Kiểm tra deployment mới** - Trong Railway Dashboard
5. **Test SePay webhook** - Sau khi deploy xong

