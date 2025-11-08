# 🔍 Debug Lỗi 404 Sau Khi Deploy

## ❌ Vấn Đề

Service đã "Live" nhưng tất cả endpoints trả về **404 Not Found**.

## 🔍 Nguyên Nhân Có Thể

1. **App chưa start đúng cách**
2. **Routing không đúng**
3. **Port không đúng**
4. **Database error khi start**

## ✅ Các Bước Kiểm Tra

### Bước 1: Xem Logs Trên Render

1. Vào: https://dashboard.render.com
2. Click service `quanlyresort-api`
3. Tab **"Logs"**
4. Scroll xuống cuối, tìm:
   - `✅ Data seeded successfully` → Database OK
   - `Now listening on: http://0.0.0.0:10000` → App đã start
   - `Application started` → App đã sẵn sàng
   - `❌ Error` → Có lỗi

### Bước 2: Kiểm Tra Port

**Trong Render Environment Variables:**
```
PORT = 10000
ASPNETCORE_URLS = http://0.0.0.0:10000
```

**Trong Dockerfile:**
```dockerfile
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
```

### Bước 3: Kiểm Tra Routing

**Trong Render Settings:**
- **Health Check Path:** Để trống hoặc `/`
- **Start Command:** Để trống (dùng Dockerfile ENTRYPOINT)

### Bước 4: Test Endpoints

**1. Test root:**
```bash
curl https://quanlyresort-api.onrender.com/
```

**2. Test webhook status:**
```bash
curl https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status
```

**3. Test swagger:**
```bash
curl https://quanlyresort-api.onrender.com/swagger
```

## 🔧 Fix Nếu Có Lỗi

### Lỗi 1: App Không Start

**Triệu chứng:** Logs không có "Application started"

**Fix:**
1. Kiểm tra logs để tìm error
2. Đảm bảo Environment Variables đúng
3. Restart service

### Lỗi 2: Port Mismatch

**Triệu chứng:** App start nhưng không respond

**Fix:**
1. Kiểm tra `PORT` và `ASPNETCORE_URLS` trong Environment
2. Đảm bảo Dockerfile expose đúng port
3. Redeploy

### Lỗi 3: Database Error

**Triệu chứng:** Logs có "SQLite Error" hoặc "no such table"

**Fix:**
1. Đảm bảo `ConnectionStrings__DefaultConnection = Data Source=resort.db`
2. Kiểm tra logs xem database đã được tạo chưa
3. Nếu vẫn lỗi, xem `FIX-DATABASE-PRODUCTION.md`

## 📋 Checklist

- [ ] Logs có "Application started"
- [ ] Logs có "Now listening on: http://0.0.0.0:10000"
- [ ] Logs có "✅ Data seeded successfully"
- [ ] Environment Variables đúng (PORT, ASPNETCORE_URLS)
- [ ] Dockerfile expose port 10000
- [ ] Test endpoints trả về 200 (không phải 404)

## 💡 Quick Fix

Nếu vẫn 404 sau khi kiểm tra:

1. **Restart service:**
   - Render Dashboard → Service → "Manual Deploy" → "Deploy latest commit"

2. **Kiểm tra lại sau 5 phút**

3. **Nếu vẫn lỗi:**
   - Copy toàn bộ logs từ Render
   - Gửi để phân tích

