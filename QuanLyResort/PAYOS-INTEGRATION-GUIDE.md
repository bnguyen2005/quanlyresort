# 📹 Hướng Dẫn Tích Hợp PayOs (Từ Video)

**Video hướng dẫn:** https://www.youtube.com/watch?v=KFaHX3aWB7E

## 📋 Các Bước Tích Hợp PayOs

### Bước 1: Đăng Ký PayOs Merchant

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Đăng ký tài khoản** PayOs merchant
3. **Xác thực tài khoản** (theo hướng dẫn của PayOs)

### Bước 2: Tạo Ứng Dụng Trên PayOs

1. **Vào PayOs Dashboard** → **"Ứng dụng"** hoặc **"Applications"**
2. **Tạo ứng dụng mới**
3. **Lấy thông tin API:**
   - **Client ID**
   - **API Key** (hoặc Client Secret)
   - **Checksum Key**

### Bước 3: Deploy Ứng Dụng Lên Railway

1. **Đăng ký Railway:** https://railway.app
2. **Tạo project mới** và kết nối với GitHub repository
3. **Railway tự động detect** Dockerfile và deploy

### Bước 4: Config Environment Variables Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Thêm các biến sau:**

```env
# PayOs Configuration
BankWebhook__PayOs__ClientId=YOUR_CLIENT_ID
BankWebhook__PayOs__ApiKey=YOUR_API_KEY
BankWebhook__PayOs__ChecksumKey=YOUR_CHECKSUM_KEY
BankWebhook__PayOs__SecretKey=YOUR_CHECKSUM_KEY
BankWebhook__PayOs__VerifySignature=false
BankWebhook__PayOs__WebhookUrl=https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

### Bước 5: Cập Nhật Webhook URL Trên PayOs

1. **Vào PayOs Dashboard** → **Settings** → **Webhook**
2. **Nhập Webhook URL:**
   ```
   https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```
3. **PayOs sẽ tự động verify** webhook URL

**⚠️ Lưu ý:** Nếu PayOs báo lỗi 404, có thể:
- Đợi 10-15 phút để PayOs verify
- Hoặc dùng Render URL tạm thời

### Bước 6: Test Integration

1. **Tạo booking mới**
2. **Click "Thanh toán"**
3. **Tạo payment link** (sẽ gọi PayOs API)
4. **Quét QR code và thanh toán**
5. **Kiểm tra webhook** nhận được từ PayOs

## ✅ Code Đã Được Implement

### PayOsService.cs

Service đã được implement với:
- ✅ Tạo payment link qua PayOs API
- ✅ Tính signature đúng format (HMAC-SHA256)
- ✅ Xử lý response từ PayOs
- ✅ Logging chi tiết

### SimplePaymentController.cs

Controller đã được implement với:
- ✅ Endpoint tạo payment link: `POST /api/simplepayment/create-link`
- ✅ Webhook endpoint: `POST /api/simplepayment/webhook`
- ✅ Verify webhook endpoint: `GET /api/simplepayment/webhook`
- ✅ Xử lý PayOs webhook format

## 🔍 Kiểm Tra Integration

### 1. Kiểm Tra Environment Variables

**Trên Railway:**
```env
BankWebhook__PayOs__ClientId=90ad103f-aa49-4c33-9692-76d739a68b1b
BankWebhook__PayOs__ApiKey=acb138f1-a0f0-4a1f-9692-16d54332a580
BankWebhook__PayOs__ChecksumKey=44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c
```

### 2. Kiểm Tra Webhook URL

**Test endpoint:**
```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

### 3. Test Tạo Payment Link

1. **Login để lấy token**
2. **Tạo payment link:**
   ```bash
   curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/create-link" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{"bookingId": 4}'
   ```

### 4. Test Webhook

Sau khi thanh toán, kiểm tra Railway logs:
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: 4
✅ Booking 4 updated to Paid successfully!
```

## 🐛 Vấn Đề Hiện Tại

### PayOs Không Verify Được Railway URL

**Lỗi:**
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Webhook url invalid"
}
```

**Giải pháp:**
1. **Liên hệ PayOs support** về vấn đề Railway domain
2. **Dùng Render URL tạm thời** nếu cần
3. **Đợi PayOs fix** (có thể mất vài ngày)

## 📋 Checklist

- [x] Đã đăng ký PayOs merchant
- [x] Đã lấy Client ID, API Key, Checksum Key
- [x] Đã deploy lên Railway
- [x] Đã config environment variables
- [ ] Đã config webhook URL trên PayOs (đang gặp vấn đề)
- [ ] PayOs đã verify webhook URL thành công
- [ ] Đã test tạo payment link
- [ ] Đã test thanh toán và nhận webhook

## 💡 Lưu Ý Từ Video

1. **Webhook URL phải chính xác:** Không có khoảng trắng, đúng format
2. **Environment variables:** Phải được config đúng trên Railway
3. **PayOs verify:** Có thể mất 10-15 phút để PayOs verify webhook URL
4. **Test thử nghiệm:** Nên test với số tiền nhỏ trước

## 🎯 Kết Quả Mong Đợi

Sau khi tích hợp thành công:
- ✅ PayOs webhook URL đã được config
- ✅ PayOs đã verify webhook URL thành công
- ✅ Có thể tạo payment link thành công
- ✅ PayOs gửi webhook sau khi thanh toán
- ✅ Booking status được update tự động thành "Paid"
- ✅ QR code tự động ẩn sau khi thanh toán

## 🔗 Links Quan Trọng

- **Video hướng dẫn:** https://www.youtube.com/watch?v=KFaHX3aWB7E
- **PayOs Dashboard:** https://payos.vn
- **Railway Dashboard:** https://railway.app
- **Railway Webhook:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

