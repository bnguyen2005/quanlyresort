# 🔧 Fix PayOs Không Gửi Webhook Sau Khi Thanh Toán

## ❌ Vấn Đề

- ✅ Giao dịch đã xuất hiện trên PayOs Dashboard
- ✅ Đã thanh toán thành công (1 giao dịch "Đã thanh toán")
- ❌ PayOs chưa gửi webhook đến Railway
- ❌ Booking status chưa được update thành "Paid"
- ❌ QR code chưa ẩn

## 🔍 Kiểm Tra

### Bước 1: Kiểm Tra PayOs Webhook URL

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Settings** → **Webhook**
3. **Kiểm tra webhook URL:**
   - Phải là: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Hoặc: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
   - Trạng thái: "Active" hoặc "Inactive"

**Nếu webhook URL là Render URL:**
- Webhook sẽ được gửi đến Render, không phải Railway
- Cần cập nhật sang Railway URL

**Nếu webhook URL là Railway URL nhưng status "Inactive":**
- PayOs chưa verify được Railway URL
- Cần config lại webhook URL

### Bước 2: Kiểm Tra Railway Logs

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Logs"**
3. **Tìm sau khi thanh toán (13:11:09 - giao dịch đã thanh toán):**

**Nếu thấy:**
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: 4
✅ Booking 4 updated to Paid successfully!
```
→ Webhook đã hoạt động

**Nếu không thấy:**
→ PayOs chưa gửi webhook đến Railway

### Bước 3: Kiểm Tra PayOs Merchant

Kiểm tra xem đang dùng merchant nào:

**Merchant cũ:**
- Client ID: `c704495b-5984-4ad3-aa23-b2794a02aa83`

**Merchant mới:**
- Client ID: `90ad103f-aa49-4c33-9692-76d739a68b1b`

**Kiểm tra Railway Variables:**
- `BankWebhook__PayOs__ClientId` phải khớp với merchant đang dùng

## ✅ Giải Pháp

### Giải Pháp 1: Cập Nhật Webhook URL Trên PayOs

#### Nếu Dùng Merchant Cũ:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

#### Nếu Dùng Merchant Mới:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

**Nếu vẫn báo 404:**
- Dùng Render URL tạm thời: `https://quanlyresort.onrender.com/api/simplepayment/webhook`

### Giải Pháp 2: Update Booking Status Thủ Công

Nếu webhook không hoạt động, có thể update booking status thủ công:

1. **Vào Swagger UI:**
   ```
   https://quanlyresort-production.up.railway.app/swagger
   ```

2. **Tìm endpoint:** `PUT /api/bookings/{id}/status`
3. **Update status thành:** `"Paid"`

**Hoặc dùng curl:**
```bash
curl -X PUT "https://quanlyresort-production.up.railway.app/api/bookings/4/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"status": "Paid"}'
```

### Giải Pháp 3: Kiểm Tra Description Format

Khi thanh toán, đảm bảo nội dung chuyển khoản là:
- `BOOKING4` ✅
- `BOOKING-4` ✅
- `4` ✅
- `CSQRVZ1WKA2` ❌ (không phải booking ID)

**Từ PayOs Dashboard:**
- Description: `CSQRVZ1WKA2 BOOKING4` - Có thể extract được "BOOKING4"
- Description: `CSQRVZ1WKA2` - Không extract được booking ID

## 🔍 Debug Steps

### 1. Kiểm Tra PayOs Webhook Logs

1. **Vào PayOs Dashboard**
2. **Tìm webhook logs** hoặc **transaction details**
3. **Kiểm tra:**
   - Webhook có được gửi không
   - Webhook URL là gì
   - Response từ webhook là gì

### 2. Kiểm Tra Railway Logs

Sau khi thanh toán, kiểm tra Railway logs:

**Tìm:**
- Requests từ PayOs (IP hoặc User-Agent có "PayOs")
- Logs có chứa `[WEBHOOK]`
- Lỗi nếu có

### 3. Test Webhook Thủ Công

Test webhook với dữ liệu từ PayOs:

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "code": "00",
    "desc": "success",
    "data": {
      "orderCode": 45112,
      "amount": 5000,
      "description": "BOOKING4",
      "reference": "CSQRVZ1WKA2"
    }
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "bookingCode": "BKG2025004"
}
```

## 🐛 Troubleshooting

### Lỗi: PayOs Không Gửi Webhook

**Nguyên nhân:**
- Webhook URL chưa được config
- Webhook URL không active
- PayOs có vấn đề

**Giải pháp:**
1. Config lại webhook URL qua API
2. Đợi 10-15 phút để PayOs verify
3. Kiểm tra PayOs Dashboard

### Lỗi: Webhook Nhận Được Nhưng Không Extract Được Booking ID

**Nguyên nhân:**
- Description không đúng format
- Description là `CSQRVZ1WKA2` thay vì `BOOKING4`

**Giải pháp:**
- Kiểm tra logs để xem description nhận được là gì
- Update booking status thủ công nếu cần

### Lỗi: Booking Status Không Update

**Giải pháp:**
1. Kiểm tra logs để xem booking ID có được extract không
2. Kiểm tra booking có tồn tại không
3. Update booking status thủ công nếu cần

## 📋 Checklist

- [ ] Đã kiểm tra PayOs webhook URL
- [ ] Đã kiểm tra Railway logs (có nhận được webhook không)
- [ ] Đã config lại webhook URL (nếu cần)
- [ ] Đã kiểm tra description format
- [ ] Đã test webhook thủ công
- [ ] Đã update booking status thủ công (nếu cần)

## 💡 Khuyến Nghị

**Hiện tại:**
- Giao dịch đã thanh toán trên PayOs
- Webhook chưa được gửi đến Railway
- Booking status chưa được update

**Giải pháp:**
1. **Kiểm tra PayOs webhook URL** - Đảm bảo là Railway URL
2. **Config lại webhook URL** nếu cần
3. **Update booking status thủ công** để fix ngay
4. **Test lại thanh toán** để verify webhook hoạt động

## 🎯 Kết Quả Mong Đợi

Sau khi fix:
- ✅ PayOs gửi webhook đến Railway sau khi thanh toán
- ✅ Booking status được update thành "Paid"
- ✅ QR code tự động ẩn
- ✅ Frontend hiển thị "Thanh toán thành công"

## 🔗 URLs Quan Trọng

- **Railway Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Railway Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
- **PayOs Dashboard:** https://payos.vn
- **Swagger UI:** `https://quanlyresort-production.up.railway.app/swagger`

