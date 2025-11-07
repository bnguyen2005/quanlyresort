# 🐳 Cấu Hình Render với Docker (Cho .NET)

## ⚠️ Lưu Ý

Render không có option ".NET" trực tiếp, nhưng có thể deploy .NET app qua **Docker**.

## 📋 Cấu Hình Render

### 1. Source Code Section

- ✅ **Name:** `quanlyresort-api`
- ✅ **Language:** `Docker` (đúng rồi!)
- ✅ **Branch:** `main`
- ✅ **Region:** `Oregon (US West)` (hoặc region gần bạn)
- ✅ **Root Directory:** Để trống (hoặc `QuanLyResort` nếu cần)
- ✅ **Dockerfile Path:** `QuanLyResort/Dockerfile`

**⚠️ QUAN TRỌNG:** Dockerfile Path phải là `QuanLyResort/Dockerfile` (vì Dockerfile nằm trong thư mục QuanLyResort)

### 2. Build & Deploy

Render sẽ tự động:
- Build Docker image từ Dockerfile
- Deploy container
- Expose port 10000

### 3. Instance Type

- ✅ **Free** ($0/month) - Đủ cho development

### 4. Environment Variables

Click **"Add Environment Variable"** và thêm:

```
ASPNETCORE_ENVIRONMENT = Production
```

```
ASPNETCORE_URLS = http://0.0.0.0:10000
```

```
PORT = 10000
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

### 5. Advanced Settings

- **Dockerfile Path:** `QuanLyResort/Dockerfile`
- **Docker Context:** `.` (root của repo)
- **Auto-Deploy:** ✅ Yes

## ✅ Sau Khi Cấu Hình

1. Click **"Deploy Web Service"**
2. Render sẽ:
   - Build Docker image
   - Deploy container
   - Tạo HTTPS URL

## ⏱️ Thời Gian Deploy

- **Lần đầu:** 10-15 phút (build Docker image)
- **Các lần sau:** 5-10 phút

## 🎯 Sau Khi Deploy Thành Công

URL sẽ là:
```
https://quanlyresort-api.onrender.com
```

## 📋 Tiếp Theo

1. **Test backend:**
   ```bash
   curl https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status
   ```

2. **Config PayOs webhook:**
   ```bash
   cd QuanLyResort
   ./config-payos-after-deploy.sh https://quanlyresort-api.onrender.com
   ```

## ❓ Troubleshooting

### Lỗi: "Dockerfile not found"
→ Kiểm tra Dockerfile Path: `QuanLyResort/Dockerfile`

### Lỗi: "Build failed"
→ Kiểm tra Dockerfile có đúng không

### Lỗi: "Application error"
→ Kiểm tra Environment Variables, đặc biệt là `ASPNETCORE_URLS`

### Service không start
→ Kiểm tra port trong Dockerfile (phải expose port 10000)

