# 🔧 Fix PayOs Webhook 404 Error Khi Cập Nhật Sang Railway

## ❌ Vấn Đề

Khi cập nhật PayOs webhook URL từ Render sang Railway:
- **Render URL (cũ):** `https://quanlyresort.onrender.com/api/simplepayment/webhook` ✅ Hoạt động
- **Railway URL (mới):** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ❌ PayOs báo 404

**Lỗi:**
```
Webhook url của bạn hiện đang không hoạt động. mã lỗi: Request failed with status code 404
```

## ✅ Giải Pháp

### ⚠️ Giải Pháp Tạm Thời: Giữ Cả 2 URL

Nếu PayOs vẫn báo 404 khi cập nhật sang Railway, có thể:
1. **Giữ Render URL tạm thời** để webhook vẫn hoạt động
2. **Đợi PayOs fix** hoặc liên hệ PayOs support
3. **Hoặc dùng cả 2 URL** (nếu có thể)

### Bước 1: Kiểm Tra Railway Service Đang Chạy

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Deployments"** → Kiểm tra có deployment "ACTIVE" không
3. **Tab "Logs"** → Kiểm tra service đã start chưa

✅ **Thành công:**
```
Application started
Now listening on: http://0.0.0.0:10000
```

❌ **Nếu service đã dừng:**
- Tab "Deployments" → Click "Redeploy"

### Bước 2: Test Webhook Endpoint

Test endpoint để đảm bảo hoạt động:

```bash
# Test GET request
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

# Test POST request (empty body - PayOs verification)
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d ''
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2025-11-13T..."
}
```

### Bước 3: Cập Nhật Webhook URL Qua API (Không Dùng Dashboard)

Vì PayOs Dashboard có thể báo lỗi 404, dùng API trực tiếp:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
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

**Nếu vẫn lỗi 404:**
- Đợi 5-10 phút và thử lại
- Kiểm tra Railway service đang chạy
- Kiểm tra endpoint có hoạt động không

### Bước 4: Đợi PayOs Verify

Sau khi cập nhật webhook URL qua API:
1. **Đợi 5-10 phút** để PayOs verify webhook URL
2. **Kiểm tra Railway Logs** để xem PayOs có gửi verification request không:
   ```
   [WEBHOOK-VERIFY] PayOs verification request received
   ```

### Bước 5: Kiểm Tra Trên PayOs Dashboard

Sau 10-15 phút:
1. **Vào PayOs Dashboard:** https://payos.vn
2. **Settings** → **Webhook**
3. **Kiểm tra webhook URL:**
   - Phải là: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Trạng thái: "Active" (không còn "không hoạt động")

## 🔍 Debug Steps

### 1. Kiểm Tra Railway Service

```bash
# Test endpoint
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

# Test webhook status
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status
```

### 2. Kiểm Tra Railway Logs

Vào Railway Dashboard → Logs và tìm:
- Service đã start chưa
- Có requests từ PayOs không
- Có lỗi gì không

### 3. So Sánh Render vs Railway

**Render URL (hoạt động):**
```
https://quanlyresort.onrender.com/api/simplepayment/webhook
```

**Railway URL (cần fix):**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Khác biệt:**
- Render: `onrender.com`
- Railway: `up.railway.app`

## 🐛 Troubleshooting

### Lỗi: PayOs Vẫn Báo 404 Sau Khi Cập Nhật

**Nguyên nhân 1: Railway service chưa chạy**
- **Giải pháp:** Redeploy service trên Railway

**Nguyên nhân 2: PayOs chưa verify được**
- **Giải pháp:** Đợi 10-15 phút và thử lại

**Nguyên nhân 3: Endpoint không trả về đúng response**
- **Giải pháp:** Test endpoint bằng curl (xem Bước 2)

**Nguyên nhân 4: PayOs đang cache URL cũ**
- **Giải pháp:** Đợi thêm 10-15 phút hoặc liên hệ PayOs support

**Nguyên nhân 5: PayOs đang verify bằng cách khác**
- **Giải pháp:** 
  - Kiểm tra Railway logs để xem PayOs có gửi request không
  - Có thể PayOs đang gọi endpoint khác hoặc với headers khác
  - Liên hệ PayOs support để hỏi về cách verify webhook URL

### Giải Pháp Tạm Thời: Giữ Render URL

Nếu PayOs vẫn báo 404, có thể:
1. **Giữ Render URL tạm thời:** `https://quanlyresort.onrender.com/api/simplepayment/webhook`
2. **Đảm bảo Render service vẫn chạy** (nếu có)
3. **Hoặc redirect từ Render sang Railway** (nếu có thể)
4. **Liên hệ PayOs support** để hỏi về vấn đề verify Railway URL

### Lỗi: API Trả Về Code 20 "Webhook url invalid"

**Giải pháp:**
1. Kiểm tra Railway service đang chạy
2. Test endpoint bằng curl
3. Đợi 10-15 phút và thử lại API

### Lỗi: Webhook URL Vẫn Là Render URL

**Giải pháp:**
1. Dùng API để cập nhật (không dùng Dashboard)
2. Đợi 10-15 phút
3. Kiểm tra lại trên Dashboard

## 📋 Checklist

- [ ] Railway service đang chạy (ACTIVE)
- [ ] Test endpoint thành công (GET và POST)
- [ ] Đã gọi PayOs API để cập nhật webhook URL
- [ ] Đã đợi 10-15 phút để PayOs verify
- [ ] Đã kiểm tra Railway logs (có verification request không)
- [ ] Đã kiểm tra PayOs Dashboard (URL đã đổi chưa, status là gì)

## 💡 Lưu Ý

- **PayOs có thể cần thời gian để verify:** 10-15 phút
- **Dùng API thay vì Dashboard:** Dashboard có thể báo lỗi nhưng API vẫn hoạt động
- **Kiểm tra Railway service:** Đảm bảo service đang chạy trước khi cập nhật webhook URL
- **Test endpoint trước:** Đảm bảo endpoint hoạt động trước khi cập nhật trên PayOs

## 🎯 Kết Quả Mong Đợi

Sau khi fix:
- ✅ PayOs webhook URL đã được cập nhật sang Railway
- ✅ PayOs đã verify webhook URL thành công
- ✅ Webhook URL status là "Active" trên PayOs Dashboard
- ✅ PayOs có thể gửi webhook đến Railway khi thanh toán thành công

## 🔗 URLs Quan Trọng

- **Railway Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Railway Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
- **PayOs API:** `https://api-merchant.payos.vn/confirm-webhook`
- **PayOs Dashboard:** https://payos.vn

