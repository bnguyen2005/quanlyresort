# 🔧 Fix 502 Error - Application Failed to Respond

## 🐛 Vấn Đề

Service trả về **502 Bad Gateway**:
```json
{
  "status": "error",
  "code": 502,
  "message": "Application failed to respond"
}
```

## 🔍 Nguyên Nhân Có Thể

1. **Service đang restart** - Code mới đang được deploy
2. **Service bị crash** - Có lỗi trong code khiến service không start được
3. **Database connection error** - Không kết nối được database
4. **Port conflict** - Port đang bị conflict
5. **Environment variables missing** - Thiếu biến môi trường quan trọng

## ✅ Giải Pháp

### Bước 1: Kiểm Tra Logs

**Vào Railway Dashboard:**
1. Service `quanlyresort`
2. Tab **"Logs"**
3. Xem logs gần nhất

**Tìm các lỗi:**
- `Unhandled exception`
- `Database connection failed`
- `Port already in use`
- `Environment variable not found`

### Bước 2: Kiểm Tra Environment Variables

**Vào Railway Dashboard:**
1. Service `quanlyresort`
2. Tab **"Variables"**
3. Kiểm tra các biến sau:

**Bắt buộc:**
- ✅ `PORT` = `10000` (hoặc `80`)
- ✅ `ConnectionStrings__DefaultConnection` = `Data Source=resort.db`
- ✅ `ASPNETCORE_ENVIRONMENT` = `Production`

**PayOs (nếu dùng):**
- ✅ `BankWebhook__PayOs__ClientId`
- ✅ `BankWebhook__PayOs__ApiKey`
- ✅ `BankWebhook__PayOs__ChecksumKey`
- ✅ `BankWebhook__PayOs__SecretKey`
- ✅ `BankWebhook__PayOs__WebhookUrl`

### Bước 3: Restart Service

**Cách 1: Redeploy**
1. Tab **"Deployments"**
2. Click **"Redeploy"** trên deployment mới nhất
3. Chọn **"Deploy"**

**Cách 2: Restart từ Settings**
1. Tab **"Settings"**
2. Scroll xuống **"Danger Zone"**
3. Click **"Restart"**

### Bước 4: Kiểm Tra Database

**Nếu dùng SQLite:**
- Database file `resort.db` phải tồn tại
- Railway sẽ tự tạo nếu chưa có

**Nếu dùng SQL Server:**
- Kiểm tra connection string
- Kiểm tra database server có accessible không

### Bước 5: Kiểm Tra Port

**Kiểm tra PORT variable:**
- Railway tự động inject `PORT` environment variable
- Application phải đọc `PORT` và bind vào port đó

**Kiểm tra docker-entrypoint.sh:**
```bash
# File: QuanLyResort/docker-entrypoint.sh
# Phải có logic đọc PORT và set ASPNETCORE_URLS
```

## 🔍 Debug Steps

### 1. Xem Logs Chi Tiết

**Railway Dashboard → Logs:**
```
=== PORT Debug Info ===
PORT env var: '10000'
Using PORT: 10000
ASPNETCORE_URLS: http://0.0.0.0:10000
=======================
```

**Nếu không thấy:**
- Service chưa start
- docker-entrypoint.sh có vấn đề

### 2. Test Health Check

```bash
curl https://quanlyresort-production.up.railway.app/health
```

**Kết quả mong đợi:**
```json
{
  "status": "healthy",
  "database": "connected"
}
```

### 3. Test API Endpoint

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
- 200 OK: Service hoạt động
- 502: Service chưa start hoặc bị crash

## 🎯 Các Lỗi Thường Gặp

### Lỗi 1: Database Connection

**Logs:**
```
Format of the initialization string does not conform to specification
```

**Fix:**
- Set `ConnectionStrings__DefaultConnection` = `Data Source=resort.db`

### Lỗi 2: Port Conflict

**Logs:**
```
Address already in use
```

**Fix:**
- Railway tự động inject PORT, không cần set thủ công
- Kiểm tra docker-entrypoint.sh có đọc PORT đúng không

### Lỗi 3: Missing Environment Variables

**Logs:**
```
Configuration value not found: BankWebhook__PayOs__ClientId
```

**Fix:**
- Thêm các biến môi trường cần thiết vào Railway Variables

### Lỗi 4: Service Crash on Startup

**Logs:**
```
Unhandled exception: System.NullReferenceException
```

**Fix:**
- Xem stack trace trong logs
- Fix lỗi trong code
- Redeploy

## 📋 Checklist

- [ ] Đã kiểm tra Railway Logs
- [ ] Đã kiểm tra Environment Variables
- [ ] Đã restart service
- [ ] Đã test health check endpoint
- [ ] Đã test webhook endpoint
- [ ] Service đã hoạt động (200 OK)

## 🔗 Links Quan Trọng

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Service Variables:** Railway Dashboard → Variables
- **Service Settings:** Railway Dashboard → Settings

## 💡 Lưu Ý

1. **Deploy time** - Railway có thể mất 2-3 phút để deploy
2. **Service restart** - Service sẽ restart tự động sau khi deploy
3. **Logs delay** - Logs có thể delay vài giây
4. **502 temporary** - 502 có thể là tạm thời khi service đang restart

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Xem logs chi tiết** - Railway Dashboard → Logs
2. **Kiểm tra code** - Xem có lỗi syntax không
3. **Test local** - Test code local trước khi deploy
4. **Contact support** - Railway support nếu cần

