# 📋 Tóm Tắt SePay Webhook

## ✅ Code Đã Sẵn Sàng

**SimplePaymentController đã hỗ trợ SePay:**
- ✅ Hỗ trợ `Content` và `Description` field
- ✅ Hỗ trợ `Amount` và `TransferAmount` field
- ✅ Hỗ trợ camelCase properties (`transferAmount`, `description`)
- ✅ Endpoint: `/api/simplepayment/webhook`
- ✅ URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 📋 Các Bước Setup

### 1. Vào SePay Dashboard
- **URL:** https://my.sepay.vn/webhooks
- **Click:** "Thêm Webhook"

### 2. Điền Form

**Các trường quan trọng:**

| Trường | Giá Trị |
|--------|---------|
| **Gọi đến URL** | `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ⭐ |
| **Bỏ qua nếu không có Code thanh toán?** | `Có` ⭐ |
| **Là WebHooks xác thực thanh toán?** | `Có` ⭐ |
| **Bắn WebHooks khi** | `Có tiền vào` ✅ |
| **Request Content type** | `application/json` ✅ |

**Xem chi tiết:** `HUONG-DAN-SETUP-SEPAY-WEBHOOK.md`

### 3. Test Webhook

**Sử dụng script:**
```bash
./QuanLyResort/test-sepay-webhook-production.sh
```

**Hoặc test thủ công:**
```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 150000,
    "transferType": "IN"
  }'
```

## 🔍 Kiểm Tra

### 1. Railway Logs
**Railway Dashboard → Service → Logs**

**Tìm:**
- `[WEBHOOK] 📥 Webhook received`
- `[WEBHOOK] 📋 Detected Simple/SePay format`
- `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId`

### 2. Booking Status
- Vào website → Booking details
- Kiểm tra status có tự động update thành "Paid" không

## 🐛 Troubleshooting

**Webhook không được gửi:**
- Kiểm tra URL trong SePay dashboard
- Test endpoint với curl
- Kiểm tra code thanh toán format: `BOOKING{id}`

**Webhook được gửi nhưng không xử lý:**
- Kiểm tra Railway logs
- Xem webhook format từ SePay
- Kiểm tra booking ID có được extract không

## 📋 Checklist

- [ ] Đã setup SePay webhook trong dashboard
- [ ] URL đúng Railway URL
- [ ] Đã test với script
- [ ] Đã test với giao dịch thật
- [ ] Đã kiểm tra Railway logs
- [ ] Đã kiểm tra booking status tự động update

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Test Script:** `./QuanLyResort/test-sepay-webhook-production.sh`
- **Hướng dẫn chi tiết:** `SEPAY-WEBHOOK-GUIDE.md`

## 💡 Lưu Ý

1. **Code thanh toán:** Format `BOOKING{id}` (ví dụ: `BOOKING4`)
2. **Webhook format:** SePay có thể gửi `description` hoặc `content`
3. **Amount format:** SePay có thể gửi `amount` hoặc `transferAmount`
4. **Logs:** Luôn kiểm tra Railway logs để debug

