# 🔧 Hướng Dẫn SePay Webhook - Production

## ✅ Tình Trạng Hiện Tại

**Code đã sẵn sàng:**
- ✅ `SimplePaymentController` đã hỗ trợ SePay webhook format
- ✅ Hỗ trợ cả `Content` và `Description` field
- ✅ Hỗ trợ cả `Amount` và `TransferAmount` field
- ✅ Hỗ trợ camelCase properties (`transferAmount`, `description`)
- ✅ Endpoint: `/api/simplepayment/webhook`

**Railway Production URL:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

## 📋 Các Bước Setup SePay Webhook

### Bước 1: Vào SePay Dashboard

1. **Đăng nhập:** https://my.sepay.vn
2. **Vào Webhooks:** https://my.sepay.vn/webhooks
3. **Click "Thêm Webhook"**

### Bước 2: Điền Form

**Tham khảo:** `HUONG-DAN-SETUP-SEPAY-WEBHOOK.md`

**Các trường quan trọng:**

| Trường | Giá Trị |
|--------|---------|
| **Đặt tên** | `ResortDeluxe` hoặc `Resort Payment Webhook` |
| **Bắn WebHooks khi** | `Có tiền vào` ✅ |
| **Khi tài khoản ngân hàng là** | `0901329227` (hoặc để trống) |
| **Bỏ qua nếu không có Code thanh toán?** | `Có` ⭐ |
| **Gọi đến URL** | `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ⭐ |
| **Là WebHooks xác thực thanh toán?** | `Có` ⭐ |
| **Gọi lại Webhooks khi?** | ☑ Check (HTTP Status Code không 200-299) |
| **Kiểu chứng thực** | `Không cần chứng thực` (test) hoặc `API Key` (production) |
| **Request Content type** | `application/json` ✅ |
| **Trạng thái** | `Kích hoạt` ✅ |

### Bước 3: Click "Thêm"

Sau khi điền xong, click nút **"Thêm"** để tạo webhook.

## 🧪 Test Webhook

### Test 1: Test Endpoint Trực Tiếp

**Sử dụng script:**
```bash
./QuanLyResort/test-sepay-webhook-production.sh
```

**Hoặc test thủ công:**
```bash
# Test empty body (verification)
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d ''

# Test SePay format với Description
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 150000,
    "transferType": "IN"
  }'
```

### Test 2: Test Với Giao Dịch Thật

1. **Tạo booking mới** trên website
2. **Thanh toán** với nội dung: `BOOKING{id}` (ví dụ: `BOOKING4`)
3. **Kiểm tra Railway logs** xem có nhận được webhook không
4. **Kiểm tra booking status** có tự động update thành "Paid" không

## 🔍 Kiểm Tra Webhook Hoạt Động

### 1. Kiểm Tra Railway Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[WEBHOOK] 📥 Webhook received`
- `[WEBHOOK] 📋 Detected Simple/SePay format`
- `[WEBHOOK] 🔍 Using Description field (SePay format)`
- `[WEBHOOK] 🔍 Using TransferAmount field (SePay format)`
- `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId`

**Ví dụ log:**
```
[WEBHOOK] 📥 [WEBHOOK-abc12345] Webhook received at 2025-01-14 10:30:00
[WEBHOOK] 📋 [WEBHOOK-abc12345] Detected Simple/SePay format
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Simple request fields: Content='NULL', Description='BOOKING4', Amount=0, TransferAmount=150000
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Using Description field (SePay format): 'BOOKING4'
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Using TransferAmount field (SePay format): 150000
[WEBHOOK] ✅ [WEBHOOK-abc12345] ✅✅✅ SUCCESS: Extracted bookingId from description: 4
```

### 2. Kiểm Tra SePay Dashboard

**SePay Dashboard → Webhooks → Xem webhook vừa tạo**

**Kiểm tra:**
- ✅ Trạng thái: "Kích hoạt"
- ✅ URL: Đúng Railway URL
- ✅ Logs: Xem có webhook được gửi không

### 3. Kiểm Tra Booking Status

**Sau khi thanh toán:**
1. Vào website → Booking details
2. Kiểm tra status có tự động update thành "Paid" không
3. Kiểm tra invoice có được tạo không

## 🐛 Troubleshooting

### Webhook Không Được Gửi

**Nguyên nhân:**
- URL không đúng
- Server không trả về 200 OK
- Code thanh toán không khớp (nếu chọn "Có" cho "Bỏ qua nếu không có Code thanh toán")

**Giải pháp:**
1. Kiểm tra URL chính xác trong SePay dashboard
2. Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. Kiểm tra code thanh toán format: `BOOKING{id}`

### Webhook Được Gửi Nhưng Không Xử Lý

**Nguyên nhân:**
- Webhook format không đúng
- Server lỗi khi xử lý
- Booking ID không được extract

**Giải pháp:**
1. Kiểm tra Railway logs để xem webhook format
2. Xem có lỗi gì trong logs không
3. Kiểm tra booking ID có được extract không
4. Cập nhật code xử lý webhook nếu cần

### Booking ID Không Được Extract

**Nguyên nhân:**
- Format code thanh toán không đúng
- SePay gửi field khác (không phải `description` hoặc `content`)

**Giải pháp:**
1. Kiểm tra Railway logs để xem SePay gửi field gì
2. Cập nhật code để hỗ trợ field mới nếu cần
3. Đảm bảo code thanh toán format: `BOOKING{id}` hoặc `ORDER{id}`

## 📋 Checklist

- [ ] Đã setup SePay webhook trong dashboard
- [ ] URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] Đã chọn "Có" cho "Bỏ qua nếu không có Code thanh toán"
- [ ] Đã chọn "Có" cho "Là WebHooks xác thực thanh toán"
- [ ] Đã test endpoint với script
- [ ] Đã test với giao dịch thật
- [ ] Đã kiểm tra Railway logs
- [ ] Đã kiểm tra booking status tự động update

## 🔗 Links Quan Trọng

- **SePay Dashboard:** https://my.sepay.vn
- **Webhook Management:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Test Script:** `./QuanLyResort/test-sepay-webhook-production.sh`

## 💡 Lưu Ý

1. **Code thanh toán format:** `BOOKING{id}` (ví dụ: `BOOKING4`)
2. **Webhook format:** SePay có thể gửi `description` hoặc `content` field
3. **Amount format:** SePay có thể gửi `amount` hoặc `transferAmount` field
4. **Logs:** Luôn kiểm tra Railway logs để debug
5. **Test:** Test với script trước khi test với giao dịch thật

## 🎯 Kết Luận

**Code đã sẵn sàng:**
- ✅ Hỗ trợ SePay webhook format
- ✅ Hỗ trợ cả `Content` và `Description`
- ✅ Hỗ trợ cả `Amount` và `TransferAmount`
- ✅ Endpoint đã sẵn sàng trên Railway

**Bước tiếp theo:**
1. Setup SePay webhook trong dashboard
2. Test với script
3. Test với giao dịch thật
4. Kiểm tra logs và booking status

