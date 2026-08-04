# 🚀 Hướng dẫn Deploy và Test

## 📋 Mục lục
1. [Chuẩn bị trước khi deploy](#chuẩn-bị)
2. [Deploy lên GitHub](#deploy-github)
3. [Deploy lên Render/Railway/Vercel](#deploy-cloud)
4. [Test các tính năng](#test-tính-năng)

---

## 🔧 Chuẩn bị trước khi deploy

### 1. Kiểm tra cấu hình

#### Email Settings (appsettings.json)
```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUsername": "phamthahlam@gmail.com",
  "SmtpPassword": "mylghnnnbhxowmvb",
  "FromEmail": "phamthahlam@gmail.com",
  "FromName": "Resort Deluxe",
  "EnableSsl": "true"
}
```

✅ **Đã cấu hình sẵn**

#### SMS Settings (đã tắt)
```json
"SmsSettings": {
  "Enabled": "false"
}
```

✅ **SMS đã tắt theo yêu cầu**

### 2. Kiểm tra Database Migration

✅ **2FA fields đã được thêm vào database**

### 3. Build project
```bash
cd QuanLyResort
dotnet build
```

✅ **Build thành công**

---

## 📤 Deploy lên GitHub

### Bước 1: Kiểm tra thay đổi
```bash
cd "/Users/vyto/Downloads/QuanLyResort-main (1)/QuanLyResort-main"
git status
```

### Bước 2: Add và commit
```bash
git add .
git commit -m "Add advanced features: i18n, 2FA, notifications"
```

### Bước 3: Push lên GitHub
```bash
git push origin main
```

**Lưu ý:** Nếu cần token:
```bash
git remote set-url origin https://YOUR_TOKEN@github.com/bnguyen2005/quanlyresort.git
git push origin main
git remote set-url origin https://github.com/bnguyen2005/quanlyresort.git
```

---

## ☁️ Deploy lên Cloud (Render/Railway)

### Render.com

#### Bước 1: Tạo Web Service
1. Vào https://render.com
2. Chọn **New** → **Web Service**
3. Connect GitHub repository
4. Chọn branch `main`

#### Bước 2: Cấu hình
- **Name**: `quanlyresort`
- **Environment**: `.NET Core`
- **Build Command**: `dotnet publish -c Release -o ./publish`
- **Start Command**: `dotnet ./publish/QuanLyResort.dll`

#### Bước 3: Environment Variables
Thêm các biến môi trường:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=your-connection-string
EmailSettings__SmtpUsername=phamthahlam@gmail.com
EmailSettings__SmtpPassword=mylghnnnbhxowmvb
JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
```

#### Bước 4: Deploy
Click **Create Web Service** và chờ deploy hoàn tất.

---

## 🧪 Test các tính năng

### 1. Test Email Notifications

#### Test đặt phòng
1. Đăng nhập với tài khoản customer
2. Vào trang đặt phòng
3. Chọn phòng và đặt phòng
4. **Kiểm tra email** `phamthahlam@gmail.com` để xem email xác nhận

#### Test thanh toán
1. Vào "Đặt phòng của tôi"
2. Thanh toán một booking
3. **Kiểm tra email** để xem email xác nhận thanh toán

#### API Test (Postman/curl)
```bash
# Test gửi email trực tiếp
curl -X POST https://your-domain.com/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "subject": "Test",
    "message": "Test message"
  }'
```

---

### 2. Test 2FA Authentication

#### Bước 1: Generate Secret & QR Code
```bash
# Đăng nhập trước để lấy token
TOKEN="your-jwt-token"

# Generate secret
curl -X POST https://your-domain.com/api/auth/2fa/generate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

**Response:**
```json
{
  "secret": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/...",
  "qrCodeImage": "base64-encoded-image",
  "message": "Scan QR code with authenticator app"
}
```

#### Bước 2: Scan QR Code
1. Mở app **Google Authenticator** hoặc **Microsoft Authenticator**
2. Scan QR code từ response
3. Lấy 6-digit code từ app

#### Bước 3: Enable 2FA
```bash
# Enable 2FA với code từ authenticator
curl -X POST https://your-domain.com/api/auth/2fa/enable \
  -H "Authorization: Bearer $TOKEN" \
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

⚠️ **Lưu recovery codes ở nơi an toàn!**

#### Bước 4: Test Login với 2FA
1. Đăng xuất
2. Đăng nhập lại với email/password
3. Hệ thống sẽ yêu cầu nhập 2FA code
4. Nhập code từ authenticator app
5. Đăng nhập thành công

#### Bước 5: Test Recovery Code
```bash
# Nếu mất authenticator, dùng recovery code
curl -X POST https://your-domain.com/api/auth/2fa/verify \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "code": "12345678"
  }'
```

#### Bước 6: Disable 2FA (nếu cần)
```bash
curl -X POST https://your-domain.com/api/auth/2fa/disable \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "password": "your-password"
  }'
```

---

### 3. Test Multi-language Support (i18n)

#### Bước 1: Get Current Language
```bash
curl https://your-domain.com/api/localization/current
```

**Response:**
```json
{
  "language": "vi"
}
```

#### Bước 2: Get Translations
```bash
# Tiếng Việt
curl https://your-domain.com/api/localization/strings?lang=vi

# Tiếng Anh
curl https://your-domain.com/api/localization/strings?lang=en
```

**Response:**
```json
{
  "language": "vi",
  "strings": {
    "common.save": "Lưu",
    "common.cancel": "Hủy",
    "auth.login": "Đăng nhập",
    ...
  }
}
```

#### Bước 3: Change Language
```bash
# Đăng nhập trước
TOKEN="your-jwt-token"

# Đổi sang tiếng Anh
curl -X POST https://your-domain.com/api/localization/set-language \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "language": "en"
  }'
```

#### Bước 4: Test trên Frontend
1. Mở website
2. Thêm `?lang=en` vào URL để xem tiếng Anh
3. Hoặc dùng dropdown language selector (nếu có)

---

### 4. Test Push Notifications

#### Bước 1: Request Permission
Mở browser console và chạy:
```javascript
// Request notification permission
if ('Notification' in window) {
  Notification.requestPermission().then(permission => {
    console.log('Permission:', permission);
  });
}
```

#### Bước 2: Test Notification Service
```javascript
// Check notification service
if (window.notificationService) {
  console.log('Notification service loaded');
  
  // Load unread count
  window.notificationService.loadUnreadCount();
  
  // Get notifications
  window.notificationService.getNotifications().then(notifications => {
    console.log('Notifications:', notifications);
  });
}
```

#### Bước 3: Test Browser Notification
```javascript
// Show test notification
window.notificationService.showBrowserNotification('Test Notification', {
  body: 'This is a test notification',
  icon: '/customer/images/logo.png'
});
```

#### Bước 4: Test Real Notifications
1. Đặt phòng hoặc thanh toán
2. Kiểm tra xem có nhận được browser notification không
3. Kiểm tra dropdown notification icon trên navbar

---

### 5. Test In-App Notifications

#### Bước 1: Get Notifications
```bash
TOKEN="your-jwt-token"

curl https://your-domain.com/api/notifications \
  -H "Authorization: Bearer $TOKEN"
```

#### Bước 2: Get Unread Count
```bash
curl https://your-domain.com/api/notifications/unread-count \
  -H "Authorization: Bearer $TOKEN"
```

#### Bước 3: Mark as Read
```bash
curl -X PATCH https://your-domain.com/api/notifications/1/read \
  -H "Authorization: Bearer $TOKEN"
```

#### Bước 4: Mark All as Read
```bash
curl -X PATCH https://your-domain.com/api/notifications/read-all \
  -H "Authorization: Bearer $TOKEN"
```

---

## 📝 Checklist Test

### Email Notifications
- [ ] Đặt phòng → Nhận email xác nhận
- [ ] Thanh toán → Nhận email xác nhận thanh toán
- [ ] Đặt món → Nhận email xác nhận đơn hàng
- [ ] Admin xác nhận thanh toán → Khách hàng nhận email

### 2FA Authentication
- [ ] Generate secret thành công
- [ ] QR code hiển thị đúng
- [ ] Scan QR code vào authenticator app
- [ ] Enable 2FA thành công
- [ ] Lưu recovery codes
- [ ] Đăng nhập với 2FA code
- [ ] Đăng nhập với recovery code (nếu mất app)
- [ ] Disable 2FA thành công

### Multi-language
- [ ] Get current language
- [ ] Get translations (vi, en)
- [ ] Change language
- [ ] Language được lưu trong cookie
- [ ] Frontend hiển thị đúng ngôn ngữ

### Push Notifications
- [ ] Request permission thành công
- [ ] Browser notification hiển thị
- [ ] Notification dropdown hoạt động
- [ ] Unread count hiển thị đúng
- [ ] Mark as read hoạt động
- [ ] Real-time notifications khi có sự kiện

### In-App Notifications
- [ ] API get notifications hoạt động
- [ ] Unread count API hoạt động
- [ ] Mark as read API hoạt động
- [ ] Mark all as read API hoạt động
- [ ] Notifications hiển thị trong UI

---

## 🐛 Troubleshooting

### Email không gửi được
1. Kiểm tra App Password đúng chưa
2. Kiểm tra SMTP settings trong appsettings.json
3. Kiểm tra log: `[EmailService]` trong console
4. Test với Gmail SMTP tester

### 2FA không hoạt động
1. Kiểm tra Otp.NET package đã cài chưa
2. Kiểm tra database có columns chưa
3. Kiểm tra secret được generate đúng chưa
4. Kiểm tra code từ authenticator app đúng format (6 digits)

### i18n không hoạt động
1. Kiểm tra LocalizationService đã đăng ký trong Program.cs
2. Kiểm tra cookie `language` có được set không
3. Kiểm tra API endpoint hoạt động

### Notifications không hiển thị
1. Kiểm tra browser permission
2. Kiểm tra notification service đã load chưa
3. Kiểm tra API token hợp lệ
4. Kiểm tra console logs

---

## 📞 Hỗ trợ

Nếu gặp vấn đề:
1. Kiểm tra logs trong console
2. Kiểm tra Network tab trong DevTools
3. Kiểm tra database có đúng schema không
4. Xem lại tài liệu trong `ADVANCED-FEATURES-IMPLEMENTATION.md`

