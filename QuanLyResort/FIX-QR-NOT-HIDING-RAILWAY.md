# 🔧 Fix QR Code Không Ẩn Sau Khi Thanh Toán

## ❌ Vấn Đề

Thanh toán đã thành công nhưng QR code chưa ẩn. Có thể PayOs chưa gửi webhook đến Railway.

## ✅ Giải Pháp

### Bước 1: Kiểm Tra PayOs Webhook URL

PayOs cần được cấu hình để gửi webhook đến Railway URL.

**Webhook URL cần:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

### Bước 2: Cập Nhật Webhook URL Trên PayOs (Qua API)

Vì PayOs Dashboard có thể không hoạt động, dùng API trực tiếp:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
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

### Bước 3: Kiểm Tra Logs Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Logs"**
3. **Tìm webhook requests sau khi thanh toán:**

✅ **Nếu thấy:**
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: {BookingId}
✅ Booking {BookingId} updated to Paid successfully!
```
→ Webhook đã hoạt động, booking đã được update

❌ **Nếu không thấy:**
→ PayOs chưa gửi webhook đến Railway

### Bước 4: Kiểm Tra Booking Status

Sau khi thanh toán, kiểm tra booking status:

```bash
# Thay {bookingId} bằng booking ID thật
curl -H "Authorization: Bearer {token}" \
  https://quanlyresort-production.up.railway.app/api/bookings/{bookingId}
```

**Kiểm tra:**
- `status` phải là `"Paid"` (không phải `"Pending"` hoặc `"Confirmed"`)

### Bước 5: Kiểm Tra Frontend Polling

Mở browser console (F12) và xem logs:

✅ **Nếu thấy:**
```
[FRONTEND] 🔍 [SimplePolling] Poll #X - Status: Paid
[FRONTEND] ✅✅✅ [SimplePolling] ========== PAYMENT DETECTED ==========
[FRONTEND] 🎉 [SimplePolling] Calling showPaymentSuccess()...
```
→ Frontend đã detect payment, QR sẽ ẩn

❌ **Nếu thấy:**
```
[FRONTEND] ⏳ [SimplePolling] Still waiting... Status: 'Pending'
```
→ Booking status chưa được update thành "Paid"

## 🔍 Debug Steps

### 1. Kiểm Tra Webhook Có Được Gửi Không

Sau khi thanh toán, đợi 10-30 giây và kiểm tra Railway logs:

**Tìm:**
- Requests từ PayOs (IP hoặc User-Agent có "PayOs")
- Logs có chứa `[WEBHOOK]`

### 2. Kiểm Tra Booking ID Trong Description

Khi thanh toán, đảm bảo nội dung chuyển khoản là:
- `BOOKING{id}` (ví dụ: `BOOKING4`)
- Hoặc để PayOs tự động lấy từ payment link

**Không dùng:**
- `VQRIO123` ❌ (không phải booking ID)

### 3. Kiểm Tra Booking Status Thủ Công

Nếu webhook không hoạt động, có thể update booking status thủ công:

1. **Vào Swagger UI:**
   ```
   https://quanlyresort-production.up.railway.app/swagger
   ```

2. **Tìm endpoint:** `PUT /api/bookings/{id}/status`
3. **Update status thành:** `"Paid"`

## 🐛 Troubleshooting

### Lỗi: PayOs Không Gửi Webhook

**Nguyên nhân:**
- Webhook URL chưa được cấu hình trên PayOs
- PayOs chưa verify được webhook URL

**Giải pháp:**
1. Gọi API để config webhook URL (xem Bước 2)
2. Đợi 5-10 phút để PayOs verify
3. Test lại thanh toán

### Lỗi: Webhook Nhận Được Nhưng Không Extract Được Booking ID

**Nguyên nhân:**
- Description không đúng format
- Description là `VQRIO123` thay vì `BOOKING4`

**Giải pháp:**
- Đảm bảo khi thanh toán, nội dung là `BOOKING{id}`
- Hoặc để PayOs tự động lấy từ payment link

### Lỗi: Booking Status Chưa Update

**Nguyên nhân:**
- Webhook không extract được booking ID
- Webhook không tìm thấy booking

**Giải pháp:**
1. Kiểm tra logs để xem booking ID có được extract không
2. Kiểm tra booking có tồn tại không
3. Update booking status thủ công nếu cần

### Lỗi: Frontend Polling Không Detect

**Nguyên nhân:**
- Booking status chưa được update thành "Paid"
- Frontend polling bị dừng

**Giải pháp:**
1. Kiểm tra booking status (xem Bước 4)
2. Mở browser console và xem polling logs
3. Reload page và mở lại payment modal

## 📋 Checklist

- [ ] Đã cập nhật webhook URL trên PayOs (qua API)
- [ ] Đã đợi 5-10 phút để PayOs verify
- [ ] Đã kiểm tra Railway logs sau khi thanh toán
- [ ] Đã kiểm tra booking status (phải là "Paid")
- [ ] Đã kiểm tra frontend polling logs
- [ ] Đã test lại thanh toán

## 💡 Lưu Ý

- **Webhook URL phải chính xác:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Description phải đúng format:** `BOOKING{id}` (không phải `VQRIO123`)
- **Frontend polling:** Kiểm tra mỗi 3 giây, có thể mất 3-6 giây để detect
- **Webhook delay:** PayOs có thể mất 10-30 giây để gửi webhook sau khi thanh toán

## 🎯 Kết Quả Mong Đợi

Sau khi fix:
1. ✅ PayOs gửi webhook đến Railway
2. ✅ Webhook extract được booking ID
3. ✅ Booking status được update thành "Paid"
4. ✅ Frontend polling detect được status "Paid"
5. ✅ QR code tự động ẩn
6. ✅ Hiển thị "Thanh toán thành công"

