# 🧪 Hướng dẫn Test từng bước

## 📋 Tổng quan

Hướng dẫn chi tiết để test từng tính năng đã triển khai.

---

## 🔧 Chuẩn bị

### 1. Chạy ứng dụng
```bash
cd QuanLyResort
dotnet run
```

Ứng dụng sẽ chạy tại: `http://localhost:5130`

### 2. Lấy JWT Token
Đăng nhập và lấy token từ:
- Browser: `localStorage.getItem('token')`
- Hoặc từ response của API login

---

## 1️⃣ Test Email Notifications

### Test 1: Đặt phòng → Email xác nhận

**Bước 1:** Đăng nhập với tài khoản customer
```
POST /api/auth/login
Body: {
  "email": "customer@example.com",
  "password": "password"
}
```

**Bước 2:** Đặt phòng
```
POST /api/bookings
Authorization: Bearer {token}
Body: {
  "customerId": 1,
  "requestedRoomType": "Deluxe",
  "checkInDate": "2025-12-01",
  "checkOutDate": "2025-12-03",
  "numberOfGuests": 2
}
```

**Bước 3:** Kiểm tra email
- Mở email: `phamthahlam@gmail.com`
- Tìm email với subject: "🎉 Đặt phòng thành công!"
- Kiểm tra nội dung có đầy đủ thông tin booking

**Kết quả mong đợi:**
✅ Email được gửi thành công
✅ Nội dung email có mã booking, ngày check-in/out, tổng tiền
✅ Email format HTML đẹp

---

### Test 2: Thanh toán → Email xác nhận

**Bước 1:** Lấy invoice ID từ booking vừa tạo

**Bước 2:** Thanh toán
```
POST /api/invoices/{id}/process-payment
Authorization: Bearer {token}
Body: {
  "amount": 2000000,
  "paymentMethod": "QR",
  "paymentReference": "REF123"
}
```

**Bước 3:** Kiểm tra email
- Tìm email với subject: "✅ Thanh toán thành công!"
- Kiểm tra có mã hóa đơn, số tiền, phương thức thanh toán

**Kết quả mong đợi:**
✅ Email được gửi ngay sau khi thanh toán
✅ Thông tin thanh toán chính xác

---

### Test 3: Đặt món → Email xác nhận

**Bước 1:** Đặt món tại nhà hàng
```
POST /api/restaurant-orders
Body: {
  "customerId": 1,
  "items": [
    {"serviceId": 1, "quantity": 2},
    {"serviceId": 2, "quantity": 1}
  ]
}
```

**Bước 2:** Kiểm tra email
- Tìm email với subject: "🍽️ Đặt món thành công!"

**Kết quả mong đợi:**
✅ Email được gửi với thông tin đơn hàng

---

## 2️⃣ Test 2FA Authentication

### Test 1: Generate Secret & QR Code

**Bước 1:** Generate secret
```bash
curl -X POST http://localhost:5130/api/auth/2fa/generate \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

**Response:**
```json
{
  "secret": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/...",
  "qrCodeImage": "base64...",
  "message": "Scan QR code with authenticator app"
}
```

**Bước 2:** Lưu QR code image
- Copy `qrCodeImage` (base64)
- Decode và lưu thành file PNG
- Hoặc dùng `qrCodeUri` để tạo QR code

**Bước 3:** Scan QR code
- Mở Google Authenticator hoặc Microsoft Authenticator
- Chọn "Add account" → "Scan QR code"
- Scan QR code vừa tạo

**Kết quả mong đợi:**
✅ Secret được generate thành công
✅ QR code hiển thị đúng
✅ App authenticator nhận diện được QR code

---

### Test 2: Enable 2FA

**Bước 1:** Lấy code từ authenticator app (6 digits)

**Bước 2:** Enable 2FA
```bash
curl -X POST http://localhost:5130/api/auth/2fa/enable \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "123456"
  }'
```

**Response:**
```json
{
  "message": "2FA enabled successfully",
  "recoveryCodes": ["12345678", "87654321", ...],
  "warning": "Save these recovery codes..."
}
```

**Bước 3:** Lưu recovery codes
⚠️ **QUAN TRỌNG:** Lưu 10 recovery codes ở nơi an toàn!

**Kết quả mong đợi:**
✅ 2FA được enable thành công
✅ Nhận được recovery codes
✅ Status API trả về `enabled: true`

---

### Test 3: Login với 2FA

**Bước 1:** Đăng xuất

**Bước 2:** Đăng nhập với email/password
```
POST /api/auth/login
Body: {
  "email": "user@example.com",
  "password": "password"
}
```

**Response:**
```json
{
  "token": null,
  "requires2FA": true,
  "userId": 1,
  "message": "Please enter 2FA code"
}
```

**Bước 3:** Lấy code từ authenticator app

**Bước 4:** Verify 2FA code
```
POST /api/auth/2fa/verify
Body: {
  "userId": 1,
  "code": "123456"
}
```

**Response:**
```json
{
  "message": "Code verified successfully",
  "token": "jwt-token-here"
}
```

**Bước 5:** Sử dụng token để truy cập các API

**Kết quả mong đợi:**
✅ Login yêu cầu 2FA code
✅ Code từ authenticator app được verify thành công
✅ Nhận được JWT token sau khi verify

---

### Test 4: Recovery Code

**Bước 1:** Giả sử mất authenticator app

**Bước 2:** Dùng recovery code
```
POST /api/auth/2fa/verify
Body: {
  "userId": 1,
  "code": "12345678"  // Recovery code
}
```

**Kết quả mong đợi:**
✅ Recovery code được verify thành công
✅ Code đã dùng sẽ bị xóa khỏi danh sách

---

### Test 5: Disable 2FA

**Bước 1:** Disable 2FA
```
POST /api/auth/2fa/disable
Authorization: Bearer YOUR_TOKEN
Body: {
  "password": "your-password"
}
```

**Kết quả mong đợi:**
✅ 2FA được disable thành công
✅ Login không còn yêu cầu 2FA code

---

## 3️⃣ Test Multi-language Support

### Test 1: Get Current Language

```bash
curl http://localhost:5130/api/localization/current
```

**Response:**
```json
{
  "language": "vi"
}
```

**Kết quả mong đợi:**
✅ Trả về language hiện tại (mặc định: "vi")

---

### Test 2: Get Translations

**Tiếng Việt:**
```bash
curl http://localhost:5130/api/localization/strings?lang=vi
```

**Tiếng Anh:**
```bash
curl http://localhost:5130/api/localization/strings?lang=en
```

**Kết quả mong đợi:**
✅ Trả về đúng translations theo ngôn ngữ
✅ Có đầy đủ các keys: common.*, auth.*, booking.*, etc.

---

### Test 3: Change Language

```bash
curl -X POST http://localhost:5130/api/localization/set-language \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "language": "en"
  }'
```

**Bước 2:** Kiểm tra cookie
- Mở DevTools → Application → Cookies
- Tìm cookie `language` = "en"

**Kết quả mong đợi:**
✅ Language được set thành công
✅ Cookie được lưu
✅ Lần sau sẽ tự động dùng language đã set

---

### Test 4: Frontend Integration

**Bước 1:** Mở website với `?lang=en`
```
http://localhost:5130/customer/index.html?lang=en
```

**Bước 2:** Kiểm tra UI
- Các text hiển thị tiếng Anh
- Navigation, buttons, labels đều đổi ngôn ngữ

**Kết quả mong đợi:**
✅ Frontend tự động load translations
✅ UI hiển thị đúng ngôn ngữ

---

## 4️⃣ Test Push Notifications

### Test 1: Request Permission

Mở browser console và chạy:
```javascript
if ('Notification' in window) {
  Notification.requestPermission().then(permission => {
    console.log('Permission:', permission);
    // Expected: "granted"
  });
}
```

**Kết quả mong đợi:**
✅ Browser hiển thị popup xin phép
✅ Permission = "granted" sau khi cho phép

---

### Test 2: Test Notification Service

```javascript
// Check service loaded
console.log(window.notificationService);

// Load unread count
window.notificationService.loadUnreadCount();

// Get notifications
window.notificationService.getNotifications().then(notifications => {
  console.log('Notifications:', notifications);
});
```

**Kết quả mong đợi:**
✅ Service đã load
✅ Unread count được cập nhật
✅ Notifications được load từ API

---

### Test 3: Browser Notification

```javascript
window.notificationService.showBrowserNotification('Test Notification', {
  body: 'This is a test notification',
  icon: '/customer/images/logo.png',
  onClick: () => {
    console.log('Notification clicked!');
  }
});
```

**Kết quả mong đợi:**
✅ Browser notification hiển thị
✅ Có icon, title, body
✅ Click notification → focus window

---

### Test 4: Real-time Notifications

**Bước 1:** Đặt phòng hoặc thanh toán

**Bước 2:** Kiểm tra
- Browser notification tự động hiển thị
- Notification dropdown có badge số
- Unread count tăng lên

**Kết quả mong đợi:**
✅ Notification tự động hiển thị khi có sự kiện
✅ Badge hiển thị số thông báo chưa đọc

---

## 5️⃣ Test In-App Notifications API

### Test 1: Get Notifications

```bash
curl http://localhost:5130/api/notifications \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kết quả mong đợi:**
✅ Trả về danh sách notifications
✅ Có đầy đủ thông tin: title, message, severity, createdAt

---

### Test 2: Get Unread Count

```bash
curl http://localhost:5130/api/notifications/unread-count \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "count": 3
}
```

**Kết quả mong đợi:**
✅ Trả về số lượng notifications chưa đọc

---

### Test 3: Mark as Read

```bash
curl -X PATCH http://localhost:5130/api/notifications/1/read \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kết quả mong đợi:**
✅ Notification được đánh dấu đã đọc
✅ Unread count giảm đi 1

---

### Test 4: Mark All as Read

```bash
curl -X PATCH http://localhost:5130/api/notifications/read-all \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kết quả mong đợi:**
✅ Tất cả notifications được đánh dấu đã đọc
✅ Unread count = 0

---

## 🎯 Test Script Tự động

Chạy script test tự động:

```bash
# Test với localhost
./test-advanced-features.sh

# Test với production URL
./test-advanced-features.sh https://your-domain.com YOUR_TOKEN
```

**Kết quả:**
✅ Tất cả endpoints được test
✅ Hiển thị kết quả pass/fail
✅ JSON response được format đẹp

---

## ✅ Checklist Test Hoàn chỉnh

### Email Notifications
- [ ] Đặt phòng → Email xác nhận
- [ ] Thanh toán → Email xác nhận
- [ ] Đặt món → Email xác nhận
- [ ] Admin xác nhận → Email thông báo

### 2FA Authentication
- [ ] Generate secret thành công
- [ ] QR code hiển thị đúng
- [ ] Scan QR code vào app
- [ ] Enable 2FA thành công
- [ ] Lưu recovery codes
- [ ] Login với 2FA code
- [ ] Login với recovery code
- [ ] Disable 2FA thành công

### Multi-language
- [ ] Get current language
- [ ] Get translations (vi, en)
- [ ] Change language
- [ ] Language lưu trong cookie
- [ ] Frontend hiển thị đúng

### Push Notifications
- [ ] Request permission
- [ ] Browser notification hiển thị
- [ ] Notification dropdown hoạt động
- [ ] Unread count đúng
- [ ] Mark as read hoạt động
- [ ] Real-time notifications

### In-App Notifications
- [ ] API get notifications
- [ ] API unread count
- [ ] API mark as read
- [ ] API mark all as read
- [ ] UI hiển thị notifications

---

## 🐛 Troubleshooting

Xem phần Troubleshooting trong `DEPLOYMENT-GUIDE.md`

---

## 📞 Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. Logs trong console
2. Network tab trong DevTools
3. Database schema
4. API responses

