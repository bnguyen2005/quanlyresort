# Hướng Dẫn Debug Thanh Toán

## 🐛 Vấn Đề: QR Code Không Tắt, Không Hiển Thị "Thanh Toán Thành Công"

## ✅ Đã Sửa

1. **Thêm logging chi tiết** vào polling và showPaymentSuccess
2. **Status check case-insensitive** (xử lý "Paid", "paid", "PAID")
3. **Đảm bảo QR section hiển thị** khi mở modal

## 🧪 Cách Debug

### Bước 1: Mở Browser Console
- Nhấn `F12` hoặc `Ctrl+Shift+I` (Windows/Linux) hoặc `Cmd+Option+I` (Mac)
- Chuyển sang tab **Console**

### Bước 2: Test Thanh Toán
1. Login as customer
2. Vào "My Bookings"
3. Click "Thanh toán" trên booking chưa thanh toán
4. Xem console logs

### Bước 3: Kiểm Tra Logs

**Khi mở modal:**
```
✅ [updatePaymentModal] QR image set, display: block
✅ [updatePaymentModal] QR section set, display: block
🔄 [SimplePolling] Starting polling for booking: 39
```

**Khi polling (mỗi 5 giây):**
```
🔍 [SimplePolling] Booking status: Pending for booking: 39
🔍 [SimplePolling] Booking status: Pending for booking: 39
...
```

**Khi thanh toán thành công:**
```
🔍 [SimplePolling] Booking status: Paid for booking: 39
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
🎉 [showPaymentSuccess] Showing payment success...
✅ [showPaymentSuccess] Hidden waiting message
✅ [showPaymentSuccess] Showed success message
✅ [showPaymentSuccess] Hidden QR image
✅ [showPaymentSuccess] Hidden QR section
✅ [showPaymentSuccess] Completed
```

## 🔍 Các Vấn Đề Có Thể Gặp

### 1. Polling Không Chạy
**Triệu chứng:** Không thấy logs `[SimplePolling]`

**Nguyên nhân:**
- Script `simple-payment.js` chưa được load
- Modal chưa được mở đúng cách

**Giải pháp:**
- Kiểm tra Network tab xem script có load không
- Kiểm tra console có lỗi JavaScript không
- Đảm bảo `openSimplePayment()` được gọi

### 2. Status Không Match
**Triệu chứng:** Logs hiển thị status nhưng không detect "Paid"

**Nguyên nhân:**
- Status format khác (ví dụ: "PAID" thay vì "Paid")
- Status có whitespace

**Giải pháp:**
- Đã sửa: Status check case-insensitive + trim whitespace
- Kiểm tra backend trả về status đúng format

### 3. Elements Không Tồn Tại
**Triệu chứng:** Logs hiển thị `⚠️ element not found`

**Nguyên nhân:**
- Modal HTML chưa có đúng IDs
- Modal chưa được render

**Giải pháp:**
- Kiểm tra modal HTML có đúng IDs:
  - `spWaiting`
  - `spSuccess`
  - `spQRImage`
  - `spQRSection`
- Đảm bảo modal được render trong DOM

### 4. Polling Không Phát Hiện Status Change
**Triệu chứng:** Status vẫn là "Pending" sau khi gọi webhook

**Nguyên nhân:**
- Webhook chưa update database
- Backend chưa restart
- Database chưa được update

**Giải pháp:**
1. Test webhook:
   ```bash
   ./test-simple-webhook.sh {bookingId}
   ```
2. Kiểm tra response có `"success": true` không
3. Kiểm tra database: `SELECT Status FROM Bookings WHERE BookingId = {id}`
4. Restart backend nếu cần

## 📋 Checklist Debug

- [ ] Browser console mở (F12)
- [ ] Script `simple-payment.js` đã load
- [ ] Modal mở đúng cách (check console logs)
- [ ] Polling đang chạy (logs mỗi 5 giây)
- [ ] Webhook được gọi (check backend logs)
- [ ] Database được update (status = "Paid")
- [ ] Frontend polling detect status change
- [ ] showPaymentSuccess() được gọi
- [ ] Modal elements tồn tại (không có warning)

## 🔧 Quick Fix

Nếu vẫn không hoạt động, thử:

1. **Hard refresh browser:**
   - `Ctrl+F5` (Windows/Linux)
   - `Cmd+Shift+R` (Mac)

2. **Clear cache:**
   - Browser DevTools → Network tab
   - Check "Disable cache"
   - Refresh page

3. **Kiểm tra backend logs:**
   ```bash
   # Xem logs backend khi gọi webhook
   # Tìm: "📥 Webhook received"
   # Tìm: "✅ Booking {id} updated to Paid"
   ```

4. **Test webhook trực tiếp:**
   ```bash
   curl -X POST "http://localhost:5130/api/simplepayment/webhook" \
     -H "Content-Type: application/json" \
     -d '{"content":"BOOKING-39","amount":15000,"transactionId":"TEST"}'
   ```

## 📝 Logs Mẫu

### Khi Mọi Thứ Hoạt Động Đúng:

```
✅ [updatePaymentModal] QR image set, display: block
✅ [updatePaymentModal] QR section set, display: block
🔄 [SimplePolling] Starting polling for booking: 39
🔍 [SimplePolling] Booking status: Pending for booking: 39
🔍 [SimplePolling] Booking status: Pending for booking: 39
🔍 [SimplePolling] Booking status: Paid for booking: 39
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
🎉 [showPaymentSuccess] Showing payment success...
✅ [showPaymentSuccess] Hidden waiting message
✅ [showPaymentSuccess] Showed success message
✅ [showPaymentSuccess] Hidden QR image
✅ [showPaymentSuccess] Hidden QR section
✅ [showPaymentSuccess] Completed
```

## 🎯 Kết Luận

Nếu vẫn không hoạt động sau khi check các bước trên:
1. Copy toàn bộ console logs
2. Copy backend logs
3. Check network requests trong DevTools
4. Verify database có được update không

