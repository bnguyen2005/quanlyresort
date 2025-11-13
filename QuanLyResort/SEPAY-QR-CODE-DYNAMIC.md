# 📱 QR Code Động SePay - Tóm Tắt

## ✅ Đã Tạo

**Backend:**
- ✅ `SePayService.cs` - Service để gọi SePay API
- ✅ `POST /api/simplepayment/create-qr-booking` - Tạo QR code cho booking
- ✅ `POST /api/simplepayment/create-qr-restaurant` - Tạo QR code cho restaurant order

## 📋 Cấu Hình Cần Thiết

**Railway Variables:**
- `SePay__ApiToken` - API Token từ SePay
- `SePay__AccountId` - Account ID từ SePay
- `SePay__BankCode` - Mã ngân hàng (MB, BIDV, VCB, etc.) - Optional, default: MB
- `SePay__ApiBaseUrl` - Base URL - Optional, default: `https://my.sepay.vn/userapi`

**Xem chi tiết:** `SEPAY-API-SETUP.md`

## 🔧 Cách Sử Dụng

### 1. Cấu Hình SePay API

**Railway Dashboard → Variables:**
- Thêm `SePay__ApiToken`
- Thêm `SePay__AccountId`
- Thêm `SePay__BankCode` (optional)

### 2. Gọi API Từ Frontend

**Tạo QR code cho booking:**
```javascript
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
  document.getElementById('qrCodeImage').src = result.qrCode;
}
```

**Tạo QR code cho restaurant order:**
```javascript
const response = await fetch('/api/simplepayment/create-qr-restaurant', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ orderId: 7 })
});

const result = await response.json();

// Hiển thị QR code
if (result.qrCode) {
  document.getElementById('qrCodeImage').src = result.qrCode;
}
```

## 📋 So Sánh QR Code Tĩnh vs Động

### QR Code Tĩnh (Hiện Tại)

**Ưu điểm:**
- ✅ Đơn giản, không cần API
- ✅ Sử dụng VietQR (miễn phí)

**Nhược điểm:**
- ❌ Khách hàng cần tự nhập nội dung chuyển khoản
- ❌ Dễ nhầm lẫn booking ID

### QR Code Động (SePay API)

**Ưu điểm:**
- ✅ QR code chứa sẵn số tiền và nội dung
- ✅ Khách hàng chỉ cần quét và xác nhận
- ✅ Mỗi booking/order có QR code riêng
- ✅ Tự động nhận webhook khi thanh toán

**Nhược điểm:**
- ❌ Cần SePay API credentials
- ❌ Cần cấu hình environment variables

## 🔄 Migration Path

**Có thể dùng cả 2:**
1. **QR Code Tĩnh (VietQR)** - Fallback nếu SePay API không hoạt động
2. **QR Code Động (SePay)** - Ưu tiên nếu đã cấu hình

**Logic trong frontend:**
```javascript
// Thử tạo QR code động trước
try {
  const sepayQR = await createSePayQRCode(bookingId);
  if (sepayQR) {
    // Sử dụng QR code động
    displayQRCode(sepayQR.qrCode);
  } else {
    // Fallback về QR code tĩnh
    displayStaticQRCode(bookingId);
  }
} catch (error) {
  // Fallback về QR code tĩnh
  displayStaticQRCode(bookingId);
}
```

## 📋 Checklist

- [ ] Đã cấu hình SePay API credentials trong Railway
- [ ] Đã test endpoint `/api/simplepayment/create-qr-booking`
- [ ] Đã test endpoint `/api/simplepayment/create-qr-restaurant`
- [ ] Đã update frontend để sử dụng endpoint mới (optional)
- [ ] Đã setup SePay webhook để nhận thông báo thanh toán

## 🔗 Links

- **Hướng dẫn cấu hình:** `SEPAY-API-SETUP.md`
- **SePay Dashboard:** https://my.sepay.vn
- **SePay API Docs:** https://docs.sepay.vn

## 💡 Lưu Ý

1. **API Credentials:** Cần lấy từ SePay Dashboard
2. **Fallback:** Có thể dùng QR code tĩnh nếu SePay API không hoạt động
3. **Webhook:** Đảm bảo đã setup SePay webhook
4. **Testing:** Test với booking/order thật sau khi cấu hình

