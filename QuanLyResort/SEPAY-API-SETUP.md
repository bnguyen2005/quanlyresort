# 🔧 Hướng Dẫn Cấu Hình SePay API

## 📋 Tổng Quan

**SePay API cho phép:**
- ✅ Tạo QR code động cho từng booking/order
- ✅ QR code chứa sẵn số tiền và nội dung chuyển khoản
- ✅ Khách hàng chỉ cần quét và xác nhận
- ✅ Tự động nhận webhook khi thanh toán thành công

## 🔑 Cấu Hình SePay API Credentials

### Bước 1: Lấy API Credentials từ SePay

1. **Đăng nhập SePay Dashboard:** https://my.sepay.vn
2. **Vào phần API Settings** hoặc **Developer Settings**
3. **Lấy các thông tin:**
   - **API Token** (Bearer token)
   - **Account ID** (ID tài khoản SePay của bạn)
   - **Bank Code** (Mã ngân hàng, ví dụ: `MB`, `BIDV`, `VCB`)

### Bước 2: Cấu Hình Environment Variables

**Trong Railway Dashboard → Variables:**

| Variable | Giá Trị | Ví Dụ |
|----------|---------|-------|
| `SePay__ApiBaseUrl` | Base URL của SePay API | `https://my.sepay.vn/userapi` |
| `SePay__ApiToken` | Bearer token từ SePay | `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` |
| `SePay__AccountId` | Account ID từ SePay | `123456` |
| `SePay__BankCode` | Mã ngân hàng | `MB` |

**Hoặc trong `appsettings.json` (development):**

```json
{
  "SePay": {
    "ApiBaseUrl": "https://my.sepay.vn/userapi",
    "ApiToken": "your-api-token-here",
    "AccountId": "your-account-id-here",
    "BankCode": "MB"
  }
}
```

## 📋 Endpoints Đã Tạo

### 1. Tạo QR Code Cho Booking

**Endpoint:** `POST /api/simplepayment/create-qr-booking`

**Request:**
```json
{
  "bookingId": 10
}
```

**Response:**
```json
{
  "success": true,
  "orderId": "f23cc0fe-c343-11ef-9c27-52c7e9b4f41b",
  "orderCode": "BOOKING10",
  "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=...",
  "amount": 500000,
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "vaNumber": "963NQDORDZVTBPJ3Z7H",
  "expiredAt": "2024-12-26 11:53:26",
  "description": "BOOKING10"
}
```

### 2. Tạo QR Code Cho Restaurant Order

**Endpoint:** `POST /api/simplepayment/create-qr-restaurant`

**Request:**
```json
{
  "orderId": 7
}
```

**Response:**
```json
{
  "success": true,
  "orderId": "f23cc0fe-c343-11ef-9c27-52c7e9b4f41b",
  "orderCode": "ORDER7",
  "qrCode": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
  "qrCodeUrl": "https://qr.sepay.vn/img?acc=...",
  "amount": 150000,
  "accountNumber": "0901329227",
  "accountName": "Resort Deluxe",
  "bankName": "MB",
  "vaNumber": "963NQDORDZVTBPJ3Z7H",
  "expiredAt": "2024-12-26 11:53:26",
  "description": "ORDER7"
}
```

## 🔧 Cách Sử Dụng Trong Frontend

### Option 1: Sử Dụng QR Code Base64

**Hiển thị QR code từ base64:**
```javascript
// Gọi API
const response = await fetch('/api/simplepayment/create-qr-booking', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ bookingId: 10 })
});

const result = await response.json();

// Hiển thị QR code
if (result.qrCode) {
  const qrImg = document.getElementById('qrCodeImage');
  qrImg.src = result.qrCode; // Base64 image
}
```

### Option 2: Sử Dụng QR Code URL

**Hiển thị QR code từ URL:**
```javascript
// Gọi API
const response = await fetch('/api/simplepayment/create-qr-booking', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ bookingId: 10 })
});

const result = await response.json();

// Hiển thị QR code
if (result.qrCodeUrl) {
  const qrImg = document.getElementById('qrCodeImage');
  qrImg.src = result.qrCodeUrl; // URL to QR code
}
```

## 📋 Checklist Cấu Hình

- [ ] Đã lấy API Token từ SePay Dashboard
- [ ] Đã lấy Account ID từ SePay Dashboard
- [ ] Đã xác định Bank Code (MB, BIDV, VCB, etc.)
- [ ] Đã cấu hình environment variables trong Railway
- [ ] Đã test endpoint `/api/simplepayment/create-qr-booking`
- [ ] Đã test endpoint `/api/simplepayment/create-qr-restaurant`
- [ ] Đã update frontend để sử dụng endpoint mới

## 🐛 Troubleshooting

### Lỗi: "SePay service chưa được cấu hình"

**Nguyên nhân:**
- Environment variables chưa được set
- API Token hoặc Account ID chưa được cấu hình

**Giải pháp:**
1. Kiểm tra Railway Variables
2. Đảm bảo có các variables:
   - `SePay__ApiToken`
   - `SePay__AccountId`
   - `SePay__BankCode` (optional, default: MB)

### Lỗi: "SePay API error: Status=401"

**Nguyên nhân:**
- API Token không đúng hoặc đã hết hạn

**Giải pháp:**
1. Kiểm tra API Token trong SePay Dashboard
2. Tạo token mới nếu cần
3. Update environment variable `SePay__ApiToken`

### Lỗi: "SePay API error: Status=404"

**Nguyên nhân:**
- Account ID không đúng
- Bank Code không đúng
- API endpoint không đúng

**Giải pháp:**
1. Kiểm tra Account ID trong SePay Dashboard
2. Kiểm tra Bank Code (MB, BIDV, VCB, etc.)
3. Kiểm tra API Base URL: `https://my.sepay.vn/userapi`

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **SePay API Documentation:** https://docs.sepay.vn
- **Railway Dashboard:** https://railway.app

## 💡 Lưu Ý

1. **API Token:** Cần bảo mật, không commit vào git
2. **Account ID:** Là ID tài khoản SePay của bạn
3. **Bank Code:** Mã ngân hàng (MB, BIDV, VCB, etc.)
4. **Duration:** QR code có thời gian hiệu lực (mặc định: 24 giờ)
5. **Webhook:** Đảm bảo đã setup SePay webhook để nhận thông báo thanh toán

## 🎯 Kết Luận

**Sau khi cấu hình:**
- ✅ Có thể tạo QR code động cho booking
- ✅ Có thể tạo QR code động cho restaurant order
- ✅ QR code chứa sẵn số tiền và nội dung chuyển khoản
- ✅ Khách hàng chỉ cần quét và xác nhận
- ✅ Webhook tự động nhận thông báo thanh toán

