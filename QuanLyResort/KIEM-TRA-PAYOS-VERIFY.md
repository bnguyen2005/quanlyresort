# 🔍 Kiểm Tra PayOs Webhook URL Verification

## 📋 Cách Kiểm Tra

### Cách 1: Chạy Script Verify

```bash
cd QuanLyResort
./verify-payos-webhook.sh
```

Script sẽ:
1. Kiểm tra webhook endpoint hoạt động
2. Gọi PayOs API để verify webhook URL
3. Hiển thị kết quả chi tiết

### Cách 2: Kiểm Tra Trên PayOs Dashboard

1. **Vào PayOs Dashboard:**
   - https://payos.vn
   - Đăng nhập tài khoản

2. **Vào Settings → Webhook:**
   - Xem webhook URL hiện tại
   - Kiểm tra trạng thái verify

3. **Kiểm tra trạng thái:**
   - ✅ **"Active"** hoặc **"Đã xác thực"** = Đã verify thành công
   - ⚠️ **"Không hoạt động"** hoặc **"Chưa xác thực"** = Chưa verify
   - ❌ **"Lỗi"** hoặc **"Invalid"** = Verify thất bại

### Cách 3: Kiểm Tra Qua PayOs API

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

**Kết quả mong đợi:**

✅ **Thành công:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"
  }
}
```

❌ **Thất bại:**
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 404"
}
```

## 🔍 Kết Quả Kiểm Tra

### Nếu Chưa Verify

**Triệu chứng:**
- PayOs Dashboard hiển thị "Webhook url của bạn hiện đang không hoạt động"
- API trả về code "20" - "Webhook url invalid"
- PayOs không gửi webhook sau khi thanh toán

**Nguyên nhân có thể:**
1. PayOs không verify được Railway domain (vấn đề đã biết)
2. Webhook endpoint không hoạt động
3. PayOs firewall/network chặn Railway domain

**Giải pháp:**
1. **Đợi 10-15 phút** và thử lại
2. **Liên hệ PayOs support:** support@payos.vn
3. **Tạm thời dùng Render URL** nếu cần

### Nếu Đã Verify

**Triệu chứng:**
- PayOs Dashboard hiển thị "Active" hoặc "Đã xác thực"
- API trả về code 0 - "success"
- PayOs sẽ gửi webhook sau khi thanh toán thành công

**Kết quả:**
- ✅ Webhook URL đã được verify
- ✅ PayOs sẽ gửi webhook khi có thanh toán
- ✅ Booking status sẽ tự động update thành "Paid"

## 📊 Trạng Thái Hiện Tại

**Webhook URL:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Webhook Endpoint Status:**
- ✅ Hoạt động (HTTP 200 OK)
- ✅ Trả về `{"status":"active",...}`

**PayOs Verification:**
- ⚠️ Chưa verify được (Code 20 - Webhook url invalid)
- ⚠️ PayOs không verify được Railway domain

## 🔧 Các Bước Tiếp Theo

### Bước 1: Kiểm Tra Lại

Chạy script verify để kiểm tra lại:
```bash
./verify-payos-webhook.sh
```

### Bước 2: Thử Verify Lại Trên PayOs Dashboard

1. Vào PayOs Dashboard → Settings → Webhook
2. Xóa webhook URL cũ (nếu có)
3. Nhập lại: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
4. Click "Lưu" hoặc "Verify"
5. Đợi 10-15 phút

### Bước 3: Liên Hệ PayOs Support

Nếu vẫn không verify được:

**Email:** support@payos.vn

**Tiêu đề:** Vấn đề verify webhook URL với Railway domain

**Nội dung:**
```
Xin chào PayOs support,

Tôi đang gặp vấn đề khi verify webhook URL với Railway domain.

Thông tin:
- Webhook URL: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
- Client ID: 90ad103f-aa49-4c33-9692-76d739a68b1b
- Lỗi: Code 20 - Webhook url invalid
- Test endpoint: Đã test và trả về HTTP 200 OK với {"status":"active",...}

Yêu cầu: Hỗ trợ verify webhook URL với Railway domain.

Cảm ơn!
```

### Bước 4: Tạm Thời Dùng Render URL

Nếu cần gấp, có thể dùng Render URL tạm thời:
```
https://quanlyresort.onrender.com/api/simplepayment/webhook
```

**Lưu ý:** Render free plan có thể sleep, không ổn định bằng Railway.

## 📋 Checklist

- [ ] Đã chạy script verify webhook URL
- [ ] Đã kiểm tra PayOs Dashboard
- [ ] Đã thử verify lại trên PayOs Dashboard
- [ ] Đã liên hệ PayOs support (nếu cần)
- [ ] PayOs đã verify webhook URL thành công
- [ ] Đã test thanh toán và nhận webhook

## 💡 Lưu Ý

1. **PayOs có thể mất thời gian để verify:**
   - Có thể mất 10-15 phút
   - Hoặc vài giờ đến vài ngày

2. **Railway domain có thể có vấn đề:**
   - PayOs có thể không verify được Railway domain
   - Đây là vấn đề từ phía PayOs, không phải code

3. **Webhook vẫn có thể hoạt động:**
   - Mặc dù verify thất bại, PayOs vẫn có thể gửi webhook
   - Cần test với thanh toán thật để xác nhận

## 🔗 Links Quan Trọng

- **PayOs Dashboard:** https://payos.vn
- **PayOs Support:** support@payos.vn
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
- **Verify Script:** `./verify-payos-webhook.sh`

