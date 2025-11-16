# 📋 Tổng Hợp Các Endpoint SePay

## ✅ Danh Sách Endpoint SePay

### 1. ✅ Webhook Endpoint (Nhận callback từ SePay)

**Endpoint:**
```
POST /api/simplepayment/webhook
```

**Mô tả:**
- Nhận webhook từ SePay khi có thanh toán thành công
- Hỗ trợ cả PayOs format và SePay format
- Tự động cập nhật booking/order status thành "Paid"

**Request Body (SePay format):**
```json
{
  "id": 92704,
  "gateway": "MB",
  "content": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "in",
  "accountNumber": "0901329227",
  "referenceCode": "MBMB.3278907687"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "webhookId": "abc12345"
}
```

**HTTP Status:** `201` (Created)

**Authentication:** `[AllowAnonymous]` - Không cần authentication

---

### 2. ✅ Tạo QR Code Cho Booking (SePay)

**Endpoint:**
```
POST /api/simplepayment/create-qr-booking
```

**Mô tả:**
- Tạo QR code động cho booking bằng SePay API
- Nếu SePay API không hoạt động, fallback sang static QR code với amount động

**Request Body:**
```json
{
  "bookingId": 4
}
```

**Response:**
```json
{
  "success": true,
  "orderId": "BOOKING4",
  "orderCode": "BOOKING4",
  "qrCode": "iVBORw0KGgo...", // Base64 image (nếu SePay API thành công)
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4", // URL (nếu fallback)
  "amount": 5000,
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "description": "BOOKING4"
}
```

**HTTP Status:** `200` (OK)

**Authentication:** `[Authorize]` - Cần JWT token

---

### 3. ✅ Tạo QR Code Cho Restaurant Order (SePay)

**Endpoint:**
```
POST /api/simplepayment/create-qr-restaurant
```

**Mô tả:**
- Tạo QR code động cho restaurant order bằng SePay API
- Nếu SePay API không hoạt động, fallback sang static QR code với amount động

**Request Body:**
```json
{
  "orderId": 7
}
```

**Response:**
```json
{
  "success": true,
  "orderId": "ORDER7",
  "orderCode": "ORDER7",
  "qrCode": "iVBORw0KGgo...", // Base64 image (nếu SePay API thành công)
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=50000&des=ORDER7", // URL (nếu fallback)
  "amount": 50000,
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "description": "ORDER7"
}
```

**HTTP Status:** `200` (OK)

**Authentication:** `[Authorize]` - Cần JWT token

---

## 📊 Tổng Kết

**Có 3 endpoint SePay chính:**

1. ✅ **Webhook** - `/api/simplepayment/webhook` (POST)
   - Nhận callback từ SePay
   - Cập nhật booking/order status

2. ✅ **Create QR Booking** - `/api/simplepayment/create-qr-booking` (POST)
   - Tạo QR code động cho booking
   - Fallback sang static QR nếu API không hoạt động

3. ✅ **Create QR Restaurant** - `/api/simplepayment/create-qr-restaurant` (POST)
   - Tạo QR code động cho restaurant order
   - Fallback sang static QR nếu API không hoạt động

## 🔍 Endpoint Bổ Sung

### 4. GET Webhook Status (Kiểm tra trạng thái)

**Endpoint:**
```
GET /api/simplepayment/webhook-status
```

**Mô tả:**
- Kiểm tra trạng thái webhook system
- Không cần authentication

**Response:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "2023-03-25T14:02:37Z",
  "supportedFormats": [
    "BOOKING-{id}",
    "BOOKING-BKG{id}",
    "{id} (direct booking ID)"
  ],
  "message": "Webhook system is ready to receive payments"
}
```

### 5. GET Webhook Verify (PayOs verification)

**Endpoint:**
```
GET /api/simplepayment/webhook
```

**Mô tả:**
- PayOs sẽ gửi GET request để verify webhook URL
- Không cần authentication

**Response:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2023-03-25T14:02:37Z"
}
```

## 🔗 Endpoint URLs

**Base URL:**
```
https://quanlyresort-production.up.railway.app
```

**Full URLs:**
1. `POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
2. `POST https://quanlyresort-production.up.railway.app/api/simplepayment/create-qr-booking`
3. `POST https://quanlyresort-production.up.railway.app/api/simplepayment/create-qr-restaurant`
4. `GET https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
5. `GET https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## ✅ Checklist

- [x] **Webhook endpoint** - Đã có và hoạt động
- [x] **Create QR Booking endpoint** - Đã có và hoạt động
- [x] **Create QR Restaurant endpoint** - Đã có và hoạt động
- [x] **Webhook status endpoint** - Đã có (bổ sung)
- [x] **Webhook verify endpoint** - Đã có (bổ sung)

## 💡 Lưu Ý

1. **Webhook endpoint** không cần authentication (SePay gửi từ bên ngoài)
2. **Create QR endpoints** cần JWT token (chỉ user đã đăng nhập mới tạo được)
3. **Fallback mechanism:** Nếu SePay API không hoạt động, sẽ tự động fallback sang static QR code
4. **Format nội dung:** QR code sẽ có nội dung `BOOKING{id}` hoặc `ORDER{id}` để SePay detect

## 🧪 Test Endpoints

### Test Webhook:
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "in",
    "id": "TEST-123",
    "gateway": "MB"
  }'
```

### Test Create QR Booking:
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/create-qr-booking" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "bookingId": 4
  }'
```

### Test Create QR Restaurant:
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/create-qr-restaurant" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "orderId": 7
  }'
```

