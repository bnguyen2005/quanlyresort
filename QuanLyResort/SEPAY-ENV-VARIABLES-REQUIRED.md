# 🔧 SePay Biến Môi Trường Bắt Buộc

## 📋 Yêu Cầu SePay

**SePay yêu cầu 4 biến môi trường bắt buộc:**

1. **SEPAY_CLIENT_ID** - Mã định danh ứng dụng
2. **SEPAY_API_KEY** - Khóa bí mật để call API
3. **SEPAY_SECRET_KEY** - Khóa để verify signature từ webhook
4. **SEPAY_WEBHOOK_URL** - URL webhook

## 🔧 Cấu Hình Railway Variables

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Thêm/Cập Nhật Các Biến

#### ✅ Biến 1: CLIENT_ID (Mã định danh ứng dụng)
```
Name:  SePay__ClientId
Value: {CLIENT_ID từ SePay Dashboard}
```

**Hoặc dùng format mới:**
```
Name:  SEPAY_CLIENT_ID
Value: {CLIENT_ID từ SePay Dashboard}
```

#### ✅ Biến 2: API_KEY (Khóa bí mật để call API)
```
Name:  SePay__ApiToken
Value: {API_KEY từ SePay Dashboard}
```

**Hoặc dùng format mới:**
```
Name:  SEPAY_API_KEY
Value: {API_KEY từ SePay Dashboard}
```

**Lưu ý:** Đây là khóa quan trọng nhất để tạo payment request.

#### ✅ Biến 3: SECRET_KEY (Khóa để verify signature)
```
Name:  SePay__SecretKey
Value: {SECRET_KEY từ SePay Dashboard}
```

**Hoặc dùng format mới:**
```
Name:  SEPAY_SECRET_KEY
Value: {SECRET_KEY từ SePay Dashboard}
```

**Lưu ý:** BẮT BUỘC phải có để validate webhook signature.

#### ✅ Biến 4: WEBHOOK_URL (URL webhook)
```
Name:  SePay__WebhookUrl
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Hoặc dùng format mới:**
```
Name:  SEPAY_WEBHOOK_URL
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Lưu ý:** Phải trỏ đúng route API của bạn.

## 📊 Mapping Biến

### Format Cũ (Hiện Tại):
```
SePay__ApiToken      → SEPAY_API_KEY
SePay__AccountId     → SEPAY_CLIENT_ID (hoặc có thể khác)
SePay__SecretKey     → SEPAY_SECRET_KEY (MỚI - cần thêm)
SePay__WebhookUrl    → SEPAY_WEBHOOK_URL (MỚI - cần thêm)
```

### Format Mới (SePay Yêu Cầu):
```
SEPAY_CLIENT_ID      → Mã định danh ứng dụng
SEPAY_API_KEY        → Khóa bí mật để call API
SEPAY_SECRET_KEY     → Khóa để verify signature
SEPAY_WEBHOOK_URL    → URL webhook
```

## 🔍 Lấy Thông Tin Từ SePay Dashboard

### Bước 1: Đăng Nhập SePay Dashboard

1. **Vào:** https://my.sepay.vn
2. **Đăng nhập** với tài khoản của bạn

### Bước 2: Vào Phần API

1. **Menu:** **API** hoặc **Cài đặt → API**
2. **Xem thông tin:**
   - **CLIENT_ID:** Mã định danh ứng dụng
   - **API_KEY:** Khóa bí mật để call API
   - **SECRET_KEY:** Khóa để verify signature

### Bước 3: Lấy Webhook URL

**Webhook URL:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

## 📋 Checklist Cấu Hình

- [ ] SEPAY_CLIENT_ID đã được thêm vào Railway
- [ ] SEPAY_API_KEY đã được thêm vào Railway
- [ ] SEPAY_SECRET_KEY đã được thêm vào Railway
- [ ] SEPAY_WEBHOOK_URL đã được thêm vào Railway
- [ ] Code đã được deploy với các biến mới
- [ ] SePay webhook đã được setup với URL đúng

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables

## 💡 Lưu Ý

1. **SECRET_KEY:** BẮT BUỘC phải có để verify webhook signature
2. **API_KEY:** Quan trọng nhất để call API SePay
3. **WEBHOOK_URL:** Phải trỏ đúng route API của bạn
4. **CLIENT_ID:** Mã định danh ứng dụng của bạn

