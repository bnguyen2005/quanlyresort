# Cấu hình VietQR - Thông tin đã cấu hình

## ✅ Thông tin đã được cấu hình

### Credentials từ VietQR Dashboard:

```
Client ID:    c704495b-5984-4ad3-aa23-b2794a02aa83
Api Key:      f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
Checksum Key: 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

### Đã cập nhật vào `appsettings.json`:

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
    }
  }
}
```

## ⚠️ Cần cấu hình Webhook URL

### Bước 1: Cấu hình trong VietQR Dashboard

1. Đăng nhập vào VietQR Dashboard: https://vietqr.io/
2. Vào phần **Settings** → **Webhook Configuration**
3. Cấu hình Webhook URL:

   **Development (Local):**
   ```
   http://localhost:5130/api/payment/vietqr-webhook
   ```
   
   **Production:**
   ```
   https://your-domain.com/api/payment/vietqr-webhook
   ```

4. Lưu cấu hình

### Bước 2: Cập nhật Webhook URL trong `appsettings.json`

Cập nhật `WebhookUrl` với URL thực tế của bạn:

```json
{
  "BankWebhook": {
    "VietQR": {
      "WebhookUrl": "https://your-actual-domain.com/api/payment/vietqr-webhook"
    }
  }
}
```

## 🔐 Security

- ✅ **Checksum Key** đã được cấu hình để verify signature
- ✅ **Signature Verification** đã được bật (`VerifySignature: true`)
- ✅ Hệ thống sẽ tự động verify mọi webhook từ VietQR

## 🧪 Testing

### Test webhook locally với ngrok (nếu cần):

1. Cài đặt ngrok: https://ngrok.com/
2. Chạy ngrok:
   ```bash
   ngrok http 5130
   ```
3. Copy HTTPS URL từ ngrok (ví dụ: `https://abc123.ngrok.io`)
4. Cấu hình trong VietQR dashboard:
   ```
   https://abc123.ngrok.io/api/payment/vietqr-webhook
   ```

### Test webhook với curl:

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

## 📝 Lưu ý

1. **Development:** Có thể tạm thời tắt signature verification để test:
   ```json
   "VerifySignature": false
   ```

2. **Production:** **Bắt buộc** bật signature verification:
   ```json
   "VerifySignature": true
   ```

3. **Webhook URL:** Phải là HTTPS trong production (VietQR yêu cầu)

4. **IP Whitelist:** Có thể thêm IP whitelist trong middleware để chỉ nhận webhook từ IP của VietQR

## ✅ Checklist

- [x] Client ID đã được cấu hình
- [x] Api Key đã được cấu hình  
- [x] Checksum Key đã được cấu hình
- [x] Signature verification đã được cấu hình
- [ ] Webhook URL cần cấu hình trong VietQR dashboard
- [ ] Webhook URL cần cập nhật trong `appsettings.json` (production)

## 🚀 Sau khi cấu hình xong

1. Restart server để áp dụng cấu hình mới
2. Test webhook từ VietQR dashboard (nếu có chức năng test)
3. Tạo booking và test thanh toán thực tế
4. Kiểm tra logs để đảm bảo webhook được xử lý đúng

