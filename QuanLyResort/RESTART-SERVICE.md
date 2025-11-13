# 🔄 Cách Restart Service Trên Railway

## ❌ Vấn Đề

Service đã dừng với log:
```
Application is shutting down...
```

## ✅ Giải Pháp: Restart Service

### Cách 1: Redeploy (Khuyến nghị)

1. **Vào Railway Dashboard**
2. **Click vào service `quanlyresort`**
3. **Click tab "Deployments"**
4. **Tìm deployment mới nhất** (có thể đang ở trạng thái "Stopped" hoặc "Failed")
5. **Click nút "Redeploy"** (hoặc 3 dots menu → "Redeploy")
6. **Chọn "Deploy"** để confirm

### Cách 2: Tạo Deployment Mới

1. **Vào tab "Deployments"**
2. **Click "New Deployment"** hoặc **"Deploy"**
3. Railway sẽ build và deploy lại từ code mới nhất

### Cách 3: Trigger Deploy Từ GitHub

1. **Push một commit mới lên GitHub** (nếu có auto-deploy enabled)
2. Railway sẽ tự động detect và deploy

## 🔍 Kiểm Tra Service Đã Chạy

### 1. Xem Logs

Vào tab **"Logs"** và tìm:

✅ **Thành công:**
```
=== PORT Debug Info ===
Using PORT: 10000
ASPNETCORE_URLS: http://0.0.0.0:10000
Now listening on: http://0.0.0.0:10000
Application started
```

❌ **Vẫn lỗi:**
- Xem logs để tìm lỗi cụ thể
- Kiểm tra environment variables

### 2. Kiểm Tra Status

Vào tab **"Deployments"**:
- ✅ **ACTIVE** = Service đang chạy
- ❌ **Stopped** = Service đã dừng
- ⚠️ **Failed** = Deploy thất bại

### 3. Test Endpoint

Sau khi restart, test:
```bash
curl https://quanlyresort-production.up.railway.app/api/health
```

## 🐛 Tại Sao Service Dừng?

### Nguyên Nhân Thường Gặp:

1. **Crash do lỗi runtime**
   - Database connection failed
   - Missing environment variables
   - Application error

2. **Resource limit**
   - Hết memory
   - CPU limit

3. **Manual stop**
   - Ai đó đã stop service thủ công

4. **Deploy failed**
   - Build error
   - Configuration error

## 🔧 Fix Nếu Service Không Start

### 1. Kiểm Tra Environment Variables

Vào tab **"Variables"** và đảm bảo có:
- `PORT=10000`
- `ASPNETCORE_ENVIRONMENT=Production`
- Database connection string
- JWT settings
- PayOs settings (nếu dùng)

### 2. Kiểm Tra Logs

Xem logs để tìm lỗi cụ thể:
- Database connection errors
- Missing configuration
- Application startup errors

### 3. Kiểm Tra Database

Nếu dùng SQLite:
- Đảm bảo có persistent volume
- Mount path: `/data`

Nếu dùng PostgreSQL/MySQL:
- Đảm bảo connection string đúng
- Database service đang chạy

## 📋 Checklist Restart

- [ ] Vào tab "Deployments"
- [ ] Click "Redeploy" hoặc "New Deployment"
- [ ] Đợi build và deploy hoàn tất
- [ ] Kiểm tra logs có "Application started"
- [ ] Test endpoint `/api/health`
- [ ] Test endpoint `/api/reviews` hoặc `/swagger`

## 🎯 Sau Khi Restart

1. ✅ Service sẽ tự động start lại
2. ✅ Application sẽ listen trên port 10000
3. ✅ Có thể truy cập qua public URL
4. ✅ Tất cả endpoints hoạt động bình thường

## ⚠️ Lưu Ý

- Railway free tier không có auto-restart nếu service crash
- Cần redeploy thủ công nếu service dừng
- Kiểm tra logs thường xuyên để phát hiện vấn đề sớm

