# 📱 Hướng Dẫn Mã QR Thanh Toán SePay

## 🎯 Mã QR Thanh Toán SePay

**Mã QR thanh toán SePay là gì?**
- ✅ Là mã QR code được tạo bởi SePay
- ✅ Chứa thông tin tài khoản ngân hàng của bạn (MB: `0901329227`)
- ✅ Chứa nội dung chuyển khoản (code thanh toán: `BOOKING{id}`)
- ✅ Khách hàng quét QR → Chuyển khoản vào tài khoản MB của bạn → SePay gửi webhook

## 📋 Cách Hoạt Động

### 1. Tạo Mã QR Thanh Toán

**Có 2 cách:**

#### Cách 1: Tạo QR Code Tĩnh (Static QR)

**Trong SePay Dashboard:**
1. Vào **"QR Code"** hoặc **"Tạo QR Code"**
2. Chọn **"QR Code Tĩnh"**
3. Điền thông tin:
   - **Tài khoản ngân hàng:** `0901329227` (MB)
   - **Tên người nhận:** Tên của bạn/công ty
   - **Nội dung mặc định:** `BOOKING` (hoặc để trống)
4. **Tạo QR Code**
5. **Download** QR code và hiển thị trên website

**Lưu ý:**
- QR Code tĩnh không có số booking ID cụ thể
- Khách hàng cần tự nhập nội dung: `BOOKING{id}` khi chuyển khoản
- Hoặc bạn có thể tạo QR code động cho từng booking

#### Cách 2: Tạo QR Code Động (Dynamic QR) - Khuyến Nghị ⭐

**Trong code/API:**
1. Khi khách hàng tạo booking, tạo QR code động với nội dung: `BOOKING{id}`
2. Sử dụng SePay API để tạo QR code
3. Hiển thị QR code trên trang thanh toán

**Ví dụ:**
- Booking ID = 10 → QR code với nội dung: `BOOKING10`
- Booking ID = 25 → QR code với nội dung: `BOOKING25`

**Lợi ích:**
- ✅ Mỗi booking có QR code riêng
- ✅ Nội dung đã có sẵn booking ID
- ✅ Khách hàng không cần nhập thủ công
- ✅ Webhook sẽ tự động nhận đúng booking ID

## 🔍 Tài Khoản Ngân Hàng

**Tài khoản ngân hàng MB của bạn:**
- **Số tài khoản:** `0901329227`
- **Ngân hàng:** MB (Military Bank)
- **Tên người nhận:** Tên của bạn/công ty

**Lưu ý:**
- ✅ Mã QR sẽ chứa thông tin tài khoản này
- ✅ Khi khách hàng quét QR và chuyển khoản, tiền sẽ vào tài khoản MB này
- ✅ SePay sẽ gửi webhook khi có tiền vào tài khoản này

## 📱 Cách Khách Hàng Thanh Toán

### Với QR Code Tĩnh:

1. **Quét QR code** trên website
2. **Mở app ngân hàng** (MB, Vietcombank, BIDV, etc.)
3. **Nhập số tiền** cần thanh toán
4. **Nhập nội dung chuyển khoản:** `BOOKING{id}` (ví dụ: `BOOKING10`)
5. **Xác nhận** chuyển khoản
6. **SePay nhận được** → Gửi webhook → Booking tự động update thành "Paid"

### Với QR Code Động:

1. **Quét QR code** trên website (đã có sẵn nội dung `BOOKING{id}`)
2. **Mở app ngân hàng**
3. **Kiểm tra nội dung** đã có sẵn: `BOOKING{id}`
4. **Nhập số tiền** (nếu chưa có)
5. **Xác nhận** chuyển khoản
6. **SePay nhận được** → Gửi webhook → Booking tự động update thành "Paid"

## 🔧 Tích Hợp Vào Website

### Option 1: QR Code Tĩnh (Đơn Giản)

**Hiển thị QR code tĩnh trên trang thanh toán:**
```html
<img src="/images/sepay-qr-code.png" alt="QR Code Thanh Toán">
<p>Nội dung chuyển khoản: <strong>BOOKING{id}</strong></p>
```

**Lưu ý:**
- Thay `{id}` bằng booking ID thực tế
- Khách hàng cần tự nhập nội dung khi chuyển khoản

### Option 2: QR Code Động (Khuyến Nghị) ⭐

**Sử dụng SePay API để tạo QR code động:**

1. **Tạo endpoint** để generate QR code:
```csharp
[HttpGet("booking/{id}/qr-code")]
public async Task<IActionResult> GetBookingQRCode(int id)
{
    // Tạo QR code với nội dung: BOOKING{id}
    var qrContent = $"BOOKING{id}";
    var qrCode = GenerateQRCode(qrContent, "0901329227", "Tên người nhận");
    return Ok(new { qrCode, content = qrContent });
}
```

2. **Hiển thị QR code** trên trang thanh toán:
```html
<img src="/api/bookings/{id}/qr-code" alt="QR Code Thanh Toán">
<p>Nội dung: <strong>BOOKING{id}</strong></p>
```

## 📋 Checklist Setup QR Code

- [ ] Đã có tài khoản ngân hàng MB: `0901329227`
- [ ] Đã tạo QR code tĩnh hoặc động
- [ ] Đã hiển thị QR code trên trang thanh toán
- [ ] Đã hướng dẫn khách hàng nhập nội dung: `BOOKING{id}`
- [ ] Đã setup SePay webhook
- [ ] Đã test với giao dịch thật

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **QR Code Management:** https://my.sepay.vn/qr-codes (nếu có)
- **SePay API Documentation:** https://docs.sepay.vn (nếu có)

## 💡 Lưu Ý

1. **QR Code tĩnh:** Đơn giản nhưng khách hàng cần nhập nội dung thủ công
2. **QR Code động:** Phức tạp hơn nhưng tiện lợi hơn cho khách hàng
3. **Nội dung chuyển khoản:** Phải đúng format `BOOKING{id}` để webhook hoạt động
4. **Tài khoản ngân hàng:** Mã QR sẽ chứa thông tin tài khoản MB của bạn

## 🎯 Kết Luận

**Mã QR thanh toán SePay:**
- ✅ Chứa thông tin tài khoản ngân hàng MB của bạn (`0901329227`)
- ✅ Chứa nội dung chuyển khoản (`BOOKING{id}`)
- ✅ Khách hàng quét QR → Chuyển khoản → SePay gửi webhook → Booking tự động update

**Bạn có thể:**
- ✅ Tạo QR code tĩnh trong SePay dashboard
- ✅ Hoặc tạo QR code động bằng SePay API
- ✅ Hiển thị QR code trên website
- ✅ Hướng dẫn khách hàng nhập nội dung: `BOOKING{id}`

