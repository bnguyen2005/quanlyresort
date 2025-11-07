# 🧪 Hướng Dẫn Test Chức Năng Thanh Toán Tự Động

## 📋 Checklist Test

### ✅ Bước 1: Kiểm Tra Webhook System

**Test webhook status endpoint:**
```bash
curl http://localhost:5130/api/simplepayment/webhook-status
```

**Expected Response:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "...",
  "supportedFormats": [...],
  "message": "Webhook system is ready to receive payments"
}
```

✅ **Pass nếu:** Trả về JSON với `status: "active"`

---

### ✅ Bước 2: Tìm Booking ID Để Test

**Option A: Dùng Script**
```bash
./find-booking-id.sh
```

**Option B: Mở Browser**
1. Mở `http://localhost:5130/customer/my-bookings.html`
2. Đăng nhập với tài khoản customer
3. Xem danh sách booking
4. Chọn booking có status = "Pending" hoặc "Confirmed"
5. Lấy booking ID từ URL hoặc Developer Console

**Option C: Kiểm Tra Trực Tiếp**
```bash
# Thử các ID phổ biến
curl http://localhost:5130/api/bookings/39 -H "Authorization: Bearer TOKEN"
```

✅ **Pass nếu:** Tìm được booking ID có status = "Pending"

---

### ✅ Bước 3: Mở Payment Modal

1. Mở `http://localhost:5130/customer/my-bookings.html`
2. Đăng nhập
3. Tìm booking có status "Pending"
4. Click nút **"Thanh toán"** hoặc **"Pay"**
5. Payment modal sẽ mở với QR code

**Kiểm tra:**
- ✅ Modal mở thành công
- ✅ QR code hiển thị
- ✅ Amount hiển thị đúng (ví dụ: 10,000 VND)
- ✅ Booking code hiển thị đúng
- ✅ Bank info hiển thị (MB Bank, số tài khoản)

**Browser Console sẽ hiển thị:**
```
✅ [openSimplePayment] Using amount from backend: 10000
✅ [updatePaymentModal] QR image set
🔄 [SimplePolling] Starting polling for booking: 4
🔍 [SimplePolling] Booking status: Pending for booking: 4
```

✅ **Pass nếu:** Modal mở, QR code hiển thị, polling bắt đầu

---

### ✅ Bước 4: Test Webhook (Simulate Payment)

**Mở terminal và chạy:**
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-4",
    "amount": 10000,
    "transactionId": "TEST-123456"
  }'
```

**Hoặc dùng script:**
```bash
./test-webhook.sh 4 10000
```

**Expected Response:**
```json
{
    "success": true,
    "message": "Thanh toán thành công",
    "bookingId": 4,
    "bookingCode": "BKG2025004",
    "webhookId": "abc12345",
    "processedAt": "2025-11-06T...",
    "durationMs": 70
}
```

✅ **Pass nếu:** Response có `success: true` và `webhookId`

---

### ✅ Bước 5: Kiểm Tra Backend Console Logs

**Xem terminal chạy backend**, bạn sẽ thấy:

```
═══════════════════════════════════════════════════════════
📥 [WEBHOOK-abc12345] Webhook received at 2025-11-06 10:30:00
   Content: BOOKING-4
   Amount: 10,000 VND
   TransactionId: TEST-123456
   IP Address: 127.0.0.1

🔍 [WEBHOOK-abc12345] Extracting booking ID from content...
✅ [WEBHOOK-abc12345] Extracted booking ID: 4
🔍 [WEBHOOK-abc12345] Fetching booking 4...
✅ [WEBHOOK-abc12345] Booking found: Code=BKG2025004, Status=Pending, Amount=10,000 VND
🔄 [WEBHOOK-abc12345] Updating booking 4 to Paid status...
✅ [WEBHOOK-abc12345] Booking 4 (BKG2025004) updated to Paid successfully!
⏱️ [WEBHOOK-abc12345] Processing time: 70ms
═══════════════════════════════════════════════════════════
```

✅ **Pass nếu:** Thấy logs đầy đủ với unique webhook ID

---

### ✅ Bước 6: Kiểm Tra Frontend Polling Phát Hiện Payment

**Mở Browser Developer Console (F12)**, sau 5-10 giây bạn sẽ thấy:

```
🔍 [SimplePolling] Booking status: Pending for booking: 4
🔍 [SimplePolling] Booking status: Pending for booking: 4
🔍 [SimplePolling] Booking status: Paid for booking: 4  ← Phát hiện!
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
✅ Thanh toán thành công!
```

✅ **Pass nếu:** Polling phát hiện status = "Paid" và log "Payment detected!"

---

### ✅ Bước 7: Kiểm Tra UI Tự Động Update

**Sau khi polling phát hiện payment, UI sẽ tự động:**

1. ✅ **QR code biến mất**
   - QR image `display: none`
   - QR section ẩn

2. ✅ **Success message hiện**
   - Message: "✅ Thanh toán thành công!"
   - Màu xanh (success)
   - Hiển thị rõ ràng

3. ✅ **Waiting message ẩn**
   - "Đang chờ thanh toán..." biến mất

4. ✅ **Modal tự động đóng** (sau 2 giây)
   - Modal tự động hide
   - Trở về danh sách booking

5. ✅ **Booking list tự động reload**
   - Danh sách booking refresh
   - Booking đã thanh toán hiển thị badge "Paid" (màu xanh)

✅ **Pass nếu:** Tất cả các thay đổi UI tự động xảy ra

---

### ✅ Bước 8: Kiểm Tra Database

**Kiểm tra booking đã được update trong database:**

```bash
curl http://localhost:5130/api/bookings/4 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kiểm tra:**
- ✅ `status` = `"Paid"` (không phải "Pending")
- ✅ `invoice` được tạo
  - `invoice.invoiceNumber` có giá trị
  - `invoice.status` = "Paid"
  - `invoice.paidAt` có giá trị
- ✅ `paidAt` có giá trị (nếu có field này)

✅ **Pass nếu:** Booking status = "Paid" và invoice được tạo

---

## 🎯 Test Flow Hoàn Chỉnh

### Scenario 1: Test End-to-End (Khuyến Nghị)

1. **Chuẩn bị:**
   - Backend đang chạy
   - Đã đăng nhập với customer account
   - Có booking với status "Pending"

2. **Thực hiện:**
   ```bash
   # Terminal 1: Mở my-bookings.html và click "Thanh toán"
   # Terminal 2: Chạy webhook test
   ./test-webhook.sh [booking_id] [amount]
   ```

3. **Quan sát:**
   - Browser: UI tự động update (QR biến mất, success hiện)
   - Browser Console: Polling phát hiện "Paid"
   - Backend Console: Logs chi tiết với webhook ID

4. **Kết quả:**
   - ✅ Booking status = "Paid"
   - ✅ Invoice được tạo
   - ✅ UI tự động update
   - ✅ Modal tự động đóng

---

### Scenario 2: Test Với Ngân Hàng Thực (Production)

1. **Cấu hình webhook URL** trong PayOs/VietQR:
   ```
   https://your-domain.com/api/simplepayment/webhook
   ```

2. **Tạo booking** và mở payment modal

3. **Quét QR code** và thanh toán thực bằng app ngân hàng

4. **Ngân hàng sẽ tự động gọi webhook** với:
   - Content: "BOOKING-{id}"
   - Amount: Số tiền đã chuyển
   - TransactionId: Mã giao dịch từ ngân hàng

5. **Hệ thống tự động:**
   - Nhận webhook
   - Update booking status
   - Frontend polling phát hiện và update UI

---

## 📊 Test Checklist Summary

- [ ] Webhook status endpoint trả về "active"
- [ ] Tìm được booking ID để test
- [ ] Payment modal mở và hiển thị QR code
- [ ] Polling bắt đầu chạy
- [ ] Webhook test thành công (success: true)
- [ ] Backend logs hiển thị đầy đủ
- [ ] Polling phát hiện status = "Paid"
- [ ] QR code biến mất
- [ ] Success message hiện
- [ ] Modal tự động đóng
- [ ] Booking list tự động reload
- [ ] Booking status = "Paid" trong database
- [ ] Invoice được tạo

---

## 🔍 Troubleshooting

### Nếu webhook trả về 404:
- ✅ Restart backend: `dotnet run`
- ✅ Kiểm tra route: `/api/simplepayment/webhook`

### Nếu polling không phát hiện "Paid":
- ✅ Kiểm tra booking status đã được update chưa
- ✅ Kiểm tra console logs để tìm lỗi
- ✅ Kiểm tra token có còn hợp lệ không

### Nếu UI không tự động update:
- ✅ Kiểm tra console logs
- ✅ Kiểm tra polling có đang chạy không
- ✅ Refresh trang và thử lại

---

## 📝 Quick Test Command

```bash
# 1. Check status
curl http://localhost:5130/api/simplepayment/webhook-status

# 2. Test webhook (thay booking_id và amount)
./test-webhook.sh [booking_id] [amount]

# 3. Check booking status
curl http://localhost:5130/api/bookings/[booking_id] \
  -H "Authorization: Bearer TOKEN"
```

---

## ✅ Kết Luận

Nếu tất cả các bước trên đều pass, **chức năng thanh toán tự động đã hoạt động hoàn hảo!** 🎉

