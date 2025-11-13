# 🔧 Update Booking Status Thủ Công

## ✅ Đã Test Thành Công

### 1. Login và Lấy Token ✅

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/auth/customer-login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Guest@123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {...}
}
```

### 2. Get Booking Với Token ✅

```bash
TOKEN="YOUR_TOKEN_HERE"

curl -X GET "https://quanlyresort-production.up.railway.app/api/bookings/4" \
  -H "accept: */*" \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{
  "bookingId": 4,
  "bookingCode": "BKG2025004",
  "status": "Pending",
  ...
}
```

### 3. Update Booking Status ✅

```bash
TOKEN="YOUR_TOKEN_HERE"

curl -X PUT "https://quanlyresort-production.up.railway.app/api/bookings/4/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "status": "Paid"
  }'
```

## 📋 Hướng Dẫn Chi Tiết

### Bước 1: Login Để Lấy Token

**Customer Login:**
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/auth/customer-login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Guest@123"
  }'
```

**Admin Login:**
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@resort.test",
    "password": "Admin@123",
    "role": "Admin"
  }'
```

### Bước 2: Copy Token

Copy token từ response (ví dụ: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`)

### Bước 3: Update Booking Status Thành "Paid"

**Sử dụng endpoint `pay-online` (đúng cách):**

```bash
# Set token
TOKEN="YOUR_TOKEN_HERE"

# Update status thành "Paid" qua pay-online endpoint
curl -X POST "https://quanlyresort-production.up.railway.app/api/bookings/4/pay-online" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN"
```

**Endpoint này sẽ:**
- Update booking status thành "Paid"
- Tạo invoice nếu chưa có
- Xử lý đúng business logic

### Bước 4: Kiểm Tra Status Đã Update

```bash
curl -X GET "https://quanlyresort-production.up.railway.app/api/bookings/4" \
  -H "accept: */*" \
  -H "Authorization: Bearer $TOKEN"
```

**Kiểm tra:** `"status": "Paid"` trong response

## 🎯 Sử Dụng Swagger UI (Dễ Hơn)

### Cách 1: Swagger UI

1. **Vào Swagger UI:**
   ```
   https://quanlyresort-production.up.railway.app/swagger
   ```

2. **Login để lấy token:**
   - Tìm endpoint: `POST /api/auth/customer-login`
   - Click "Try it out"
   - Nhập credentials và Execute
   - Copy token từ response

3. **Authorize:**
   - Click nút "Authorize" ở đầu trang
   - Paste token vào ô "Value"
   - Click "Authorize" và "Close"

4. **Update booking status:**
   - Tìm endpoint: `POST /api/bookings/{id}/pay-online`
   - Click "Try it out"
   - Nhập booking ID: `4`
   - Click "Execute" (không cần body)

## 📝 Credentials

### Customer
- **Email:** `customer1@guest.test`
- **Password:** `Guest@123`

### Admin
- **Email:** `admin@resort.test`
- **Password:** `Admin@123`

## 💡 Lưu Ý

- **Token có thời hạn:** Thường 24 giờ, cần login lại khi hết hạn
- **Header format:** `Authorization: Bearer {token}`
- **Swagger UI:** Cách dễ nhất để test và update status

## 🎯 Kết Quả

Sau khi update status thành "Paid":
- ✅ Booking status = "Paid"
- ✅ QR code sẽ ẩn (nếu frontend đang polling)
- ✅ Frontend sẽ hiển thị "Thanh toán thành công"

## 🔗 URLs

- **Swagger UI:** `https://quanlyresort-production.up.railway.app/swagger`
- **Login:** `https://quanlyresort-production.up.railway.app/api/auth/customer-login`
- **Pay Online:** `https://quanlyresort-production.up.railway.app/api/bookings/{id}/pay-online`

