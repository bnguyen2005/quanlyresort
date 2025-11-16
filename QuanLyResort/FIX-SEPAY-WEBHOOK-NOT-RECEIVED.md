# 🔧 Fix: SePay Webhook Không Được Nhận - Chỉ Có PayOs

## 📋 Vấn Đề

**Từ logs:**
- ✅ Có webhook received từ **PayOs**
- ❌ **Không có webhook từ SePay**

**Nguyên nhân có thể:**
1. SePay chưa gửi webhook thật (chỉ PayOs gửi)
2. SePay webhook format khác với PayOs
3. Backend đang ưu tiên PayOs format trước

## 🔍 Kiểm Tra

### Bước 1: Kiểm Tra SePay Dashboard

**Vào SePay Dashboard:**
1. https://my.sepay.vn/webhooks
2. Kiểm tra webhook "ResortDeluxe":
   - ✅ Trạng thái: **Kích hoạt**
   - ✅ Thống kê: Có tăng không? (Hôm nay: X / Y)
   - ✅ Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

**Nếu thống kê = 0 / 0:**
→ SePay chưa gửi webhook thật
→ Kiểm tra nội dung chuyển khoản có đúng format không

### Bước 2: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] 🔍 Attempting to deserialize as PayOs format...
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] 🔍 Using Description field (SePay format)
```

**Nếu chỉ thấy PayOs format:**
→ SePay webhook chưa được gửi hoặc format không đúng

### Bước 3: Kiểm Tra Nội Dung Chuyển Khoản

**Khi thanh toán với SePay QR code:**
- Nội dung chuyển khoản phải là: `BOOKING{id}` (ví dụ: `BOOKING4`)
- Không có khoảng trắng: `BOOKING 4` ❌

**Nếu nội dung sai:**
→ SePay không gửi webhook
→ Hoặc webhook không extract được booking ID

## 🎯 Giải Pháp

### Giải Pháp 1: Tắt PayOs Webhook (Nếu Đã Chuyển Sang SePay)

**Nếu bạn đã chuyển sang SePay hoàn toàn:**

1. **Vào PayOs Dashboard:**
   - https://payos.vn
   - Vào Settings → Webhook
   - Xóa hoặc tắt webhook URL

2. **Hoặc cập nhật PayOs webhook URL thành URL khác:**
   - Để tránh nhận webhook từ PayOs

### Giải Pháp 2: Đảm Bảo SePay Webhook Được Gửi

**Kiểm tra các điều kiện:**

1. **Nội dung chuyển khoản:**
   - Phải là: `BOOKING{id}` (ví dụ: `BOOKING4`)
   - Không có khoảng trắng
   - Không có ký tự đặc biệt

2. **Tài khoản ngân hàng:**
   - Phải đúng: `0901329227`
   - Phải khớp với cấu hình trong SePay webhook

3. **Loại sự kiện:**
   - Phải là: "Có tiền vào"
   - Không phải "Có tiền ra"

4. **Thời gian:**
   - SePay có thể mất 1-5 phút để gửi webhook
   - Đợi vài phút sau khi thanh toán

### Giải Pháp 3: Test SePay Webhook Thủ Công

**Test xem SePay webhook có hoạt động không:**

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

### Giải Pháp 4: Kiểm Tra Format Webhook

**SePay webhook format:**
```json
{
  "description": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "IN",
  "accountNumber": "0901329227",
  "bankCode": "MB"
}
```

**PayOs webhook format:**
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 123,
    "amount": 3000,
    "description": "BOOKING4"
  },
  "signature": "..."
}
```

**Backend đã hỗ trợ cả 2 format:**
- ✅ PayOs format → Extract từ `data.description`
- ✅ SePay format → Extract từ `description` hoặc `content`

## 🔍 Debug Steps

### Step 1: Kiểm Tra SePay Dashboard

**Vào:** https://my.sepay.vn/webhooks

**Kiểm tra:**
- Webhook status = Kích hoạt?
- Thống kê có tăng không? (sau khi thanh toán)
- Webhook URL đúng không?

### Step 2: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

**Sau khi thanh toán với SePay, tìm:**
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] 🔍 Attempting to deserialize as PayOs format...
[WEBHOOK] ⚠️ PayOs format check failed
[WEBHOOK] 🔍 PayOs format not detected, trying Simple format...
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] 🔍 Using Description field (SePay format): 'BOOKING4'
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 4
```

**Nếu KHÔNG thấy logs SePay:**
→ SePay chưa gửi webhook thật

### Step 3: Test Với Booking Thật

1. **Tạo booking mới:**
   - Vào website → Đặt phòng
   - Tạo booking mới (ví dụ: booking 4)

2. **Thanh toán với SePay:**
   - Click "Thanh toán"
   - Quét QR code SePay
   - **Chuyển khoản với nội dung:** `BOOKING4` (không có khoảng trắng)
   - Số tiền: Đúng với booking

3. **Đợi 1-5 phút:**
   - SePay cần thời gian để xử lý

4. **Kiểm tra:**
   - SePay dashboard → Thống kê có tăng không?
   - Railway logs → Có webhook SePay không?
   - Booking status → Có = "Paid" không?

## 📊 So Sánh PayOs vs SePay Webhook

### PayOs Webhook:
- ✅ Đang hoạt động (có webhook received)
- ✅ Format: `{ "code": "00", "data": { "description": "BOOKING4" } }`
- ✅ Backend đã hỗ trợ

### SePay Webhook:
- ❌ Chưa được nhận
- ✅ Format: `{ "description": "BOOKING4", "transferAmount": 5000 }`
- ✅ Backend đã hỗ trợ

## 🎯 Checklist

- [ ] SePay webhook status = Kích hoạt?
- [ ] SePay webhook URL đúng?
- [ ] Nội dung chuyển khoản = `BOOKING{id}` (không có khoảng trắng)?
- [ ] Đã đợi 1-5 phút sau khi thanh toán?
- [ ] SePay dashboard thống kê có tăng không?
- [ ] Railway logs có webhook SePay không?
- [ ] PayOs webhook có cần tắt không?

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **PayOs Dashboard:** https://payos.vn
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

## 💡 Lưu Ý

1. **Backend hỗ trợ cả PayOs và SePay:** Có thể nhận cả 2 loại webhook
2. **Nếu chỉ dùng SePay:** Có thể tắt PayOs webhook để tránh nhầm lẫn
3. **Nội dung chuyển khoản:** Phải chính xác `BOOKING{id}` cho cả PayOs và SePay
4. **Thời gian xử lý:** SePay có thể mất 1-5 phút để gửi webhook

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Kiểm tra SePay webhook logs** (nếu có trong dashboard)
2. **Test webhook thủ công** với format SePay
3. **Kiểm tra nội dung chuyển khoản** có đúng format không
4. **Liên hệ SePay support** nếu cần

