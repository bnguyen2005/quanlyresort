# Hướng Dẫn Test Chức Năng Thanh Toán

## Tổng Quan

Hệ thống thanh toán hỗ trợ:
- **VietQR**: Quét QR code để thanh toán
- **MB Bank**: Tích hợp API MB Bank
- **PayOs**: Payment gateway của MB Bank

## Các Bước Test

### 1. Test Từ Frontend (UI Test)

#### Bước 1: Tạo Booking
1. Đăng nhập với tài khoản Customer
2. Vào trang **Rooms** → Chọn phòng → Đặt phòng
3. Điền thông tin đặt phòng
4. Xác nhận booking

#### Bước 2: Thanh Toán Booking
1. Vào trang **My Bookings** (`/customer/my-bookings.html`)
2. Tìm booking có status "Pending"
3. Click nút **"Thanh toán"** hoặc **"Pay"**
4. Modal thanh toán sẽ hiển thị:
   - QR Code
   - Mã booking (BKG2025XXX)
   - Số tiền cần thanh toán
   - Thông tin tài khoản ngân hàng

#### Bước 3: Kiểm Tra Real-time Updates
- Sau khi quét QR và thanh toán thành công, hệ thống sẽ:
  - ✅ Ẩn QR code
  - ✅ Hiển thị "Thanh toán thành công!"
  - ✅ Cập nhật status booking thành "Paid"
  - ✅ Hiển thị thời gian thanh toán và mã giao dịch

### 2. Test Payment Session (API Test)

#### Test 1: Tạo Payment Session
```bash
# Lấy JWT token từ login
TOKEN="your-jwt-token-here"

# Tạo payment session
curl -X POST http://localhost:5130/api/payment/session/create \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "bookingId": 39,
    "amount": 15000
  }'
```

**Response mong đợi:**
```json
{
  "sessionId": "abc123...",
  "bookingId": 39,
  "amount": 15000,
  "status": "Pending",
  "expiresAt": "2025-11-04T..."
}
```

#### Test 2: Kiểm Tra Payment Status
```bash
# Kiểm tra status của session
curl -X GET "http://localhost:5130/api/payment/status/{sessionId}" \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Test Database Check Endpoint

#### Test Booking Cụ Thể
```bash
# Kiểm tra booking ID 39
curl -X GET "http://localhost:5130/api/payment/test/db-check?bookingId=39" \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{
  "success": true,
  "message": "Database check completed",
  "data": {
    "timestamp": "2025-11-04T...",
    "paymentSessions": [
      {
        "sessionId": "abc123...",
        "bookingId": 39,
        "amount": 15000,
        "status": "Paid",
        "transactionId": "TXN123",
        "paidAt": "2025-11-04T..."
      }
    ],
    "bookings": [
      {
        "bookingId": 39,
        "bookingCode": "BKG2025039",
        "status": "Paid",
        "estimatedTotalAmount": 15000,
        "invoice": {
          "invoiceNumber": "INV001",
          "status": "Paid"
        }
      }
    ]
  }
}
```

#### Test Tất Cả Bookings Đã Thanh Toán (Admin Only)
```bash
# Admin có thể xem tất cả bookings đã thanh toán
curl -X GET "http://localhost:5130/api/payment/test/db-check" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

### 4. Test Webhook (Simulate Payment)

#### Test PayOs Webhook
```bash
# Simulate PayOs webhook callback
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
      "currency": "VND",
      "paymentLinkId": "abc123",
      "code": 0,
      "desc": "Success",
      "counterAccountBankId": null,
      "counterAccountBankName": null,
      "counterAccountName": null,
      "counterAccountNumber": null,
      "virtualAccountName": null,
      "virtualAccountNumber": null
    },
    "signature": "calculated-signature-here"
  }'
```

**Lưu ý:** Signature cần được tính toán đúng theo PayOs documentation. Trong môi trường test, có thể tạm thời disable signature verification trong `appsettings.json`:
```json
{
  "BankWebhook": {
    "PayOs": {
      "VerifySignature": false
    }
  }
}
```

#### Test VietQR Webhook
```bash
curl -X POST http://localhost:5130/api/payment/vietqr-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": "TXN123",
    "amount": 15000,
    "content": "BOOKING-39",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "transactionDate": "2025-11-04T10:00:00Z",
    "signature": "calculated-signature"
  }'
```

#### Test MB Bank Webhook
```bash
curl -X POST http://localhost:5130/api/payment/mbbank-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": "TXN123",
    "mbTransactionId": "MB123",
    "amount": 15000,
    "content": "BOOKING-39",
    "accountNumber": "0901329227",
    "transactionDate": "2025-11-04T10:00:00Z",
    "signature": "calculated-signature"
  }'
```

### 5. Test Test Payment Endpoint (Simulate Success)

Để test nhanh mà không cần thực sự thanh toán, có thể dùng endpoint test:

```bash
# Simulate successful payment cho booking ID 39
curl -X POST "http://localhost:5130/api/payment/test/39" \
  -H "Authorization: Bearer $TOKEN"
```

**Response mong đợi:**
```json
{
  "message": "Thanh toán test thành công",
  "bookingId": 39,
  "status": "Paid"
}
```

**Lưu ý:** Endpoint này chỉ hiển thị khi đang ở `localhost`. Để test, cần:
1. Mở payment modal trong browser
2. Mở Browser Console
3. Gọi API này từ console hoặc Postman
4. QR code sẽ tự động ẩn và hiển thị "Thanh toán thành công!"

Endpoint này sẽ:
- ✅ Tạo payment session với status "Paid"
- ✅ Cập nhật booking status thành "Paid"
- ✅ Broadcast SignalR message để frontend cập nhật real-time

### 6. Test WebSocket/SignalR (Real-time Updates)

#### Test Từ Browser Console
1. Mở trang **My Bookings** và mở payment modal
2. Mở Browser Console (F12)
3. Kiểm tra logs:
   ```
   ✅ [PaymentWebSocket] Session created: abc123
   ✅ [PaymentWebSocket] Connected
   ✅ [PaymentWebSocket] Joined session: abc123
   ```

4. Khi thanh toán thành công, sẽ thấy:
   ```
   ✅ [PaymentWebSocket] Payment status changed: Paid
   ```

#### Test Polling Fallback
Nếu WebSocket fail, hệ thống sẽ tự động chuyển sang polling:
```
🔄 [startPaymentPolling] Starting payment polling for booking: 39
🔍 [PaymentPolling] Current status: Pending
```

### 7. Test Flow Hoàn Chỉnh

#### Scenario 1: Thanh Toán Thành Công
1. ✅ Tạo booking mới
2. ✅ Mở payment modal → QR code hiển thị
3. ✅ WebSocket/SignalR kết nối thành công
4. ✅ Simulate webhook thành công (hoặc dùng Test Payment endpoint)
5. ✅ QR code tự động ẩn
6. ✅ Hiển thị "Thanh toán thành công!"
7. ✅ Booking status = "Paid" trong database
8. ✅ Refresh bookings list → status đã cập nhật

#### Scenario 2: Thanh Toán Thất Bại
1. ✅ Tạo booking mới
2. ✅ Mở payment modal
3. ✅ Simulate webhook với status "Failed"
4. ✅ Hiển thị "Thanh toán thất bại"
5. ✅ Booking status vẫn là "Pending"

#### Scenario 3: QR Code Hết Hạn
1. ✅ Tạo booking mới
2. ✅ Mở payment modal
3. ✅ Đợi 15 phút (expiry time)
4. ✅ QR code tự động ẩn
5. ✅ Hiển thị "QR hết hạn"
6. ✅ Payment session status = "Expired"

### 8. Kiểm Tra Database

#### SQL Query để kiểm tra Bookings
```sql
-- Xem tất cả bookings đã thanh toán
SELECT 
    BookingId,
    BookingCode,
    Status,
    EstimatedTotalAmount,
    CreatedAt,
    UpdatedAt
FROM Bookings
WHERE Status = 'Paid'
ORDER BY UpdatedAt DESC;

-- Xem invoices liên quan
SELECT 
    i.InvoiceId,
    i.InvoiceNumber,
    i.TotalAmount,
    i.PaidAmount,
    i.Status,
    i.PaidDate,
    b.BookingCode
FROM Invoices i
JOIN Bookings b ON i.BookingId = b.BookingId
WHERE b.Status = 'Paid'
ORDER BY i.PaidDate DESC;
```

**Lưu ý:** Payment sessions hiện tại lưu trong memory (in-memory), không lưu vào database. Để kiểm tra sessions, dùng endpoint `/api/payment/test/db-check`.

### 9. Test Cases Checklist

- [ ] Tạo booking mới thành công
- [ ] Payment modal hiển thị đúng QR code
- [ ] QR code có amount đúng (không bị nhân 100)
- [ ] WebSocket/SignalR kết nối thành công
- [ ] Payment session được tạo trong memory
- [ ] Webhook nhận được và xử lý đúng
- [ ] Booking status cập nhật thành "Paid" sau khi thanh toán
- [ ] QR code tự động ẩn sau khi thanh toán thành công
- [ ] Hiển thị thông báo "Thanh toán thành công!"
- [ ] Hiển thị thời gian thanh toán và mã giao dịch
- [ ] Polling fallback hoạt động khi WebSocket fail
- [ ] QR code hết hạn sau 15 phút
- [ ] Test Payment endpoint hoạt động đúng
- [ ] Database check endpoint trả về đúng dữ liệu

### 10. Troubleshooting

#### Lỗi: "Forbidden" khi tạo payment session
- **Nguyên nhân:** JWT token không có quyền Customer
- **Giải pháp:** Kiểm tra middleware `JwtAuthorizationMiddleware.cs` có allow `/api/payment` cho Customer role

#### Lỗi: QR code không cập nhật
- **Nguyên nhân:** Browser cache
- **Giải pháp:** Đã thêm cache buster (`&_t=${Date.now()}`) vào QR URL

#### Lỗi: WebSocket không kết nối
- **Nguyên nhân:** SignalR chưa được cấu hình đúng
- **Giải pháp:** Kiểm tra `Program.cs` có map `/ws/payment` và JWT authentication cho SignalR

#### Lỗi: Webhook không được xử lý
- **Nguyên nhân:** Signature verification fail
- **Giải pháp:** Tạm thời disable `VerifySignature: false` trong `appsettings.json` để test

#### Lỗi: Booking status không cập nhật
- **Nguyên nhân:** Webhook không parse được booking ID từ content
- **Giải pháp:** Kiểm tra content format trong `BankWebhookService.cs` - phải có "BOOKING-{id}"

## Notes

- Payment sessions hiện tại lưu trong memory, sẽ mất khi restart server
- Để production, nên migrate payment sessions sang Redis hoặc Database
- Signature verification cần được bật lại khi deploy production
- Webhook URL cần được cấu hình trong PayOs/VietQR/MB Bank dashboard

