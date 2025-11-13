# 🔧 Config PayOs Webhook Qua API (Railway)

## ❌ Vấn Đề

PayOs Dashboard báo lỗi 400 khi cập nhật webhook URL. Cần dùng API trực tiếp.

## ✅ Giải Pháp: Gọi PayOs API Trực Tiếp

### Bước 1: Chuẩn Bị Thông Tin

- **Client ID:** `c704495b-5984-4ad3-aa23-b2794a02aa83`
- **API Key:** `f6ea421b-a8b7-46b8-92be-209eb1a9b2fb`
- **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### Bước 2: Gọi PayOs API

Mở terminal và chạy:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

### Bước 3: Kiểm Tra Kết Quả

✅ **Thành công (HTTP 200):**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"
  }
}
```

❌ **Lỗi (HTTP 400):**
```json
{
  "code": 400,
  "desc": "Webhook URL không hợp lệ"
}
```

**Nguyên nhân có thể:**
- Webhook URL không thể truy cập được
- PayOs chưa verify được endpoint

**Giải pháp:**
1. Test webhook endpoint trước: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
2. Đảm bảo service đang chạy
3. Đảm bảo URL đúng format

❌ **Lỗi (HTTP 401):**
```json
{
  "code": 401,
  "desc": "Unauthorized"
}
```

**Nguyên nhân:**
- Client ID hoặc API Key không đúng

**Giải pháp:**
- Kiểm tra lại Client ID và API Key từ PayOs Dashboard

## 🔍 Sau Khi Config Thành Công

### 1. PayOs Sẽ Tự Động Verify

PayOs sẽ gửi GET request đến webhook URL để verify:
```
GET https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kiểm tra logs trên Railway:**
```
[WEBHOOK-VERIFY] PayOs verification request received
```

### 2. Test Webhook

Sau khi verify thành công, PayOs có thể gửi test webhook. Kiểm tra logs:
```
[WEBHOOK] 📥 Webhook received
```

## 📋 Checklist

- [ ] Đã test webhook endpoint hoạt động (GET request)
- [ ] Đã gọi PayOs API để config webhook
- [ ] Nhận được response code 200
- [ ] PayOs đã verify webhook URL
- [ ] Đã test tạo payment link
- [ ] Đã test thanh toán và nhận webhook

## 🐛 Troubleshooting

### Lỗi 400: "Webhook URL không hợp lệ"

**Kiểm tra:**
1. Webhook endpoint có hoạt động không:
   ```bash
   curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```

2. Service có đang chạy không:
   - Railway Dashboard → Deployments → Kiểm tra ACTIVE

3. URL format đúng chưa:
   - Phải bắt đầu bằng `https://`
   - Phải kết thúc bằng `/api/simplepayment/webhook`

### Lỗi 401: "Unauthorized"

**Giải pháp:**
- Kiểm tra lại Client ID và API Key
- Lấy từ PayOs Dashboard → Settings → API Keys

### PayOs Không Verify Được

**Nguyên nhân:**
- Endpoint không trả về đúng response
- Service chưa chạy

**Giải pháp:**
1. Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
2. Đảm bảo trả về: `{"status":"active","endpoint":"/api/simplepayment/webhook",...}`
3. Redeploy service nếu cần

## 💡 Lưu Ý

- **PayOs API endpoint:** `https://api-merchant.payos.vn/confirm-webhook` (không phải `api-app.payos.vn`)
- **Method:** POST
- **Headers:** Phải có `x-client-id` và `x-api-key`
- **Body:** JSON với field `webhookUrl`

## 🎯 Kết Quả

Sau khi config thành công:
- ✅ PayOs sẽ tự động gọi webhook khi thanh toán thành công
- ✅ Backend sẽ tự động update booking status
- ✅ Frontend sẽ tự động ẩn QR code và hiện success message

