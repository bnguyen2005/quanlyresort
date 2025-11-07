# Hướng dẫn đăng ký và cấu hình MB Bank API

## 📋 Tổng quan

MB Bank (Ngân hàng Quân đội) cung cấp API để tích hợp thanh toán và nhận webhook. Hệ thống đã được tích hợp sẵn để hỗ trợ MB Bank API.

## 🔑 Thông tin cần thiết

Để tích hợp với MB Bank, bạn cần có:
- **Client ID**: Để authenticate với MB Bank API
- **Client Secret**: Để authenticate với MB Bank API
- **Api Key** (nếu có): Cho một số API endpoints
- **Secret Key** (nếu có): Để verify webhook signature

## 📝 Cách đăng ký

### Bước 1: Truy cập MB Bank Developer Portal

1. Truy cập: **https://developer.mbbank.com.vn/**
2. Đăng ký tài khoản developer
3. Tạo ứng dụng mới (Application)
4. Điền thông tin:
   - Tên ứng dụng
   - Mô tả
   - Webhook URL (sẽ cấu hình sau)

### Bước 2: Nhận Credentials

Sau khi đăng ký thành công, bạn sẽ nhận được:
- **Client ID**: Chuỗi UUID
- **Client Secret**: Chuỗi bí mật
- **Api Key** (nếu có)
- **Secret Key** (nếu có, để verify webhook)

### Bước 3: Cấu hình vào hệ thống

Cập nhật `appsettings.json` với thông tin bạn nhận được:

```json
{
  "BankWebhook": {
    "MBBank": {
      "ClientId": "your-client-id-from-mbbank",
      "ClientSecret": "your-client-secret-from-mbbank",
      "ApiKey": "your-api-key-from-mbbank",
      "SecretKey": "your-secret-key-from-mbbank",
      "ApiBaseUrl": "https://api-sandbox.mbbank.com.vn",
      "OAuth2TokenUrl": "https://api-sandbox.mbbank.com.vn/oauth2/v1/token",
      "VerifySignature": true,
      "WebhookUrl": "https://your-domain.com/api/payment/mbbank-webhook"
    }
  }
}
```

### Bước 4: Cấu hình Webhook URL trong MB Bank Dashboard

1. Đăng nhập vào MB Bank Developer Portal
2. Vào phần **Webhook Configuration** hoặc **Callback URL**
3. Nhập Webhook URL:
   - **Development**: `http://localhost:5130/api/payment/mbbank-webhook`
   - **Production**: `https://your-domain.com/api/payment/mbbank-webhook`
4. Lưu cấu hình

## 🔐 OAuth2 Authentication

MB Bank sử dụng OAuth2 với `client_credentials` grant type:

### Flow:

1. **Lấy Access Token:**
   ```
   POST https://api-sandbox.mbbank.com.vn/oauth2/v1/token
   Authorization: Basic [base64(client_id:client_secret)]
   Content-Type: application/x-www-form-urlencoded
   
   grant_type=client_credentials
   ```

2. **Sử dụng Access Token:**
   ```
   Authorization: Bearer [access_token]
   clientMessageId: [UUID]
   ```

### Service đã được implement:

- `MBBankApiService`: Tự động lấy và refresh OAuth2 token
- `MBBankWebhookService`: Xử lý webhook từ MB Bank

## 🧪 Testing

### Test OAuth2 Token:

```bash
curl -X POST https://api-sandbox.mbbank.com.vn/oauth2/v1/token \
  -H "Authorization: Basic $(echo -n 'client_id:client_secret' | base64)" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials"
```

### Test Webhook:

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

## 📚 Tài liệu tham khảo

- **MB Bank Developer Portal**: https://developer.mbbank.com.vn/
- **API Documentation**: Xem trong MB Bank Developer Portal sau khi đăng ký
- **OAuth2 Spec**: https://oauth.net/2/

## ⚠️ Lưu ý

1. **Sandbox vs Production:**
   - Sandbox: `https://api-sandbox.mbbank.com.vn`
   - Production: `https://api.mbbank.com.vn` (sau khi được approve)

2. **Security:**
   - Không commit credentials vào git
   - Sử dụng environment variables hoặc secret management
   - Bật signature verification trong production

3. **Webhook:**
   - Production phải dùng HTTPS
   - Cần verify signature để đảm bảo tính xác thực

## ✅ Checklist

- [ ] Đăng ký tài khoản tại https://developer.mbbank.com.vn/
- [ ] Tạo Application và nhận Client ID, Client Secret
- [ ] Cập nhật `appsettings.json` với credentials
- [ ] Cấu hình Webhook URL trong MB Bank dashboard
- [ ] Test OAuth2 token
- [ ] Test webhook endpoint
- [ ] Verify signature verification hoạt động đúng

## 💡 Tips

1. **Development**: Có thể tạm thời tắt signature verification để test nhanh
2. **Production**: Bắt buộc bật signature verification
3. **Token Refresh**: Service tự động refresh token khi hết hạn
4. **Logging**: Enable detailed logging để debug dễ hơn

