# Cấu hình PayOs (MB Bank Payment Gateway) - Đã cấu hình sẵn

## ✅ Thông tin đã được cấu hình

### Credentials từ PayOs Dashboard:

```
Client ID:    c704495b-5984-4ad3-aa23-b2794a02aa83
Api Key:      f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
Checksum Key: 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

### Đã cập nhật vào `appsettings.json`:

```json
{
  "BankWebhook": {
    "PayOs": {
      "ClientId": "c704495b-5984-4ad3-aa23-b2794a02aa83",
      "ApiKey": "f6ea421b-a8b7-46b8-92be-209eb1a9b2fb",
      "ChecksumKey": "429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313",
      "SecretKey": "429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313",
      "VerifySignature": true,
      "WebhookUrl": "https://your-domain.com/api/payment/payos-webhook"
    }
  }
}
```

## ⚠️ Cần cấu hình Webhook URL

### Bước 1: Cấu hình trong PayOs Dashboard

1. Đăng nhập vào PayOs Dashboard (từ MB Bank)
2. Vào phần **Webhook Configuration** hoặc **Callback URL**
3. Cấu hình Webhook URL:

   **Development (Local):**
   ```
   http://localhost:5130/api/payment/payos-webhook
   ```
   
   **Production:**
   ```
   https://your-domain.com/api/payment/payos-webhook
   ```

4. Lưu cấu hình

### Bước 2: Cập nhật Webhook URL trong `appsettings.json`

Cập nhật `WebhookUrl` với URL thực tế của bạn khi deploy production:

```json
{
  "BankWebhook": {
    "PayOs": {
      "WebhookUrl": "https://your-actual-domain.com/api/payment/payos-webhook"
    }
  }
}
```

## 🎯 Cách hoạt động

### Flow thanh toán tự động:

1. **Khách hàng quét QR code** → PayOs app
2. **Nhập nội dung chuyển khoản:** `BOOKING-BKG2025039`
3. **Chuyển tiền** → Giao dịch được xử lý
4. **PayOs gửi webhook** → Endpoint `/api/payment/payos-webhook`
5. **Hệ thống tự động:**
   - ✅ Verify signature (bảo mật)
   - ✅ Parse booking ID từ nội dung (`BOOKING-BKG2025039` → Booking ID: 39)
   - ✅ Verify amount và booking tồn tại
   - ✅ Cập nhật payment session status = "Paid"
   - ✅ Cập nhật booking status = "Paid"
   - ✅ **Broadcast qua SignalR** → Frontend nhận real-time
   - ✅ **QR code tự động ẩn**
   - ✅ **Hiển thị "Thanh toán thành công!"**
   - ✅ Đóng modal sau 2 giây và reload danh sách bookings

## 🔐 Security

- ✅ **ChecksumKey** đã được cấu hình để verify signature
- ✅ **Signature Verification** đã được bật (`VerifySignature: true`)
- ✅ Hệ thống sẽ tự động verify mọi webhook từ PayOs
- ✅ Signature format: HMAC-SHA256 với ChecksumKey

## 🧪 Testing

### Test PayOs webhook với curl:

```bash
curl -X POST http://localhost:5130/api/payment/payos-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "code": 0,
    "desc": "success",
    "id": "PAYOS-TEST-123",
    "signature": "test-signature",
    "data": {
      "transactionId": "TXN-TEST-123",
      "amount": 15000,
      "description": "BOOKING-BKG2025039",
      "accountNumber": "0901329227",
      "accountName": "Resort Deluxe",
      "transactionDateTime": "2025-11-04T10:30:00Z",
      "status": "SUCCESS"
    }
  }'
```

### Test từ Frontend:

1. Mở modal thanh toán cho một booking
2. QR code sẽ hiển thị
3. Simulate webhook từ PayOs (hoặc test payment thực tế)
4. Frontend sẽ tự động:
   - ✅ Ẩn QR code
   - ✅ Ẩn bank section
   - ✅ Hiển thị "Thanh toán thành công!"
   - ✅ Đóng modal sau 2 giây

## 📋 Format webhook từ PayOs

### Request Body:

```json
{
  "code": 0,              // 0 = thành công, khác 0 = lỗi
  "desc": "success",      // Mô tả
  "id": "PAYOS-123",      // PayOs transaction ID
  "signature": "...",     // HMAC-SHA256 signature
  "data": {
    "transactionId": "TXN-123",
    "amount": 15000,
    "description": "BOOKING-BKG2025039",  // Quan trọng: chứa booking ID
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "transactionDateTime": "2025-11-04T10:30:00Z",
    "status": "SUCCESS"
  }
}
```

## ✅ Checklist

- [x] Client ID đã được cấu hình
- [x] Api Key đã được cấu hình  
- [x] Checksum Key đã được cấu hình
- [x] Signature verification đã được cấu hình
- [x] Webhook endpoint đã được tạo (`/api/payment/payos-webhook`)
- [x] SignalR broadcast đã được implement
- [ ] Webhook URL cần cấu hình trong PayOs dashboard
- [ ] Webhook URL cần cập nhật trong `appsettings.json` (production)

## 🚀 Sau khi cấu hình xong

1. **Restart server** để áp dụng cấu hình mới
2. **Test webhook** từ PayOs dashboard (nếu có chức năng test)
3. **Tạo booking và test thanh toán thực tế**
4. **Kiểm tra logs** để đảm bảo webhook được xử lý đúng
5. **Kiểm tra frontend** - QR code sẽ tự động ẩn và hiển thị "Thanh toán thành công!"

## 💡 Lưu ý

1. **Development:** Có thể tạm thời tắt signature verification để test:
   ```json
   "VerifySignature": false
   ```

2. **Production:** **Bắt buộc** bật signature verification:
   ```json
   "VerifySignature": true
   ```

3. **Webhook URL:** Phải là HTTPS trong production

4. **Real-time Update:** Frontend sẽ nhận update qua SignalR ngay lập tức (< 1 giây)

5. **Fallback:** Nếu SignalR không khả dụng, polling sẽ tự động detect payment

## 🎉 Kết quả

Sau khi tích hợp PayOs, khi khách hàng quét QR và thanh toán:
- ✅ Webhook tự động được gửi từ PayOs
- ✅ Hệ thống tự động cập nhật booking = "Paid"
- ✅ **QR code tự động ẩn** (không cần refresh)
- ✅ **Hiển thị "Thanh toán thành công!"** ngay lập tức
- ✅ Modal tự động đóng sau 2 giây
- ✅ Danh sách bookings tự động reload với trạng thái mới

