# ⚡ Quick Test Guide - Thanh Toán Tự Động

## 🚀 Test Nhanh (3 Bước)

### Bước 1: Mở Payment Modal

1. Mở: `http://localhost:5130/customer/my-bookings.html`
2. Đăng nhập
3. Click **"Thanh toán"** cho booking có status "Pending"
4. Modal mở với QR code → ✅ OK

---

### Bước 2: Test Webhook

**Mở terminal và chạy:**
```bash
./quick-test-payment.sh [booking_id] [amount]

# Example:
./quick-test-payment.sh 4 10000
```

**Hoặc manual:**
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content":"BOOKING-4","amount":10000,"transactionId":"TEST-123"}'
```

**Expected:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "webhookId": "abc12345"
}
```

---

### Bước 3: Kiểm Tra Kết Quả

**Trong Browser (F12 Console):**
```
🔍 [SimplePolling] Booking status: Pending
🔍 [SimplePolling] Booking status: Paid  ← Phát hiện!
✅ [SimplePolling] Payment detected!
✅ Thanh toán thành công!
```

**UI Tự Động:**
- ✅ QR code biến mất
- ✅ Success message hiện
- ✅ Modal tự động đóng (sau 2 giây)
- ✅ Booking list reload với status "Paid"

---

## ✅ Checklist Nhanh

- [ ] Webhook status: `curl http://localhost:5130/api/simplepayment/webhook-status`
- [ ] Payment modal mở và hiển thị QR
- [ ] Polling đang chạy (xem console logs)
- [ ] Webhook test thành công (success: true)
- [ ] Backend logs hiển thị (xem terminal backend)
- [ ] Polling phát hiện "Paid" (xem browser console)
- [ ] UI tự động update (QR biến mất, success hiện)

---

## 🔍 Xem Logs

### Backend Console
```
📥 [WEBHOOK-xxxxx] Webhook received...
✅ [WEBHOOK-xxxxx] SUCCESS! Booking updated to Paid
```

### Browser Console
```
🔄 [SimplePolling] Starting polling...
✅ [SimplePolling] Payment detected!
```

---

## ❌ Nếu Có Lỗi

1. **404 Not Found:**
   - Restart backend: `dotnet run`

2. **Booking không tồn tại:**
   - Tìm booking ID khác: `./find-booking-id.sh`

3. **Polling không phát hiện:**
   - Kiểm tra booking status đã update chưa
   - Xem console logs để tìm lỗi

---

## 📝 Full Guide

Xem file `TEST-THANH-TOAN-TU-DONG.md` để có hướng dẫn chi tiết đầy đủ.

