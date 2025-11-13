# 🔧 Cập Nhật PayOs Cho Railway

## ✅ Thông Tin PayOs Đã Có

- **Client ID:** `c704495b-5984-4ad3-aa23-b2794a02aa83`
- **Api Key:** `f6ea421b-a8b7-46b8-92be-209eb1a9b2fb`
- **Checksum Key:** `429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313`
- **Webhook URL cũ:** `https://quanlyresort.onrender.com/api/simplepayment/webhook` ❌
- **Webhook URL mới:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ✅

## 📋 Bước 1: Cập Nhật Webhook URL Trên PayOs Dashboard

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Vào Settings** → **Webhook**
3. **Tìm Webhook URL hiện tại:** `https://quanlyresort.onrender.com/api/simplepayment/webhook`
4. **Cập nhật thành:**
   ```
   https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```
5. **Click "Save"** hoặc **"Update"**

## 📋 Bước 2: Kiểm Tra Environment Variables Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Kiểm tra và cập nhật các biến sau:**

### PayOs Configuration

```env
BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__VerifySignature=false
BankWebhook__PayOs__WebhookUrl=https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**⚠️ LƯU Ý:**
- **ChecksumKey** và **SecretKey** có thể giống nhau (như trong trường hợp này)
- **WebhookUrl** phải là Railway URL, không phải Render URL
- **Không có khoảng trắng** ở đầu/cuối giá trị

## 📋 Bước 3: Redeploy

1. **Save** tất cả environment variables
2. **Vào tab "Deployments"**
3. **Click "Redeploy"**
4. **Chọn "Deploy"**

## 🔍 Kiểm Tra Sau Khi Cập Nhật

### 1. Test Tạo Payment Link

Thử tạo payment link từ frontend và kiểm tra logs:

✅ **Thành công:**
```
[PAYOS] ✅ Payment link created successfully
[PAYOS] Payment URL: https://pay.payos.vn/web/...
```

❌ **Vẫn lỗi signature:**
- Kiểm tra lại ChecksumKey đã copy đúng chưa
- Đảm bảo không có khoảng trắng

### 2. Test Webhook

Sau khi thanh toán thành công, PayOs sẽ gửi webhook đến Railway URL. Kiểm tra logs:

✅ **Thành công:**
```
[PAYOS-WEBHOOK] Processing PayOs webhook
[PAYOS-WEBHOOK] ✅ PayOs webhook processed successfully
```

## 📋 Checklist

- [ ] Đã cập nhật Webhook URL trên PayOs Dashboard
- [ ] Đã kiểm tra `BankWebhook__PayOs__ClientId` trên Railway
- [ ] Đã kiểm tra `BankWebhook__PayOs__ApiKey` trên Railway
- [ ] Đã kiểm tra `BankWebhook__PayOs__ChecksumKey` trên Railway
- [ ] Đã cập nhật `BankWebhook__PayOs__WebhookUrl` trên Railway
- [ ] Đã redeploy sau khi cập nhật
- [ ] Đã test tạo payment link
- [ ] Đã test webhook (sau khi thanh toán)

## 🐛 Troubleshooting

### Lỗi: "Mã kiểm tra(signature) không hợp lệ"

**Nguyên nhân:**
- ChecksumKey không đúng
- Có khoảng trắng trong ChecksumKey

**Giải pháp:**
1. Copy lại ChecksumKey từ PayOs Dashboard
2. Xóa và tạo lại biến `BankWebhook__PayOs__ChecksumKey`
3. Đảm bảo không có khoảng trắng
4. Redeploy

### Lỗi: "Webhook không nhận được"

**Nguyên nhân:**
- Webhook URL chưa được cập nhật trên PayOs
- Railway service chưa expose public domain

**Giải pháp:**
1. Kiểm tra Webhook URL trên PayOs Dashboard
2. Đảm bảo Railway service đã có public domain
3. Test webhook URL: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`

## ✅ Sau Khi Hoàn Thành

1. ✅ PayOs Webhook URL đã được cập nhật sang Railway
2. ✅ Environment variables đã được cấu hình đúng
3. ✅ Payment link có thể tạo thành công
4. ✅ Webhook có thể nhận được từ PayOs

## 🔗 URLs Quan Trọng

- **Railway URL:** `https://quanlyresort-production.up.railway.app`
- **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status`
- **PayOs Dashboard:** https://payos.vn

