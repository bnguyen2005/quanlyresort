# 🔧 Fix: SePay Chưa Gửi Webhook Thật

## 📋 Vấn Đề

**SePay chưa gửi webhook thật khi có thanh toán.**

**Triệu chứng:**
- ✅ Webhook đã được kích hoạt trong SePay dashboard
- ✅ Webhook URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- ❌ Thống kê: Hôm nay: 0 / 0, Tổng: 0 / 0
- ❌ Railway logs không có webhook từ SePay
- ❌ Booking status không tự động update

## 🔍 Nguyên Nhân Có Thể

### 1. Nội Dung Chuyển Khoản Không Đúng Format

**SePay chỉ gửi webhook khi:**
- Nội dung chuyển khoản khớp với format đã cấu hình
- Format thường là: `BOOKING{id}` hoặc pattern cụ thể

**Kiểm tra:**
- Nội dung chuyển khoản có đúng format không?
- Format có khớp với cấu hình trong SePay không?

### 2. Điều Kiện Webhook Không Khớp

**Kiểm tra cấu hình trong SePay Dashboard:**
- Tài khoản ngân hàng có đúng không? (`0901329227`)
- Số tiền có trong khoảng cho phép không?
- Loại giao dịch có đúng không? ("Có tiền vào")
- Có điều kiện nào khác không?

### 3. SePay Chưa Xử Lý Giao Dịch

**SePay có thể:**
- Cần thời gian để xử lý (vài phút đến vài giờ)
- Chỉ gửi webhook cho giao dịch hợp lệ
- Không gửi webhook cho giao dịch test hoặc số tiền quá nhỏ

### 4. Webhook URL Có Vấn Đề

**Kiểm tra:**
- Webhook URL có thể truy cập được không?
- Response code có phải 200 OK không?
- SePay có verify được URL không?

## 🎯 Giải Pháp

### Bước 1: Kiểm Tra Nội Dung Chuyển Khoản

**Khi thanh toán với SePay QR code:**
- Nội dung chuyển khoản phải là: `BOOKING{id}` (ví dụ: `BOOKING4`)
- Không có khoảng trắng: `BOOKING 4` ❌
- Không có ký tự đặc biệt: `BOOKING-4` (vẫn OK nhưng format khác)

**Test:**
1. Tạo booking mới (ví dụ: booking 4)
2. Click "Thanh toán" → QR code hiển thị
3. Quét QR code bằng app ngân hàng
4. **Quan trọng:** Khi chuyển khoản, nội dung phải là `BOOKING4` (không có khoảng trắng)
5. Số tiền: Đúng với booking

### Bước 2: Kiểm Tra SePay Webhook Cấu Hình

**Vào SePay Dashboard:**
1. https://my.sepay.vn/webhooks
2. Click vào webhook "ResortDeluxe"
3. Kiểm tra các cấu hình:

#### ✅ Cấu Hình 1: Webhook URL
```
URL: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```
- Không có dấu `/` ở cuối
- URL có thể truy cập được

#### ✅ Cấu Hình 2: Loại Sự Kiện
```
Loại sự kiện: Có tiền vào
```
- Phải chọn "Có tiền vào" hoặc "Cả hai"
- Không chọn "Có tiền ra"

#### ✅ Cấu Hình 3: Tài Khoản Ngân Hàng
```
Tài khoản: MBBank 0901329227
```
- Phải đúng tài khoản: `0901329227`
- Phải đúng ngân hàng: `MBBank` hoặc `MB`

#### ✅ Cấu Hình 4: Điều Kiện (Nếu Có)
- Số tiền tối thiểu: Có thể có (ví dụ: 1000 VND)
- Số tiền tối đa: Có thể có
- Nội dung chuyển khoản: Có thể có pattern (ví dụ: `BOOKING*`)

### Bước 3: Test Webhook URL

**Test xem webhook URL có hoạt động không:**

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

**Nếu không truy cập được:**
→ Kiểm tra Railway service đang chạy
→ Kiểm tra URL có đúng không

### Bước 4: Test Webhook Thủ Công

**Test xem backend có nhận được webhook SePay format không:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook/1.0" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN",
    "accountNumber": "0901329227",
    "bankCode": "MB"
  }'
```

**Sau đó kiểm tra Railway logs:**
- Phải thấy: `[WEBHOOK] 📋 Detected Simple/SePay format`
- Phải thấy: `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 4`

### Bước 5: Thanh Toán Và Kiểm Tra

1. **Tạo booking mới:**
   - Vào website → Đặt phòng
   - Tạo booking mới (ví dụ: booking 4)
   - Click "Thanh toán"

2. **Quét QR code và thanh toán:**
   - QR code sẽ hiển thị: `https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4`
   - Quét QR code bằng app ngân hàng
   - **Quan trọng:** Nội dung chuyển khoản phải là `BOOKING4` (không có khoảng trắng)
   - Số tiền: Đúng với booking

3. **Đợi 1-5 phút:**
   - SePay cần thời gian để xử lý và gửi webhook
   - Có thể mất đến 5 phút

4. **Kiểm tra SePay Dashboard:**
   - Vào: https://my.sepay.vn/webhooks
   - Click vào webhook "ResortDeluxe"
   - Kiểm tra **Thống kê:**
     - Hôm nay: X / Y (phải tăng)
     - Tổng: X / Y (phải tăng)

5. **Kiểm tra Railway Logs:**
   - Railway Dashboard → Service → Logs
   - Tìm: `[WEBHOOK] 📥 Webhook received`
   - Tìm: `[WEBHOOK] 📋 Detected Simple/SePay format`
   - Tìm: `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId`

6. **Kiểm tra Booking Status:**
   - Booking status phải = "Paid"
   - QR code phải tự động ẩn

## 🔍 Debug Checklist

### Checklist 1: Cấu Hình SePay Webhook

- [ ] Webhook status = Kích hoạt
- [ ] Webhook URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] Loại sự kiện = "Có tiền vào"
- [ ] Tài khoản ngân hàng = `0901329227`
- [ ] Ngân hàng = `MBBank` hoặc `MB`

### Checklist 2: Thanh Toán

- [ ] Đã tạo booking mới
- [ ] Đã click "Thanh toán"
- [ ] QR code đã hiển thị
- [ ] Đã quét QR code và chuyển khoản
- [ ] Nội dung chuyển khoản = `BOOKING{id}` (không có khoảng trắng)
- [ ] Số tiền đúng với booking

### Checklist 3: Kiểm Tra Sau Thanh Toán

- [ ] Đã đợi 1-5 phút
- [ ] SePay dashboard thống kê có tăng không?
- [ ] Railway logs có webhook SePay không?
- [ ] Booking status có = "Paid" không?
- [ ] QR code có tự động ẩn không?

## 🆘 Nếu Vẫn Không Hoạt Động

### 1. Kiểm Tra SePay Webhook Logs

**Trong SePay Dashboard:**
- Vào webhook "ResortDeluxe"
- Xem phần **"Lịch sử"** hoặc **"Webhook Logs"** (nếu có)
- Kiểm tra:
  - Có webhook nào được gửi không?
  - Response code là gì? (200 OK / 404 / 500?)
  - Có lỗi gì không?

### 2. Kiểm Tra Điều Kiện Webhook

**Trong SePay Dashboard:**
- Kiểm tra có điều kiện nào khác không?
- Ví dụ:
  - Số tiền tối thiểu: 1000 VND
  - Số tiền tối đa: 100000000 VND
  - Nội dung chuyển khoản: Pattern cụ thể

### 3. Liên Hệ SePay Support

**Nếu vẫn không hoạt động:**
1. **Liên hệ SePay support:**
   - Email: support@sepay.vn (hoặc email trong dashboard)
   - Hoặc chat support trong dashboard

2. **Cung cấp thông tin:**
   - Webhook ID: 17510
   - Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Tài khoản: `0901329227`
   - Mô tả vấn đề: Webhook đã kích hoạt nhưng không gửi khi có thanh toán

### 4. Test Với Giao Dịch Khác

**Thử thanh toán với:**
- Số tiền khác (ví dụ: 10000 VND thay vì 5000 VND)
- Nội dung chuyển khoản khác (ví dụ: `TEST123`)
- Xem SePay có gửi webhook không

## 📊 Format Webhook SePay Gửi

**Khi SePay gửi webhook, format sẽ là:**
```json
{
  "description": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "IN",
  "accountNumber": "0901329227",
  "bankCode": "MB",
  "id": "TXN123456",
  "referenceCode": "REF123456"
}
```

**Backend đã hỗ trợ:**
- ✅ Extract `description` → Booking ID (`BOOKING4` → `4`)
- ✅ Extract `transferAmount` → Amount
- ✅ Update booking status = "Paid"

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

## 💡 Lưu Ý Quan Trọng

1. **Nội dung chuyển khoản:** Phải chính xác `BOOKING{id}` (không có khoảng trắng)
2. **Thời gian xử lý:** SePay có thể mất 1-5 phút (thậm chí lâu hơn) để gửi webhook
3. **Điều kiện webhook:** Kiểm tra có điều kiện nào khác không (số tiền, pattern, etc.)
4. **Test:** Luôn test với booking thật và nội dung chuyển khoản đúng format
5. **Liên hệ support:** Nếu vẫn không hoạt động, liên hệ SePay support để được hỗ trợ

