# 🔍 Kiểm Tra Webhook PayOs

## ✅ Kết Quả Kiểm Tra

### 1. Test GET Request (PayOs Verification)

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2025-11-13T..."
}
```

✅ **Endpoint hoạt động tốt!**

### 2. Test POST Request (Empty Body - Verification)

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d ''
```

**Kết quả:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2025-11-13T..."
}
```

✅ **Endpoint xử lý verification request tốt!**

### 3. Test Webhook Status

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status
```

**Kết quả:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "2025-11-13T...",
  "supportedFormats": [
    "BOOKING-{id}",
    "BOOKING-BKG{id}",
    "{id} (direct booking ID)"
  ],
  "message": "Webhook system is ready to receive payments"
}
```

✅ **Webhook system sẵn sàng!**

## 📋 Checklist Kiểm Tra Webhook

### ✅ Endpoint Hoạt Động

- [x] GET `/api/simplepayment/webhook` - ✅ Hoạt động
- [x] POST `/api/simplepayment/webhook` (empty body) - ✅ Hoạt động
- [x] GET `/api/simplepayment/webhook-status` - ✅ Hoạt động

### ⚠️ Cần Kiểm Tra

- [ ] PayOs webhook URL đã được config chưa
- [ ] PayOs có gửi webhook đến Railway không
- [ ] Railway logs có nhận được webhook không
- [ ] Booking status có được update không

## 🔍 Kiểm Tra Chi Tiết

### 1. Kiểm Tra PayOs Webhook URL

**Trên PayOs Dashboard:**
1. Vào: https://payos.vn
2. Settings → Webhook
3. Kiểm tra webhook URL:
   - Railway: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Hoặc Render: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
4. Trạng thái: "Active" hoặc "Inactive"

### 2. Kiểm Tra Railway Logs

**Vào Railway Dashboard:**
1. Service `quanlyresort`
2. Tab "Logs"
3. Tìm:
   - `[WEBHOOK-VERIFY]` - PayOs verification requests
   - `[WEBHOOK] 📥` - Webhook received
   - `✅✅✅ SUCCESS` - Booking ID extracted
   - `✅ Booking updated to Paid` - Payment processed

**Nếu thấy:**
```
[WEBHOOK-VERIFY] PayOs verification request received
```
→ PayOs đã verify webhook URL thành công

**Nếu thấy:**
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: {BookingId}
✅ Booking {BookingId} updated to Paid successfully!
```
→ Webhook đã hoạt động và xử lý thanh toán thành công

### 3. Test Webhook Thủ Công

**Test với booking ID thật:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "code": "00",
    "desc": "success",
    "data": {
      "orderCode": 123,
      "amount": 5000,
      "description": "BOOKING4",
      "reference": "TEST-123456"
    }
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "bookingCode": "BKG2025004"
}
```

### 4. Kiểm Tra Environment Variables

**Trên Railway Dashboard:**
1. Service `quanlyresort`
2. Tab "Variables"
3. Kiểm tra:

```env
BankWebhook__PayOs__ClientId=90ad103f-aa49-4c33-9692-76d739a68b1b
BankWebhook__PayOs__ApiKey=acb138f1-a0f0-4a1f-9692-16d54332a580
BankWebhook__PayOs__ChecksumKey=44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c
BankWebhook__PayOs__WebhookUrl=https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

## 🧪 Test Full Flow

### 1. Tạo Payment Link

1. Tạo booking mới hoặc chọn booking chưa thanh toán
2. Click "Thanh toán"
3. Tạo payment link
4. Kiểm tra logs:

✅ **Thành công:**
```
[PAYOS] ✅ Payment link created successfully
[PAYOS] Payment URL: https://pay.payos.vn/web/...
```

### 2. Thanh Toán

1. Quét QR code
2. Thanh toán với nội dung: `BOOKING{id}` (ví dụ: `BOOKING4`)
3. Xác nhận thanh toán

### 3. Kiểm Tra Webhook

Sau khi thanh toán, đợi 10-30 giây và kiểm tra Railway logs:

✅ **Thành công:**
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: 4
✅ Booking 4 updated to Paid successfully!
```

### 4. Kiểm Tra Frontend

1. Mở browser console (F12)
2. Kiểm tra polling logs:

✅ **Thành công:**
```
[FRONTEND] 🔍 [SimplePolling] Poll #X - Status: Paid
[FRONTEND] ✅✅✅ [SimplePolling] ========== PAYMENT DETECTED ==========
[FRONTEND] 🎉 [SimplePolling] Calling showPaymentSuccess()...
```

3. QR code sẽ tự động ẩn
4. Hiển thị "Thanh toán thành công"

## 🐛 Troubleshooting

### Lỗi: PayOs Không Gửi Webhook

**Kiểm tra:**
1. PayOs webhook URL đã được config chưa
2. PayOs webhook status là "Active" chưa
3. Railway logs có nhận được verification request không

**Giải pháp:**
- Config lại webhook URL qua API
- Đợi 10-15 phút để PayOs verify
- Kiểm tra PayOs Dashboard

### Lỗi: Webhook Nhận Được Nhưng Không Extract Được Booking ID

**Kiểm tra:**
1. Description có đúng format không (`BOOKING{id}`)
2. Railway logs có log extraction không

**Giải pháp:**
- Đảm bảo description là `BOOKING{id}` khi thanh toán
- Kiểm tra logs để xem description nhận được là gì

### Lỗi: Booking Status Không Update

**Kiểm tra:**
1. Booking ID có được extract không
2. Booking có tồn tại không
3. Railway logs có lỗi gì không

**Giải pháp:**
- Kiểm tra logs để xem booking ID
- Kiểm tra booking có tồn tại trong database không
- Update booking status thủ công nếu cần

## 📊 Tóm Tắt

### ✅ Đã Hoạt Động

- Endpoint webhook hoạt động tốt
- GET và POST requests được xử lý đúng
- Webhook status endpoint hoạt động

### ⚠️ Cần Kiểm Tra

- PayOs webhook URL đã được config chưa
- PayOs có gửi webhook đến Railway không
- Booking status có được update không

## 🎯 Kết Luận

**Webhook endpoint đã sẵn sàng và hoạt động tốt!**

Cần đảm bảo:
1. PayOs webhook URL đã được config
2. PayOs có thể gửi webhook đến Railway
3. Test với thanh toán thật để verify full flow

## 🔗 URLs Quan Trọng

- **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
- **PayOs Dashboard:** https://payos.vn
- **Railway Logs:** Railway Dashboard → Service → Logs

