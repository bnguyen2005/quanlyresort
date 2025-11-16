# 🔍 Checklist Debug: QR Code Không Ẩn Sau Khi Thanh Toán

## 📋 Vấn Đề

QR code không tự động ẩn sau khi thanh toán thành công.

## ✅ Checklist Kiểm Tra

### 1. SePay Webhook Đã Được Gửi?

**Kiểm tra Railway Logs:**
- [ ] Có logs: `[WEBHOOK] 📥 Webhook received`?
- [ ] Có logs: `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId`?
- [ ] Có logs: `[WEBHOOK] ✅ Booking found`?
- [ ] Có logs: `[WEBHOOK] ✅ Booking updated to Paid successfully!`?

**Nếu KHÔNG có logs webhook:**
→ SePay chưa gửi webhook thật (chỉ verify URL)
→ **Giải pháp:** Đảm bảo nội dung chuyển khoản = `BOOKING{id}` và SePay webhook đã được setup

### 2. Booking Status Đã Được Update?

**Kiểm tra Database hoặc API:**
```bash
# Test API
curl -X GET https://quanlyresort-production.up.railway.app/api/bookings/{id} \
  -H "Authorization: Bearer {token}"
```

**Kiểm tra:**
- [ ] Booking status = "Paid"?
- [ ] Nếu vẫn là "Pending" → Webhook không update được

**Nếu status vẫn là "Pending":**
→ Xem logs webhook có lỗi gì không
→ Kiểm tra database connection

### 3. Frontend Polling Có Chạy Không?

**Mở Browser Console (F12):**
- [ ] Có logs: `[FRONTEND] 🔄 [SimplePolling] Starting polling for booking: {id}`?
- [ ] Có logs: `[FRONTEND] 🔍 [SimplePolling] Poll #X - Status: ...`?
- [ ] Có logs: `[FRONTEND] ✅✅✅ [SimplePolling] ========== PAYMENT DETECTED ==========`?

**Nếu KHÔNG có logs polling:**
→ Frontend polling không chạy
→ **Giải pháp:** Kiểm tra `startSimplePolling()` có được gọi không

### 4. Frontend Có Detect Được Status "Paid"?

**Kiểm tra Browser Console:**
- [ ] Có logs: `[FRONTEND] 🔍 [SimplePolling] Poll #X - Raw status: 'Paid'`?
- [ ] Có logs: `[FRONTEND] 🔍 [SimplePolling] isPaid check: true`?
- [ ] Có logs: `[FRONTEND] ✅ [SimplePolling] Payment detected!`?

**Nếu KHÔNG detect được:**
→ Có thể status format khác (ví dụ: "paid" lowercase)
→ **Giải pháp:** Kiểm tra format status trong database

### 5. showPaymentSuccess() Có Được Gọi?

**Kiểm tra Browser Console:**
- [ ] Có logs: `[FRONTEND] 🎉🎉🎉 [showPaymentSuccess] ========== STARTING ==========`?
- [ ] Có logs: `[FRONTEND] ✅ [showPaymentSuccess] Hidden QR image`?
- [ ] Có logs: `[FRONTEND] ✅ [showPaymentSuccess] Showed success message`?

**Nếu KHÔNG có logs:**
→ `showPaymentSuccess()` không được gọi
→ **Giải pháp:** Kiểm tra polling có gọi `showPaymentSuccess()` không

### 6. QR Code Element Có Tồn Tại?

**Kiểm tra Browser Console:**
- [ ] Element `spQRImage` có tồn tại?
- [ ] Element `spSuccess` có tồn tại?
- [ ] Element `spWaiting` có tồn tại?

**Kiểm tra trong Console:**
```javascript
document.getElementById('spQRImage')
document.getElementById('spSuccess')
document.getElementById('spWaiting')
```

**Nếu KHÔNG tồn tại:**
→ HTML elements không đúng
→ **Giải pháp:** Kiểm tra HTML modal có đúng ID không

## 🔍 Các Trường Hợp Có Thể Xảy Ra

### Trường Hợp 1: Webhook Không Được Gửi

**Triệu chứng:**
- Không thấy logs webhook trong Railway
- Booking status vẫn là "Pending"

**Giải pháp:**
1. Kiểm tra SePay webhook status = Active
2. Kiểm tra nội dung chuyển khoản = `BOOKING{id}`
3. Test webhook thủ công

### Trường Hợp 2: Webhook Được Gửi Nhưng Không Update Status

**Triệu chứng:**
- Có logs webhook received
- Có logs extract booking ID
- Nhưng không có logs: `✅ Booking updated to Paid`

**Giải pháp:**
1. Kiểm tra logs có lỗi gì không
2. Kiểm tra database connection
3. Kiểm tra booking có tồn tại không

### Trường Hợp 3: Status Được Update Nhưng Frontend Không Detect

**Triệu chứng:**
- Có logs: `✅ Booking updated to Paid`
- Nhưng frontend polling không detect được

**Giải pháp:**
1. Kiểm tra status format (phải là "Paid" không phải "paid")
2. Kiểm tra frontend polling có chạy không
3. Kiểm tra API response có đúng không

### Trường Hợp 4: Frontend Detect Được Nhưng QR Không Ẩn

**Triệu chứng:**
- Có logs: `✅ Payment detected!`
- Nhưng QR code vẫn hiển thị

**Giải pháp:**
1. Kiểm tra `showPaymentSuccess()` có được gọi không
2. Kiểm tra QR element có tồn tại không
3. Kiểm tra CSS có override display không

## 🎯 Debug Steps

### Step 1: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

Tìm các dòng:
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId
[WEBHOOK] ✅ Booking found
[WEBHOOK] ✅ Booking updated to Paid successfully!
```

### Step 2: Kiểm Tra Browser Console

**Mở Browser Console (F12) → Console tab**

Tìm các dòng:
```
[FRONTEND] 🔄 [SimplePolling] Starting polling
[FRONTEND] 🔍 [SimplePolling] Poll #X - Status: Paid
[FRONTEND] ✅✅✅ [SimplePolling] PAYMENT DETECTED
[FRONTEND] 🎉 [showPaymentSuccess] Hidden QR image
```

### Step 3: Test API Trực Tiếp

**Test booking status:**
```bash
curl -X GET https://quanlyresort-production.up.railway.app/api/bookings/{id} \
  -H "Authorization: Bearer {token}"
```

**Kiểm tra:**
- `status` field = "Paid"?

### Step 4: Test Webhook Thủ Công

**Test với booking ID có thật:**
```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING{id}",
    "transferAmount": {amount},
    "transferType": "IN"
  }'
```

**Sau đó kiểm tra:**
- Railway logs có nhận được không?
- Booking status có update không?

## 📊 Thông Tin Cần Cung Cấp

Nếu vẫn không hoạt động, cung cấp:

1. **Railway Logs:**
   - Từ khi thanh toán đến bây giờ
   - Tìm các dòng có `[WEBHOOK]`

2. **Browser Console Logs:**
   - Mở F12 → Console
   - Copy tất cả logs từ khi mở modal thanh toán

3. **Booking ID:**
   - Booking ID thực tế đang test
   - Booking status hiện tại (Pending/Paid?)

4. **SePay Webhook:**
   - SePay có gửi webhook không?
   - Webhook status trong SePay dashboard?

## 🔗 Links

- **Railway Logs:** Railway Dashboard → Service → Logs
- **Browser Console:** F12 → Console tab
- **API Test:** https://quanlyresort-production.up.railway.app/api/bookings/{id}

