# 🧪 Test Ngay: QR Có Biến Mất Không?

## ⚡ Quick Test (2 phút)

### Bước 1: Mở Payment Modal
1. Mở browser → Đăng nhập
2. Vào "Đặt phòng của tôi"
3. Click "Thanh toán" cho một booking
4. **Mở Console (F12)** → Tab "Console"

### Bước 2: Kiểm Tra Polling
Trong Console, bạn sẽ thấy:
```
🔄 [SimplePolling] Starting polling for booking: X
🔍 [SimplePolling] Booking status: Pending for booking: X
```

**Nếu KHÔNG thấy logs này:**
- ❌ Polling không chạy → Modal chưa mở đúng
- ✅ Refresh page và thử lại

### Bước 3: Mô Phỏng Thanh Toán
Mở terminal và chạy:
```bash
cd QuanLyResort
./test-qr-auto-hide.sh 4 10000
```

Hoặc manual:
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING-4", "amount": 10000}'
```

### Bước 4: Quan Sát Console
Trong vòng **5 giây**, bạn sẽ thấy:
```
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
🎉 [showPaymentSuccess] Showing payment success...
✅ [showPaymentSuccess] Hidden waiting message
✅ [showPaymentSuccess] Showed success message
✅ [showPaymentSuccess] Hidden QR image
✅ [showPaymentSuccess] Hidden QR section
✅ [showPaymentSuccess] Completed
```

### Bước 5: Quan Sát UI
Trong browser:
- ✅ QR code **BIẾN MẤT**
- ✅ "Đang chờ thanh toán..." **BIẾN MẤT**
- ✅ "✅ Thanh toán thành công!" **HIỆN RA**
- ✅ Modal tự đóng sau 2 giây

## 🔍 Nếu Vẫn Không Hoạt Động

### Kiểm Tra 1: Webhook Có Được Gọi Không?
Xem terminal backend:
```
📥 [WEBHOOK-xxxx] Webhook received: BOOKING-4 - 10000 VND
✅ [WEBHOOK-xxxx] Booking ID: 4
✅ [WEBHOOK-xxxx] Booking BKG2025004 - Status: Paid
```

**Nếu KHÔNG thấy:**
- Webhook không được gọi → Vấn đề ở PayOs config

### Kiểm Tra 2: Booking Status Có Update Không?
Trong Console (F12), chạy:
```javascript
const token = localStorage.getItem('token');
fetch('/api/bookings/4', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => console.log('Status:', data.status));
```

**Nếu status ≠ "Paid":**
- Backend chưa update → Kiểm tra webhook xử lý

### Kiểm Tra 3: Polling Có Phát Hiện Không?
Trong Console, tìm:
```
🔍 [SimplePolling] Booking status: Paid for booking: 4
```

**Nếu thấy "Paid" nhưng QR không biến mất:**
- `showPaymentSuccess()` không hoạt động → Kiểm tra elements

### Kiểm Tra 4: Elements Có Tồn Tại Không?
Trong Console, chạy:
```javascript
console.log('Modal:', document.getElementById('simplePaymentModal'));
console.log('QR:', document.getElementById('spQRImage'));
console.log('Success:', document.getElementById('spSuccess'));
```

**Nếu elements = null:**
- Modal không đúng → Có thể đang dùng modal cũ

## 🐛 Quick Fix

Nếu vội, có thể manual trigger trong Console:
```javascript
// Manual trigger showPaymentSuccess
if (window.showPaymentSuccess) {
  window.showPaymentSuccess();
}
```

Hoặc manual update UI:
```javascript
const qr = document.getElementById('spQRImage');
const success = document.getElementById('spSuccess');
if (qr) qr.style.display = 'none';
if (success) success.style.display = 'block';
```

## 📝 Checklist

- [ ] Console có logs polling không?
- [ ] Backend logs có nhận webhook không?
- [ ] Booking status đã thành "Paid" chưa?
- [ ] Console có log "Payment detected!" không?
- [ ] Console có log "[showPaymentSuccess]" không?
- [ ] Elements có tồn tại không?
- [ ] QR có biến mất không?
- [ ] Success message có hiện không?

## ✅ Kết Luận

**Nếu tất cả đều OK nhưng vẫn không hoạt động:**
1. Refresh page (Ctrl+F5)
2. Clear cache
3. Kiểm tra có JavaScript errors không
4. Kiểm tra network tab xem API calls có thành công không

