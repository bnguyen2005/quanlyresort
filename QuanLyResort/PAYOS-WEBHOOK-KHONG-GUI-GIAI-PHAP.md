# 🔧 Giải Pháp PayOs Chưa Gửi Webhook Đến Railway

## ❌ Vấn Đề

PayOs chưa gửi webhook đến Railway sau khi thanh toán thành công.

## 🔍 Nguyên Nhân Có Thể

1. **Webhook URL chưa được config** trên PayOs
2. **Webhook URL không active** trên PayOs Dashboard
3. **Webhook URL là Render URL** thay vì Railway URL
4. **PayOs có vấn đề** khi verify Railway URL

## ✅ Giải Pháp

### Bước 1: Kiểm Tra PayOs Webhook URL

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Settings** → **Webhook**
3. **Kiểm tra:**
   - Webhook URL hiện tại là gì?
   - Trạng thái: "Active" hay "Inactive"?

**Nếu webhook URL là:**
- `https://quanlyresort.onrender.com/api/simplepayment/webhook` → Cần cập nhật sang Railway
- `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` → Đã đúng, nhưng có thể chưa active

### Bước 2: Config Webhook URL Qua API

#### Nếu Dùng Merchant Cũ (Client ID: c704495b...):

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

#### Nếu Dùng Merchant Mới (Client ID: 90ad103f...):

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"
  }
}
```

**Nếu vẫn báo 404:**
- PayOs có vấn đề với Railway domain
- Có thể dùng Render URL tạm thời

### Bước 3: Dùng Render URL Tạm Thời (Nếu Railway Không Hoạt Động)

Nếu PayOs vẫn báo 404 với Railway URL:

```bash
# Merchant cũ
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"}'
```

**Sau đó cập nhật Railway Variables:**
```env
BankWebhook__PayOs__WebhookUrl=https://quanlyresort.onrender.com/api/simplepayment/webhook
```

### Bước 4: Kiểm Tra Railway Logs

Sau khi config webhook URL:

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Logs"**
3. **Tìm sau khi thanh toán:**

**Nếu thấy:**
```
[WEBHOOK-VERIFY] PayOs verification request received
```
→ PayOs đã verify webhook URL thành công

**Nếu thấy:**
```
[WEBHOOK] 📥 Webhook received
✅✅✅ SUCCESS: Extracted bookingId from description: 4
✅ Booking 4 updated to Paid successfully!
```
→ Webhook đã hoạt động và xử lý thanh toán thành công

**Nếu không thấy:**
→ PayOs chưa gửi webhook, cần kiểm tra lại

### Bước 5: Đợi PayOs Verify

Sau khi config webhook URL qua API:
1. **Đợi 10-15 phút** để PayOs verify webhook URL
2. **Kiểm tra PayOs Dashboard** xem webhook URL đã active chưa
3. **Test lại thanh toán** để verify webhook hoạt động

## 🐛 Troubleshooting

### Lỗi: PayOs Vẫn Báo 404

**Giải pháp:**
1. Dùng Render URL tạm thời
2. Đợi PayOs fix (có thể mất vài giờ đến vài ngày)
3. Liên hệ PayOs support

### Lỗi: Webhook URL Không Active

**Giải pháp:**
1. Config lại webhook URL qua API
2. Đợi 10-15 phút
3. Kiểm tra PayOs Dashboard

### Lỗi: PayOs Không Gửi Webhook Sau Khi Thanh Toán

**Kiểm tra:**
1. Webhook URL đã được config chưa
2. Webhook URL status là "Active" chưa
3. Railway logs có nhận được webhook không

**Giải pháp:**
1. Config lại webhook URL
2. Đợi PayOs verify
3. Test lại thanh toán

## 📋 Checklist

- [ ] Đã kiểm tra PayOs webhook URL trên Dashboard
- [ ] Đã config webhook URL qua API (Railway hoặc Render)
- [ ] Đã đợi 10-15 phút để PayOs verify
- [ ] Đã kiểm tra Railway logs (có nhận được webhook không)
- [ ] Đã test lại thanh toán để verify webhook hoạt động

## 💡 Khuyến Nghị

**Hiện tại:**
- PayOs chưa gửi webhook đến Railway
- Có thể do webhook URL chưa được config hoặc không active

**Giải pháp:**
1. **Config webhook URL qua API** (Railway hoặc Render)
2. **Đợi 10-15 phút** để PayOs verify
3. **Kiểm tra Railway logs** để xem có nhận được webhook không
4. **Test lại thanh toán** để verify

## 🎯 Kết Quả Mong Đợi

Sau khi fix:
- ✅ PayOs webhook URL đã được config
- ✅ PayOs đã verify webhook URL thành công
- ✅ PayOs gửi webhook đến Railway sau khi thanh toán
- ✅ Booking status được update thành "Paid"
- ✅ QR code tự động ẩn

## 🔗 URLs Quan Trọng

- **Railway Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Render Webhook URL:** `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- **PayOs API:** `https://api-merchant.payos.vn/confirm-webhook`
- **PayOs Dashboard:** https://payos.vn

