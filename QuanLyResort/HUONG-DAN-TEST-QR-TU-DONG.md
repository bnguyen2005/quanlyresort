# 🎯 Hướng Dẫn Test QR Tự Động Biến Mất Sau Thanh Toán

## ✅ Câu Trả Lời Ngắn Gọn

**CÓ!** Khi quét mã QR ngân hàng và thanh toán thành công, hệ thống sẽ:
1. ✅ QR code **tự động biến mất**
2. ✅ Hiển thị thông báo **"Thanh toán thành công!"**
3. ✅ Modal tự động đóng sau 2 giây
4. ✅ Trạng thái booking tự động cập nhật thành "Paid"

## 🔄 Flow Hoạt Động

```
1. User mở payment modal → QR hiển thị
2. User quét QR → Thanh toán qua ngân hàng
3. Ngân hàng gọi webhook → Backend update booking status = "Paid"
4. Frontend polling (mỗi 5 giây) phát hiện status = "Paid"
5. Hàm showPaymentSuccess() được gọi:
   - Ẩn QR code (spQRImage, spQRSection)
   - Hiện success message (spSuccess)
   - Ẩn waiting message (spWaiting)
6. Modal tự động đóng sau 2 giây
```

## 📋 Cách Test Chi Tiết

### Bước 1: Mở Payment Modal
1. Đăng nhập vào hệ thống
2. Vào trang **"Đặt phòng của tôi"** (`my-bookings.html`)
3. Tìm một booking có status **"Pending"** hoặc **"Confirmed"**
4. Click nút **"Thanh toán"**
5. Modal sẽ hiển thị với:
   - QR code
   - Số tiền
   - Thông báo "Đang chờ thanh toán..."

### Bước 2: Kiểm Tra Polling
Mở **Console** (F12) và kiểm tra logs:
```
🔵 [openSimplePayment] Opening payment modal for booking: X
✅ [updatePaymentModal] QR image set, display: block
🔄 [startSimplePolling] Starting polling for booking: X
🔍 [SimplePolling] Booking status: Pending for booking: X
```

**Quan trọng**: Polling sẽ chạy mỗi 5 giây và log status hiện tại.

### Bước 3: Mô Phỏng Thanh Toán (Test)

#### Option A: Dùng Script Test
Mở terminal trong thư mục `QuanLyResort` và chạy:
```bash
./quick-test-payment.sh <BOOKING_ID> <AMOUNT>
```

Ví dụ:
```bash
./quick-test-payment.sh 4 10000
```

Script này sẽ:
- Gọi webhook endpoint
- Update booking status thành "Paid"
- Backend sẽ xử lý và update database

#### Option B: Test Manual Webhook
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-4",
    "amount": 10000
  }'
```

### Bước 4: Quan Sát UI Tự Động Update

Sau khi webhook được gọi:

1. **Trong Console**, bạn sẽ thấy:
```
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
🎉 [showPaymentSuccess] Showing payment success...
✅ [showPaymentSuccess] Hidden waiting message
✅ [showPaymentSuccess] Showed success message
✅ [showPaymentSuccess] Hidden QR image
✅ [showPaymentSuccess] Hidden QR section
✅ [showPaymentSuccess] Completed
```

2. **Trong UI**, bạn sẽ thấy:
   - ✅ QR code **biến mất**
   - ✅ Thông báo "Đang chờ thanh toán..." **biến mất**
   - ✅ Thông báo **"✅ Thanh toán thành công!"** **hiện ra**
   - ✅ Modal tự động đóng sau 2 giây

### Bước 5: Kiểm Tra Database

Sau khi thanh toán, booking status trong database sẽ là `"Paid"`:
```sql
SELECT BookingId, BookingCode, Status, EstimatedTotalAmount 
FROM Bookings 
WHERE BookingId = 4;
```

Kết quả mong đợi:
```
Status = "Paid"
```

## 🐛 Troubleshooting

### ❌ QR Không Biến Mất

**Nguyên nhân có thể:**
1. Polling không chạy
2. Booking status không được update
3. Webhook không được gọi

**Cách kiểm tra:**
1. Mở Console và kiểm tra logs polling
2. Kiểm tra `booking.status` trong response API
3. Kiểm tra backend logs xem webhook có được nhận không

**Cách fix:**
- Đảm bảo modal đang mở và polling đang chạy
- Kiểm tra `/api/simplepayment/webhook` có hoạt động không
- Kiểm tra booking ID có đúng không

### ❌ Success Message Không Hiện

**Nguyên nhân có thể:**
1. Elements không tồn tại trong DOM
2. CSS display bị override

**Cách kiểm tra:**
```javascript
// Mở Console và chạy:
document.getElementById('spSuccess') // Phải trả về element
document.getElementById('spQRImage') // Phải trả về element
```

**Cách fix:**
- Đảm bảo modal `simplePaymentModal` đang được sử dụng (không phải `paymentModal` cũ)
- Kiểm tra HTML structure của modal

### ❌ Polling Không Phát Hiện Status Change

**Nguyên nhân có thể:**
1. API `/api/bookings/{id}` trả về status cũ
2. Polling bị dừng sớm

**Cách kiểm tra:**
```bash
# Test API trực tiếp
curl http://localhost:5130/api/bookings/4 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Cách fix:**
- Đảm bảo backend đã update booking status
- Kiểm tra `ProcessOnlinePaymentAsync` có được gọi không
- Kiểm tra logs backend

## 📝 Lưu Ý Quan Trọng

1. **Polling Interval**: 5 giây (có thể thay đổi trong `simple-payment.js`)
2. **Modal Auto-close**: 2 giây sau khi success
3. **Webhook Endpoint**: `/api/simplepayment/webhook` (public, không cần auth)
4. **Booking Status**: Phải là "Pending" hoặc "Confirmed" mới có thể thanh toán

## ✅ Checklist Test

- [ ] Modal mở và hiển thị QR
- [ ] Polling logs xuất hiện trong Console
- [ ] Webhook được gọi (test hoặc thật)
- [ ] Backend update booking status thành "Paid"
- [ ] Frontend polling phát hiện status change
- [ ] QR code biến mất
- [ ] Success message hiện ra
- [ ] Modal tự động đóng sau 2 giây
- [ ] Booking list tự động refresh

## 🎯 Kết Luận

Hệ thống **HOẠT ĐỘNG TỰ ĐỘNG** khi:
- ✅ Ngân hàng gọi webhook thành công
- ✅ Backend update booking status thành "Paid"
- ✅ Frontend polling phát hiện status change
- ✅ UI tự động update (QR biến mất, success hiện)

**Thời gian phản hồi**: Tối đa 5 giây (polling interval) sau khi webhook được xử lý.

