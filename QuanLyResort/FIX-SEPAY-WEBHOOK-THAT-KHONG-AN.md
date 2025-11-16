# 🔧 Fix: SePay Webhook Thật Không Ẩn QR Code

## 📋 Vấn Đề

**Tình huống:**
- ✅ Test webhook thủ công (curl) → QR code ẩn và hiển thị "Thanh toán thành công" ✅
- ❌ Quét mã QR thật và chuyển tiền vào MB Bank → QR code không ẩn ❌

**Nguyên nhân:** SePay chưa gửi webhook thật khi có giao dịch thật từ MB Bank.

## 🔍 Nguyên Nhân Có Thể

### 1. Nội Dung Chuyển Khoản Không Đúng Format

**Khi quét QR code và chuyển tiền:**
- App ngân hàng có thể tự động điền nội dung chuyển khoản
- Nội dung có thể không đúng format: `BOOKING{id}`
- Có thể có khoảng trắng hoặc ký tự đặc biệt

**Kiểm tra:**
- Nội dung chuyển khoản có đúng `BOOKING4` không?
- Không có khoảng trắng: `BOOKING 4` ❌
- Không có ký tự đặc biệt

### 2. SePay Chưa Gửi Webhook Thật

**SePay có thể:**
- Cần thời gian để xử lý (vài phút đến vài giờ)
- Chỉ gửi webhook cho giao dịch hợp lệ
- Không gửi webhook cho giao dịch test hoặc số tiền quá nhỏ

**Kiểm tra:**
- SePay dashboard → Thống kê có tăng không?
- Railway logs có webhook received không?

### 3. Điều Kiện Webhook Không Khớp

**Trong SePay Dashboard, kiểm tra:**
- Tài khoản ngân hàng có đúng không? (`0901329227`)
- Số tiền có trong khoảng cho phép không?
- Loại giao dịch có đúng không? ("Có tiền vào")
- Có điều kiện nào khác không? (pattern nội dung, etc.)

## 🎯 Giải Pháp

### Bước 1: Kiểm Tra Nội Dung Chuyển Khoản

**Khi quét QR code và chuyển tiền:**

1. **Quét QR code:**
   - QR code sẽ hiển thị thông tin: Số tài khoản, Số tiền, Nội dung
   - Nội dung phải là: `BOOKING4` (hoặc `BOOKING{id}`)

2. **Kiểm tra nội dung trong app ngân hàng:**
   - Khi chuyển tiền, app ngân hàng có thể tự động điền nội dung
   - **Quan trọng:** Đảm bảo nội dung = `BOOKING4` (không có khoảng trắng)
   - Nếu app tự động điền sai → Sửa lại thành `BOOKING4`

3. **Chuyển tiền:**
   - Số tiền: Đúng với booking (5000 VND trong ví dụ)
   - Nội dung: `BOOKING4` (không có khoảng trắng)

### Bước 2: Kiểm Tra SePay Dashboard

**Vào SePay Dashboard:**
1. https://my.sepay.vn/webhooks
2. Click vào webhook "ResortDeluxe"
3. Kiểm tra **Thống kê:**
   - Hôm nay: X / Y (phải tăng sau khi chuyển tiền)
   - Tổng: X / Y (phải tăng)

**Nếu thống kê KHÔNG tăng:**
→ SePay chưa gửi webhook thật
→ Kiểm tra nội dung chuyển khoản có đúng format không

### Bước 3: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

**Sau khi chuyển tiền, tìm:**
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId
[WEBHOOK] ✅ Booking updated to Paid successfully!
```

**Nếu KHÔNG thấy logs:**
→ SePay chưa gửi webhook thật
→ Kiểm tra SePay dashboard → Thống kê có tăng không?

### Bước 4: Đợi Vài Phút

**SePay có thể mất thời gian để xử lý:**
- Có thể mất 1-5 phút (thậm chí lâu hơn)
- Đợi vài phút sau khi chuyển tiền
- Kiểm tra lại SePay dashboard và Railway logs

### Bước 5: Kiểm Tra Điều Kiện Webhook

**Trong SePay Dashboard, kiểm tra:**
- Có điều kiện số tiền tối thiểu không? (ví dụ: 1000 VND)
- Có pattern nội dung chuyển khoản không? (ví dụ: `BOOKING*`)
- Có điều kiện nào khác không?

**Nếu có điều kiện:**
→ Đảm bảo giao dịch thật khớp với điều kiện

## 🔍 Debug Steps

### Step 1: Kiểm Tra Nội Dung Chuyển Khoản

**Khi quét QR code:**
- QR code sẽ hiển thị: `https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4`
- Nội dung trong QR code: `BOOKING4`

**Khi chuyển tiền:**
- App ngân hàng có thể tự động điền nội dung
- **Quan trọng:** Kiểm tra và sửa nội dung thành `BOOKING4` (không có khoảng trắng)

### Step 2: Kiểm Tra SePay Dashboard

**Vào:** https://my.sepay.vn/webhooks

**Kiểm tra:**
- Webhook status = Kích hoạt?
- Thống kê có tăng không? (sau khi chuyển tiền)
- Webhook URL đúng không?

### Step 3: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

**Sau khi chuyển tiền (đợi 1-5 phút), tìm:**
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId
```

**Nếu KHÔNG thấy:**
→ SePay chưa gửi webhook thật

### Step 4: Test Với Giao Dịch Khác

**Thử thanh toán với:**
- Số tiền khác (ví dụ: 10000 VND thay vì 5000 VND)
- Nội dung chuyển khoản khác (ví dụ: `TEST123`)
- Xem SePay có gửi webhook không

## 📊 So Sánh: Test vs Thật

### Test Webhook (curl):
- ✅ Hoạt động ngay lập tức
- ✅ QR code ẩn và hiển thị "Thanh toán thành công"
- ✅ Backend update booking status = "Paid"

### Webhook Thật (MB Bank):
- ❌ Chưa hoạt động
- ❌ QR code không ẩn
- ❌ Backend chưa update booking status

**Nguyên nhân:**
- SePay chưa gửi webhook thật
- Nội dung chuyển khoản có thể không đúng format
- SePay cần thời gian để xử lý

## 🎯 Checklist

- [ ] Nội dung chuyển khoản = `BOOKING{id}` (không có khoảng trắng)?
- [ ] Đã đợi 1-5 phút sau khi chuyển tiền?
- [ ] SePay dashboard thống kê có tăng không?
- [ ] Railway logs có webhook received không?
- [ ] Điều kiện webhook có khớp không?

## 🔧 Giải Pháp Nhanh

### Giải Pháp 1: Kiểm Tra Nội Dung Chuyển Khoản

**Khi quét QR code và chuyển tiền:**
1. Quét QR code
2. **Kiểm tra nội dung chuyển khoản trong app ngân hàng**
3. **Sửa nội dung thành:** `BOOKING4` (không có khoảng trắng)
4. Chuyển tiền

### Giải Pháp 2: Đợi Vài Phút

**SePay có thể mất thời gian:**
1. Chuyển tiền
2. Đợi 1-5 phút
3. Kiểm tra SePay dashboard → Thống kê có tăng không?
4. Kiểm tra Railway logs → Có webhook received không?

### Giải Pháp 3: Kiểm Tra SePay Webhook Logs

**Trong SePay Dashboard:**
- Vào webhook "ResortDeluxe"
- Xem phần "Lịch sử" hoặc "Webhook Logs" (nếu có)
- Kiểm tra có webhook nào được gửi không?
- Response code là gì? (200 OK / 404 / 500?)

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

## 💡 Lưu Ý Quan Trọng

1. **Nội dung chuyển khoản:** Phải chính xác `BOOKING{id}` (không có khoảng trắng)
2. **Thời gian xử lý:** SePay có thể mất 1-5 phút (thậm chí lâu hơn) để gửi webhook
3. **App ngân hàng:** Có thể tự động điền nội dung sai → Cần kiểm tra và sửa lại
4. **Test vs Thật:** Test webhook hoạt động ngay, nhưng webhook thật có thể mất thời gian

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Kiểm tra SePay webhook logs** (nếu có trong dashboard)
2. **Liên hệ SePay support** để hỏi về webhook delay
3. **Kiểm tra nội dung chuyển khoản** có đúng format không
4. **Test với giao dịch khác** để xem có pattern nào không

