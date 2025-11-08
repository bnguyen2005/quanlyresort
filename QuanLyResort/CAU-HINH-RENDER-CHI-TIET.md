# 🔧 Cấu Hình Render Web Service - Chi Tiết

## ⚠️ Lưu Ý Quan Trọng

Bạn đang ở trang cấu hình Render. Cần điều chỉnh các mục sau:

## 📋 Cấu Hình Chi Tiết

### 1. Source Code Section

- ✅ **Name:** `quanlyresort-api` (hoặc `quanlyresort`)
- ❌ **Language:** Đang là "Docker" → **ĐỔI THÀNH `.NET`**
- ✅ **Branch:** `main` (đúng)
- ✅ **Region:** `Oregon (US West)` (hoặc region gần bạn)
- ✅ **Root Directory:** Để trống (hoặc `QuanLyResort` nếu cần)
- ❌ **Dockerfile Path:** Xóa hoặc để trống (không dùng Docker)

### 2. Build & Deploy Section

Sau khi chọn `.NET`, Render sẽ tự động hiện:
- **Build Command:** `dotnet publish -c Release -o ./publish`
- **Start Command:** `dotnet ./publish/QuanLyResort.dll`

**Nếu không tự động, thêm thủ công:**
- **Build Command:** `dotnet publish -c Release -o ./publish`
- **Start Command:** `dotnet ./publish/QuanLyResort.dll`

### 3. Instance Type

- ✅ **Free** ($0/month) - Đủ cho development
- ⚠️ Lưu ý: Free tier sẽ sleep sau 15 phút không có request

### 4. Environment Variables (QUAN TRỌNG!)

Click **"Add Environment Variable"** và thêm từng biến:

```
ASPNETCORE_ENVIRONMENT = Production
```

```
ASPNETCORE_URLS = http://0.0.0.0:$PORT
```

```
ConnectionStrings__DefaultConnection = Server=(localdb)\mssqllocaldb;Database=ResortManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

```
JwtSettings__SecretKey = YourSuperSecretKeyForJWTTokenGeneration2025!@#$
```

```
JwtSettings__Issuer = ResortManagementAPI
```

```
JwtSettings__Audience = ResortManagementClient
```

```
JwtSettings__ExpirationHours = 24
```

```
BankWebhook__PayOs__ClientId = c704495b-5984-4ad3-aa23-b2794a02aa83
```

```
BankWebhook__PayOs__ApiKey = f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
```

```
BankWebhook__PayOs__ChecksumKey = 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

```
BankWebhook__PayOs__SecretKey = 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

```
BankWebhook__PayOs__VerifySignature = false
```

### 5. Advanced Settings (Tùy chọn)

- **Health Check Path:** `/api/health` (nếu có)
- **Auto-Deploy:** ✅ Yes (tự động deploy khi push code)

## ✅ Sau Khi Cấu Hình Xong

1. Click **"Deploy Web Service"**
2. Render sẽ:
   - Clone code từ GitHub
   - Build project
   - Deploy lên server
   - Tạo HTTPS URL

## ⏱️ Thời Gian Deploy

- **Lần đầu:** 5-10 phút
- **Các lần sau:** 2-5 phút

## 🎯 Sau Khi Deploy Thành Công

Bạn sẽ có URL:
```
https://quanlyresort-api.onrender.com
```

Hoặc:
```
https://quanlyresort.onrender.com
```

## 📋 Tiếp Theo

1. **Test backend:** `https://your-url.onrender.com/api/simplepayment/webhook-status`
2. **Config PayOs webhook:** `./config-payos-after-deploy.sh https://your-url.onrender.com`
3. **Test payment:** Xem `TEST-THANH-TOAN-THAT.md`

## ❓ Troubleshooting

### Lỗi: "Build failed"
→ Kiểm tra Build Command và Start Command

### Lỗi: "Application error"
→ Kiểm tra Environment Variables, đặc biệt là `ConnectionStrings__DefaultConnection`

### Lỗi: "Port already in use"
→ Đảm bảo `ASPNETCORE_URLS=http://0.0.0.0:$PORT`

### Service sleep sau 15 phút
→ Đây là hạn chế của Free tier. Upgrade lên Starter ($7/month) để tránh sleep.

