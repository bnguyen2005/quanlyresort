# Các Bước Test Payment - Hướng Dẫn Chi Tiết

## Bước 1: Đăng nhập (✅ Đã xong)
```bash
curl -X POST http://localhost:5130/api/auth/customer-login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@guest.test",
    "password": "Guest@123"
  }'
```

**Token của bạn:** `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4IiwidW5pcXVlX25hbWUiOiJjdXN0b21lcjEiLCJlbWFpbCI6ImN1c3RvbWVyMUBndWVzdC50ZXN0Iiwicm9sZSI6IkN1c3RvbWVyIiwiQ3VzdG9tZXJJZCI6IjEiLCJFbXBsb3llZUlkIjoiIiwibmJmIjoxNzYyMjgxMzc3LCJleHAiOjE3NjIzNjc3NzcsImlhdCI6MTc2MjI4MTM3NywiaXNzIjoiUmVzb3J0TWFuYWdlbWVudEFQSSIsImF1ZCI6IlJlc29ydE1hbmFnZW1lbnRDbGllbnQifQ.ZQftE9b9GVcACupHHVfkFqjKh3sywUpoW-4zOHSAbEc`

---

## Bước 2: Xem danh sách bookings của bạn

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4IiwidW5pcXVlX25hbWUiOiJjdXN0b21lcjEiLCJlbWFpbCI6ImN1c3RvbWVyMUBndWVzdC50ZXN0Iiwicm9sZSI6IkN1c3RvbWVyIiwiQ3VzdG9tZXJJZCI6IjEiLCJFbXBsb3llZUlkIjoiIiwibmJmIjoxNzYyMjgxMzc3LCJleHAiOjE3NjIzNjc3NzcsImlhdCI6MTc2MjI4MTM3NywiaXNzIjoiUmVzb3J0TWFuYWdlbWVudEFQSSIsImF1ZCI6IlJlc29ydE1hbmFnZW1lbnRDbGllbnQifQ.ZQftE9b9GVcACupHHVfkFqjKh3sywUpoW-4zOHSAbEc"

# Xem bookings của bạn
curl -X GET "http://localhost:5130/api/bookings/my" \
  -H "Authorization: Bearer $TOKEN"
```

**Tìm booking ID có status "Pending" để test thanh toán**

---

## Bước 3: Test Payment Endpoint

### Option A: Test bằng curl (nhanh nhất)

```bash
# Thay [BOOKING_ID] bằng ID booking thực tế (ví dụ: 39)
curl -X POST "http://localhost:5130/api/payment/test/39" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

**Response mong đợi:**
```json
{
  "message": "Thanh toán test thành công",
  "bookingId": 39,
  "status": "Paid"
}
```

### Option B: Test từ Browser Console

1. Mở trang **My Bookings**: `http://localhost:5130/customer/my-bookings.html`
2. Mở Browser Console (F12)
3. Paste và chạy:

```javascript
// Lấy token từ localStorage hoặc paste token của bạn
const token = localStorage.getItem('token') || "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4IiwidW5pcXVlX25hbWUiOiJjdXN0b21lcjEiLCJlbWFpbCI6ImN1c3RvbWVyMUBndWVzdC50ZXN0Iiwicm9sZSI6IkN1c3RvbWVyIiwiQ3VzdG9tZXJJZCI6IjEiLCJFbXBsb3llZUlkIjoiIiwibmJmIjoxNzYyMjgxMzc3LCJleHAiOjE3NjIzNjc3NzcsImlhdCI6MTc2MjI4MTM3NywiaXNzIjoiUmVzb3J0TWFuYWdlbWVudEFQSSIsImF1ZCI6IlJlc29ydE1hbmFnZW1lbnRDbGllbnQifQ.ZQftE9b9GVcACupHHVfkFqjKh3sywUpoW-4zOHSAbEc";

// Test payment cho booking ID 39 (thay bằng ID thực tế)
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
  alert('Thanh toán test thành công! Đang reload...');
  setTimeout(() => location.reload(), 1000);
})
.catch(err => {
  console.error('❌ Error:', err);
  alert('Lỗi: ' + err.message);
});
```

### Option C: Test từ UI (Dễ nhất)

1. Mở trang **My Bookings**: `http://localhost:5130/customer/my-bookings.html`
2. Tìm booking có status "Pending"
3. Click nút **"Thanh toán"** hoặc **"Pay"**
4. Modal payment sẽ hiển thị QR code
5. Nếu có nút **"Test Payment"** (chỉ hiển thị khi ở localhost), click vào đó
6. Hoặc dùng Browser Console để chạy code ở trên

---

## Bước 4: Kiểm Tra Kết Quả

### 4.1. Kiểm tra Database

```bash
# Kiểm tra booking status đã chuyển sang "Paid" chưa
curl -X GET "http://localhost:5130/api/payment/test/db-check?bookingId=39" \
  -H "Authorization: Bearer $TOKEN"
```

**Response sẽ hiển thị:**
- Payment sessions (nếu có)
- Booking status và thông tin
- Invoice (nếu có)

### 4.2. Kiểm tra UI

Sau khi test payment thành công:
- ✅ QR code tự động ẩn
- ✅ Hiển thị "Thanh toán thành công!"
- ✅ Booking status = "Paid"
- ✅ Modal tự động đóng sau 2 giây
- ✅ Danh sách bookings được reload với status mới

### 4.3. Kiểm tra Browser Console

Bạn sẽ thấy các logs:
```
✅ [PaymentWebSocket] Session created: abc123...
✅ [PaymentWebSocket] Connected
✅ [PaymentWebSocket] Joined session: abc123
📨 [PaymentWebSocket] Received PaymentStatusChanged: {status: "paid", ...}
✅ [PaymentPolling] Payment detected! Stopping polling...
```

---

## Bước 5: Test Webhook (Simulate Payment từ Bank)

### Test PayOs Webhook

```bash
curl -X POST http://localhost:5130/api/payment/payos-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "code": 0,
    "desc": "Success",
    "data": {
      "orderCode": 123456,
      "amount": 15000,
      "description": "BOOKING-39",
      "accountNumber": "0901329227",
      "reference": "TXN123456",
      "transactionDateTime": "2025-11-04T10:00:00Z",
      "currency": "VND"
    },
    "signature": "test-signature"
  }'
```

**Lưu ý:** Thay `BOOKING-39` bằng booking code thực tế (ví dụ: `BOOKING-BKG2025039`)

---

## Quick Test Script

Tạo file `quick-test.sh`:

```bash
#!/bin/bash

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4IiwidW5pcXVlX25hbWUiOiJjdXN0b21lcjEiLCJlbWFpbCI6ImN1c3RvbWVyMUBndWVzdC50ZXN0Iiwicm9sZSI6IkN1c3RvbWVyIiwiQ3VzdG9tZXJJZCI6IjEiLCJFbXBsb3llZUlkIjoiIiwibmJmIjoxNzYyMjgxMzc3LCJleHAiOjE3NjIzNjc3NzcsImlhdCI6MTc2MjI4MTM3NywiaXNzIjoiUmVzb3J0TWFuYWdlbWVudEFQSSIsImF1ZCI6IlJlc29ydE1hbmFnZW1lbnRDbGllbnQifQ.ZQftE9b9GVcACupHHVfkFqjKh3sywUpoW-4zOHSAbEc"

BOOKING_ID=${1:-39}

echo "🧪 Testing Payment for Booking ID: $BOOKING_ID"
echo ""

# Test payment
echo "1️⃣  Testing Payment..."
RESPONSE=$(curl -s -X POST "http://localhost:5130/api/payment/test/$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json")

echo "Response: $RESPONSE"
echo ""

# Check database
echo "2️⃣  Checking Database..."
DB_CHECK=$(curl -s -X GET "http://localhost:5130/api/payment/test/db-check?bookingId=$BOOKING_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Database Check: $DB_CHECK"
echo ""

echo "✅ Test completed!"
```

Chạy: `chmod +x quick-test.sh && ./quick-test.sh 39`

---

## Troubleshooting

### Lỗi: "Forbidden"
- **Nguyên nhân:** Booking không thuộc về bạn
- **Giải pháp:** Kiểm tra booking.CustomerId = 1 (customer ID của bạn)

### Lỗi: "Not Found"
- **Nguyên nhân:** Booking ID không tồn tại
- **Giải pháp:** Dùng booking ID thực tế từ danh sách bookings

### QR không ẩn sau khi thanh toán
- **Nguyên nhân:** SignalR không kết nối hoặc không nhận được message
- **Giải pháp:** 
  - Kiểm tra browser console có logs không
  - Kiểm tra WebSocket connection
  - Polling sẽ tự động detect status = "Paid" và update UI

---

## Next Steps

Sau khi test thành công:
1. ✅ Test payment từ UI (mở payment modal, click test payment)
2. ✅ Test webhook từ PayOs/VietQR
3. ✅ Test real payment flow (quét QR, thanh toán thật)
4. ✅ Kiểm tra database có lưu đúng không

