# 💰 Payment Gateway Miễn Phí - Lựa Chọn Tốt Nhất

## 📋 Yêu Cầu

**Payment gateway cần:**
- ✅ **Miễn phí** hoặc phí thấp
- ✅ Hỗ trợ QR code
- ✅ Webhook tự động
- ✅ Tương thích Railway
- ✅ Phù hợp thị trường Việt Nam

## 🎯 Lựa Chọn Tốt Nhất: VietQR (Miễn Phí)

### ✅ VietQR - QR Code Miễn Phí

**Ưu điểm:**
- ✅ **HOÀN TOÀN MIỄN PHÍ** - Không có phí giao dịch
- ✅ QR Code động
- ✅ Tự động detect thanh toán qua webhook
- ✅ Hỗ trợ tất cả ngân hàng Việt Nam
- ✅ Không cần đăng ký merchant
- ✅ Tương thích Railway
- ✅ Dễ tích hợp

**Cách hoạt động:**
1. Tạo QR code với nội dung: `BOOKING{id}`
2. Khách hàng quét QR và chuyển khoản
3. Ngân hàng gửi SMS/notification
4. Backend polling hoặc webhook (nếu có service hỗ trợ)

**Website:** https://www.vietqr.io/

### ⚠️ Hạn Chế:
- ⚠️ Không có webhook tự động từ VietQR
- ⚠️ Cần polling hoặc dùng service khác để detect thanh toán
- ⚠️ Cần có tài khoản ngân hàng để nhận tiền

## 🔄 Giải Pháp: VietQR + SePay (Chỉ Dùng Webhook)

**Ý tưởng:**
- ✅ Dùng VietQR để tạo QR code (miễn phí)
- ✅ Dùng SePay chỉ để nhận webhook (không cần tạo order)
- ✅ SePay detect thanh toán và gửi webhook

**Cách hoạt động:**
1. Tạo QR code bằng VietQR với nội dung: `BOOKING{id}`
2. Khách hàng quét QR và chuyển khoản
3. SePay detect thanh toán (nếu đã link tài khoản)
4. SePay gửi webhook → Backend cập nhật booking

**Lưu ý:** Cần SePay account đã link với tài khoản ngân hàng để nhận webhook.

## 💡 Các Lựa Chọn Khác

### 1. Stripe (Có Free Tier)

**Free Tier:**
- ✅ $0 phí setup
- ✅ Phí giao dịch: 2.9% + $0.30 (cho thẻ quốc tế)
- ✅ Không có phí hàng tháng
- ✅ Webhook miễn phí

**Nhược điểm:**
- ❌ Không phổ biến tại Việt Nam
- ❌ Chủ yếu cho thẻ quốc tế
- ❌ Không hỗ trợ QR code trực tiếp

**Website:** https://stripe.com

### 2. PayPal (Có Free Tier)

**Free Tier:**
- ✅ $0 phí setup
- ✅ Phí giao dịch: 3.4% + fixed fee
- ✅ Webhook miễn phí

**Nhược điểm:**
- ❌ Không phổ biến tại Việt Nam
- ❌ Không hỗ trợ QR code trực tiếp

**Website:** https://developer.paypal.com

### 3. Momo (Có Free Tier)

**Free Tier:**
- ✅ Có thể miễn phí cho một số giao dịch
- ✅ Phí giao dịch: Thấp (cần kiểm tra)
- ✅ Hỗ trợ QR code

**Nhược điểm:**
- ⚠️ Cần đăng ký merchant
- ⚠️ Webhook có thể không ổn định

**Website:** https://developers.momo.vn

### 4. VNPay (Có Free Tier)

**Free Tier:**
- ⚠️ Có thể có phí setup
- ⚠️ Phí giao dịch: Cần kiểm tra
- ✅ Hỗ trợ QR code

**Nhược điểm:**
- ⚠️ Cần đăng ký merchant
- ⚠️ Có thể có phí

**Website:** https://vnpay.vn

## 🎯 Giải Pháp Tốt Nhất: VietQR + Polling

### Cách Hoạt Động:

1. **Tạo QR Code bằng VietQR:**
   - Nội dung: `BOOKING{id}`
   - Số tài khoản: Tài khoản của bạn
   - Số tiền: Động (thay đổi theo booking)

2. **Khách hàng quét QR và chuyển khoản:**
   - App ngân hàng tự động điền thông tin
   - Chuyển khoản thành công

3. **Backend Polling:**
   - Frontend polling backend mỗi 3-5 giây
   - Backend kiểm tra booking status
   - Nếu có thay đổi → Cập nhật UI

4. **Manual Verification (Nếu cần):**
   - Admin có thể verify thanh toán thủ công
   - Hoặc dùng SePay chỉ để nhận webhook (không tạo order)

### Ưu Điểm:
- ✅ **HOÀN TOÀN MIỄN PHÍ**
- ✅ Không cần đăng ký merchant
- ✅ QR code động
- ✅ Tương thích Railway
- ✅ Dễ tích hợp

### Nhược Điểm:
- ⚠️ Không có webhook tự động
- ⚠️ Cần polling hoặc manual verification
- ⚠️ Có thể delay vài giây

## 🔧 Implement VietQR

### Bước 1: Tạo QR Code URL

**Format:**
```
https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={content}
```

**Ví dụ:**
```
https://img.vietqr.io/image/MB-0901329227-compact2.png?amount=5000&addInfo=BOOKING4
```

### Bước 2: Hiển Thị QR Code

**Frontend:**
```javascript
const qrCodeUrl = `https://img.vietqr.io/image/MB-0901329227-compact2.png?amount=${amount}&addInfo=BOOKING${bookingId}`;
document.getElementById('qr-code').src = qrCodeUrl;
```

### Bước 3: Polling Payment Status

**Frontend polling backend mỗi 3-5 giây:**
```javascript
setInterval(async () => {
    const booking = await fetchBookingStatus(bookingId);
    if (booking.status === 'Paid') {
        hideQRCode();
        showSuccess();
    }
}, 3000);
```

### Bước 4: Manual Verification (Optional)

**Admin có thể verify thanh toán thủ công:**
- Kiểm tra tài khoản ngân hàng
- Xác nhận thanh toán
- Cập nhật booking status = "Paid"

## 📊 So Sánh

| Payment Gateway | Phí | QR Code | Webhook | Railway | Khuyến Nghị |
|----------------|-----|---------|---------|---------|-------------|
| **VietQR** | ✅ FREE | ✅ | ❌ (Polling) | ✅ | ⭐⭐⭐ |
| **Stripe** | ⚠️ 2.9% | ❌ | ✅ | ✅ | ⭐ (quốc tế) |
| **PayPal** | ⚠️ 3.4% | ❌ | ✅ | ✅ | ⭐ (quốc tế) |
| **Momo** | ⚠️ Thấp | ✅ | ⚠️ | ✅ | ⭐⭐ |
| **VNPay** | ⚠️ Có phí | ✅ | ✅ | ✅ | ⭐⭐ |
| **SePay** | ⚠️ Có phí | ✅ | ⚠️ | ✅ | ⭐ |

## 🎯 Khuyến Nghị

**Giải pháp tốt nhất: VietQR (Miễn Phí)**

**Lý do:**
- ✅ **HOÀN TOÀN MIỄN PHÍ**
- ✅ QR code động
- ✅ Tương thích Railway
- ✅ Dễ tích hợp
- ✅ Không cần đăng ký merchant

**Cách sử dụng:**
1. Tạo QR code bằng VietQR URL
2. Frontend polling backend để check payment status
3. Admin có thể verify thủ công nếu cần

## 💡 Lưu Ý

1. **VietQR miễn phí:** Không có phí giao dịch
2. **Polling:** Cần polling thay vì webhook
3. **Manual verification:** Có thể cần verify thủ công
4. **SePay webhook:** Có thể dùng SePay chỉ để nhận webhook (không tạo order)

## 🔗 Links

- **VietQR:** https://www.vietqr.io/
- **VietQR Generator:** https://www.vietqr.io/generator
- **VietQR API:** https://www.vietqr.io/api

## 🎯 Kết Luận

**VietQR là lựa chọn tốt nhất cho payment gateway miễn phí!**

**Bước tiếp theo:**
1. Implement VietQR QR code generation
2. Implement polling mechanism
3. Test với giao dịch thật
4. Deploy lên Railway

Bạn có muốn tôi implement VietQR service cho bạn không?

