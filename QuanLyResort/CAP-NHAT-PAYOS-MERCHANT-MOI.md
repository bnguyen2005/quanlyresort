# 🔧 Cập Nhật PayOs Merchant Mới

## ✅ Thông Tin PayOs Merchant Mới

- **Client ID:** `90ad103f-aa49-4c33-9692-76d739a68b1b`
- **Api Key:** `acb138f1-a0f0-4a1f-9692-16d54332a580`
- **Checksum Key:** `44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c`
- **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 📋 Các Bước Cập Nhật

### Bước 1: Cập Nhật Environment Variables Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Cập nhật các biến sau:**

```env
BankWebhook__PayOs__ClientId=90ad103f-aa49-4c33-9692-76d739a68b1b
BankWebhook__PayOs__ApiKey=acb138f1-a0f0-4a1f-9692-16d54332a580
BankWebhook__PayOs__ChecksumKey=44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c
BankWebhook__PayOs__SecretKey=44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c
BankWebhook__PayOs__VerifySignature=false
BankWebhook__PayOs__WebhookUrl=https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**⚠️ LƯU Ý:**
- **ChecksumKey** và **SecretKey** có thể giống nhau
- **WebhookUrl** phải là Railway URL
- **Không có khoảng trắng** ở đầu/cuối giá trị

### Bước 2: Config Webhook URL Qua API

Sau khi cập nhật environment variables, config webhook URL:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"
  }
}
```

**⚠️ Nếu vẫn báo 404:**
- PayOs có thể cần thời gian để verify (10-15 phút)
- Hoặc PayOs có vấn đề với Railway domain
- Có thể dùng Render URL tạm thời: `https://quanlyresort.onrender.com/api/simplepayment/webhook`

### Bước 3: Redeploy Railway Service

1. **Save** tất cả environment variables
2. **Tab "Deployments"** → **"Redeploy"**
3. **Chọn "Deploy"**

### Bước 4: Kiểm Tra Sau Khi Cập Nhật

#### 1. Kiểm Tra Logs

Vào Railway Dashboard → Logs và tìm:

✅ **Thành công:**
```
[PAYOS] ✅ Service initialized with ClientId: 90ad103f
```

#### 2. Test Tạo Payment Link

1. Tạo booking mới
2. Click "Thanh toán"
3. Tạo payment link
4. Kiểm tra logs:

✅ **Thành công:**
```
[PAYOS] ✅ Payment link created successfully
[PAYOS] Payment URL: https://pay.payos.vn/web/...
```

#### 3. Test Webhook

Sau khi thanh toán thành công, PayOs sẽ gửi webhook. Kiểm tra logs:

✅ **Thành công:**
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: {BookingId}
✅ Booking {BookingId} updated to Paid successfully!
```

## 🔍 Kiểm Tra Webhook URL

Sau khi config webhook URL, đợi 5-10 phút và kiểm tra:

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Settings** → **Webhook**
3. **Kiểm tra webhook URL:**
   - Phải là: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Trạng thái: "Active" (không còn "không hoạt động")

## ⚠️ Lưu Ý Quan Trọng

PayOs merchant mới vẫn có thể báo 404 khi verify Railway URL. Đây có thể là vấn đề với PayOs và Railway domain, không phải với merchant.

**Giải pháp tạm thời:**
- Có thể dùng Render URL: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- Hoặc đợi PayOs fix (có thể mất vài giờ đến vài ngày)

## 🐛 Troubleshooting

### Lỗi: "Webhook url invalid" hoặc 404

**Giải pháp:**
1. Kiểm tra Railway service đang chạy
2. Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. Đợi 10-15 phút và thử lại API

### Lỗi: "Mã kiểm tra(signature) không hợp lệ"

**Giải pháp:**
1. Kiểm tra ChecksumKey đã copy đúng chưa
2. Đảm bảo không có khoảng trắng ở đầu/cuối
3. Redeploy sau khi cập nhật

### Lỗi: "ClientId không hợp lệ"

**Giải pháp:**
- Kiểm tra Client ID đã copy đúng chưa
- Lấy từ PayOs Dashboard → Settings → API Keys

## 📋 Checklist

- [ ] Đã cập nhật `BankWebhook__PayOs__ClientId` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__ApiKey` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__ChecksumKey` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__SecretKey` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__WebhookUrl` trên Railway
- [ ] Đã gọi PayOs API để config webhook URL
- [ ] Đã redeploy Railway service
- [ ] Đã kiểm tra logs (Service initialized với ClientId mới)
- [ ] Đã test tạo payment link
- [ ] Đã kiểm tra PayOs Dashboard (webhook URL đã được cập nhật)

## 💡 Lưu Ý

- **Merchant mới:** Tất cả thông tin API đã thay đổi
- **Webhook URL:** Có thể config Railway URL ngay từ đầu
- **Redeploy:** Cần redeploy để load environment variables mới
- **Test:** Test tạo payment link và thanh toán để verify

## 🎯 Kết Quả Mong Đợi

Sau khi cập nhật:
- ✅ PayOs merchant mới đã được cấu hình
- ✅ Webhook URL đã được cập nhật sang Railway
- ✅ Payment link có thể tạo thành công
- ✅ Webhook có thể nhận được từ PayOs khi thanh toán thành công
- ✅ QR code sẽ tự động ẩn sau khi thanh toán

## 🔗 URLs Quan Trọng

- **Railway URL:** `https://quanlyresort-production.up.railway.app`
- **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
- **PayOs Dashboard:** https://payos.vn
- **PayOs API:** `https://api-merchant.payos.vn/confirm-webhook`

