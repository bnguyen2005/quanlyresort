# 🔍 Nghiên Cứu SePay và Railway

## 📚 Thông Tin Tổng Quan

### SePay là gì?

**SePay** là một cổng thanh toán trực tuyến và API ngân hàng tại Việt Nam, cho phép:
- ✅ Tích hợp thanh toán QR Code
- ✅ Phát hiện biến động số dư tức thì (trong vòng 10 giây)
- ✅ Gửi webhook tự động khi có giao dịch
- ✅ Hợp tác trực tiếp với nhiều ngân hàng Việt Nam
- ✅ Tiết kiệm chi phí giao dịch

**Tài liệu chính thức:**
- Website: https://sepay.vn
- Developer Docs: https://developer.sepay.vn
- API Docs: https://docs.sepay.vn

### Railway là gì?

**Railway** là nền tảng triển khai ứng dụng đám mây, cho phép:
- ✅ Triển khai ứng dụng nhanh chóng
- ✅ Quản lý biến môi trường dễ dàng
- ✅ Theo dõi logs real-time
- ✅ Tự động scale
- ✅ Hỗ trợ nhiều ngôn ngữ và framework

**Tài liệu chính thức:**
- Website: https://railway.app
- Docs: https://docs.railway.com

## 🔑 SePay API - Thông Tin Quan Trọng

### 1. API Rate Limit

**⚠️ QUAN TRỌNG:**
- **Giới hạn:** 2 yêu cầu mỗi giây
- **Nếu vượt quá:** API trả về HTTP 429 (Too Many Requests)
- **Giải pháp:** Implement rate limiting hoặc retry logic

### 2. API Authentication

**Format:**
```
Authorization: Bearer {API_TOKEN}
```

**API Token:**
- Format: `spsk_live_...` (production)
- Format: `spsk_test_...` (test)
- Lấy từ SePay Dashboard → API

### 3. API Endpoints

**Có thể có nhiều endpoint:**
- Production API: `https://pgapi.sepay.vn/api/v1/...`
- User API: `https://my.sepay.vn/userapi/...`

**Cần kiểm tra SePay Dashboard để xác định endpoint chính xác.**

### 4. Request Format

**Tạo Order:**
```json
{
    "amount": 5000,
    "order_code": "BOOKING4",
    "description": "BOOKING4",
    "duration": 86400,
    "with_qrcode": true,
    "merchant_id": "SP-LIVE-LT39A334"  // Có thể bắt buộc
}
```

**Lưu ý:**
- `merchant_id` có thể BẮT BUỘC cho Production API
- `amount` phải là số nguyên (long)
- `duration` tính bằng giây

### 5. Webhook Format

**SePay gửi webhook với format:**
```json
{
    "id": 92704,
    "gateway": "Vietcombank",
    "transactionDate": "2023-03-25 14:02:37",
    "accountNumber": "0123499999",
    "code": null,
    "content": "BOOKING4",
    "transferType": "in",
    "transferAmount": 2277000,
    "accumulated": 19077000,
    "subAccount": null,
    "referenceCode": "MBVCB.3278907687",
    "description": ""
}
```

**Response yêu cầu:**
- JSON có `success: true`
- HTTP Status Code: 201 (hoặc 200)
- Nếu không đúng, SePay sẽ xem là webhook thất bại

## 🔧 Railway - Best Practices

### 1. Environment Variables

**Format trong .NET:**
- `SePay__ApiToken` → `SePay:ApiToken`
- `SEPAY_API_KEY` → `SEPAY_API_KEY`
- `.NET hỗ trợ cả 2 format`

**Cấu hình trong Railway:**
1. Railway Dashboard → Project → Variables
2. Thêm biến: `Name` và `Value`
3. Railway tự động inject vào ứng dụng

### 2. Configuration trong .NET

**appsettings.json:**
```json
{
  "SePay": {
    "ApiBaseUrl": "https://pgapi.sepay.vn",
    "ApiToken": "spsk_live_...",
    "AccountId": "5365",
    "MerchantId": "SP-LIVE-LT39A334",
    "BankCode": "MB",
    "BankAccountNumber": "0901329227"
  }
}
```

**Environment Variables (Railway):**
```
SePay__ApiToken=spsk_live_...
SePay__AccountId=5365
SePay__MerchantId=SP-LIVE-LT39A334
```

**Hoặc:**
```
SEPAY_API_KEY=spsk_live_...
SEPAY_CLIENT_ID=5365
SEPAY_MERCHANT_ID=SP-LIVE-LT39A334
```

### 3. Logging

**Railway cung cấp:**
- Real-time logs
- Log retention
- Log search

**Best Practice:**
- Log đầy đủ thông tin để debug
- Không log sensitive data (API keys, tokens)
- Sử dụng log levels phù hợp

### 4. Deployment

**Railway hỗ trợ:**
- Auto-deploy từ GitHub
- Manual deploy
- Preview deployments

**Best Practice:**
- Sử dụng auto-deploy cho production
- Test trên preview trước khi merge
- Monitor logs sau khi deploy

## 📋 Checklist Tích Hợp SePay + Railway

### Bước 1: SePay Setup

- [ ] Đăng ký tài khoản SePay
- [ ] Liên kết tài khoản ngân hàng
- [ ] Lấy API Token từ SePay Dashboard
- [ ] Lấy CLIENT_ID từ SePay Dashboard
- [ ] Lấy MERCHANT_ID từ SePay Dashboard (nếu có)
- [ ] Cấu hình webhook URL trong SePay Dashboard

### Bước 2: Railway Setup

- [ ] Đăng ký tài khoản Railway
- [ ] Tạo project mới
- [ ] Kết nối GitHub repository
- [ ] Cấu hình biến môi trường:
  - [ ] `SEPAY_API_KEY` hoặc `SePay__ApiToken`
  - [ ] `SEPAY_CLIENT_ID` hoặc `SePay__AccountId`
  - [ ] `SEPAY_MERCHANT_ID` hoặc `SePay__MerchantId` (nếu có)
  - [ ] `SEPAY_WEBHOOK_URL` hoặc `SePay__WebhookUrl`
  - [ ] `SePay__BankAccountNumber` (cho static QR code)
- [ ] Deploy ứng dụng

### Bước 3: Code Integration

- [ ] Implement SePay API client
- [ ] Implement webhook handler
- [ ] Implement rate limiting (2 requests/second)
- [ ] Implement error handling
- [ ] Implement retry logic
- [ ] Test với SePay test environment

### Bước 4: Testing

- [ ] Test tạo QR code
- [ ] Test webhook nhận được
- [ ] Test webhook response format
- [ ] Test với giao dịch thật
- [ ] Monitor Railway logs
- [ ] Monitor SePay Dashboard

## 🔍 Debugging Tips

### 1. SePay API 404 Error

**Nguyên nhân có thể:**
- API endpoint không đúng
- Thiếu `merchant_id` trong request
- API token không hợp lệ
- Account ID không đúng

**Giải pháp:**
- Kiểm tra SePay Dashboard → API → Endpoint
- Đảm bảo `merchant_id` được thêm vào request
- Verify API token trong SePay Dashboard
- Kiểm tra Account ID/CLIENT_ID

### 2. Webhook Không Nhận Được

**Nguyên nhân có thể:**
- Webhook URL không đúng
- Response format không đúng
- HTTP status code không đúng
- SePay chưa được cấu hình đúng

**Giải pháp:**
- Verify webhook URL trong SePay Dashboard
- Đảm bảo response có `success: true`
- Đảm bảo HTTP status code = 201 (hoặc 200)
- Kiểm tra SePay Dashboard → Webhooks → Status

### 3. Rate Limit 429 Error

**Nguyên nhân:**
- Gọi API quá nhanh (> 2 requests/second)

**Giải pháp:**
- Implement rate limiting
- Thêm delay giữa các requests
- Implement retry với exponential backoff

## 📚 Tài Liệu Tham Khảo

### SePay
- **Website:** https://sepay.vn
- **Developer Docs:** https://developer.sepay.vn
- **API Docs:** https://docs.sepay.vn
- **Dashboard:** https://my.sepay.vn

### Railway
- **Website:** https://railway.app
- **Docs:** https://docs.railway.com
- **Dashboard:** https://railway.app/dashboard

## 💡 Lưu Ý Quan Trọng

1. **Rate Limit:** SePay giới hạn 2 requests/second
2. **Webhook Response:** Phải có `success: true` và HTTP 201/200
3. **Merchant ID:** Có thể BẮT BUỘC cho Production API
4. **API Endpoint:** Cần kiểm tra SePay Dashboard để xác định endpoint chính xác
5. **Environment Variables:** Railway hỗ trợ cả format `SePay__*` và `SEPAY_*`

## 🎯 Kết Luận

**SePay + Railway** là một combo mạnh mẽ để xây dựng hệ thống thanh toán:
- ✅ SePay cung cấp API thanh toán mạnh mẽ
- ✅ Railway cung cấp nền tảng deploy dễ dàng
- ✅ Tích hợp nhanh chóng và hiệu quả
- ✅ Hỗ trợ tốt cho các ứng dụng .NET

**Bước tiếp theo:**
- Kiểm tra SePay Dashboard để xác định endpoint chính xác
- Cấu hình đầy đủ các biến môi trường trên Railway
- Test kỹ trước khi deploy production

