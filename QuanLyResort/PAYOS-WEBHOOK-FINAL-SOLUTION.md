# 🔧 Giải Pháp Cuối Cùng Cho PayOs Webhook

## ❌ Tình Hình Hiện Tại

1. **Railway URL:** PayOs báo 404 khi verify
2. **Render URL:** PayOs báo timeout (có thể Render service đã dừng)
3. **PayOs không gửi webhook** sau khi thanh toán

## ✅ Giải Pháp

### Giải Pháp 1: Restart Render Service Và Dùng Render URL

#### Bước 1: Restart Render Service

1. **Vào Render Dashboard:** https://dashboard.render.com
2. **Tìm service** `quanlyresort` hoặc tương tự
3. **Click "Restart"** hoặc **"Manual Deploy"**
4. **Đợi service start** (1-2 phút)

#### Bước 2: Test Render Endpoint

```bash
curl https://quanlyresort.onrender.com/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

#### Bước 3: Config Webhook URL Sang Render

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"}'
```

#### Bước 4: Cập Nhật Railway Variables

1. **Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Cập nhật:**
   ```env
   BankWebhook__PayOs__WebhookUrl=https://quanlyresort.onrender.com/api/simplepayment/webhook
   ```

### Giải Pháp 2: Update Booking Status Thủ Công

Nếu webhook không hoạt động, có thể update booking status thủ công:

#### Qua Swagger UI:

1. **Vào:** `https://quanlyresort-production.up.railway.app/swagger`
2. **Endpoint:** `PUT /api/bookings/{id}/status`
3. **Body:**
   ```json
   {
     "status": "Paid"
   }
   ```

#### Qua API:

```bash
curl -X PUT "https://quanlyresort-production.up.railway.app/api/bookings/4/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"status": "Paid"}'
```

### Giải Pháp 3: Đợi PayOs Fix Railway Domain

PayOs có thể cần thời gian để fix vấn đề với Railway domain:

1. **Đợi 24-48 giờ**
2. **Thử lại API call** với Railway URL
3. **Hoặc liên hệ PayOs support**

## 🔍 Kiểm Tra

### 1. Kiểm Tra Render Service

```bash
# Test Render endpoint
curl https://quanlyresort.onrender.com/api/simplepayment/webhook

# Test Render health
curl https://quanlyresort.onrender.com/api/health
```

### 2. Kiểm Tra Railway Service

```bash
# Test Railway endpoint
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

# Test Railway health
curl https://quanlyresort-production.up.railway.app/api/health
```

### 3. Kiểm Tra PayOs Dashboard

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Settings** → **Webhook**
3. **Kiểm tra:**
   - Webhook URL là gì?
   - Trạng thái: "Active" hay "Inactive"?

## 📋 Checklist

- [ ] Đã restart Render service (nếu dùng Render URL)
- [ ] Đã test Render endpoint hoạt động
- [ ] Đã config webhook URL qua PayOs API
- [ ] Đã cập nhật Railway Variables
- [ ] Đã redeploy Railway service
- [ ] Đã test thanh toán để verify webhook
- [ ] Đã update booking status thủ công (nếu cần)

## 💡 Khuyến Nghị

**Hiện tại:**
- Railway URL: PayOs báo 404
- Render URL: PayOs báo timeout (có thể service đã dừng)

**Giải pháp tốt nhất:**
1. **Restart Render service** nếu có
2. **Config webhook URL sang Render** (nếu Render hoạt động)
3. **Hoặc update booking status thủ công** để fix ngay
4. **Đợi PayOs fix Railway domain** hoặc liên hệ PayOs support

## 🎯 Kết Quả Mong Đợi

Sau khi fix:
- ✅ Render service đang chạy (nếu dùng Render URL)
- ✅ PayOs webhook URL đã được config
- ✅ PayOs gửi webhook sau khi thanh toán
- ✅ Booking status được update thành "Paid"
- ✅ QR code tự động ẩn

## 🔗 URLs Quan Trọng

- **Railway Webhook:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Render Webhook:** `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- **PayOs API:** `https://api-merchant.payos.vn/confirm-webhook`
- **PayOs Dashboard:** https://payos.vn
- **Swagger UI:** `https://quanlyresort-production.up.railway.app/swagger`

