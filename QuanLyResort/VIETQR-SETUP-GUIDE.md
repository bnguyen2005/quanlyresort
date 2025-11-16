# 🎉 Hướng Dẫn Cấu Hình VietQR (Miễn Phí)

## ✅ Đã Hoàn Thành

**VietQR service đã được implement thành công!**

- ✅ `VietQRService.cs` - Service tạo QR code URL
- ✅ `SimplePaymentController.cs` - Endpoints tạo QR code VietQR
- ✅ `simple-payment.js` - Frontend ưu tiên VietQR, fallback SePay
- ✅ `restaurant-payment.js` - Frontend ưu tiên VietQR, fallback SePay
- ✅ `Program.cs` - Đã register VietQRService

## 🔧 Cấu Hình Railway Variables

### Bước 1: Thêm Environment Variables

Vào **Railway Dashboard** → **Service** → **Variables** → Thêm các biến sau:

#### ✅ Biến Bắt Buộc:

**1. Bank Account Number:**
```
Name:  VietQR__BankAccountNumber
Value: 0901329227
```

**Hoặc dùng SePay config (nếu đã có):**
```
Name:  SePay__BankAccountNumber
Value: 0901329227
```

**2. Bank Code (Optional - mặc định: MB):**
```
Name:  VietQR__BankCode
Value: MB
```

**Hoặc dùng SePay config (nếu đã có):**
```
Name:  SePay__BankCode
Value: MB
```

**3. Bank Account Name (Optional - mặc định: Resort Deluxe):**
```
Name:  VietQR__BankAccountName
Value: Resort Deluxe
```

### Bước 2: Redeploy Service

Sau khi thêm variables:
1. Railway sẽ tự động redeploy
2. Hoặc click **"Redeploy"** trong tab **"Deployments"**

### Bước 3: Kiểm Tra Logs

Vào **Railway Dashboard** → **Service** → **Logs** → Tìm dòng:

```
[VIETQR] ✅ Service initialized with BankCode: MB, AccountNumber: ****9227
```

Nếu thấy warning:
```
[VIETQR] ⚠️ Bank Account Number chưa được cấu hình
```
→ Kiểm tra lại tên biến và giá trị

## 🎯 Cách Hoạt Động

### 1. Frontend Ưu Tiên VietQR

**Frontend sẽ:**
1. ✅ Gọi endpoint VietQR trước: `/api/simplepayment/create-qr-booking-vietqr`
2. ✅ Nếu VietQR không có hoặc lỗi → Fallback sang SePay: `/api/simplepayment/create-qr-booking`
3. ✅ Hiển thị QR code từ VietQR hoặc SePay

### 2. QR Code Format

**VietQR URL format:**
```
https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={content}
```

**Ví dụ:**
```
https://img.vietqr.io/image/MB-0901329227-compact2.png?amount=5000&addInfo=BOOKING4
```

### 3. Webhook & Polling

**VietQR không có webhook tự động**, nhưng:
- ✅ **SePay webhook** vẫn hoạt động (nếu đã cấu hình)
- ✅ **Frontend polling** vẫn hoạt động (check booking status mỗi 2 giây)

## 📊 So Sánh VietQR vs SePay

| Tính Năng | VietQR | SePay |
|-----------|--------|-------|
| **Phí** | ✅ FREE | ⚠️ Có phí |
| **QR Code** | ✅ URL | ✅ URL hoặc Base64 |
| **Webhook** | ❌ Không có | ✅ Có (nếu cấu hình) |
| **Cấu hình** | ✅ Đơn giản (chỉ cần bank account) | ⚠️ Phức tạp (API token, account ID, etc.) |
| **Tương thích** | ✅ Tất cả ngân hàng VN | ✅ Tất cả ngân hàng VN |

## 🎉 Ưu Điểm VietQR

1. ✅ **HOÀN TOÀN MIỄN PHÍ** - Không có phí giao dịch
2. ✅ **Đơn giản** - Chỉ cần bank account number
3. ✅ **QR code động** - Số tiền thay đổi theo booking/order
4. ✅ **Tương thích** - Hỗ trợ tất cả ngân hàng Việt Nam
5. ✅ **Fallback** - Tự động fallback sang SePay nếu VietQR không có

## ⚠️ Lưu Ý

1. **Bank Account Number là bắt buộc** - Nếu không có, VietQR sẽ không hoạt động
2. **Webhook** - VietQR không có webhook, nhưng SePay webhook vẫn hoạt động (nếu đã cấu hình)
3. **Polling** - Frontend vẫn polling để check payment status
4. **Fallback** - Nếu VietQR không có, frontend tự động fallback sang SePay

## 🔗 Links

- **VietQR:** https://www.vietqr.io/
- **VietQR Generator:** https://www.vietqr.io/generator
- **VietQR API Docs:** https://www.vietqr.io/api

## ✅ Checklist

- [ ] Đã thêm `VietQR__BankAccountNumber` hoặc `SePay__BankAccountNumber` vào Railway
- [ ] Đã thêm `VietQR__BankCode` hoặc `SePay__BankCode` (optional)
- [ ] Đã redeploy service
- [ ] Đã kiểm tra logs - thấy `[VIETQR] ✅ Service initialized`
- [ ] Đã test tạo QR code cho booking
- [ ] Đã test tạo QR code cho restaurant order

## 🎯 Kết Luận

**VietQR đã được implement thành công!**

**Bước tiếp theo:**
1. ✅ Thêm bank account number vào Railway variables
2. ✅ Redeploy service
3. ✅ Test tạo QR code cho booking/restaurant order
4. ✅ Kiểm tra QR code hiển thị đúng

**Lưu ý:** VietQR sẽ tự động được ưu tiên, nếu không có thì fallback sang SePay.

