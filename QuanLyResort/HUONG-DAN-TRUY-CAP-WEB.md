# 🌐 Hướng Dẫn Truy Cập Web Sau Khi Deploy

## ✅ Service Đã Deploy Thành Công!

Bây giờ cần expose service để có public URL.

## 📋 Bước 1: Generate Public Domain

### Trên Railway Dashboard:

1. **Vào service `quanlyresort`**
2. **Click tab "Settings"** (hoặc "Networking")
3. **Tìm section "Networking"** hoặc **"Public Domain"**
4. **Click "Generate Domain"** hoặc **"Generate Public URL"**

### Kết Quả:

Bạn sẽ có URL dạng:
```
https://quanlyresort-production-XXXX.up.railway.app
```

Hoặc:
```
https://quanlyresort.up.railway.app
```

**Lưu ý:** URL sẽ khác nhau tùy theo tên service và project.

## 📋 Bước 2: Kiểm Tra Service Đã Chạy

### Vào tab "Logs" và tìm:

✅ **Thành công:**
```
=== PORT Debug Info ===
Using PORT: 10000
ASPNETCORE_URLS: http://0.0.0.0:10000
Now listening on: http://0.0.0.0:10000
Application started
```

## 📋 Bước 3: Test Các Endpoints

### 1. Health Check (Kiểm tra service hoạt động)

```bash
curl https://YOUR_RAILWAY_URL.up.railway.app/api/health
```

**Kết quả mong đợi:**
```json
{
  "status": "healthy",
  "timestamp": "2025-11-13T..."
}
```

### 2. Swagger UI (API Documentation)

Mở trình duyệt và vào:
```
https://YOUR_RAILWAY_URL.up.railway.app/swagger
```

**Swagger sẽ hiển thị:**
- Tất cả API endpoints
- Có thể test API trực tiếp trên Swagger
- Xem request/response schemas

### 3. Test Webhook Status

```bash
curl https://YOUR_RAILWAY_URL.up.railway.app/api/simplepayment/webhook-status
```

### 4. Test Public Endpoints (Không cần đăng nhập)

#### Xem danh sách phòng:
```bash
curl https://YOUR_RAILWAY_URL.up.railway.app/api/rooms
```

#### Xem loại phòng:
```bash
curl https://YOUR_RAILWAY_URL.up.railway.app/api/room-types
```

#### Xem reviews:
```bash
curl https://YOUR_RAILWAY_URL.up.railway.app/api/reviews
```

## 📋 Bước 4: Cấu Hình Frontend (Nếu Có)

Nếu bạn có frontend riêng, cần cập nhật API base URL:

### Tìm file config API:

Tìm các file JavaScript có chứa:
- `localhost:7000`
- `localhost:5130`
- `http://localhost`
- `baseURL`
- `API_URL`

### Cập nhật thành Railway URL:

```javascript
// Thay đổi từ:
const API_URL = 'http://localhost:7000';

// Thành:
const API_URL = 'https://YOUR_RAILWAY_URL.up.railway.app';
```

## 📋 Bước 5: Cập Nhật PayOs Webhook URL

Sau khi có Railway URL:

1. **Copy webhook URL:**
   ```
   https://YOUR_RAILWAY_URL.up.railway.app/api/simplepayment/webhook
   ```

2. **Cập nhật trong PayOs Dashboard:**
   - Vào: https://payos.vn
   - Settings → Webhook URL
   - Paste URL Railway

3. **Cập nhật Environment Variable trên Railway:**
   - Vào tab **Variables**
   - Tìm `BankWebhook__PayOs__WebhookUrl`
   - Cập nhật thành: `https://YOUR_RAILWAY_URL.up.railway.app/api/simplepayment/webhook`

## 🔍 Các URL Quan Trọng

Sau khi có Railway URL, các endpoint chính:

| Mục Đích | URL |
|----------|-----|
| **Swagger UI** | `https://YOUR_URL/swagger` |
| **Health Check** | `https://YOUR_URL/api/health` |
| **Webhook Status** | `https://YOUR_URL/api/simplepayment/webhook-status` |
| **API Base** | `https://YOUR_URL/api` |

## 🎯 Test Đầy Đủ

### 1. Test Authentication:

```bash
# Customer Login
curl -X POST https://YOUR_URL/api/auth/customer-login \
  -H "Content-Type: application/json" \
  -d '{"email":"customer1@guest.test","password":"Password123!"}'
```

### 2. Test Booking:

```bash
# Tạo booking (cần token từ login)
curl -X POST https://YOUR_URL/api/bookings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "roomTypeId": 1,
    "checkInDate": "2025-11-15",
    "checkOutDate": "2025-11-17",
    "numberOfGuests": 2
  }'
```

## 🐛 Troubleshooting

### Lỗi: "Service not found" hoặc 404

**Nguyên nhân:**
- Service chưa được expose
- URL sai

**Giải pháp:**
1. Kiểm tra service đã có public domain chưa
2. Generate domain trong Settings → Networking

### Lỗi: "Connection refused"

**Nguyên nhân:**
- Service chưa start
- Port không đúng

**Giải pháp:**
1. Kiểm tra logs xem service đã start chưa
2. Đảm bảo PORT=10000 trong Variables

### Lỗi: "CORS error" (Khi gọi từ frontend)

**Giải pháp:**
- Cần cấu hình CORS trong backend để cho phép frontend domain
- Hoặc dùng Railway URL cho cả frontend và backend

## ✅ Checklist

- [ ] Đã generate public domain trên Railway
- [ ] Đã test health check endpoint
- [ ] Đã mở Swagger UI thành công
- [ ] Đã cập nhật PayOs webhook URL (nếu dùng)
- [ ] Đã cập nhật frontend API URL (nếu có)
- [ ] Đã test một vài API endpoints

## 🎉 Hoàn Thành!

Bây giờ bạn đã có:
- ✅ Public HTTPS URL
- ✅ API backend hoạt động
- ✅ Swagger documentation
- ✅ Sẵn sàng kết nối với frontend

**Lưu ý:** Railway free tier không sleep như Render, service sẽ luôn online!

