# 🔐 Hướng Dẫn Lấy JWT Token Và Sử Dụng API

## ❌ Lỗi 401 Unauthorized

Khi gặp lỗi:
```json
{
  "message": "Unauthorized. Please login to access this resource.",
  "path": "/api/bookings/4"
}
```

**Nguyên nhân:** Endpoint cần JWT token để authenticate.

## ✅ Giải Pháp

### Bước 1: Đăng Nhập Để Lấy Token

#### Option A: Login As Customer

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
  "email": "customer1@guest.test",
  "role": "Customer",
  "user": {
    "userId": 1,
    "username": "customer1",
    "email": "customer1@guest.test",
    "role": "Customer",
    "fullName": "Customer One"
  }
}
```

#### Option B: Login As Admin

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@resort.test",
    "password": "Admin@123",
    "role": "Admin"
  }'
```

### Bước 2: Copy Token Từ Response

Copy token từ response (ví dụ: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`)

### Bước 3: Sử Dụng Token Để Truy Cập API

```bash
# Set token variable
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Truy cập booking API với token
curl -X GET "https://quanlyresort-production.up.railway.app/api/bookings/4" \
  -H "accept: */*" \
  -H "Authorization: Bearer $TOKEN"
```

**Lưu ý:** 
- Header phải là: `Authorization: Bearer {token}`
- Không có dấu ngoặc kép quanh token trong header

## 📋 Ví Dụ Đầy Đủ

### 1. Login và Lấy Token

```bash
# Login as customer
LOGIN_RESPONSE=$(curl -s -X POST "https://quanlyresort-production.up.railway.app/api/auth/customer-login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Guest@123"
  }')

# Extract token (cần jq hoặc parse thủ công)
TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)
echo "Token: $TOKEN"
```

### 2. Sử Dụng Token

```bash
# Get booking với token
curl -X GET "https://quanlyresort-production.up.railway.app/api/bookings/4" \
  -H "accept: */*" \
  -H "Authorization: Bearer $TOKEN"
```

## 🔍 Sử Dụng Swagger UI (Dễ Hơn)

### Cách 1: Swagger UI

1. **Vào Swagger UI:**
   ```
   https://quanlyresort-production.up.railway.app/swagger
   ```

2. **Tìm endpoint:** `POST /api/auth/customer-login`
3. **Click "Try it out"**
4. **Nhập credentials:**
   ```json
   {
     "email": "customer1@guest.test",
     "password": "Guest@123"
   }
   ```
5. **Click "Execute"**
6. **Copy token** từ response
7. **Click nút "Authorize"** ở đầu trang Swagger
8. **Paste token** vào ô "Value"
9. **Click "Authorize"** và **"Close"**
10. **Bây giờ có thể test các endpoints** khác mà không cần token trong mỗi request

### Cách 2: Update Booking Status Qua Swagger

1. **Vào Swagger UI**
2. **Authorize với token** (xem Cách 1)
3. **Tìm endpoint:** `PUT /api/bookings/{id}/status`
4. **Click "Try it out"**
5. **Nhập booking ID:** `4`
6. **Body:**
   ```json
   {
     "status": "Paid"
   }
   ```
7. **Click "Execute"**

## 📝 Credentials Mặc Định

### Customer Account

- **Email:** `customer1@guest.test`
- **Password:** `Guest@123`

### Admin Account

- **Email:** `admin@resort.test`
- **Password:** `Admin@123`
- **Role:** `Admin`

## 🐛 Troubleshooting

### Lỗi: "Invalid credentials"

**Giải pháp:**
- Kiểm tra email và password đúng chưa
- Thử customer-login nếu admin-login không hoạt động

### Lỗi: "Token expired"

**Giải pháp:**
- Login lại để lấy token mới
- Token có thời hạn (thường 24 giờ)

### Lỗi: "Unauthorized" sau khi có token

**Giải pháp:**
- Kiểm tra header: `Authorization: Bearer {token}`
- Đảm bảo không có dấu ngoặc kép quanh token
- Kiểm tra token còn hợp lệ chưa

## 💡 Lưu Ý

- **Token có thời hạn:** Thường 24 giờ, cần login lại khi hết hạn
- **Header format:** `Authorization: Bearer {token}` (có chữ "Bearer" và space)
- **Swagger UI:** Cách dễ nhất để test API với authentication

## 🔗 URLs Quan Trọng

- **Swagger UI:** `https://quanlyresort-production.up.railway.app/swagger`
- **Login Endpoint:** `https://quanlyresort-production.up.railway.app/api/auth/customer-login`
- **Bookings API:** `https://quanlyresort-production.up.railway.app/api/bookings/{id}`

