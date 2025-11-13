# 🔍 Hướng Dẫn Verify PayOs Webhook URL

## 📋 Tổng Quan

Script tự động verify webhook URL qua PayOs API `confirm-webhook` endpoint.

## 🚀 Cách Sử Dụng

### Cách 1: Chạy Script Trực Tiếp

```bash
cd QuanLyResort
./verify-payos-webhook.sh
```

### Cách 2: Chạy Với Bash

```bash
bash QuanLyResort/verify-payos-webhook.sh
```

### Cách 3: Chạy Từ Thư Mục Gốc

```bash
bash verify-payos-webhook.sh
```

## 📊 Script Sẽ Làm Gì?

### Bước 1: Kiểm Tra Webhook Endpoint

Script sẽ test webhook endpoint trước:
```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
- HTTP 200 OK
- Response: `{"status":"active","endpoint":"/api/simplepayment/webhook",...}`

### Bước 2: Gọi PayOs API

Script sẽ gọi PayOs API để verify webhook URL:
```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
  -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

## ✅ Kết Quả Mong Đợi

### Thành Công

```
═══════════════════════════════════════════════════════════
✅ THÀNH CÔNG! Webhook URL đã được verify
═══════════════════════════════════════════════════════════

   Code: 0
   Desc: success
   Webhook URL: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

🎉 PayOs đã chấp nhận webhook URL!
   Bây giờ PayOs sẽ gửi webhook khi có thanh toán thành công.
```

### Lỗi (Code 20 - Invalid URL)

```
═══════════════════════════════════════════════════════════
⚠️  PayOs trả về lỗi
═══════════════════════════════════════════════════════════

   Code: 20
   Desc: Webhook url invalid

💡 Có thể PayOs chưa verify được Railway domain
   - Đợi 10-15 phút và thử lại
   - Hoặc liên hệ PayOs support
```

### Lỗi (401 - Unauthorized)

```
═══════════════════════════════════════════════════════════
❌ LỖI HTTP: 401
═══════════════════════════════════════════════════════════

💡 Lỗi xác thực (401 Unauthorized)
   - Kiểm tra Client ID và API Key
```

## 🔧 Cấu Hình Script

Script sử dụng các giá trị sau (có thể chỉnh sửa trong file):

```bash
CLIENT_ID="90ad103f-aa49-4c33-9692-76d739a68b1b"
API_KEY="acb138f1-a0f0-4a1f-9692-16d54332a580"
WEBHOOK_URL="https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"
PAYOS_API_URL="https://api-merchant.payos.vn/confirm-webhook"
```

## 🐛 Troubleshooting

### Lỗi: "Permission denied"

**Giải pháp:**
```bash
chmod +x verify-payos-webhook.sh
```

### Lỗi: "curl: command not found"

**Giải pháp:**
- Cài đặt curl: `brew install curl` (macOS) hoặc `apt-get install curl` (Linux)

### Lỗi: "Webhook endpoint không hoạt động"

**Giải pháp:**
1. Kiểm tra Railway service đang chạy
2. Test thủ công: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. Kiểm tra Railway logs

### Lỗi: "401 Unauthorized"

**Giải pháp:**
1. Kiểm tra Client ID và API Key trong script
2. Đảm bảo credentials đúng với PayOs Dashboard
3. Kiểm tra environment variables trên Railway

### Lỗi: "Code 20 - Webhook url invalid"

**Giải pháp:**
1. Đợi 10-15 phút và thử lại
2. Kiểm tra webhook URL có đúng format không
3. Liên hệ PayOs support nếu vẫn không được

## 📋 Checklist

- [ ] Đã chạy script verify webhook URL
- [ ] Webhook endpoint trả về 200 OK
- [ ] PayOs API trả về code 0 (success)
- [ ] PayOs Dashboard hiển thị webhook URL đã được verify
- [ ] Đã test thanh toán và nhận webhook

## 💡 Lưu Ý

1. **Script tự động:** Chỉ cần chạy một lần, script sẽ tự động verify
2. **Kết quả:** Script sẽ hiển thị kết quả chi tiết (thành công hoặc lỗi)
3. **Retry:** Nếu lỗi, có thể chạy lại script sau 10-15 phút
4. **Manual:** Nếu script không hoạt động, có thể verify thủ công qua PayOs Dashboard

## 🔗 Links Quan Trọng

- **PayOs API:** https://api-merchant.payos.vn/confirm-webhook
- **PayOs Dashboard:** https://payos.vn
- **Webhook Endpoint:** https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
- **Railway Dashboard:** https://railway.app

## 🎯 Kết Quả Mong Đợi

Sau khi verify thành công:
- ✅ PayOs đã chấp nhận webhook URL
- ✅ PayOs sẽ gửi webhook khi có thanh toán thành công
- ✅ Booking status sẽ tự động update thành "Paid"
- ✅ QR code sẽ tự động ẩn sau khi thanh toán

