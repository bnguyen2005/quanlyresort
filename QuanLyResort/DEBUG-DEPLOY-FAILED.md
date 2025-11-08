# 🔍 Debug Deploy Thất Bại Trên Render

## ❌ Vấn Đề

Deploy thất bại với commit: `Fix: Use SQLite for production on Linux (Render)`

## 🔍 Các Bước Debug

### Bước 1: Xem Logs Chi Tiết

1. **Vào Render Dashboard:**
   - https://dashboard.render.com
   - Click vào service `quanlyresort-api`
   - Click tab **"Logs"**

2. **Tìm lỗi:**
   - Scroll xuống cuối logs
   - Tìm các dòng có `error`, `fail`, `exception`
   - Copy toàn bộ error message

### Bước 2: Các Lỗi Thường Gặp

#### Lỗi 1: Database Connection
```
System.PlatformNotSupportedException: LocalDB is not supported
```
**Giải pháp:** Đảm bảo Environment Variable:
```
ConnectionStrings__DefaultConnection = Data Source=resort.db
```

#### Lỗi 2: Dockerfile Path
```
Dockerfile not found
```
**Giải pháp:** Kiểm tra Dockerfile Path trong Render:
- Phải là: `QuanLyResort/Dockerfile`

#### Lỗi 3: Build Failed
```
Build failed
```
**Giải pháp:** Kiểm tra:
- Dockerfile có đúng syntax không
- Dependencies có đầy đủ không

#### Lỗi 4: Port Conflict
```
Port already in use
```
**Giải pháp:** Đảm bảo:
```
ASPNETCORE_URLS = http://0.0.0.0:10000
PORT = 10000
```

### Bước 3: Kiểm Tra Environment Variables

Đảm bảo có các biến sau:

```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://0.0.0.0:10000
PORT = 10000
ConnectionStrings__DefaultConnection = Data Source=resort.db
JwtSettings__SecretKey = [KEY_CỦA_BẠN]
JwtSettings__Issuer = ResortManagementAPI
JwtSettings__Audience = ResortManagementClient
JwtSettings__ExpirationHours = 24
BankWebhook__PayOs__ClientId = c704495b-5984-4ad3-aa23-b2794a02aa83
BankWebhook__PayOs__ApiKey = f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
BankWebhook__PayOs__ChecksumKey = 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__SecretKey = 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__VerifySignature = false
```

### Bước 4: Kiểm Tra Dockerfile

Đảm bảo Dockerfile Path trong Render:
- **Dockerfile Path:** `QuanLyResort/Dockerfile`
- **Root Directory:** Để trống (hoặc `.`)

## 🔧 Quick Fix

### Nếu Lỗi Database:

1. Vào Render → Service → **"Environment"**
2. Tìm `ConnectionStrings__DefaultConnection`
3. Đổi thành: `Data Source=resort.db`
4. Click **"Save Changes"**
5. Click **"Manual Deploy"** → **"Deploy latest commit"**

### Nếu Lỗi Dockerfile:

1. Kiểm tra Dockerfile có tồn tại: `QuanLyResort/Dockerfile`
2. Kiểm tra Dockerfile Path trong Render config
3. Đảm bảo path đúng: `QuanLyResort/Dockerfile`

## 📋 Checklist

- [ ] Đã xem logs chi tiết
- [ ] Đã kiểm tra Environment Variables
- [ ] Đã kiểm tra Dockerfile Path
- [ ] Đã cập nhật Connection String sang SQLite
- [ ] Đã thử redeploy

## 💡 Gửi Logs Để Debug

Nếu vẫn lỗi, copy toàn bộ error logs từ Render và gửi để phân tích.

