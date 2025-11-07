# Tích hợp API Ngân hàng - Webhook

## Tổng quan

Hệ thống đã được tích hợp sẵn để nhận webhook từ các ngân hàng/API thanh toán, tự động phát hiện và cập nhật trạng thái thanh toán cho bookings.

## Endpoint Webhook

### `/api/payment/bank-webhook` (POST)

Endpoint này nhận webhook từ ngân hàng khi có giao dịch chuyển khoản.

**Request Body:**
```json
{
  "bankName": "MB",           // Tên ngân hàng: "MB", "VCB", "TCB", "VietQR", etc.
  "transactionId": "TXN123456789",
  "amount": 15000.00,
  "content": "BOOKING-BKG2025039",  // Nội dung chuyển khoản (quan trọng!)
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "transactionDate": "2025-11-04T10:30:00Z",
  "signature": "optional_signature_for_verification",
  "rawData": {
    // Dữ liệu raw từ ngân hàng (tùy chọn)
  }
}
```

**Response:**
```json
{
  "message": "Thanh toán được xử lý thành công",
  "bookingId": 39,
  "sessionId": "abc123",
  "bookingUpdated": true
}
```

## Cách hoạt động

1. **Khách hàng chuyển khoản** với nội dung: `BOOKING-BKG2025039` (hoặc `BOOKING-39`)
2. **Ngân hàng gửi webhook** đến endpoint `/api/payment/bank-webhook`
3. **Hệ thống tự động:**
   - Parse booking ID từ nội dung chuyển khoản
   - Verify amount và booking tồn tại
   - Cập nhật payment session status = "Paid"
   - Cập nhật booking status = "Paid"
   - Broadcast qua SignalR để frontend cập nhật real-time
   - Ẩn QR code và hiển thị "Thanh toán thành công"

## Format nội dung chuyển khoản

Hệ thống hỗ trợ các format sau:
- `BOOKING-BKG2025039` (recommended)
- `BOOKING-BKG39`
- `BOOKING-39`
- `39` (chỉ số booking ID, nếu hợp lý)

## Tích hợp với các ngân hàng

### 1. VietQR API

Nếu sử dụng VietQR API, cấu hình webhook URL trong VietQR dashboard:
```
https://your-domain.com/api/payment/bank-webhook
```

### 2. VNPay Gateway

Cấu hình IPN URL trong VNPay merchant dashboard:
```
https://your-domain.com/api/payment/bank-webhook
```

### 3. Ngân hàng trực tiếp (MB Bank, Vietcombank, etc.)

Cần liên hệ ngân hàng để:
1. Đăng ký webhook/callback service
2. Cấu hình webhook URL
3. Lấy secret key để verify signature (nếu có)

### 4. Open Banking API

Nhiều ngân hàng hiện hỗ trợ Open Banking API, có thể:
- Polling transactions từ API
- Nhận webhook khi có giao dịch mới
- Verify signature để đảm bảo tính xác thực

## Security

**⚠️ Quan trọng:** 
- Endpoint `/api/payment/bank-webhook` là `[AllowAnonymous]` vì webhook từ ngân hàng không dùng JWT
- **Bắt buộc** implement signature verification trong production
- Có thể thêm IP whitelist để chỉ nhận webhook từ IP của ngân hàng

## Testing

### Test với Postman/curl:

```bash
curl -X POST https://localhost:5130/api/payment/bank-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "bankName": "MB",
    "transactionId": "TEST-TXN-123",
    "amount": 15000,
    "content": "BOOKING-BKG2025039",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "transactionDate": "2025-11-04T10:30:00Z"
  }'
```

### Test từ Frontend:

Có thể dùng nút "🧪 Test Payment" trong modal thanh toán (chỉ hiển thị khi localhost).

## Cấu hình

Thêm vào `appsettings.json`:

```json
{
  "BankWebhook": {
    "SecretKey": "your-secret-key-for-signature-verification",
    "AllowedIPs": ["192.168.1.1", "10.0.0.1"],
    "VerifySignature": true
  }
}
```

## Implementation Notes

1. **Extract Booking ID:** Logic parse nội dung chuyển khoản trong `BankWebhookService.ExtractBookingIdFromContent()`
2. **Amount Verification:** Cho phép sai số nhỏ (0.01 VND), hoặc chấp nhận nếu amount >= expected
3. **Duplicate Handling:** Nếu booking đã "Paid", webhook sẽ được ignore (tránh duplicate)
4. **Real-time Update:** SignalR broadcast để frontend cập nhật ngay lập tức

## Troubleshooting

1. **Webhook không được xử lý:**
   - Kiểm tra format nội dung chuyển khoản
   - Kiểm tra booking ID có tồn tại không
   - Kiểm tra logs trong server

2. **Booking không được cập nhật:**
   - Kiểm tra `ProcessOnlinePaymentAsync` có hoạt động không
   - Kiểm tra database constraints

3. **Frontend không cập nhật:**
   - Kiểm tra SignalR connection
   - Kiểm tra polling có chạy không (fallback)

