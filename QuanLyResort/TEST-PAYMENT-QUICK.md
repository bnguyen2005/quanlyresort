# Hướng Dẫn Test Payment Nhanh

## Lỗi 401 Unauthorized

Khi gặp lỗi `{"message":"Unauthorized. Please login to access this resource."}`, có nghĩa là:
1. Bạn chưa đăng nhập
2. Token JWT không được gửi trong request header
3. Token đã hết hạn

## Cách Test Payment

### 1. Lấy JWT Token

**Bước 1:** Đăng nhập để lấy token
```bash
# Login as customer
curl -X POST http://localhost:5130/api/auth/customer-login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Guest@123"
  }'
```

**Lưu ý:** Password đúng là `Guest@123` (không phải `password123`)

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": { ... }
}
```

**Bước 2:** Copy token từ response

### 2. Test Payment Endpoint

```bash
# Set token variable
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Test payment cho booking ID 39
curl -X POST "http://localhost:5130/api/payment/test/39" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### 3. Test từ Browser Console

**Bước 1:** Mở trang My Bookings (`/customer/my-bookings.html`)

**Bước 2:** Mở Browser Console (F12)

**Bước 3:** Chạy lệnh:
```javascript
// Lấy token từ localStorage
const token = localStorage.getItem('token');
if (!token) {
  console.error('❌ Chưa đăng nhập!');
} else {
  // Test payment cho booking ID 39
  fetch(`${location.origin}/api/payment/test/39`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  })
  .then(res => res.json())
  .then(data => {
    console.log('✅ Test payment result:', data);
    // Reload bookings để xem status mới
    setTimeout(() => location.reload(), 1000);
  })
  .catch(err => {
    console.error('❌ Error:', err);
  });
}
```

### 4. Test từ Frontend UI

**Bước 1:** Vào My Bookings

**Bước 2:** Click nút **"Thanh toán"** cho một booking

**Bước 3:** Trong payment modal, nếu có nút **"Test Payment"** (chỉ hiển thị khi ở localhost), click vào đó

### 5. Kiểm Tra Kết Quả

Sau khi test payment thành công:
- ✅ Booking status = "Paid" trong database
- ✅ QR code tự động ẩn
- ✅ Hiển thị "Thanh toán thành công!"
- ✅ Modal tự động đóng sau 2 giây

## Troubleshooting

### Lỗi: "Forbidden"
- **Nguyên nhân:** Booking không thuộc về bạn hoặc không phải Admin
- **Giải pháp:** Kiểm tra booking.CustomerId có khớp với customerId trong token không

### Lỗi: "Token expired"
- **Nguyên nhân:** Token đã hết hạn (thường sau 24 giờ)
- **Giải pháp:** Đăng nhập lại để lấy token mới

### Lỗi: "No role claim found"
- **Nguyên nhân:** Token không có role claim
- **Giải pháp:** Đăng nhập lại, đảm bảo API trả về token với role "Customer"

## Test Payment từ Script

File `test-payment.sh` đã có sẵn, chỉ cần:
```bash
cd QuanLyResort
./test-payment.sh [bookingId] [jwt-token]

# Ví dụ:
./test-payment.sh 39 "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

