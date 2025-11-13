# 🚂 Hướng Dẫn Fix PORT Variable trên Railway

## ✅ Đã Hoàn Thành

1. ✅ Đã sửa Dockerfile để xử lý biến PORT động
2. ✅ Đã tạo entrypoint script để validate PORT (0-65535)
3. ✅ Đã cập nhật cả 2 Dockerfile (root và QuanLyResort/)

## 📋 Các Bước Tiếp Theo Trên Railway

### Bước 1: Cấu Hình Root Directory (QUAN TRỌNG!)

**Trong Railway Settings → Source:**

1. **KHÔNG set Root Directory** (để trống)
   - Vì Dockerfile build context là root của repo
   - File `railway.json` đã cấu hình `dockerfilePath: "QuanLyResort/Dockerfile"`

**Lý do:** 
- Dockerfile copy từ `QuanLyResort/QuanLyResort.csproj` (từ root context)
- Nếu set Root Directory = `QuanLyResort`, sẽ không tìm thấy file

### Bước 2: Kiểm Tra Environment Variables

**Vào tab "Variables" và đảm bảo có:**

```env
# ⚠️ QUAN TRỌNG: PHẢI set PORT thủ công với giá trị số nguyên
# Railway có thể inject PORT nhưng format không đúng, gây lỗi validation
PORT=10000

# Environment
ASPNETCORE_ENVIRONMENT=Production

# URL - Có thể để trống, entrypoint script sẽ tự set từ PORT
# Hoặc set: ASPNETCORE_URLS=http://0.0.0.0:$PORT

# Database
ConnectionStrings__DefaultConnection=Data Source=/data/resort.db

# JWT Settings
JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
JwtSettings__Issuer=ResortManagementAPI
JwtSettings__Audience=ResortManagementClient
JwtSettings__ExpirationHours=24

# PayOs Settings
BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__VerifySignature=false
```

**⚠️ LƯU Ý QUAN TRỌNG:**
- **PHẢI set PORT=10000 thủ công** trong Variables tab
- Railway có thể tự inject PORT nhưng format có thể không đúng, gây lỗi validation
- Entrypoint script sẽ đọc `PORT` từ environment variable và fallback về 10000 nếu không hợp lệ
- Script đã được cải thiện để xử lý các trường hợp PORT rỗng hoặc không hợp lệ

### Bước 3: Kiểm Tra Build Settings

**Trong Settings → Build:**

- Railway sẽ tự động detect từ `railway.json`
- **Builder:** DOCKERFILE
- **Dockerfile Path:** `QuanLyResort/Dockerfile` (từ root của repo)

### Bước 4: Trigger Deployment

**Có 2 cách:**

#### Cách 1: Redeploy từ Railway
1. Vào tab **"Deployments"**
2. Click **"Redeploy"** trên deployment mới nhất
3. Chọn **"Deploy"**

#### Cách 2: Push code mới lên GitHub
```bash
git add .
git commit -m "Fix: Update Dockerfile to handle PORT variable dynamically"
git push origin main
```

Railway sẽ tự động detect và deploy.

### Bước 5: Kiểm Tra Logs

**Sau khi deploy, vào tab "Logs" và tìm:**

✅ **Thành công:**
```
/app/docker-entrypoint.sh: Setting ASPNETCORE_URLS=http://0.0.0.0:XXXXX
Now listening on: http://0.0.0.0:XXXXX
Application started
```

❌ **Lỗi PORT:**
```
Error: PORT must be an integer between 0 and 65535. Got: [giá trị không hợp lệ]
```

## 🔍 Troubleshooting

### Lỗi: "PORT variable must be integer between 0 and 65535"

**Nguyên nhân:**
- Railway đang validate PORT environment variable trước khi chạy container
- PORT có thể bị set thành string rỗng hoặc giá trị không hợp lệ
- Railway inject PORT nhưng format không đúng

**Giải pháp NGAY LẬP TỨC:**

1. **Vào Railway Dashboard → Variables tab**

2. **Thêm hoặc sửa biến PORT:**
   - **Key:** `PORT`
   - **Value:** `10000` (phải là số nguyên, KHÔNG có dấu ngoặc kép)
   - **Lưu ý:** Chỉ nhập số `10000`, không nhập `"10000"` hay `'10000'`

3. **Nếu PORT đã tồn tại:**
   - Xóa biến PORT cũ
   - Tạo lại với giá trị `10000` (số nguyên)

4. **Sau khi set PORT=10000:**
   - Vào tab **Deployments**
   - Click **"Redeploy"** để deploy lại

**Kiểm tra:**
- Đảm bảo trong Variables tab, PORT hiển thị là số `10000`, không phải string
- Nếu thấy `"10000"` hoặc có dấu ngoặc kép, xóa và tạo lại

### Lỗi: "Dockerfile not found"

**Nguyên nhân:**
- Root Directory được set sai
- Dockerfile path không đúng

**Giải pháp:**
1. **KHÔNG set Root Directory** (để trống)
2. Đảm bảo `railway.json` có `dockerfilePath: "QuanLyResort/Dockerfile"`

### Lỗi: "Application failed to start"

**Kiểm tra:**
1. Xem logs để tìm lỗi cụ thể
2. Đảm bảo tất cả environment variables đã được set
3. Kiểm tra database connection string

## ✅ Checklist Trước Khi Deploy

- [ ] Root Directory: **ĐỂ TRỐNG** (không set)
- [ ] Environment Variables: Đã set đầy đủ
- [ ] `railway.json`: Có `dockerfilePath: "QuanLyResort/Dockerfile"`
- [ ] Code đã được commit và push lên GitHub
- [ ] Đã kiểm tra logs sau khi deploy

## 🎯 Kết Quả Mong Đợi

Sau khi deploy thành công:
- ✅ Service sẽ start với PORT từ Railway
- ✅ Entrypoint script validate PORT đúng (0-65535)
- ✅ Application chạy trên port được Railway assign
- ✅ Health check endpoint `/api/health` hoạt động

