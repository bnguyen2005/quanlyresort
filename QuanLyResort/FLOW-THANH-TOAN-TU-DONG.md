# 💰 Flow Thanh Toán Tự Động - Tập Trung Chức Năng Chính

## 🎯 Mục Tiêu

**Khi user quét QR và thanh toán:**
1. ✅ Ngân hàng nhận tiền
2. ✅ Ngân hàng thông báo lại server (webhook)
3. ✅ Server cập nhật booking = "Paid"
4. ✅ Frontend tự động ẩn QR và hiện "Thanh toán thành công"

## 📋 Flow Chi Tiết

### Bước 1: User Quét QR và Thanh Toán

**Frontend (`simple-payment.js`):**
- User click "Thanh toán" → Mở modal
- Hiển thị QR code với nội dung: `BOOKING7` (hoặc `BOOKING-7`)
- Bắt đầu polling mỗi 5 giây để check status

**QR Code Format:**
```
https://img.vietqr.io/image/MB-0901329227-compact.png?
  amount=10000&
  addInfo=BOOKING7&
  accountName=Resort Deluxe
```

### Bước 2: Ngân Hàng Nhận Tiền

- User quét QR bằng app ngân hàng
- Nhập nội dung: `BOOKING7`
- Chuyển tiền thành công
- Ngân hàng xử lý giao dịch

### Bước 3: Ngân Hàng Gọi Webhook

**PayOs/VietQR tự động gọi:**
```
POST https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook
Content-Type: application/json

{
  "content": "BOOKING7",
  "amount": 10000,
  "transactionId": "11615536480"
}
```

**Backend (`SimplePaymentController.cs`):**
1. ✅ Nhận webhook
2. ✅ Extract booking ID từ `BOOKING7` → `7`
3. ✅ Verify booking tồn tại
4. ✅ Verify amount (cho phép sai số 10%)
5. ✅ Update booking status = "Paid"
6. ✅ Tạo invoice
7. ✅ Return success

### Bước 4: Frontend Polling Detect Status

**Frontend (`simple-payment.js` - `startSimplePolling`):**
```javascript
// Polling mỗi 5 giây
setInterval(async () => {
  const booking = await fetch(`/api/bookings/${bookingId}`);
  const status = booking.status.toLowerCase();
  
  if (status === 'paid' || booking.status === 'Paid') {
    // ✅ Payment detected!
    stopSimplePolling();
    showPaymentSuccess();  // Ẩn QR + Hiện success
    showSimpleToast('✅ Thanh toán thành công!');
    // Đóng modal sau 2 giây
  }
}, 5000);
```

### Bước 5: UI Tự Động Update

**Function `showPaymentSuccess()`:**
1. ✅ Ẩn QR code (`spQRImage`)
2. ✅ Ẩn QR section (`spQRSection`)
3. ✅ Ẩn "Đang chờ thanh toán..." (`spWaiting`)
4. ✅ Hiện "✅ Thanh toán thành công!" (`spSuccess`)
5. ✅ Đóng modal sau 2 giây

## ✅ Checklist Hoàn Chỉnh

### Backend
- [x] Webhook endpoint: `/api/simplepayment/webhook`
- [x] Extract booking ID từ `BOOKING7` (không cần dấu gạch ngang)
- [x] Update booking status = "Paid"
- [x] Tạo invoice
- [x] Logging chi tiết

### Frontend
- [x] QR code với nội dung `BOOKING7`
- [x] Polling mỗi 5 giây
- [x] Detect status = "Paid"
- [x] Ẩn QR code
- [x] Hiện success message
- [x] Đóng modal tự động

### Integration
- [x] PayOs webhook URL config (qua ngrok)
- [x] Auto-detect ngrok URL cho API calls

## 🧪 Test Flow

### Test 1: Manual Webhook (Verify Code)

```bash
# 1. Mở payment modal cho booking 7
# 2. QR code hiển thị với nội dung "BOOKING7"
# 3. Test webhook manual:
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING7", "amount": 10000}'

# 4. Kiểm tra:
#    - Backend logs → Webhook processed
#    - Frontend → QR biến mất, success hiện ra
#    - Booking status → "Paid"
```

### Test 2: Thanh Toán Thật (End-to-End)

```bash
# 1. Chạy ngrok
ngrok http 5130

# 2. Config PayOs webhook URL (nếu có thể):
#    https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook

# 3. Mở payment modal
# 4. Quét QR và thanh toán với nội dung "BOOKING7"
# 5. PayOs tự động gọi webhook
# 6. Frontend tự động ẩn QR và hiện success
```

## 🔧 Cấu Hình Cần Thiết

### 1. Backend Running
```bash
cd QuanLyResort
dotnet run
```

### 2. Ngrok Running (Cho PayOs)
```bash
ngrok http 5130
```

### 3. PayOs Webhook URL
```
https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook
```

## 📝 Lưu Ý

1. **Nội dung chuyển khoản:** Phải là `BOOKING7` hoặc `BOOKING-7` (code đã hỗ trợ cả 2)
2. **Polling interval:** 5 giây (có thể điều chỉnh nếu cần)
3. **Modal auto-close:** 2 giây sau khi thanh toán thành công
4. **Ngrok URL:** Thay đổi mỗi lần restart (free plan)

## 🎉 Kết Quả

Sau khi thanh toán thành công:
- ✅ QR code tự động biến mất
- ✅ Hiển thị "✅ Thanh toán thành công!"
- ✅ Booking status = "Paid"
- ✅ Invoice được tạo
- ✅ Modal tự động đóng sau 2 giây

