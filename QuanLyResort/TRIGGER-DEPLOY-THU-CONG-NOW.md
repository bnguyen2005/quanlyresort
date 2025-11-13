# 🔄 Trigger Deploy Thủ Công Ngay

## 📊 Tình Trạng

- ✅ Commit `3bc1366` đã push lên GitHub
- ⚠️ Railway không thấy dấu hiệu đang deploy
- Có thể Railway chưa detect hoặc cần trigger thủ công

## ✅ Giải Pháp: Trigger Deploy Thủ Công

### Cách 1: Redeploy từ Railway Dashboard (Khuyên Dùng)

**Bước 1: Vào Railway Dashboard**
1. Mở https://railway.app
2. Chọn project `alluring-nourishment`
3. Chọn service `quanlyresort`

**Bước 2: Vào Tab Deployments**
1. Click tab **"Deployments"** (ở trên cùng)
2. Tìm deployment mới nhất (có badge "ACTIVE")

**Bước 3: Redeploy**
1. Click nút **"Redeploy"** (hoặc menu 3 chấm `:` → "Redeploy")
2. Xác nhận **"Deploy"**
3. Xem progress trong tab "Deployments"

**Bước 4: Đợi Deploy**
- Railway sẽ rebuild và deploy lại
- Thời gian: ~2-3 phút
- Status: "Building" → "Deploying" → "Active"

### Cách 2: Deploy Latest Commit

**Railway Dashboard → Command Palette**
1. Nhấn **CMD + K** (Mac) hoặc **CTRL + K** (Windows/Linux)
2. Gõ **"Deploy Latest Commit"**
3. Railway sẽ deploy từ commit mới nhất trên branch `main`
4. Đợi 2-3 phút

### Cách 3: Kiểm Tra Auto Deploy

**Railway Dashboard → Tab "Settings" → "Source"**
1. Kiểm tra **"Auto Deploy"** có được bật không
2. Nếu chưa bật → Enable nó
3. Hoặc trigger deploy thủ công

## 🔍 Kiểm Tra Sau Khi Trigger Deploy

### Bước 1: Xem Tab Deployments

**Railway Dashboard → Tab "Deployments"**

**Tìm deployment mới:**
- Status: "Building" → "Deploying" → "Active"
- Timestamp: Mới nhất
- Commit: `3bc1366` hoặc `42e8ab3`

**Nếu thấy "Building" hoặc "Deploying":**
- ✅ Railway đang deploy
- Đợi 2-3 phút

### Bước 2: Xem Tab Logs

**Railway Dashboard → Tab "Logs"**

**Tìm build logs:**
```
Building Docker image...
Step 1/10 : FROM mcr.microsoft.com/dotnet/sdk:8.0
...
Successfully built ...
Deploying service...
Service started successfully
```

**Nếu thấy build logs:**
- ✅ Railway đang deploy
- Đợi 2-3 phút

## 🧪 Test Sau Khi Deploy Xong

**Sau khi deploy xong (2-3 phút):**

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN"
  }'
```

**Xem logs Railway, tìm:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=5000
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 5000
```

**Nếu thấy `TransferAmount=5000`:**
- ✅ Code mới đã hoạt động
- ✅ SePay webhook sẽ xử lý thành công

## 📋 Checklist

- [ ] Đã vào Railway Dashboard
- [ ] Đã vào tab "Deployments"
- [ ] Đã click "Redeploy" hoặc "Deploy Latest Commit"
- [ ] Đã đợi 2-3 phút
- [ ] Đã kiểm tra deployment status (Active?)
- [ ] Đã test SePay webhook
- [ ] Đã xem logs (TransferAmount được extract?)

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Logs:** Railway Dashboard → Logs

## 💡 Lưu Ý

1. **Manual deploy** - Có thể trigger deploy thủ công từ Railway Dashboard
2. **Deploy time** - Railway mất 2-3 phút để deploy
3. **Status** - Xem status trong tab "Deployments"
4. **Logs** - Xem logs trong tab "Logs" để xác nhận deploy

## 🎯 Kết Luận

**Vì Railway không tự động deploy:**
1. **Trigger deploy thủ công** từ Railway Dashboard
2. **Đợi 2-3 phút** để Railway deploy xong
3. **Test SePay webhook** để xác nhận code mới

**Cách nhanh nhất:**
- Railway Dashboard → Deployments → "Redeploy"
- Hoặc Command Palette (CMD + K) → "Deploy Latest Commit"

