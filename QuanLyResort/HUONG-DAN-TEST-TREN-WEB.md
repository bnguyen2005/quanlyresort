# 🌐 Hướng Dẫn Test Trên Web

## 📋 Các Trang Web Để Test

### Option 1: Localhost (Nếu Backend Chạy Local)

**URL:**
```
http://localhost:5130/customer/my-bookings.html
```

**Hoặc Booking Details:**
```
http://localhost:5130/customer/booking-details.html?id=7
```

### Option 2: Ngrok (Nếu Dùng Ngrok)

**URL:**
```
https://069c46a78b2b.ngrok-free.app/customer/my-bookings.html
```

**Hoặc Booking Details:**
```
https://069c46a78b2b.ngrok-free.app/customer/booking-details.html?id=7
```

## 🧪 Các Bước Test

### Bước 1: Đăng Nhập

1. **Mở trang login:**
   - Localhost: `http://localhost:5130/customer/login.html`
   - Ngrok: `https://069c46a78b2b.ngrok-free.app/customer/login.html`

2. **Đăng nhập:**
   - Email: `customer1@guest.test`
   - Password: `Guest@123`

3. **Click "Đăng Nhập"**

### Bước 2: Vào Trang Đặt Phòng

**Sau khi đăng nhập, vào một trong các trang:**

#### Option A: My Bookings (Danh Sách Đặt Phòng)
```
http://localhost:5130/customer/my-bookings.html
```
- Sẽ hiển thị danh sách tất cả bookings
- Tìm booking có status = "Pending"
- Click nút "Thanh toán"

#### Option B: Booking Details (Chi Tiết Đặt Phòng)
```
http://localhost:5130/customer/booking-details.html?id=7
```
- Thay `7` bằng booking ID bạn muốn test
- Click nút "Thanh toán"

### Bước 3: Test Thanh Toán

1. **Modal thanh toán mở ra:**
   - QR code hiển thị
   - Nội dung chuyển khoản: `BOOKING7` (hoặc `BOOKING-7`)
   - Số tiền: 10,000 VND (hoặc số tiền của booking)

2. **Mở Console (F12):**
   - Sẽ thấy logs: `🔄 [SimplePolling] Starting polling...`
   - Polling sẽ chạy mỗi 5 giây

3. **Test Webhook (Terminal):**
   ```bash
   curl -X POST http://localhost:5130/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{"content": "BOOKING7", "amount": 10000}'
   ```

4. **Quan Sát Frontend:**
   - Trong vòng **5 giây**, QR sẽ **TỰ ĐỘNG biến mất**
   - Hiển thị "✅ Thanh toán thành công!"
   - Modal tự động đóng sau 2 giây

### Bước 4: Kiểm Tra Kết Quả

1. **Backend Logs (Terminal chạy backend):**
   ```
   📥 [WEBHOOK-xxx] Webhook received: BOOKING7 - 10,000 VND
   ✅ [WEBHOOK-xxx] Extracted booking ID: 7
   ✅ [WEBHOOK-xxx] Booking BKG2025007 - Status: Paid
   ```

2. **Frontend Console (F12):**
   ```
   ✅ [SimplePolling] Payment detected! Status = Paid
   🎉 [showPaymentSuccess] Showing payment success...
   ✅ [showPaymentSuccess] Hidden QR image
   ```

3. **Trang Web:**
   - QR code biến mất
   - Success message hiện ra
   - Booking status = "Paid" (nếu reload trang)

## 🎯 Test Thanh Toán Thật (Với PayOs)

### Nếu Đã Config PayOs Webhook:

1. **Mở payment modal** (như trên)

2. **Quét QR bằng app ngân hàng:**
   - Mở app MB Bank (hoặc app hỗ trợ VietQR)
   - Quét QR code
   - Nhập nội dung: `BOOKING7`
   - Chuyển tiền

3. **PayOs tự động gọi webhook:**
   - Webhook được gọi tự động
   - Backend xử lý
   - Frontend polling detect
   - QR tự động biến mất

## ⚠️ Lưu Ý

1. **Backend phải đang chạy:**
   ```bash
   cd QuanLyResort
   dotnet run
   ```

2. **Ngrok phải đang chạy (nếu dùng ngrok):**
   ```bash
   ngrok http 5130
   ```

3. **Booking phải có status = "Pending":**
   - Nếu đã "Paid", webhook sẽ trả về "Đã thanh toán rồi"
   - Cần tạo booking mới hoặc reset booking về "Pending"

4. **Nội dung chuyển khoản:**
   - Phải là `BOOKING7` hoặc `BOOKING-7`
   - Code đã hỗ trợ cả 2 format

## 🔍 Troubleshooting

### QR Không Biến Mất?

1. **Kiểm tra Console (F12):**
   - Xem có logs polling không?
   - Status có đổi thành "Paid" không?

2. **Kiểm tra Backend Logs:**
   - Webhook có được gọi không?
   - Booking có được update không?

3. **Kiểm tra Network Tab (F12):**
   - API `/api/bookings/7` có trả về status = "Paid" không?

### Webhook Không Hoạt Động?

1. **Test manual webhook:**
   ```bash
   curl -X POST http://localhost:5130/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{"content": "BOOKING7", "amount": 10000}'
   ```

2. **Kiểm tra response:**
   - Nếu thành công → Code OK
   - Nếu lỗi → Xem error message

## ✅ Checklist Test

- [ ] Đăng nhập thành công
- [ ] Vào trang my-bookings hoặc booking-details
- [ ] Mở payment modal
- [ ] QR code hiển thị
- [ ] Console có logs polling
- [ ] Test webhook (manual hoặc thật)
- [ ] QR tự động biến mất
- [ ] Success message hiện ra
- [ ] Modal tự động đóng

## 🎉 Kết Quả Mong Đợi

Sau khi test thành công:
- ✅ QR code tự động biến mất
- ✅ Hiển thị "✅ Thanh toán thành công!"
- ✅ Booking status = "Paid"
- ✅ Modal tự động đóng
- ✅ Backend logs hiển thị webhook processed

