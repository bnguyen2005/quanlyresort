# 💰 Hướng Dẫn Test Thanh Toán Thật Bằng Ngân Hàng

## ✅ Có Thể Test Được!

Bạn **HOÀN TOÀN** có thể quét QR và thanh toán bằng ngân hàng thật!

## 📋 Điều Kiện Cần Thiết

### 1. Backend Đang Chạy
```bash
cd QuanLyResort
dotnet run
```

### 2. Ngrok Đang Chạy (Để PayOs Gọi Webhook)
```bash
ngrok http 5130
```

**Copy URL từ ngrok:**
```
Forwarding: https://069c46a78b2b.ngrok-free.app -> http://localhost:5130
```

### 3. Mở Trang Web
```
https://069c46a78b2b.ngrok-free.app/customer/my-bookings.html
```

## 🧪 Các Bước Test Thanh Toán Thật

### Bước 1: Đăng Nhập

1. Mở: `https://069c46a78b2b.ngrok-free.app/customer/login.html`
2. Đăng nhập:
   - Email: `customer1@guest.test`
   - Password: `Guest@123`

### Bước 2: Mở Payment Modal

1. Vào: `https://069c46a78b2b.ngrok-free.app/customer/my-bookings.html`
2. Tìm booking có status = "Pending"
3. Click nút **"Thanh toán"**
4. Modal mở ra với QR code

### Bước 3: Quét QR và Thanh Toán

1. **Mở app ngân hàng:**
   - MB Bank (MBB Mobile)
   - Hoặc app hỗ trợ VietQR khác

2. **Quét QR code:**
   - Mở tính năng quét QR trong app
   - Quét QR code trên màn hình

3. **Nhập nội dung chuyển khoản:**
   - Nội dung: `BOOKING7` (hoặc `BOOKING-7`)
   - Số tiền: 10,000 VND (hoặc số tiền hiển thị)

4. **Xác nhận và chuyển tiền:**
   - Kiểm tra thông tin
   - Xác nhận chuyển tiền
   - Thanh toán thành công

### Bước 4: Quan Sát Kết Quả

**Sau khi thanh toán thành công:**

1. **Backend Logs (Terminal chạy backend):**
   ```
   📥 [WEBHOOK-xxx] Webhook received: BOOKING7 - 10,000 VND
   ✅ [WEBHOOK-xxx] Extracted booking ID: 7
   ✅ [WEBHOOK-xxx] Booking BKG2025007 - Status: Paid
   ```

2. **Frontend (Trang web):**
   - Trong vòng **5 giây**, QR sẽ **TỰ ĐỘNG biến mất**
   - Hiển thị "✅ Thanh toán thành công!"
   - Modal tự động đóng sau 2 giây

3. **Console (F12):**
   ```
   ✅ [SimplePolling] Payment detected! Status = Paid
   🎉 [showPaymentSuccess] Showing payment success...
   ```

## ⚠️ Lưu Ý Quan Trọng

### 1. PayOs Webhook

**Vấn đề:** PayOs có thể chưa config được webhook URL (do ngrok free plan)

**Giải pháp:**
- **Option 1:** Sau khi thanh toán, gọi manual webhook:
  ```bash
  curl -X POST https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook \
    -H "Content-Type: application/json" \
    -d '{"content": "BOOKING7", "amount": 10000}'
  ```

- **Option 2:** PayOs vẫn có thể gọi webhook tự động (mặc dù config API báo lỗi)

### 2. Nội Dung Chuyển Khoản

**Phải đúng format:**
- ✅ `BOOKING7` (không có dấu gạch ngang)
- ✅ `BOOKING-7` (có dấu gạch ngang)
- ❌ `BOOKING 7` (có khoảng trắng - KHÔNG được)

### 3. Số Tiền

- Phải khớp với số tiền hiển thị trên QR
- Hoặc có thể nhiều hơn (code cho phép sai số 10%)

## 🔍 Kiểm Tra Nếu Không Hoạt Động

### QR Không Biến Mất?

1. **Kiểm tra Backend Logs:**
   - Webhook có được gọi không?
   - Booking có được update không?

2. **Kiểm tra Frontend Console (F12):**
   - Polling có đang chạy không?
   - Status có đổi thành "Paid" không?

3. **Gọi Manual Webhook:**
   ```bash
   curl -X POST https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{"content": "BOOKING7", "amount": 10000}'
   ```

### PayOs Không Gọi Webhook?

**Có thể do:**
1. PayOs chưa config được webhook URL (ngrok free plan)
2. Webhook URL không đúng
3. PayOs chưa được kích hoạt

**Giải pháp:**
- Gọi manual webhook sau khi thanh toán
- Hoặc dùng ngrok paid plan
- Hoặc deploy backend lên server thật

## ✅ Checklist Test

- [ ] Backend đang chạy
- [ ] Ngrok đang chạy
- [ ] Đăng nhập thành công
- [ ] Mở payment modal
- [ ] QR code hiển thị
- [ ] Quét QR bằng app ngân hàng
- [ ] Nhập nội dung: `BOOKING7`
- [ ] Chuyển tiền thành công
- [ ] QR tự động biến mất (trong 5 giây)
- [ ] Success message hiện ra
- [ ] Modal tự động đóng

## 🎉 Kết Quả

Sau khi thanh toán thành công:
- ✅ QR code tự động biến mất
- ✅ Hiển thị "✅ Thanh toán thành công!"
- ✅ Booking status = "Paid"
- ✅ Invoice được tạo
- ✅ Modal tự động đóng

## 💡 Mẹo

**Nếu PayOs không gọi webhook tự động:**
1. Thanh toán xong
2. Gọi manual webhook ngay lập tức
3. QR sẽ tự động biến mất

**Hoặc:**
- Dùng ngrok paid plan
- Deploy backend lên server thật
- Config PayOs webhook với domain thật

