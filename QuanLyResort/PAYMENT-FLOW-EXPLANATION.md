# Luồng Thanh Toán Tự Động

## ✅ CÓ - Hoàn Toàn Tự Động!

Khi người dùng quét QR code và thanh toán thành công, hệ thống sẽ **TỰ ĐỘNG**:
1. ✅ QR code biến mất
2. ✅ Hiển thị "Thanh toán thành công"
3. ✅ Ẩn "Đang chờ thanh toán"
4. ✅ Show toast notification
5. ✅ Tự động reload danh sách bookings
6. ✅ Tự động đóng modal sau 2 giây

## 🔄 Luồng Hoạt Động Chi Tiết

### Bước 1: User Click "Thanh toán"
```
User click "Thanh toán" trên booking
  ↓
Frontend: openSimplePayment(bookingId)
  ↓
Modal mở với:
  - QR code hiển thị
  - "Đang chờ thanh toán..." hiển thị
  - "Thanh toán thành công" ẩn
  ↓
Bắt đầu polling mỗi 5 giây
```

### Bước 2: User Quét QR và Thanh Toán
```
User quét QR code bằng app ngân hàng
  ↓
User chuyển khoản thành công
  ↓
Ngân hàng/PayOs gửi webhook đến backend:
  POST /api/simplepayment/webhook
  {
    "content": "BOOKING-39",
    "amount": 15000,
    "transactionId": "TXN-123"
  }
  ↓
Backend xử lý:
  - Parse booking ID từ content
  - Update booking status = "Paid"
  - Tạo/update Invoice
  - Log audit trail
```

### Bước 3: Frontend Phát Hiện Thanh Toán (TỰ ĐỘNG)
```
Frontend polling (mỗi 5 giây):
  ↓
GET /api/bookings/39
  ↓
Response: { status: "Paid", ... }
  ↓
Frontend phát hiện status = "Paid"
  ↓
TỰ ĐỘNG thực hiện:
  1. Dừng polling
  2. Gọi showPaymentSuccess()
     - Ẩn QR code (spQRImage)
     - Ẩn QR section (spQRSection)
     - Ẩn "Đang chờ" (spWaiting)
     - Hiển thị "Thanh toán thành công" (spSuccess)
  3. Show toast: "✅ Thanh toán thành công!"
  4. Sau 2 giây:
     - Reload bookings list
     - Đóng modal
```

## 📋 Code Implementation

### Polling Logic (simple-payment.js)
```javascript
// Polling mỗi 5 giây
window.paymentPollingInterval = setInterval(async () => {
  const booking = await fetch(`/api/bookings/${bookingId}`);
  const data = await booking.json();
  
  // Nếu đã thanh toán
  if (data.status === 'Paid') {
    stopSimplePolling();           // Dừng polling
    showPaymentSuccess();          // Ẩn QR, hiển thị success
    showSimpleToast('✅ Thanh toán thành công!', 'success');
    
    // Sau 2 giây: reload và đóng modal
    setTimeout(() => {
      window.loadBookings();       // Reload danh sách
      modal.hide();                // Đóng modal
    }, 2000);
  }
}, 5000);
```

### showPaymentSuccess() Function
```javascript
function showPaymentSuccess() {
  // Ẩn QR code
  document.getElementById('spQRImage').style.display = 'none';
  document.getElementById('spQRSection').style.display = 'none';
  
  // Ẩn "Đang chờ"
  document.getElementById('spWaiting').style.display = 'none';
  
  // Hiển thị "Thanh toán thành công"
  document.getElementById('spSuccess').style.display = 'block';
}
```

## ⏱️ Timeline

```
T=0s:    User click "Thanh toán"
         → Modal mở, QR hiển thị, polling bắt đầu

T=5s:    Polling check #1: status = "Pending"
T=10s:   Polling check #2: status = "Pending"
T=15s:   User quét QR và thanh toán
T=16s:   Ngân hàng gửi webhook → Backend update status = "Paid"
T=20s:   Polling check #3: status = "Paid" ✅
         → QR biến mất, hiển thị "Thanh toán thành công"
T=22s:   Tự động reload bookings và đóng modal
```

## 🎯 Kết Quả

User sẽ thấy:
1. ✅ QR code biến mất ngay lập tức
2. ✅ "Thanh toán thành công" hiển thị
3. ✅ Toast notification "✅ Thanh toán thành công!"
4. ✅ Danh sách bookings tự động cập nhật
5. ✅ Modal tự động đóng

**Tất cả đều TỰ ĐỘNG - User không cần làm gì thêm!**

## 🔍 Debug

Nếu không hoạt động, mở browser console (F12) và xem logs:

```
🔄 [SimplePolling] Starting polling for booking: 39
🔍 [SimplePolling] Booking status: Pending for booking: 39
🔍 [SimplePolling] Booking status: Pending for booking: 39
🔍 [SimplePolling] Booking status: Paid for booking: 39
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
🎉 [showPaymentSuccess] Showing payment success...
✅ [showPaymentSuccess] Hidden QR image
✅ [showPaymentSuccess] Hidden QR section
✅ [showPaymentSuccess] Showed success message
```

## ✅ Tóm Tắt

**CÓ - Hoàn toàn tự động!**

- ✅ QR code tự động biến mất
- ✅ "Thanh toán thành công" tự động hiển thị
- ✅ Không cần user làm gì thêm
- ✅ Polling tự động phát hiện khi thanh toán thành công

