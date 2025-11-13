# 🔧 Tạo PayOs Merchant Mới

## ✅ Có Thể Tạo PayOs Merchant Mới

Bạn có thể tạo một PayOs merchant account mới để:
- Tránh vấn đề với webhook URL cũ
- Có webhook URL mới từ đầu với Railway
- Test lại từ đầu

## 📋 Các Bước Tạo PayOs Merchant Mới

### Bước 1: Đăng Ký PayOs Merchant Mới

1. **Vào PayOs Website:** https://payos.vn
2. **Click "Đăng ký"** hoặc **"Tạo tài khoản"**
3. **Điền thông tin:**
   - Email (dùng email khác nếu có)
   - Số điện thoại
   - Tên doanh nghiệp
   - Thông tin liên hệ

4. **Xác thực tài khoản** (theo hướng dẫn của PayOs)

### Bước 2: Lấy Thông Tin API

Sau khi đăng ký thành công:

1. **Vào PayOs Dashboard**
2. **Settings** → **API Keys**
3. **Copy các thông tin:**
   - **Client ID**
   - **API Key**
   - **Checksum Key**

### Bước 3: Cập Nhật Environment Variables Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Variables"**
3. **Cập nhật các biến sau với thông tin mới:**

```env
BankWebhook__PayOs__ClientId=YOUR_NEW_CLIENT_ID
BankWebhook__PayOs__ApiKey=YOUR_NEW_API_KEY
BankWebhook__PayOs__ChecksumKey=YOUR_NEW_CHECKSUM_KEY
BankWebhook__PayOs__SecretKey=YOUR_NEW_CHECKSUM_KEY
BankWebhook__PayOs__VerifySignature=false
BankWebhook__PayOs__WebhookUrl=https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

### Bước 4: Config Webhook URL Mới

Sau khi có thông tin API mới:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: YOUR_NEW_CLIENT_ID" \
  -H "x-api-key: YOUR_NEW_API_KEY" \
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

### Bước 5: Redeploy Railway Service

1. **Save** tất cả environment variables
2. **Tab "Deployments"** → **"Redeploy"**
3. **Chọn "Deploy"**

### Bước 6: Test Payment

1. **Tạo booking mới**
2. **Click "Thanh toán"**
3. **Tạo payment link** (sẽ dùng PayOs merchant mới)
4. **Test thanh toán**

## ⚠️ Lưu Ý

### 1. Merchant Mới = Tài Khoản Mới

- Phải đăng ký merchant mới hoàn toàn
- Không thể dùng lại thông tin merchant cũ
- Cần xác thực tài khoản mới

### 2. Thông Tin Cần Cung Cấp

PayOs có thể yêu cầu:
- Giấy phép kinh doanh
- Thông tin ngân hàng
- Xác thực danh tính

### 3. Thời Gian Xử Lý

- Đăng ký: 1-2 ngày
- Xác thực: 1-3 ngày
- Tổng cộng: 2-5 ngày

## 🔄 So Sánh: Merchant Cũ vs Mới

### Merchant Cũ (Hiện Tại)
- ✅ Đã có sẵn
- ✅ Đã hoạt động
- ❌ Webhook URL có vấn đề với Railway

### Merchant Mới
- ✅ Webhook URL mới từ đầu
- ✅ Có thể config Railway URL ngay
- ❌ Cần đăng ký và xác thực lại
- ❌ Mất 2-5 ngày

## 💡 Khuyến Nghị

### Option 1: Giữ Merchant Cũ + Render URL (Nhanh)

- ✅ Không cần đăng ký lại
- ✅ Webhook vẫn hoạt động với Render URL
- ✅ Có thể dùng ngay

### Option 2: Tạo Merchant Mới (Lâu Hơn)

- ✅ Webhook URL mới từ đầu
- ✅ Có thể config Railway URL ngay
- ❌ Mất 2-5 ngày để đăng ký và xác thực

## 📋 Checklist Tạo Merchant Mới

- [ ] Đã đăng ký PayOs merchant mới
- [ ] Đã xác thực tài khoản
- [ ] Đã lấy Client ID, API Key, Checksum Key
- [ ] Đã cập nhật environment variables trên Railway
- [ ] Đã config webhook URL qua API
- [ ] Đã redeploy Railway service
- [ ] Đã test tạo payment link
- [ ] Đã test thanh toán

## 🎯 Kết Luận

**Có thể tạo PayOs merchant mới**, nhưng:
- Mất 2-5 ngày để đăng ký và xác thực
- Cần cập nhật lại tất cả thông tin API
- Có thể giải quyết vấn đề webhook URL

**Khuyến nghị:**
- Nếu cần dùng ngay: Giữ merchant cũ + Render URL
- Nếu có thời gian: Tạo merchant mới để có webhook URL mới

