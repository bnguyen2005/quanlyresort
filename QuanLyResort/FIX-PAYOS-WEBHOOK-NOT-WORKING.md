# 🔧 Fix PayOs Webhook URL Không Hoạt Động

## ❌ Lỗi Hiện Tại

PayOs thông báo:
```
Webhook url của bạn hiện đang không hoạt động. mã lỗi: null
```

**Nguyên nhân có thể:**
1. Railway service chưa chạy hoặc đã dừng
2. PayOs không thể kết nối đến Railway URL
3. Endpoint không trả về đúng response khi PayOs verify
4. SSL/HTTPS issues

## ✅ Giải Pháp

### Bước 1: Kiểm Tra Service Đang Chạy

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Deployments"** → Kiểm tra có deployment "ACTIVE" không
3. **Tab "Logs"** → Kiểm tra service đã start chưa

✅ **Thành công:**
```
Application started
Now listening on: http://0.0.0.0:10000
```

❌ **Nếu service đã dừng:**
- Vào tab "Deployments" → Click "Redeploy"

### Bước 2: Test Webhook Endpoint

#### Test GET Request (PayOs Verification)

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
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

#### Test POST Request (Empty Body - Verification)

```bash
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

#### Test Webhook Status

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status
```

### Bước 3: Kiểm Tra Public Domain

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Settings"** → **"Networking"**
3. **Đảm bảo có public domain:**
   - `https://quanlyresort-production.up.railway.app`

**Nếu chưa có:**
- Click "Generate Domain" để tạo public domain

### Bước 4: Cập Nhật Webhook URL Trên PayOs

Sau khi đảm bảo endpoint hoạt động:

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Settings** → **Webhook**
3. **Cập nhật Webhook URL:**
   ```
   https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```
4. **Click "Save"**

**Lưu ý:**
- URL phải bắt đầu bằng `https://` (không phải `http://`)
- URL phải kết thúc bằng `/api/simplepayment/webhook`
- Không có khoảng trắng ở đầu/cuối

### Bước 5: Đợi PayOs Verify

Sau khi save, PayOs sẽ tự động:
1. Gửi GET request đến webhook URL để verify
2. Kiểm tra response có đúng format không
3. Nếu thành công, webhook URL sẽ được chấp nhận

**Thời gian:** Thường mất 10-30 giây

## 🔍 Kiểm Tra Sau Khi Fix

### 1. Xem Logs Trên Railway

Vào tab **"Logs"** và tìm:

✅ **PayOs đã verify:**
```
[WEBHOOK-VERIFY] PayOs verification request received
```

✅ **Webhook nhận được:**
```
[WEBHOOK] 📥 Webhook received
```

### 2. Kiểm Tra Trên PayOs Dashboard

1. **Vào PayOs Dashboard**
2. **Settings** → **Webhook**
3. **Kiểm tra trạng thái:**
   - ✅ **Active** = Webhook đã hoạt động
   - ❌ **Inactive** = Vẫn có vấn đề

## 🐛 Troubleshooting

### Lỗi: "Webhook url không hoạt động"

**Nguyên nhân 1: Service chưa chạy**
- **Giải pháp:** Redeploy service trên Railway

**Nguyên nhân 2: Endpoint không trả về đúng**
- **Giải pháp:** Test endpoint bằng curl (xem Bước 2)

**Nguyên nhân 3: PayOs không thể kết nối**
- **Giải pháp:** 
  - Kiểm tra Railway service có public domain không
  - Kiểm tra firewall/network issues

**Nguyên nhân 4: SSL/HTTPS issues**
- **Giải pháp:**
  - Đảm bảo URL bắt đầu bằng `https://`
  - Railway tự động cung cấp HTTPS

### Lỗi: "Connection timeout"

**Giải pháp:**
1. Kiểm tra Railway service đang chạy
2. Kiểm tra public domain đã được generate
3. Test endpoint bằng curl từ máy local

### Lỗi: "404 Not Found"

**Giải pháp:**
1. Kiểm tra URL đúng: `/api/simplepayment/webhook`
2. Kiểm tra service đã start và routing đúng
3. Xem logs để tìm lỗi routing

## 📋 Checklist

- [ ] Railway service đang chạy (ACTIVE)
- [ ] Public domain đã được generate
- [ ] Test GET request thành công
- [ ] Test POST request (empty body) thành công
- [ ] Webhook URL đã được cập nhật trên PayOs
- [ ] Đợi PayOs verify (10-30 giây)
- [ ] Kiểm tra trạng thái trên PayOs Dashboard

## 💡 Lưu Ý

- PayOs sẽ gửi GET request để verify webhook URL
- Endpoint phải trả về status 200 OK
- Response phải có format JSON hợp lệ
- Railway tự động cung cấp HTTPS, không cần cấu hình thêm

## 🔗 URLs Quan Trọng

- **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
- **PayOs Dashboard:** https://payos.vn

