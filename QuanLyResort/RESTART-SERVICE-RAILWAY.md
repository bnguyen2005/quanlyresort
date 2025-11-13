# 🔄 Hướng Dẫn Restart Service Railway

## 🐛 Vấn Đề

Tất cả các request đều trả về **502 Bad Gateway**:
- `/api/simplepayment/webhook` → 502
- `/` → 502
- `/favicon.ico` → 502
- `/service-worker.js` → 502

**Nguyên nhân:** Service không thể phản hồi, có thể đang crash hoặc không start được.

## ✅ Giải Pháp: Restart Service

### Cách 1: Redeploy (Khuyên Dùng)

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

**Bước 4: Kiểm Tra**
- Vào tab **"Logs"** để xem service start
- Tìm: `Application started. Press Ctrl+C to shut down.`
- Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### Cách 2: Restart từ Settings

**Bước 1: Vào Settings**
1. Railway Dashboard → Service `quanlyresort`
2. Tab **"Settings"**

**Bước 2: Restart**
1. Scroll xuống **"Danger Zone"**
2. Click nút **"Restart"**
3. Xác nhận restart

**Bước 3: Kiểm Tra**
- Vào tab **"Logs"** để xem service restart
- Đợi ~30 giây
- Test endpoint

### Cách 3: Trigger Deploy Mới (Nếu Redeploy Không Hoạt Động)

**Bước 1: Push Commit Mới**
```bash
cd /Users/vyto/Downloads/QuanLyResort-main\ \(1\)/QuanLyResort-main
git commit --allow-empty -m "trigger: Force redeploy"
git push origin main
```

**Bước 2: Railway Tự Động Deploy**
- Railway sẽ detect commit mới
- Tự động trigger deploy
- Xem progress trong tab "Deployments"

## 🔍 Kiểm Tra Sau Khi Restart

### 1. Xem Logs

**Railway Dashboard → Logs**

**Tìm các dòng sau (Service hoạt động bình thường):**
```
=== PORT Debug Info ===
PORT env var: '10000'
Using PORT: 10000
ASPNETCORE_URLS: http://0.0.0.0:10000
=======================
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:10000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: QuanLyResort.Program[0]
      🔧 Checking database connection...
info: QuanLyResort.Program[0]
      ✅ Database created using EnsureCreated
info: QuanLyResort.Program[0]
      ✅ Data seeded successfully
```

**Nếu thấy lỗi:**
```
❌ Error initializing database
Unhandled exception
System.NullReferenceException
```

→ Xem phần "Fix Lỗi" bên dưới

### 2. Test Endpoint

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

**Nếu vẫn 502:**
- Đợi thêm 30 giây (service có thể đang khởi tạo)
- Xem logs có lỗi gì không
- Kiểm tra Variables

## 🐛 Fix Lỗi Nếu Vẫn 502

### Lỗi 1: Database Connection

**Logs:**
```
❌ Error initializing database
Format of the initialization string does not conform to specification
```

**Fix:**
1. Vào **Variables**
2. Kiểm tra `ConnectionStrings__DefaultConnection`
3. Set = `Data Source=resort.db`
4. **Redeploy**

### Lỗi 2: Port Issue

**Logs:**
```
Error: PORT must be an integer. Got: '' (type: ...)
```

**Fix:**
1. Vào **Variables**
2. Kiểm tra `PORT`
3. Set = `10000` (hoặc `80`)
4. **Redeploy**

### Lỗi 3: JWT SecretKey Missing

**Logs:**
```
InvalidOperationException: JWT SecretKey not configured.
```

**Fix:**
1. Vào **Variables**
2. Thêm `JwtSettings__SecretKey` = `my-secret-key-123` (hoặc bất kỳ chuỗi nào)
3. **Redeploy**

### Lỗi 4: Service Crash on Startup

**Logs:**
```
Unhandled exception: System.NullReferenceException
   at QuanLyResort.Program.Main(String[] args)
```

**Fix:**
1. Xem stack trace trong logs
2. Fix lỗi trong code
3. Commit và push
4. Railway tự động deploy

## 📋 Checklist

- [ ] Đã vào Railway Dashboard
- [ ] Đã redeploy service
- [ ] Đã đợi 2-3 phút
- [ ] Đã xem logs (service đã start)
- [ ] Đã test endpoint (200 OK)
- [ ] Service hoạt động bình thường

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Logs:** Railway Dashboard → Logs
- **Service Variables:** Railway Dashboard → Variables

## 💡 Lưu Ý

1. **Deploy time** - Railway mất 2-3 phút để deploy
2. **Service restart** - Service sẽ restart tự động sau khi deploy
3. **Logs delay** - Logs có thể delay vài giây
4. **502 temporary** - 502 có thể là tạm thời khi service đang restart

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Xem logs chi tiết** - Railway Dashboard → Logs
2. **Kiểm tra Variables** - Railway Dashboard → Variables
3. **Contact Railway support** - Nếu cần

