# 🎉 Sử Dụng Railway Public URL

## ✅ Bạn Đã Có Public Domain!

```
https://quanlyresort-production.up.railway.app
```

## 📋 Các URL Quan Trọng

### 1. Swagger UI (API Documentation) ⭐

**Mở trình duyệt và vào:**
```
https://quanlyresort-production.up.railway.app/swagger
```

**Swagger sẽ hiển thị:**
- Tất cả API endpoints
- Có thể test API trực tiếp trên Swagger
- Xem request/response schemas
- Thử các API với authentication

### 2. Health Check

**Kiểm tra service hoạt động:**
```bash
curl https://quanlyresort-production.up.railway.app/api/health
```

**Hoặc mở trình duyệt:**
```
https://quanlyresort-production.up.railway.app/api/health
```

### 3. Webhook Status

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status
```

## 🧪 Test Các API Endpoints

### Public Endpoints (Không cần đăng nhập)

#### 1. Xem danh sách phòng:
```bash
curl https://quanlyresort-production.up.railway.app/api/rooms
```

**Hoặc mở trình duyệt:**
```
https://quanlyresort-production.up.railway.app/api/rooms
```

#### 2. Xem loại phòng:
```bash
curl https://quanlyresort-production.up.railway.app/api/room-types
```

#### 3. Xem reviews:
```bash
curl https://quanlyresort-production.up.railway.app/api/reviews
```

#### 4. Xem menu nhà hàng:
```bash
curl https://quanlyresort-production.up.railway.app/api/services/restaurant/menu
```

### Authentication Endpoints

#### Customer Login:
```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/auth/customer-login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Password123!"
  }'
```

#### Admin/Staff Login:
```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@resort.test",
    "password": "Admin123!"
  }'
```

## 📝 Cập Nhật PayOs Webhook URL

Nếu bạn dùng PayOs, cần cập nhật webhook URL:

### 1. Webhook URL:
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

### 2. Cập Nhật Trên Railway:

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Tìm hoặc thêm biến:**
   - **Key:** `BankWebhook__PayOs__WebhookUrl`
   - **Value:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### 3. Cập Nhật Trên PayOs Dashboard:

1. **Vào:** https://payos.vn
2. **Settings** → **Webhook URL**
3. **Paste URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
4. **Save**

## 🔧 Cập Nhật Frontend (Nếu Có)

Nếu bạn có frontend riêng, cần cập nhật API base URL:

### Tìm các file config:

Tìm các file JavaScript có chứa:
- `localhost:7000`
- `localhost:5130`
- `http://localhost`
- `baseURL`
- `API_URL`
- `apiBaseUrl`

### Cập nhật thành:

```javascript
// Thay đổi từ:
const API_URL = 'http://localhost:7000';
const baseURL = 'http://localhost:5130';

// Thành:
const API_URL = 'https://quanlyresort-production.up.railway.app';
const baseURL = 'https://quanlyresort-production.up.railway.app';
```

### Ví dụ trong các file:

```javascript
// api-config.js hoặc tương tự
export const API_BASE_URL = 'https://quanlyresort-production.up.railway.app';

// hoặc
const config = {
  apiUrl: 'https://quanlyresort-production.up.railway.app'
};
```

## ✅ Checklist

- [x] Đã có public domain: `quanlyresort-production.up.railway.app`
- [ ] Đã mở Swagger UI thành công
- [ ] Đã test health check endpoint
- [ ] Đã test một vài API endpoints
- [ ] Đã cập nhật PayOs webhook URL (nếu dùng)
- [ ] Đã cập nhật frontend API URL (nếu có)

## 🎯 Quick Test

### Test nhanh trong trình duyệt:

1. **Swagger:**
   ```
   https://quanlyresort-production.up.railway.app/swagger
   ```

2. **Health Check:**
   ```
   https://quanlyresort-production.up.railway.app/api/health
   ```

3. **Danh sách phòng:**
   ```
   https://quanlyresort-production.up.railway.app/api/rooms
   ```

## 🔍 Kiểm Tra Service Đang Chạy

### Vào Railway Dashboard:

1. **Tab "Logs"** → Kiểm tra có log:
   ```
   Application started
   Now listening on: http://0.0.0.0:10000
   ```

2. **Tab "Metrics"** → Xem CPU, Memory usage

3. **Tab "Deployments"** → Đảm bảo có deployment "ACTIVE"

## 🐛 Troubleshooting

### Lỗi: "This site can't be reached"

**Nguyên nhân:**
- Service chưa start
- Port không đúng

**Giải pháp:**
1. Kiểm tra logs xem service đã start chưa
2. Đảm bảo PORT=10000 trong Variables

### Lỗi: 404 Not Found

**Nguyên nhân:**
- Route không tồn tại
- API path sai

**Giải pháp:**
1. Kiểm tra Swagger để xem đúng endpoint
2. Đảm bảo có `/api` prefix

### Lỗi: 500 Internal Server Error

**Nguyên nhân:**
- Database connection lỗi
- Environment variables thiếu

**Giải pháp:**
1. Kiểm tra logs để xem lỗi cụ thể
2. Đảm bảo tất cả environment variables đã được set

## 🎉 Hoàn Thành!

Bây giờ bạn có:
- ✅ Public HTTPS URL
- ✅ API backend hoạt động
- ✅ Swagger documentation
- ✅ Sẵn sàng kết nối với frontend

**Lưu ý:** Railway free tier không sleep, service sẽ luôn online!

