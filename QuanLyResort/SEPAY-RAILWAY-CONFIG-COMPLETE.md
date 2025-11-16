# 🔧 Hướng Dẫn Cấu Hình SePay Đầy Đủ Trên Railway

## 📋 Tổng Quan

**SePay yêu cầu 4 biến môi trường bắt buộc:**
1. **SEPAY_CLIENT_ID** - Mã định danh ứng dụng
2. **SEPAY_API_KEY** - Khóa bí mật để call API
3. **SEPAY_SECRET_KEY** - Khóa để verify signature từ webhook
4. **SEPAY_WEBHOOK_URL** - URL webhook

## 🔧 Bước 1: Lấy Thông Tin Từ SePay Dashboard

### 1.1. Đăng Nhập SePay Dashboard

1. **Vào:** https://my.sepay.vn
2. **Đăng nhập** với tài khoản của bạn

### 1.2. Vào Phần API

1. **Menu:** **API** hoặc **Cài đặt → API**
2. **Xem thông tin:**
   - **CLIENT_ID:** Mã định danh ứng dụng (ví dụ: `5365`)
   - **API_KEY:** Khóa bí mật để call API (ví dụ: `spsk_live_...`)
   - **SECRET_KEY:** Khóa để verify signature (ví dụ: `spsk_live_...`)

### 1.3. Lấy Webhook URL

**Webhook URL của bạn:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

## 🔧 Bước 2: Cấu Hình Railway Variables

### 2.1. Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### 2.2. Thêm/Cập Nhật Các Biến

#### ✅ Biến 1: CLIENT_ID (Mã định danh ứng dụng)

**Format 1 (Khuyến nghị - Format mới):**
```
Name:  SEPAY_CLIENT_ID
Value: {CLIENT_ID từ SePay Dashboard}
```

**Format 2 (Format cũ - vẫn hỗ trợ):**
```
Name:  SePay__ClientId
Value: {CLIENT_ID từ SePay Dashboard}
```

**Hoặc:**
```
Name:  SePay__AccountId
Value: {CLIENT_ID từ SePay Dashboard}
```

**Ví dụ:**
```
Name:  SEPAY_CLIENT_ID
Value: 5365
```

#### ✅ Biến 2: API_KEY (Khóa bí mật để call API)

**Format 1 (Khuyến nghị - Format mới):**
```
Name:  SEPAY_API_KEY
Value: {API_KEY từ SePay Dashboard}
```

**Format 2 (Format cũ - vẫn hỗ trợ):**
```
Name:  SePay__ApiToken
Value: {API_KEY từ SePay Dashboard}
```

**Ví dụ:**
```
Name:  SEPAY_API_KEY
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

**Lưu ý:** Đây là khóa quan trọng nhất để tạo payment request.

#### ✅ Biến 3: SECRET_KEY (Khóa để verify signature)

**Format 1 (Khuyến nghị - Format mới):**
```
Name:  SEPAY_SECRET_KEY
Value: {SECRET_KEY từ SePay Dashboard}
```

**Format 2 (Format cũ - vẫn hỗ trợ):**
```
Name:  SePay__SecretKey
Value: {SECRET_KEY từ SePay Dashboard}
```

**Ví dụ:**
```
Name:  SEPAY_SECRET_KEY
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

**Lưu ý:** BẮT BUỘC phải có để validate webhook signature.

#### ✅ Biến 4: WEBHOOK_URL (URL webhook)

**Format 1 (Khuyến nghị - Format mới):**
```
Name:  SEPAY_WEBHOOK_URL
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Format 2 (Format cũ - vẫn hỗ trợ):**
```
Name:  SePay__WebhookUrl
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Lưu ý:** Phải trỏ đúng route API của bạn.

### 2.3. Các Biến Khác (Tùy Chọn)

#### Biến 5: MERCHANT_ID (Nếu có)

```
Name:  SePay__MerchantId
Value: {MERCHANT_ID từ SePay Dashboard}
```

**Ví dụ:**
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```

#### Biến 6: BANK_CODE (Mặc định: MB)

```
Name:  SePay__BankCode
Value: MB
```

#### Biến 7: BANK_ACCOUNT_NUMBER (Cho static QR code)

```
Name:  SePay__BankAccountNumber
Value: {Số tài khoản ngân hàng của bạn}
```

**Ví dụ:**
```
Name:  SePay__BankAccountNumber
Value: 0901329227
```

#### Biến 8: API_BASE_URL (Mặc định: https://pgapi.sepay.vn)

```
Name:  SePay__ApiBaseUrl
Value: https://pgapi.sepay.vn
```

**Hoặc:**
```
Name:  SEPAY_API_BASE_URL
Value: https://pgapi.sepay.vn
```

## 📊 Mapping Biến

### Format Cũ (Vẫn Hỗ Trợ):
```
SePay__ApiToken         → SEPAY_API_KEY
SePay__AccountId        → SEPAY_CLIENT_ID
SePay__ClientId         → SEPAY_CLIENT_ID
SePay__SecretKey        → SEPAY_SECRET_KEY
SePay__WebhookUrl       → SEPAY_WEBHOOK_URL
SePay__MerchantId       → MERCHANT_ID (tùy chọn)
SePay__BankCode         → BANK_CODE (tùy chọn)
SePay__BankAccountNumber → BANK_ACCOUNT_NUMBER (tùy chọn)
SePay__ApiBaseUrl       → API_BASE_URL (tùy chọn)
```

### Format Mới (Khuyến Nghị):
```
SEPAY_CLIENT_ID         → Mã định danh ứng dụng
SEPAY_API_KEY           → Khóa bí mật để call API
SEPAY_SECRET_KEY         → Khóa để verify signature
SEPAY_WEBHOOK_URL        → URL webhook
SEPAY_API_BASE_URL       → API base URL (tùy chọn)
```

## ✅ Checklist Cấu Hình

- [ ] SEPAY_CLIENT_ID đã được thêm vào Railway
- [ ] SEPAY_API_KEY đã được thêm vào Railway
- [ ] SEPAY_SECRET_KEY đã được thêm vào Railway
- [ ] SEPAY_WEBHOOK_URL đã được thêm vào Railway
- [ ] SePay__MerchantId đã được thêm (nếu có)
- [ ] SePay__BankAccountNumber đã được thêm (cho static QR code)
- [ ] Code đã được deploy với các biến mới
- [ ] SePay webhook đã được setup với URL đúng trong SePay Dashboard

## 🔧 Bước 3: Cấu Hình SePay Webhook

### 3.1. Vào SePay Dashboard

1. **Vào:** https://my.sepay.vn
2. **Menu:** **Webhooks** hoặc **Cài đặt → Webhooks**

### 3.2. Thêm Webhook

1. **Click "Thêm Webhook"** hoặc **"Add Webhook"**
2. **Điền thông tin:**
   - **URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - **Chứng thực:** Chọn một trong các tùy chọn:
     - **Không cần chứng thực** (đơn giản nhất)
     - **API Key** (nếu có)
     - **OAuth 2.0** (nếu có)
3. **Click "Lưu"** hoặc **"Save"**

### 3.3. Kiểm Tra Webhook

1. **Xem trạng thái:** Webhook phải hiển thị "Hoạt động" hoặc "Active"
2. **Test webhook:** SePay có thể có nút "Test" để test webhook

## 🧪 Bước 4: Test

### 4.1. Test Tạo QR Code

1. **Vào website:** https://quanlyresort-production.up.railway.app
2. **Đăng nhập** với tài khoản customer
3. **Tạo booking mới**
4. **Click "Thanh toán"**
5. **Kiểm tra:** QR code có hiển thị không?

### 4.2. Test Webhook

1. **Quét QR code** bằng app ngân hàng
2. **Chuyển tiền:**
   - **Nội dung:** `BOOKING{id}` (ví dụ: `BOOKING4`)
   - **Số tiền:** Đúng với booking
3. **Đợi 1-5 phút**
4. **Kiểm tra:**
   - SePay dashboard → Thống kê có tăng không?
   - Railway logs → Có webhook received không?
   - Booking status → Có = "Paid" không?
   - QR code → Có tự động ẩn không?

## 🔍 Kiểm Tra Logs

### Railway Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔍 Client ID configured: ...`
- `[SEPAY] 🔍 API Key configured: ...`
- `[SEPAY] 🔍 Secret Key configured: ...`
- `[WEBHOOK] 📥 Webhook received`

### SePay Dashboard

**SePay Dashboard → Webhooks → Thống kê**

**Kiểm tra:**
- Webhook status: "Hoạt động" hoặc "Active"
- Thống kê gửi: Có tăng không?

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables
- **Website:** https://quanlyresort-production.up.railway.app

## 💡 Lưu Ý

1. **SECRET_KEY:** BẮT BUỘC phải có để verify webhook signature
2. **API_KEY:** Quan trọng nhất để call API SePay
3. **WEBHOOK_URL:** Phải trỏ đúng route API của bạn
4. **CLIENT_ID:** Mã định danh ứng dụng của bạn
5. **Format:** Code hỗ trợ cả format cũ và format mới
6. **Deploy:** Cần deploy code mới lên Railway để áp dụng thay đổi

## 🎉 Kết Luận

**Sau khi cấu hình xong:**
- ✅ Tất cả biến môi trường đã được thêm vào Railway
- ✅ SePay webhook đã được setup với URL đúng
- ✅ Code đã được deploy với các biến mới
- ✅ Test tạo QR code → Thành công
- ✅ Test webhook → Thành công

**Bước tiếp theo:**
- Test với giao dịch thật
- Kiểm tra SePay có gửi webhook không
- Kiểm tra booking status có được cập nhật không

