# Tích hợp VietQR và MB Bank - Hướng dẫn chi tiết

## 🎯 Tổng quan

Hệ thống đã được tích hợp đầy đủ với:
- **VietQR API**: Hỗ trợ nhiều ngân hàng (MB, VCB, TCB, etc.)
- **MB Bank**: Trực tiếp qua MB Bank API (nếu có)

## 📡 Endpoints

### 1. VietQR Webhook
**Endpoint:** `POST /api/payment/vietqr-webhook`

**Request Body:**
```json
{
  "transactionId": "TXN123456789",
  "vietQRTransactionId": "VQR-20251104-001",
  "amount": 15000.00,
  "content": "BOOKING-BKG2025039",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankCode": "MB",
  "bankName": "MBBank",
  "transactionDate": "2025-11-04T10:30:00Z",
  "signature": "hmac-sha256-signature-here",
  "status": "success"
}
```

### 2. MB Bank Webhook
**Endpoint:** `POST /api/payment/mbbank-webhook`

**Request Body:**
```json
{
  "transactionId": "MB-TXN-123456",
  "mbTransactionId": "MB20251104001",
  "amount": 15000.00,
  "content": "BOOKING-BKG2025039",
  "transactionDescription": "Thanh toan dat phong",
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "referenceNumber": "REF123",
  "transactionDate": "2025-11-04T10:30:00Z",
  "signature": "hmac-sha256-signature-here",
  "status": "SUCCESS",
  "transactionType": "IN"
}
```

## 🔧 Cấu hình

### 1. Cấu hình trong `appsettings.json`

**Đã cấu hình sẵn với thông tin từ VietQR dashboard:**

```json
{
  "BankWebhook": {
    "VietQR": {
      "ClientId": "c704495b-5984-4ad3-aa23-b2794a02aa83",
      "ApiKey": "f6ea421b-a8b7-46b8-92be-209eb1a9b2fb",
      "ChecksumKey": "429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313",
      "SecretKey": "429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313",
      "VerifySignature": true,
      "WebhookUrl": "https://your-domain.com/api/payment/vietqr-webhook"
    },
    "MBBank": {
      "SecretKey": "your-mbbank-secret-key-from-api",
      "VerifySignature": true,
      "WebhookUrl": "https://your-domain.com/api/payment/mbbank-webhook"
    },
    "AllowedIPs": [
      "103.xxx.xxx.xxx",  // VietQR IP range
      "203.xxx.xxx.xxx"   // MB Bank IP range
    ]
  }
}
```

### 2. Lấy Secret Key

#### VietQR:
1. ✅ **Đã cấu hình sẵn** với thông tin:
   - Client ID: `c704495b-5984-4ad3-aa23-b2794a02aa83`
   - Api Key: `f6ea421b-a8b7-46b8-92be-209eb1a9b2fb`
   - Checksum Key: `429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313`
2. **Cần cấu hình Webhook URL** trong VietQR dashboard:
   - Development: `http://localhost:5130/api/payment/vietqr-webhook`
   - Production: `https://your-domain.com/api/payment/vietqr-webhook`
3. ✅ Đã cập nhật vào `appsettings.json`

#### MB Bank:
1. **Đăng ký tại MB Bank Developer Portal:**
   - Truy cập: https://developer.mbbank.com.vn/
   - Đăng ký tài khoản developer
   - Tạo Application và nhận credentials:
     - **Client ID**: Để authenticate với OAuth2
     - **Client Secret**: Để authenticate với OAuth2
     - **Api Key** (nếu có): Cho một số API endpoints
     - **Secret Key** (nếu có): Để verify webhook signature
2. **Cập nhật `appsettings.json`** với thông tin nhận được
3. **Cấu hình Webhook URL** trong MB Bank dashboard:
   - Development: `http://localhost:5130/api/payment/mbbank-webhook`
   - Production: `https://your-domain.com/api/payment/mbbank-webhook`
4. ✅ Xem chi tiết trong file `MBBANK-SETUP.md`

## 🚀 Cách hoạt động

### Flow thanh toán:

1. **Khách hàng quét QR code** → VietQR hoặc MB Bank app
2. **Nhập nội dung chuyển khoản:** `BOOKING-BKG2025039`
3. **Chuyển tiền** → Giao dịch được xử lý
4. **VietQR/MB Bank gửi webhook** → Hệ thống nhận webhook
5. **Hệ thống tự động:**
   - Verify signature (bảo mật)
   - Parse booking ID từ nội dung
   - Verify amount và booking
   - Cập nhật payment session = "Paid"
   - Cập nhật booking status = "Paid"
   - Broadcast qua SignalR → Frontend cập nhật real-time
   - QR code biến mất, hiển thị "Thanh toán thành công"

## 🧪 Testing

### Test VietQR Webhook:

```bash
curl -X POST http://localhost:5130/api/payment/vietqr-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": "TEST-VQR-123",
    "vietQRTransactionId": "VQR-TEST-001",
    "amount": 15000,
    "content": "BOOKING-BKG2025039",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "bankCode": "MB",
    "bankName": "MBBank",
    "transactionDate": "2025-11-04T10:30:00Z",
    "status": "success"
  }'
```

### Test MB Bank Webhook:

```bash
curl -X POST http://localhost:5130/api/payment/mbbank-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": "TEST-MB-123",
    "mbTransactionId": "MB-TEST-001",
    "amount": 15000,
    "content": "BOOKING-BKG2025039",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "transactionDate": "2025-11-04T10:30:00Z",
    "status": "SUCCESS",
    "transactionType": "IN"
  }'
```

## 🔒 Security

### Signature Verification

Cả VietQR và MB Bank đều dùng **HMAC-SHA256** để verify webhook:

1. **VietQR:**
   - Payload: `{transactionId}{amount}{content}{accountNumber}{transactionDate}`
   - Algorithm: HMAC-SHA256
   - Secret: Lấy từ VietQR dashboard

2. **MB Bank:**
   - Payload: `{transactionId}{amount}{content}{accountNumber}{transactionDate}`
   - Algorithm: HMAC-SHA256
   - Secret: Lấy từ MB Bank API credentials

### IP Whitelist (Khuyến nghị)

Thêm IP whitelist trong middleware để chỉ nhận webhook từ IP của ngân hàng:
- VietQR IPs: Cần liên hệ VietQR để lấy IP range
- MB Bank IPs: Cần liên hệ MB Bank để lấy IP range

## 📋 Format nội dung chuyển khoản

Hệ thống hỗ trợ các format sau:
- ✅ `BOOKING-BKG2025039` (recommended)
- ✅ `BOOKING-BKG39`
- ✅ `BOOKING-39`
- ✅ `39` (chỉ số booking ID, nếu hợp lý)

## ⚙️ Troubleshooting

### 1. Webhook không được xử lý
- ✅ Kiểm tra format nội dung chuyển khoản
- ✅ Kiểm tra booking ID có tồn tại không
- ✅ Kiểm tra logs trong server console
- ✅ Kiểm tra signature verification (nếu enable)

### 2. Signature verification failed
- ✅ Kiểm tra Secret Key có đúng không
- ✅ Kiểm tra payload format (có thể khác nhau giữa các ngân hàng)
- ✅ Tạm thời disable verification để test: `"VerifySignature": false`

### 3. Booking không được cập nhật
- ✅ Kiểm tra `ProcessOnlinePaymentAsync` có hoạt động không
- ✅ Kiểm tra database constraints
- ✅ Kiểm tra logs để xem có lỗi gì không

### 4. Frontend không cập nhật real-time
- ✅ Kiểm tra SignalR connection
- ✅ Kiểm tra polling có chạy không (fallback)
- ✅ Kiểm tra browser console logs

## 📚 Tài liệu tham khảo

- **VietQR API Docs:** https://docs.vietqr.io/
- **MB Bank API Docs:** Liên hệ MB Bank để lấy documentation
- **HMAC-SHA256:** https://en.wikipedia.org/wiki/HMAC

## 💡 Tips

1. **Development:** Tắt signature verification để test nhanh hơn
2. **Production:** Bắt buộc bật signature verification
3. **Logging:** Enable detailed logging để debug dễ hơn
4. **Monitoring:** Setup monitoring/alerts cho webhook failures
5. **Retry:** Có thể implement retry mechanism nếu webhook fail

