# 🔧 Hướng Dẫn Config PayOs Webhook URL

## 📋 Tổng Quan

Để PayOs có thể gọi webhook khi thanh toán thành công, bạn cần:
1. **Expose localhost** ra internet (dùng ngrok)
2. **Config webhook URL** trong PayOs dashboard
3. **Test webhook** để xác nhận hoạt động

## 🔧 Bước 1: Cài Đặt Ngrok

### macOS:
```bash
# Option 1: Dùng Homebrew
brew install ngrok

# Option 2: Download từ website
# https://ngrok.com/download
```

### Windows/Linux:
- Download từ: https://ngrok.com/download
- Hoặc dùng package manager tương ứng

## 🔧 Bước 2: Chạy Ngrok

1. **Đảm bảo backend đang chạy** trên port 5130:
   ```bash
   # Kiểm tra backend đang chạy
   curl http://localhost:5130/api/simplepayment/webhook-status
   ```

2. **Chạy ngrok** trong terminal mới:
   ```bash
   ngrok http 5130
   ```

3. **Copy URL từ ngrok**:
   ```
   Forwarding: https://abc123.ngrok.io -> http://localhost:5130
   ```
   Copy URL: `https://abc123.ngrok.io` (URL của bạn sẽ khác)

## 🔧 Bước 3: Config PayOs Webhook URL

### Trong PayOs Dashboard:

1. **Đăng nhập** vào PayOs dashboard
2. **Vào phần Settings** hoặc **Webhook Configuration**
3. **Tìm mục Webhook URL** hoặc **Callback URL**
4. **Nhập URL:**
   ```
   https://abc123.ngrok.io/api/simplepayment/webhook
   ```
   (Thay `abc123.ngrok.io` bằng URL từ ngrok của bạn)

5. **Save** configuration

## 🔧 Bước 4: Kiểm Tra Webhook Endpoint

### Test 1: Kiểm Tra Endpoint Có Hoạt Động Không

```bash
# Test webhook status endpoint
curl http://localhost:5130/api/simplepayment/webhook-status

# Kết quả mong đợi:
# {
#   "active": true,
#   "supportedFormats": ["SimpleWebhookRequest"],
#   "endpoint": "/api/simplepayment/webhook"
# }
```

### Test 2: Test Webhook Endpoint Qua Ngrok

```bash
# Test webhook qua ngrok URL
curl -X POST https://abc123.ngrok.io/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-6",
    "amount": 5000
  }'
```

**Lưu ý:** Thay `abc123.ngrok.io` bằng URL ngrok của bạn.

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 6,
  "bookingCode": "BKG2025006",
  "webhookId": "xxxxx"
}
```

### Test 3: Kiểm Tra Backend Logs

Khi test webhook qua ngrok, kiểm tra backend logs:
```
📥 [WEBHOOK-xxxx] Webhook received: BOOKING-6 - 5000 VND
✅ [WEBHOOK-xxxx] Booking BKG2025006 updated to Paid
```

**Nếu thấy logs này → Webhook endpoint hoạt động tốt!**

## 🔍 Bước 5: Kiểm Tra PayOs Có Gọi Webhook Không

### Cách 1: Xem Backend Logs

1. **Mở terminal backend**
2. **Quét QR và thanh toán** qua PayOs
3. **Quan sát logs** - sẽ thấy:
   ```
   📥 [WEBHOOK-xxxx] Webhook received: BOOKING-X - X VND
   ✅ [WEBHOOK-xxxx] Booking BKG2025XXX updated to Paid
   ```

**Nếu KHÔNG thấy logs:**
- ❌ PayOs chưa gọi webhook
- ✅ Cần kiểm tra PayOs config

### Cách 2: Xem Ngrok Requests

1. **Mở browser** → Vào `http://localhost:4040` (ngrok web interface)
2. **Quét QR và thanh toán** qua PayOs
3. **Quan sát ngrok dashboard** - sẽ thấy request đến `/api/simplepayment/webhook`

**Nếu KHÔNG thấy request:**
- ❌ PayOs chưa gọi webhook
- ✅ Cần kiểm tra PayOs config

### Cách 3: Kiểm Tra PayOs Dashboard

1. **Vào PayOs dashboard**
2. **Xem phần Webhook Logs** hoặc **Transaction History**
3. **Tìm transaction vừa thanh toán**
4. **Kiểm tra webhook status:**
   - ✅ Success → Webhook được gọi thành công
   - ❌ Failed → Webhook không được gọi hoặc lỗi

## 🐛 Troubleshooting

### Vấn Đề 1: Ngrok URL Thay Đổi Mỗi Lần

**Nguyên nhân:**
- Ngrok free plan tạo URL mới mỗi lần restart

**Giải pháp:**
- Dùng ngrok paid plan để có URL cố định
- Hoặc dùng domain/subdomain của riêng bạn
- Hoặc deploy backend lên server (Azure, AWS, etc.)

### Vấn Đề 2: PayOs Không Gọi Webhook

**Kiểm tra:**
1. Webhook URL có đúng format không?
2. URL có accessible từ internet không? (test bằng curl qua ngrok)
3. PayOs có enable webhook không?
4. Signature verification có bật không? (nếu có, cần config checksum key)

**Giải pháp:**
- Test webhook endpoint trước (dùng curl)
- Kiểm tra PayOs dashboard có error logs không
- Liên hệ PayOs support nếu cần

### Vấn Đề 3: Webhook Bị Lỗi 401/403

**Nguyên nhân:**
- Middleware block webhook request

**Giải pháp:**
- Đã fix: Webhook endpoint đã được thêm vào `PublicEndpoints` trong `JwtAuthorizationMiddleware.cs`
- Nếu vẫn lỗi, kiểm tra middleware config

### Vấn Đề 4: Webhook Bị Timeout

**Nguyên nhân:**
- Backend xử lý quá lâu
- Network issue

**Giải pháp:**
- Tối ưu code xử lý webhook
- Kiểm tra database connection
- Tăng timeout trong PayOs config (nếu có)

## 📝 Checklist

- [ ] Ngrok đã được cài đặt
- [ ] Ngrok đang chạy và expose port 5130
- [ ] Copy được ngrok URL (https://xxxx.ngrok.io)
- [ ] Test webhook endpoint qua ngrok thành công
- [ ] Config webhook URL trong PayOs dashboard
- [ ] PayOs webhook URL có format đúng: `https://xxxx.ngrok.io/api/simplepayment/webhook`
- [ ] Test thanh toán thật → Kiểm tra backend logs
- [ ] Backend logs có hiển thị webhook received
- [ ] Frontend polling phát hiện status = "Paid"
- [ ] QR tự động biến mất và success message hiện ra

## 🎯 Quick Test

### Test Nhanh Webhook Endpoint:

```bash
# 1. Chạy ngrok
ngrok http 5130

# 2. Copy URL từ ngrok (ví dụ: https://abc123.ngrok.io)

# 3. Test webhook qua ngrok
curl -X POST https://abc123.ngrok.io/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING-6", "amount": 5000}'

# 4. Kiểm tra backend logs
# Sẽ thấy: 📥 [WEBHOOK-xxxx] Webhook received
```

### Test PayOs Webhook:

1. **Config PayOs webhook URL:** `https://abc123.ngrok.io/api/simplepayment/webhook`
2. **Mở payment modal** trong browser
3. **Quét QR và thanh toán** qua PayOs
4. **Xem backend logs** → Sẽ thấy webhook received
5. **Quan sát browser** → QR tự động biến mất trong 5 giây

## ✅ Kết Luận

**Để PayOs gọi webhook thành công:**

1. ✅ **Expose localhost** bằng ngrok
2. ✅ **Config webhook URL** trong PayOs dashboard
3. ✅ **Test webhook endpoint** trước khi dùng
4. ✅ **Kiểm tra backend logs** khi thanh toán thật

**Sau khi config xong, khi user thanh toán thật:**
- PayOs tự động gọi webhook
- Backend update booking status = "Paid"
- Frontend polling phát hiện và tự động ẩn QR + hiện success message

