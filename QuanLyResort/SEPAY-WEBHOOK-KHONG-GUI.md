# 🔧 Fix: SePay Webhook Đã Kích Hoạt Nhưng Không Gửi Webhook

## 📋 Vấn Đề

**Từ SePay Dashboard:**
- ✅ Trạng thái: **Kích hoạt**
- ✅ Loại: **Xác thực thanh toán**
- ✅ Sự kiện: **Có tiền vào**
- ✅ Tài khoản: **MBBank 0901329227**
- ❌ **Thống kê: Hôm nay: 0 / 0, Tổng: 0 / 0**

**Vấn đề:** Webhook đã kích hoạt nhưng khi nhận tiền thì không gửi webhook.

## 🔍 Nguyên Nhân Có Thể

### 1. Nội Dung Chuyển Khoản Không Đúng Format

**SePay chỉ gửi webhook khi:**
- Nội dung chuyển khoản khớp với format đã cấu hình
- Format thường là: `BOOKING{id}` hoặc pattern cụ thể

**Kiểm tra:**
- Nội dung chuyển khoản có đúng format không?
- Format có khớp với cấu hình trong SePay không?

### 2. Webhook URL Có Vấn Đề

**Kiểm tra trong SePay Dashboard:**
- Webhook URL có đúng không?
- URL có thể truy cập được không?
- Response code có phải 200 OK không?

### 3. SePay Chưa Xử Lý Giao Dịch

**SePay có thể:**
- Cần thời gian để xử lý (vài phút)
- Chỉ gửi webhook cho giao dịch hợp lệ
- Không gửi webhook cho giao dịch test

### 4. Điều Kiện Webhook Không Khớp

**Kiểm tra cấu hình:**
- Tài khoản ngân hàng có đúng không?
- Số tiền có trong khoảng cho phép không?
- Loại giao dịch có đúng không?

## 🎯 Giải Pháp

### Bước 1: Kiểm Tra Webhook URL

**Trong SePay Dashboard:**
1. Vào webhook: https://my.sepay.vn/webhooks
2. Click vào webhook "ResortDeluxe"
3. Kiểm tra **Webhook URL:**
   ```
   https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```
4. **Đảm bảo:**
   - URL đúng (không có dấu `/` ở cuối)
   - URL có thể truy cập được
   - Response code = 200 OK

**Test URL:**
```bash
curl -X GET https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

### Bước 2: Kiểm Tra Nội Dung Chuyển Khoản

**Khi thanh toán, nội dung chuyển khoản phải là:**
```
BOOKING{id}
```

**Ví dụ:**
- Booking ID = 4 → Nội dung: `BOOKING4`
- Booking ID = 10 → Nội dung: `BOOKING10`

**Không được là:**
- `BOOKING 4` (có khoảng trắng)
- `book4` (không có BOOKING)
- `BOOKING-4` (có dấu gạch ngang - vẫn OK nhưng format khác)

### Bước 3: Test Webhook Thủ Công

**Test xem webhook có hoạt động không:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook/1.0" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 150000,
    "transferType": "IN",
    "accountNumber": "0901329227",
    "bankCode": "MB"
  }'
```

**Sau đó kiểm tra:**
- Railway logs có nhận được webhook không?
- Booking status có được update không?

### Bước 4: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

**Sau khi thanh toán, tìm:**
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId
[WEBHOOK] ✅ Booking updated to Paid successfully!
```

**Nếu KHÔNG thấy logs:**
→ SePay chưa gửi webhook thật
→ Kiểm tra lại nội dung chuyển khoản và cấu hình

### Bước 5: Kiểm Tra SePay Webhook Logs

**Trong SePay Dashboard:**
1. Vào webhook: https://my.sepay.vn/webhooks
2. Click vào webhook "ResortDeluxe"
3. Xem phần **"Lịch sử"** hoặc **"Webhook Logs"** (nếu có)
4. Kiểm tra:
   - Có webhook nào được gửi không?
   - Response code là gì? (200 OK / 404 / 500?)
   - Có lỗi gì không?

### Bước 6: Kiểm Tra Điều Kiện Webhook

**Trong SePay Dashboard, kiểm tra:**
- **Tài khoản ngân hàng:** Có đúng `0901329227` không?
- **Loại sự kiện:** Có đúng "Có tiền vào" không?
- **Điều kiện:** Có điều kiện nào khác không? (số tiền tối thiểu, tối đa, etc.)

## 🔧 Các Trường Hợp Cụ Thể

### Trường Hợp 1: SePay Chỉ Verify URL

**Triệu chứng:**
- Webhook status = Kích hoạt
- Nhưng thống kê = 0 / 0
- Không có webhook logs

**Giải pháp:**
- SePay chỉ verify URL (gửi request rỗng)
- Webhook thật chỉ được gửi khi có giao dịch thật
- Đảm bảo nội dung chuyển khoản đúng format

### Trường Hợp 2: Nội Dung Chuyển Khoản Sai

**Triệu chứng:**
- Đã thanh toán
- Nhưng SePay không gửi webhook
- Thống kê vẫn = 0 / 0

**Giải pháp:**
- Kiểm tra nội dung chuyển khoản = `BOOKING{id}`
- Không có khoảng trắng thừa
- Format đúng với cấu hình

### Trường Hợp 3: Webhook URL Không Truy Cập Được

**Triệu chứng:**
- SePay không thể gửi webhook
- Response code = 404 / 500 / timeout

**Giải pháp:**
- Kiểm tra Railway service đang chạy
- Kiểm tra URL có đúng không
- Test URL thủ công

### Trường Hợp 4: SePay Cần Thời Gian Xử Lý

**Triệu chứng:**
- Đã thanh toán
- Nhưng webhook chưa được gửi ngay

**Giải pháp:**
- Đợi vài phút (SePay có thể mất 1-5 phút)
- Kiểm tra lại thống kê sau vài phút
- Kiểm tra Railway logs

## 📊 Checklist

- [ ] Webhook URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] Webhook URL có thể truy cập được (test thủ công)
- [ ] Nội dung chuyển khoản = `BOOKING{id}` (không có khoảng trắng)
- [ ] Tài khoản ngân hàng đúng: `0901329227`
- [ ] Loại sự kiện = "Có tiền vào"
- [ ] Railway logs có nhận được webhook không?
- [ ] SePay webhook logs có hiển thị gì không?

## 🎯 Test Thực Tế

### Test 1: Thanh Toán Với Booking Thật

1. **Tạo booking mới:**
   - Vào website → Đặt phòng
   - Tạo booking mới
   - Lưu booking ID (ví dụ: 11)

2. **Thanh toán:**
   - Click "Thanh toán"
   - Quét QR code
   - **Chuyển khoản với nội dung:** `BOOKING11` (không có khoảng trắng)
   - Số tiền: Đúng với booking

3. **Kiểm tra:**
   - Đợi 1-5 phút
   - Kiểm tra SePay dashboard → Thống kê có tăng không?
   - Kiểm tra Railway logs → Có webhook received không?
   - Kiểm tra booking status → Có = "Paid" không?

### Test 2: Test Webhook Thủ Công

**Chạy lệnh:**
```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 150000,
    "transferType": "IN"
  }'
```

**Kiểm tra Railway logs xem có nhận được không**

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

## 💡 Lưu Ý Quan Trọng

1. **Nội dung chuyển khoản:** Phải chính xác `BOOKING{id}` (không có khoảng trắng)
2. **Thời gian xử lý:** SePay có thể mất 1-5 phút để gửi webhook
3. **Thống kê:** Chỉ tăng khi có webhook thật được gửi (không phải verify URL)
4. **Test:** Luôn test với booking thật và nội dung chuyển khoản đúng

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Kiểm tra SePay webhook logs** (nếu có)
2. **Kiểm tra Railway logs** xem có lỗi gì không
3. **Test webhook thủ công** để xem endpoint có hoạt động không
4. **Liên hệ SePay support** nếu cần

