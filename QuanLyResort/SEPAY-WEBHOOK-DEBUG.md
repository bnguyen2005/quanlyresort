# 🔍 Debug SePay Webhook - QR Code Không Ẩn Sau Khi Thanh Toán

## 📋 Vấn Đề

- ✅ Đã thanh toán thành công
- ✅ Đã setup webhook trong SePay (3 thuộc tính)
- ✅ Webhook URL đã được gọi: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- ❌ QR code không tự động ẩn
- ❌ Booking status không tự động cập nhật thành "Paid"

## 🔍 Các Bước Debug

### Bước 1: Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

Tìm các dòng sau để xác nhận webhook có được nhận không:

#### ✅ Nếu Webhook Được Nhận:

```
[WEBHOOK] 📥 [WEBHOOK-xxxxx] Webhook received at ...
[WEBHOOK]    Raw request JSON: {...}
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] 🔍 Using Description field (SePay format): 'BOOKING5'
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 5
[WEBHOOK] ✅ Booking 5 updated to Paid successfully!
```

#### ❌ Nếu Webhook KHÔNG Được Nhận:

**Không thấy logs** → SePay chưa gửi webhook đến Railway

**Nguyên nhân có thể:**
1. SePay webhook chưa được kích hoạt
2. Webhook URL sai
3. SePay chưa verify được Railway URL
4. Firewall/Network issue

### Bước 2: Kiểm Tra SePay Dashboard

1. **Vào SePay Dashboard:** https://my.sepay.vn/webhooks
2. **Kiểm tra webhook status:**
   - Status phải là **"Active"** hoặc **"Hoạt động"**
   - Webhook URL phải đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

3. **Xem webhook logs (nếu có):**
   - SePay Dashboard có thể có phần **"Webhook Logs"** hoặc **"Lịch sử"**
   - Kiểm tra xem có webhook nào được gửi không
   - Kiểm tra response code (phải là 200 OK)

### Bước 3: Test Webhook Thủ Công

**Test webhook với format SePay:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING5",
    "transferAmount": 5000,
    "transferType": "IN",
    "id": "TXN123456",
    "referenceCode": "REF123456"
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 5,
  "type": "booking"
}
```

### Bước 4: Kiểm Tra Format Webhook SePay Gửi

**SePay có thể gửi webhook với format khác nhau:**

#### Format 1: Simple Format (Đã hỗ trợ)
```json
{
  "description": "BOOKING5",
  "transferAmount": 5000,
  "transferType": "IN"
}
```

#### Format 2: Có thể có thêm fields
```json
{
  "description": "BOOKING5",
  "transferAmount": 5000,
  "amount": 5000,
  "content": "BOOKING5",
  "transferType": "IN",
  "id": "TXN123456",
  "referenceCode": "REF123456",
  "accountNumber": "0901329227",
  "bankCode": "MB"
}
```

**Backend đã hỗ trợ:**
- ✅ `description` → Extract booking ID
- ✅ `content` → Fallback cho description
- ✅ `transferAmount` → Extract amount
- ✅ `amount` → Fallback cho transferAmount

### Bước 5: Kiểm Tra Booking Status

**Kiểm tra xem booking có được update không:**

1. **Vào Railway Logs:**
   - Tìm: `[WEBHOOK] ✅ Booking 5 updated to Paid successfully!`

2. **Kiểm tra database:**
   - Booking status phải = "Paid"
   - Nếu vẫn là "Pending" → Webhook không update được

3. **Kiểm tra frontend polling:**
   - Frontend polling mỗi 2 giây
   - Nếu booking status = "Paid" → QR sẽ tự động ẩn

## 🎯 Các Trường Hợp Có Thể Xảy Ra

### Trường Hợp 1: Webhook Không Được Gửi

**Triệu chứng:**
- Không thấy logs webhook trong Railway
- SePay dashboard không có webhook logs

**Giải pháp:**
1. Kiểm tra SePay webhook status = Active
2. Test webhook thủ công (xem Bước 3)
3. Kiểm tra SePay có verify được Railway URL không

### Trường Hợp 2: Webhook Được Gửi Nhưng Format Sai

**Triệu chứng:**
- Có logs webhook received
- Nhưng không extract được booking ID

**Logs sẽ hiển thị:**
```
[WEBHOOK] ⚠️ ❌ FAILED: Could not extract bookingId from content: '...'
```

**Giải pháp:**
1. Kiểm tra format description trong webhook
2. Phải là `BOOKING{id}` (ví dụ: `BOOKING5`)
3. Không có khoảng trắng thừa

### Trường Hợp 3: Webhook Extract Được ID Nhưng Không Update Status

**Triệu chứng:**
- Có logs: `✅✅✅ SUCCESS: Extracted bookingId`
- Nhưng không có logs: `✅ Booking updated to Paid`

**Giải pháp:**
1. Kiểm tra booking có tồn tại không
2. Kiểm tra booking status hiện tại
3. Kiểm tra database connection

### Trường Hợp 4: Status Được Update Nhưng QR Không Ẩn

**Triệu chứng:**
- Có logs: `✅ Booking updated to Paid`
- Nhưng QR code vẫn hiển thị

**Giải pháp:**
1. Kiểm tra frontend polling có chạy không
2. Mở browser console (F12) → Xem logs polling
3. Kiểm tra booking status có = "Paid" không

## 🔧 Giải Pháp Nhanh

### 1. Kiểm Tra Webhook URL Trong SePay

**Đảm bảo URL đúng:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Không có:**
- Dấu `/` ở cuối
- Khoảng trắng
- Ký tự đặc biệt

### 2. Test Webhook Thủ Công

**Chạy lệnh sau để test:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING5",
    "transferAmount": 5000,
    "transferType": "IN"
  }'
```

**Sau đó kiểm tra Railway logs xem có nhận được không**

### 3. Kiểm Tra Format Nội Dung Chuyển Khoản

**Khi thanh toán, nội dung chuyển khoản phải là:**
```
BOOKING5
```

**Không được là:**
- `BOOKING 5` (có khoảng trắng)
- `BOOKING-5` (có dấu gạch ngang - vẫn OK nhưng format khác)
- `book5` (không có BOOKING)

### 4. Kiểm Tra SePay Webhook Events

**Trong SePay Dashboard, đảm bảo:**
- Events: Chọn **"Có tiền vào"** hoặc **"Cả hai"**
- Không chọn **"Có tiền ra"** (chỉ khi có tiền vào)

## 📊 Checklist Debug

- [ ] Railway logs có hiển thị webhook received không?
- [ ] SePay webhook status = Active?
- [ ] Webhook URL đúng không?
- [ ] Format description trong webhook = `BOOKING{id}`?
- [ ] Backend có extract được booking ID không?
- [ ] Booking status có được update = "Paid" không?
- [ ] Frontend polling có detect được status "Paid" không?
- [ ] Browser console có logs polling không?

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Gửi Railway logs** (từ khi thanh toán đến bây giờ)
2. **Gửi SePay webhook logs** (nếu có)
3. **Gửi browser console logs** (F12 → Console)
4. **Gửi format webhook** SePay gửi (nếu có thể xem được)

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

