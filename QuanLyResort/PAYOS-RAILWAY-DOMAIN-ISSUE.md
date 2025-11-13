# ⚠️ PayOs Có Vấn Đề Với Railway Domain

## ❌ Vấn Đề

PayOs không thể verify webhook URL với Railway domain (`up.railway.app`):
- ✅ Railway endpoint hoạt động tốt (đã test GET và POST)
- ❌ PayOs API báo 404 khi verify Railway URL
- ❌ PayOs không gửi webhook đến Railway sau khi thanh toán

## 🔍 Phân Tích

### Railway Endpoint Hoạt Động ✅

Endpoint Railway đã được test và hoạt động tốt:
```bash
# GET request
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
# Response: {"status":"active","endpoint":"/api/simplepayment/webhook",...}

# POST request (empty body)
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d ''
# Response: {"status":"active","endpoint":"/api/simplepayment/webhook",...}
```

### PayOs Vẫn Báo 404 ❌

Khi config webhook URL qua PayOs API:
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 404"
}
```

**Nguyên nhân có thể:**
1. PayOs có firewall/network issues với Railway domain
2. PayOs đang verify bằng cách khác (không phải GET/POST thông thường)
3. PayOs có vấn đề với subdomain `up.railway.app`
4. PayOs đang cache kết quả verify cũ

## ✅ Giải Pháp

### Giải Pháp 1: Dùng Render URL Tạm Thời (Khuyến Nghị)

Vì PayOs có vấn đề với Railway domain, dùng Render URL tạm thời:

#### Bước 1: Config Webhook URL Sang Render

**Với Merchant Mới:**
```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"}'
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"
  }
}
```

**⚠️ Nếu báo timeout:**
- Render service có thể đã dừng
- Cần restart Render service trước
- Hoặc dùng giải pháp khác

#### Bước 2: Cập Nhật Railway Variables

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Cập nhật:**
   ```env
   BankWebhook__PayOs__WebhookUrl=https://quanlyresort.onrender.com/api/simplepayment/webhook
   ```

#### Bước 3: Redeploy Railway Service

1. **Save** environment variables
2. **Tab "Deployments"** → **"Redeploy"**

#### Bước 4: Đảm Bảo Render Service Chạy

Nếu Render service đã dừng:
1. **Vào Render Dashboard**
2. **Restart service** nếu cần
3. **Đảm bảo service đang chạy**

### Giải Pháp 2: Đợi PayOs Fix

PayOs có thể cần thời gian để fix vấn đề với Railway domain:

1. **Đợi 24-48 giờ**
2. **Thử lại API call** với Railway URL
3. **Kiểm tra PayOs Dashboard** xem có update không

### Giải Pháp 3: Liên Hệ PayOs Support

Nếu vẫn lỗi sau 48 giờ:

1. **Liên hệ PayOs support**
2. **Cung cấp thông tin:**
   - Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Lỗi: "Request failed with status code 404"
   - Test result: Endpoint hoạt động khi test bằng curl
   - Client ID: `90ad103f-aa49-4c33-9692-76d739a68b1b`

3. **Hỏi về:**
   - Có vấn đề gì với Railway domain không
   - Có thể dùng Railway URL không
   - Cách PayOs verify webhook URL

## 🔄 Workaround: Redirect Từ Render Sang Railway

Nếu muốn dùng Railway nhưng PayOs chỉ chấp nhận Render URL:

1. **Config webhook URL là Render URL** trên PayOs
2. **Render service nhận webhook** và forward đến Railway
3. **Railway xử lý webhook**

**Lưu ý:** Cần có Render service đang chạy để forward webhook.

## 📋 Checklist

- [ ] Đã thử config Railway URL - ❌ Vẫn báo 404
- [ ] Đã test Railway endpoint - ✅ Hoạt động
- [ ] Đã config Render URL - Cần làm
- [ ] Đã cập nhật Railway Variables với Render URL
- [ ] Đã redeploy Railway service
- [ ] Đã đảm bảo Render service chạy
- [ ] Đã test thanh toán để verify webhook hoạt động

## 💡 Khuyến Nghị

**Hiện tại:**
- ✅ Railway endpoint hoạt động tốt
- ❌ PayOs có vấn đề với Railway domain
- ✅ Render URL hoạt động với PayOs

**Giải pháp tốt nhất:**
1. **Dùng Render URL tạm thời** để webhook hoạt động ngay
2. **Đợi PayOs fix** hoặc liên hệ PayOs support
3. **Khi PayOs fix xong**, cập nhật lại sang Railway URL

## 🎯 Kết Quả Mong Đợi

Sau khi dùng Render URL:
- ✅ PayOs webhook URL đã được config thành công
- ✅ PayOs đã verify webhook URL thành công
- ✅ PayOs gửi webhook đến Render sau khi thanh toán
- ✅ Render forward webhook đến Railway (nếu có)
- ✅ Booking status được update thành "Paid"
- ✅ QR code tự động ẩn

## 🔗 URLs Quan Trọng

- **Railway Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` (PayOs không verify được)
- **Render Webhook URL:** `https://quanlyresort.onrender.com/api/simplepayment/webhook` (PayOs verify được)
- **PayOs API:** `https://api-merchant.payos.vn/confirm-webhook`
- **PayOs Dashboard:** https://payos.vn

## 📝 Lưu Ý

- **Railway domain hoạt động tốt** - Vấn đề là ở PayOs
- **Render URL là giải pháp tạm thời** - Webhook sẽ hoạt động ngay
- **Có thể chuyển lại Railway URL** khi PayOs fix xong
- **Cần đảm bảo Render service chạy** nếu dùng Render URL

