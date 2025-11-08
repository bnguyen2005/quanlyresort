# 🔧 Fix Lỗi Database - Hướng Dẫn Nhanh

## ❌ Lỗi Hiện Tại

```
System.PlatformNotSupportedException: LocalDB is not supported on this platform.
```

## ✅ Giải Pháp: Đổi Connection String

### Bước 1: Vào Render Dashboard

1. Vào: https://dashboard.render.com
2. Click vào service `quanlyresort-api`
3. Click tab **"Environment"**

### Bước 2: Tìm Và Sửa Environment Variable

Tìm biến:
```
ConnectionStrings__DefaultConnection
```

**Giá trị cũ (XÓA):**
```
Server=(localdb)\mssqllocaldb;Database=ResortManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

**Giá trị mới (THÊM):**
```
Data Source=resort.db
```

### Bước 3: Save Và Redeploy

1. Click **"Save Changes"**
2. Click **"Manual Deploy"** → **"Deploy latest commit"**

## ✅ Kết Quả

Sau khi redeploy:
- ✅ App sẽ tạo file `resort.db` (SQLite)
- ✅ Tự động seed data
- ✅ Hoạt động bình thường

## 📋 Environment Variables Đầy Đủ

Sau khi fix, đảm bảo có các biến sau:

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

## 💡 Lưu Ý

- Code đã được fix để tự động dùng SQLite trên Linux
- Chỉ cần cập nhật Environment Variable trong Render
- SQLite file sẽ được tạo tự động

