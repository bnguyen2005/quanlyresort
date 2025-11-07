# 🔍 Debug: QR Code Không Biến Mất Sau Khi Thanh Toán

## ❌ Vấn Đề

Đã chuyển tiền nhưng QR code không biến mất và không hiển thị "Thanh toán thành công".

## 🔍 Các Bước Kiểm Tra

### Bước 1: Kiểm Tra Webhook Có Nhận Được Request Không

**Xem logs trên Render:**
1. Vào: https://dashboard.render.com
2. Click service `quanlyresort-api`
3. Tab "Logs"
4. Tìm các dòng:
   ```
   📥 [WEBHOOK-xxx] Webhook received
   ✅ [WEBHOOK-xxx] Booking xxx updated to Paid
   ```

**Nếu KHÔNG thấy webhook logs:**
- PayOs chưa gọi webhook
- Có thể do PayOs chưa config webhook URL
- Hoặc PayOs không gửi webhook tự động

**Giải pháp:**
- Test webhook thủ công (xem Bước 2)
- Hoặc dùng polling (đã có sẵn, mỗi 2 giây)

### Bước 2: Test Webhook Thủ Công

**Lấy bookingId từ booking vừa thanh toán:**
- Ví dụ: bookingId = 7

**Test webhook:**
```bash
curl -X POST https://quanlyresort.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING7",
    "amount": 5000,
    "transactionId": "TEST123"
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 7,
  "bookingCode": "BKG2025007"
}
```

### Bước 3: Kiểm Tra Booking Status

**Kiểm tra booking status trong database hoặc API:**
```bash
# Lấy token từ browser console: localStorage.getItem('token')
TOKEN="your-token-here"
BOOKING_ID=7

curl -H "Authorization: Bearer $TOKEN" \
  https://quanlyresort.onrender.com/api/bookings/$BOOKING_ID
```

**Kiểm tra:**
- `status` có phải `"Paid"` không?
- Nếu vẫn là `"Pending"` → Webhook chưa được gọi hoặc chưa update

### Bước 4: Kiểm Tra Frontend Polling

**Mở browser console (F12) và tìm:**
```
🔍 [SimplePolling] Booking status: Paid
✅ [SimplePolling] Payment detected!
🎉 [showPaymentSuccess] Showing payment success...
```

**Nếu KHÔNG thấy logs:**
- Polling có thể không chạy
- Hoặc status chưa đổi thành "Paid"

**Kiểm tra polling có chạy không:**
```javascript
// Trong browser console
console.log('Polling interval:', window.paymentPollingInterval);
console.log('Current booking ID:', window.currentPaymentBookingId);
```

### Bước 5: Kiểm Tra UI Elements

**Kiểm tra các elements có tồn tại không:**
```javascript
// Trong browser console
console.log('QR Image:', document.getElementById('spQRImage'));
console.log('QR Section:', document.getElementById('spQRSection'));
console.log('Success Message:', document.getElementById('spSuccess'));
console.log('Waiting Message:', document.getElementById('spWaiting'));
console.log('Modal:', document.getElementById('simplePaymentModal'));
```

**Nếu elements không tồn tại:**
- HTML có thể không đúng
- Hoặc modal ID khác

## ✅ Giải Pháp

### Giải Pháp 1: Test Webhook Thủ Công (Nhanh Nhất)

1. **Lấy bookingId từ booking vừa thanh toán**
2. **Test webhook:**
   ```bash
   curl -X POST https://quanlyresort.onrender.com/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{"content":"BOOKING7","amount":5000}'
   ```
3. **Kiểm tra:**
   - Backend logs có update booking không?
   - Frontend có detect status "Paid" không?
   - QR có biến mất không?

### Giải Pháp 2: Kiểm Tra Polling

**Mở browser console và chạy:**
```javascript
// Force check booking status
const bookingId = window.currentPaymentBookingId || 7; // Thay 7 bằng bookingId thật
const token = localStorage.getItem('token');

fetch(`${location.origin}/api/bookings/${bookingId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
})
  .then(r => r.json())
  .then(booking => {
    console.log('Booking status:', booking.status);
    if (booking.status === 'Paid') {
      // Force show success
      if (window.showPaymentSuccess) {
        window.showPaymentSuccess();
      }
    }
  });
```

### Giải Pháp 3: Force Update UI

**Nếu status đã là "Paid" nhưng UI chưa update:**
```javascript
// Trong browser console
const qrImg = document.getElementById('spQRImage');
const qrSection = document.getElementById('spQRSection');
const successEl = document.getElementById('spSuccess');
const waitingEl = document.getElementById('spWaiting');

if (qrImg) qrImg.style.display = 'none';
if (qrSection) qrSection.style.display = 'none';
if (successEl) {
  successEl.style.display = 'block';
  successEl.style.visibility = 'visible';
  successEl.style.opacity = '1';
}
if (waitingEl) waitingEl.style.display = 'none';
```

## 🎯 Checklist

- [ ] Webhook có nhận được request không? (Xem logs Render)
- [ ] Booking status có đổi thành "Paid" không? (Test API)
- [ ] Frontend polling có chạy không? (Xem browser console)
- [ ] UI elements có tồn tại không? (Test trong console)
- [ ] showPaymentSuccess() có được gọi không? (Xem logs)

## 💡 Lưu Ý

1. **PayOs có thể không gọi webhook tự động:**
   - Nếu PayOs chưa config webhook URL
   - Hoặc PayOs không hỗ trợ webhook cho loại thanh toán này

2. **Polling vẫn hoạt động:**
   - Frontend polling mỗi 2 giây
   - Sẽ detect status "Paid" và ẩn QR
   - Nhưng cần backend update status trước

3. **Test thủ công:**
   - Có thể test webhook thủ công để update booking
   - Sau đó frontend sẽ tự động detect và ẩn QR

