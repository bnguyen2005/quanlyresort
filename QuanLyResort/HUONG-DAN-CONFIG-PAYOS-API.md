# 🔧 Hướng Dẫn Config PayOs Webhook Qua API

## ⚠️ Quan Trọng

**PayOs KHÔNG có dashboard để config webhook URL!**

PayOs sử dụng **API** để config webhook. Bạn phải gọi API để đăng ký webhook URL.

## 🚀 Cách 1: Dùng Script Tự Động (Khuyến Nghị)

### Bước 1: Chạy Ngrok

```bash
ngrok http 5130
```

**Copy URL từ output:**
```
Forwarding: https://abc123.ngrok.io -> http://localhost:5130
```
→ URL của bạn: `https://abc123.ngrok.io`

### Bước 2: Config Webhook Qua Script

```bash
cd QuanLyResort
./config-payos-webhook.sh https://abc123.ngrok.io/api/simplepayment/webhook
```

Script sẽ tự động:
- ✅ Đọc Client ID và API Key từ `appsettings.json`
- ✅ Gọi PayOs API để config webhook URL
- ✅ Hiển thị kết quả

## 🚀 Cách 2: Gọi API Thủ Công

### Bước 1: Chuẩn Bị

1. **Client ID** (từ `appsettings.json`):
   ```
   c704495b-5984-4ad3-aa23-b2794a02aa83
   ```

2. **API Key** (từ `appsettings.json`):
   ```
   f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
   ```

3. **Webhook URL** (từ ngrok):
   ```
   https://abc123.ngrok.io/api/simplepayment/webhook
   ```

### Bước 2: Gọi API

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://abc123.ngrok.io/api/simplepayment/webhook"}'
```

### Bước 3: Kiểm Tra Kết Quả

**Thành công (HTTP 200):**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://abc123.ngrok.io/api/simplepayment/webhook"
  }
}
```

**Lỗi (HTTP 400):**
```json
{
  "code": 400,
  "desc": "Webhook URL không hợp lệ"
}
```

**Lỗi (HTTP 401):**
```json
{
  "code": 401,
  "desc": "Thiếu API Key hoặc Client ID"
}
```

## ✅ Sau Khi Config Thành Công

### PayOs Sẽ Tự Động:

1. **Gửi test webhook** để verify webhook URL hoạt động
2. **Kiểm tra backend logs** để xem test webhook:
   ```
   📥 [PAYOS-WEBHOOK-xxx] Processing PayOs webhook
   ```

3. **Nếu test thành công**, PayOs sẽ:
   - ✅ Tự động gọi webhook khi thanh toán thành công
   - ✅ Gửi thông tin giao dịch trong webhook
   - ✅ Backend sẽ tự động update booking status = "Paid"
   - ✅ Frontend sẽ tự động ẩn QR và hiện success message

## 🧪 Test Webhook Sau Khi Config

### Test 1: Manual Webhook (Để Verify)

```bash
curl -X POST https://abc123.ngrok.io/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING-6", "amount": 5000}'
```

### Test 2: Thanh Toán Thật

1. Mở payment modal
2. Quét QR và thanh toán
3. Xem backend logs → Sẽ thấy webhook received
4. QR tự động biến mất trong 5 giây

## 📋 PayOs API Documentation

- **Endpoint:** `https://api-merchant.payos.vn/confirm-webhook`
- **Method:** `POST`
- **Headers:**
  - `x-client-id`: Client ID
  - `x-api-key`: API Key
  - `Content-Type`: `application/json`
- **Body:**
  ```json
  {
    "webhookUrl": "https://your-webhook-url.com/api/simplepayment/webhook"
  }
  ```

## 🔐 Security

- ✅ **Client ID** và **API Key** phải được giữ bí mật
- ✅ **Webhook URL** phải là HTTPS (trong production)
- ✅ PayOs sẽ verify webhook URL bằng cách gửi test webhook

## ⚠️ Lưu Ý

1. **Ngrok free plan:** URL thay đổi mỗi lần restart
   - Giải pháp: Dùng ngrok paid plan hoặc deploy backend

2. **Test webhook:** PayOs sẽ gửi test webhook sau khi config
   - Nếu test webhook fail → PayOs sẽ không gọi webhook khi thanh toán
   - Đảm bảo backend đang chạy và webhook endpoint hoạt động

3. **Production:** Phải deploy backend và dùng domain thật
   - Không thể dùng ngrok free plan cho production

## 🎉 Kết Quả

Sau khi config thành công, mỗi khi user thanh toán:
- ✅ PayOs tự động gọi webhook
- ✅ QR tự động biến mất
- ✅ Success message tự động hiện ra
- ✅ Booking status tự động update = "Paid"

