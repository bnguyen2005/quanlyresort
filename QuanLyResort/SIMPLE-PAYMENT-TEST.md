# Test Flow Đơn Giản: QR → Thanh toán → Webhook → Cập nhật UI

## ✅ Đã Setup

1. **Backend:** `SimplePaymentController` - `/api/simplepayment/webhook`
2. **Frontend:** `simple-payment.js` - Modal đơn giản + polling
3. **Modal:** Thêm vào `my-bookings.html`

## 🧪 Test Flow

### Bước 1: Khởi động Backend

```bash
cd QuanLyResort
dotnet run
```

### Bước 2: Mở Frontend

1. Mở browser: `http://localhost:5130/customer/my-bookings.html`
2. Đăng nhập với tài khoản customer:
   - Email: `customer1@guest.test`
   - Password: `Guest@123`

### Bước 3: Test Thanh Toán

#### Option 1: Test bằng Webhook (Simulate Payment)

1. **Tạo booking mới** hoặc dùng booking có sẵn (status = "Pending")
2. **Click nút "Thanh toán"** → Modal hiển thị QR code
3. **Mở terminal** và chạy:
   ```bash
   cd QuanLyResort
   ./test-simple-webhook.sh 39
   ```
   (Thay `39` bằng booking ID thật)

4. **Quan sát:**
   - Backend log: `✅ Booking {BookingId} updated to Paid`
   - Frontend modal: QR code ẩn, hiển thị "✅ Thanh toán thành công!"
   - Sau 2 giây: Modal tự đóng, danh sách booking reload

#### Option 2: Test Real Payment (PayOs)

1. **Tạo booking mới** (status = "Pending")
2. **Click nút "Thanh toán"** → QR code hiển thị
3. **Quét QR bằng app ngân hàng** (MB Bank)
4. **Chuyển khoản** với nội dung: `BOOKING-{bookingId}` (ví dụ: `BOOKING-39`)
5. **PayOs gửi webhook** → Backend tự động cập nhật
6. **Frontend polling detect** → UI tự động cập nhật

### Bước 4: Kiểm Tra Database

```bash
# Kiểm tra booking status
curl -X GET "http://localhost:5130/api/bookings/39" \
  -H "Authorization: Bearer $TOKEN"
```

Nếu `status = "Paid"` → ✅ Webhook đã hoạt động!

## 🔍 Debug

### Webhook không hoạt động?

1. **Kiểm tra endpoint:**
   ```bash
   curl -X POST http://localhost:5130/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{"content": "BOOKING-39", "amount": 15000}'
   ```

2. **Kiểm tra logs backend:**
   - Tìm: `📥 Webhook received...`
   - Tìm: `✅ Booking {BookingId} updated to Paid`

### QR không ẩn sau khi thanh toán?

1. **Mở browser console** (F12)
2. **Kiểm tra polling:**
   - Tìm: `🔍 [Polling] Current status: ...`
   - Nếu status = "Paid" nhưng QR không ẩn → Check `showPaymentSuccess()` function

3. **Kiểm tra network:**
   - Xem API call: `GET /api/bookings/{id}`
   - Response có `status: "Paid"` không?

### Booking ID không parse được?

1. **Kiểm tra content webhook:**
   - Phải có format: `BOOKING-39` hoặc `BOOKING-BKG2025039`
   - Backend log: `⚠️ Cannot extract booking ID from content...`

## 📋 Checklist

- [ ] Backend đang chạy
- [ ] Frontend mở được trang my-bookings
- [ ] Đăng nhập thành công
- [ ] Có booking với status = "Pending"
- [ ] Click "Thanh toán" → Modal hiển thị QR
- [ ] QR code hiển thị đúng (có nội dung BOOKING-{id})
- [ ] Test webhook → Backend log success
- [ ] UI tự động cập nhật (QR ẩn, success hiển thị)
- [ ] Modal tự đóng sau 2 giây
- [ ] Booking list reload và hiển thị status = "Paid"

## 🎯 Expected Flow

```
1. User click "Thanh toán"
   → openSimplePayment(39) called
   → Modal shows with QR code
   → Polling starts (every 5 seconds)

2. User scans QR and pays
   → Content: "BOOKING-39"
   → PayOs sends webhook

3. Webhook received
   → POST /api/simplepayment/webhook
   → Parse booking ID = 39
   → Update booking status = "Paid"
   → Return OK

4. Frontend polling detects
   → GET /api/bookings/39
   → Status = "Paid"
   → showPaymentSuccess() called
   → QR hidden, success message shown
   → Modal auto-closes after 2 seconds
   → Booking list reloads
```

## ✅ Success Criteria

- ✅ QR code hiển thị đúng
- ✅ Webhook nhận và xử lý thành công
- ✅ Booking status cập nhật = "Paid"
- ✅ UI tự động cập nhật (QR ẩn, success hiển thị)
- ✅ Modal tự đóng
- ✅ Booking list reload

