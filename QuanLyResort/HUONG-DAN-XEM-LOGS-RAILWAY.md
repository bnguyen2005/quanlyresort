# 📋 Hướng Dẫn Xem Logs Railway

## 🔍 Cách Xem Logs

### Bước 1: Vào Railway Dashboard

1. Mở https://railway.app
2. Chọn project `alluring-nourishment`
3. Chọn service `quanlyresort`

### Bước 2: Xem Logs

1. Click tab **"Logs"** (ở trên cùng, bên cạnh "Deployments", "Variables", "Metrics", "Settings")
2. Xem logs real-time

## 🔍 Tìm Lỗi Trong Logs

### Lỗi 1: Database Connection

**Tìm:**
```
❌ Error initializing database
Format of the initialization string does not conform to specification
```

**Fix:**
- Kiểm tra `ConnectionStrings__DefaultConnection` trong Variables
- Phải là: `Data Source=resort.db`

### Lỗi 2: Port Issue

**Tìm:**
```
=== PORT Debug Info ===
PORT env var: '...'
```

**Nếu không thấy:**
- docker-entrypoint.sh không chạy
- Service không start được

**Fix:**
- Kiểm tra `PORT` variable trong Railway
- Phải là số: `10000` hoặc `80`

### Lỗi 3: Service Crash

**Tìm:**
```
Unhandled exception
System.NullReferenceException
System.InvalidOperationException
```

**Fix:**
- Xem stack trace để biết lỗi ở đâu
- Fix code và redeploy

### Lỗi 4: JWT SecretKey Missing

**Tìm:**
```
JWT SecretKey not configured
InvalidOperationException
```

**Fix:**
- Thêm `JwtSettings__SecretKey` vào Variables

### Lỗi 5: Service Start Successfully

**Tìm:**
```
✅ Database created using EnsureCreated
✅ Data seeded successfully
Now listening on: http://0.0.0.0:10000
Application started. Press Ctrl+C to shut down.
```

**Nếu thấy:**
- ✅ Service đã start thành công
- ✅ Có thể test endpoint

## 📊 Logs Mẫu - Service Hoạt Động Bình Thường

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
         Database can connect: True
info: QuanLyResort.Program[0]
         Database provider: Microsoft.EntityFrameworkCore.Sqlite
info: QuanLyResort.Program[0]
      📦 Using SQLite - creating database with EnsureCreated...
info: QuanLyResort.Program[0]
      ✅ Database created using EnsureCreated
info: QuanLyResort.Program[0]
      🌱 Seeding initial data...
info: QuanLyResort.Program[0]
      ✅ Data seeded successfully
info: QuanLyResort.Services.PayOsService[0]
      [PAYOS] ✅ Service initialized with ClientId: 90ad103f
```

## 🐛 Logs Mẫu - Service Bị Lỗi

### Lỗi Database Connection

```
info: QuanLyResort.Program[0]
      🔧 Checking database connection...
fail: QuanLyResort.Program[0]
      ❌ Error initializing database
      System.ArgumentException: Format of the initialization string does not conform to specification starting at index 0.
```

### Lỗi Port

```
=== PORT Debug Info ===
PORT env var: ''
Error: PORT must be an integer. Got: '' (type: ...)
Falling back to default PORT=10000
```

### Lỗi JWT

```
fail: Microsoft.AspNetCore.Authentication.JwtBearer[0]
      InvalidOperationException: JWT SecretKey not configured.
```

## 🔧 Cách Fix Dựa Trên Logs

### Nếu Thấy Lỗi Database:

1. Vào **Variables**
2. Kiểm tra `ConnectionStrings__DefaultConnection`
3. Set = `Data Source=resort.db`
4. **Redeploy**

### Nếu Thấy Lỗi Port:

1. Vào **Variables**
2. Kiểm tra `PORT`
3. Set = `10000` (hoặc `80`)
4. **Redeploy**

### Nếu Thấy Lỗi JWT:

1. Vào **Variables**
2. Thêm `JwtSettings__SecretKey` = một chuỗi bất kỳ (ví dụ: `my-secret-key-123`)
3. **Redeploy**

### Nếu Service Đã Start Nhưng Vẫn 502:

1. Đợi thêm 30 giây (service có thể đang khởi tạo)
2. Test lại endpoint
3. Nếu vẫn 502, xem logs có lỗi gì không

## 📋 Checklist

- [ ] Đã vào Railway Dashboard
- [ ] Đã mở tab "Logs"
- [ ] Đã tìm lỗi trong logs
- [ ] Đã fix lỗi (nếu có)
- [ ] Đã redeploy
- [ ] Đã test lại endpoint

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs

