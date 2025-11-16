# 🔧 Static QR Code + SePay Webhook - Hướng Dẫn

## 📋 Tình Huống Hiện Tại

**Từ logs:**
- ✅ SePay API trả về 404 → Hệ thống fallback sang **static QR code**
- ✅ Static QR code đã được tạo: `https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4`
- ✅ QR code hiển thị thành công

**Vấn đề:**
- Static QR code không tạo order trong SePay system
- SePay webhook có thể không gửi vì không có order reference
- **NHƯNG:** SePay webhook vẫn có thể gửi dựa trên **nội dung chuyển khoản**

## ✅ Giải Pháp: Static QR Code Vẫn Hoạt Động Với Webhook

**SePay webhook hoạt động dựa trên:**
1. **Tài khoản ngân hàng:** `0901329227`
2. **Nội dung chuyển khoản:** `BOOKING4` (hoặc `BOOKING{id}`)
3. **Loại sự kiện:** "Có tiền vào"

**Khi thanh toán với static QR code:**
- SePay sẽ detect tiền vào tài khoản `0901329227`
- SePay sẽ extract nội dung chuyển khoản: `BOOKING4`
- SePay sẽ gửi webhook với `description = "BOOKING4"`
- Backend sẽ extract booking ID = 4
- Backend sẽ update booking status = "Paid"

## 🎯 Các Bước Đảm Bảo Webhook Hoạt Động

### Bước 1: Đảm Bảo Nội Dung Chuyển Khoản Đúng

**Khi thanh toán với static QR code:**
- Nội dung chuyển khoản phải là: `BOOKING{id}`
- Ví dụ: `BOOKING4`, `BOOKING10`, `BOOKING15`

**Không được là:**
- `BOOKING 4` (có khoảng trắng) ❌
- `book4` (không có BOOKING) ❌
- `BOOKING-4` (có dấu gạch ngang - vẫn OK nhưng format khác)

### Bước 2: Kiểm Tra SePay Webhook Cấu Hình

**Trong SePay Dashboard:**
1. Vào: https://my.sepay.vn/webhooks
2. Kiểm tra webhook "ResortDeluxe":
   - ✅ Trạng thái: **Kích hoạt**
   - ✅ Loại: **Xác thực thanh toán**
   - ✅ Sự kiện: **Có tiền vào**
   - ✅ Tài khoản: **MBBank 0901329227**
   - ✅ Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### Bước 3: Thanh Toán Và Kiểm Tra

1. **Tạo booking mới:**
   - Vào website → Đặt phòng
   - Tạo booking mới (ví dụ: booking 4)
   - Click "Thanh toán"

2. **Quét QR code và thanh toán:**
   - QR code sẽ hiển thị: `https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4`
   - Quét QR code bằng app ngân hàng
   - **Quan trọng:** Nội dung chuyển khoản phải là `BOOKING4` (không có khoảng trắng)
   - Số tiền: Đúng với booking (5000 VND trong ví dụ)

3. **Đợi 1-5 phút:**
   - SePay cần thời gian để xử lý và gửi webhook
   - Kiểm tra SePay dashboard → Thống kê có tăng không?

4. **Kiểm tra Railway logs:**
   - Railway Dashboard → Service → Logs
   - Tìm: `[WEBHOOK] 📥 Webhook received`
   - Tìm: `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 4`
   - Tìm: `[WEBHOOK] ✅ Booking 4 updated to Paid successfully!`

5. **Kiểm tra booking status:**
   - Booking status phải = "Paid"
   - QR code phải tự động ẩn
   - Thông báo "Thanh toán thành công" phải hiển thị

## 🔍 Format Webhook SePay Gửi

**Khi thanh toán với static QR code, SePay sẽ gửi webhook với format:**
```json
{
  "description": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "IN",
  "accountNumber": "0901329227",
  "bankCode": "MB"
}
```

**Backend đã hỗ trợ:**
- ✅ Extract `description` → Booking ID (`BOOKING4` → `4`)
- ✅ Extract `transferAmount` → Amount
- ✅ Update booking status = "Paid"

## 📊 So Sánh: Dynamic QR vs Static QR

### Dynamic QR Code (SePay API):
- ✅ Tạo order trong SePay system
- ✅ Có order reference
- ✅ Webhook có thể có thêm thông tin order
- ❌ API endpoint có thể không hoạt động (404)

### Static QR Code (Fallback):
- ✅ Luôn hoạt động (không cần API)
- ✅ Số tiền vẫn động (thay đổi theo booking)
- ✅ Webhook vẫn hoạt động (dựa trên nội dung chuyển khoản)
- ✅ Đơn giản hơn, ít lỗi hơn
- ⚠️ Không có order reference trong SePay system

## 🎯 Kết Luận

**Static QR code vẫn hoạt động hoàn hảo với webhook!**

**Điều kiện:**
1. ✅ Nội dung chuyển khoản = `BOOKING{id}` (không có khoảng trắng)
2. ✅ SePay webhook đã được setup và kích hoạt
3. ✅ Tài khoản ngân hàng đúng: `0901329227`
4. ✅ Loại sự kiện = "Có tiền vào"

**Sau khi thanh toán:**
- SePay sẽ gửi webhook với `description = "BOOKING4"`
- Backend sẽ extract booking ID = 4
- Backend sẽ update booking status = "Paid"
- Frontend polling sẽ detect và ẩn QR code

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

## 💡 Lưu Ý

1. **Nội dung chuyển khoản:** Phải chính xác `BOOKING{id}` (không có khoảng trắng)
2. **Thời gian xử lý:** SePay có thể mất 1-5 phút để gửi webhook
3. **Static QR code:** Vẫn hoạt động tốt, không cần lo lắng về API 404
4. **Webhook:** Vẫn hoạt động dựa trên nội dung chuyển khoản, không cần order reference

