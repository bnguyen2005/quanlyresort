# 🔧 Cấu Hình PayOs Webhook Cho Thanh Toán Tự Động

## ✅ Điều Kiện

- ✅ App đã deploy lên Render với HTTPS domain: `https://quanlyresort.onrender.com`
- ✅ Database đã được tạo và seed
- ✅ Webhook endpoint: `/api/simplepayment/webhook`

## 📋 Các Bước Cấu Hình

### Bước 1: Lấy Webhook URL

**Webhook URL:**
```
https://quanlyresort.onrender.com/api/simplepayment/webhook
```

### Bước 2: Cấu Hình PayOs Webhook

**Cách 1: Dùng API (Khuyến Nghị)**

Chạy script:
```bash
cd QuanLyResort
./config-payos-webhook.sh
```

Hoặc thủ công:
```bash
curl -X POST https://api.payos.vn/v2/webhook-url \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{
    "webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"
  }'
```

**Cách 2: Qua PayOs Dashboard (Nếu Có)**

1. Đăng nhập PayOs Dashboard
2. Vào mục "Webhook Configuration"
3. Nhập URL: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
4. Lưu cấu hình

### Bước 3: Kiểm Tra Webhook

**Test webhook endpoint:**
```bash
curl -X POST https://quanlyresort.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING1",
    "amount": 5000,
    "transactionId": "TEST123"
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 1,
  "bookingCode": "BKG2025001"
}
```

## 🔄 Luồng Thanh Toán Tự Động

### 1. User Quét QR Code

- User click "Thanh toán" trên booking
- Modal hiển thị QR code với thông tin:
  - Số tiền
  - Mã booking (BOOKING-{id})
  - Thông tin ngân hàng

### 2. User Thanh Toán

- User mở app ngân hàng
- Quét QR code
- Xác nhận thanh toán
- Ngân hàng xử lý payment

### 3. PayOs Gửi Webhook

- PayOs gửi POST request đến webhook URL
- Backend nhận webhook:
  - Parse booking ID từ content
  - Verify amount
  - Update booking status → "Paid"
  - Log transaction

### 4. Frontend Tự Động Cập Nhật

- Frontend polling mỗi 2 giây
- Detect status = "Paid"
- Ẩn QR code
- Hiển thị "Thanh toán thành công"
- Đóng modal sau 2 giây

## 🧪 Test Thanh Toán Tự Động

### Test 1: Manual Webhook

```bash
# Tạo booking mới (bookingId = 1)
# Sau đó test webhook:
curl -X POST https://quanlyresort.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING1",
    "amount": 5000,
    "transactionId": "TEST123"
  }'
```

### Test 2: Real Payment

1. **Tạo booking mới:**
   - Vào trang booking
   - Tạo booking mới
   - Lưu bookingId (ví dụ: 7)

2. **Mở modal thanh toán:**
   - Click "Thanh toán" trên booking
   - Copy bookingId từ QR code (ví dụ: BOOKING7)

3. **Thanh toán thật:**
   - Mở app ngân hàng
   - Quét QR code
   - Xác nhận thanh toán

4. **Kiểm tra:**
   - Xem logs trên Render → tìm webhook logs
   - Frontend tự động ẩn QR và hiển thị success
   - Booking status đổi thành "Paid"

## 📊 Monitor Webhook

### Xem Logs Trên Render

1. Vào: https://dashboard.render.com
2. Click service `quanlyresort-api`
3. Tab "Logs"
4. Tìm các dòng:
   - `📥 [WEBHOOK-xxx] Webhook received`
   - `✅ [WEBHOOK-xxx] Booking xxx updated to Paid`
   - `✅ [WEBHOOK-xxx] SUCCESS!`

### Test Webhook Status

```bash
curl https://quanlyresort.onrender.com/api/simplepayment/webhook-status
```

**Kết quả:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "2025-11-08T02:00:00Z",
  "message": "Webhook system is ready to receive payments"
}
```

## ⚠️ Troubleshooting

### Webhook Không Nhận Được

1. **Kiểm tra URL:**
   - Đảm bảo URL đúng: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
   - Không có trailing slash

2. **Kiểm tra CORS:**
   - Webhook endpoint đã được thêm vào `PublicEndpoints`
   - Không cần authentication

3. **Kiểm tra Logs:**
   - Xem logs trên Render
   - Tìm lỗi 404, 500, hoặc CORS

### QR Code Không Biến Mất

1. **Kiểm tra polling:**
   - Mở browser console
   - Tìm logs: `🔍 [SimplePolling] Booking status`
   - Đảm bảo status = "Paid"

2. **Kiểm tra UI elements:**
   - `spQRImage` - QR image
   - `spQRSection` - QR section
   - `spSuccess` - Success message
   - `spWaiting` - Waiting message

3. **Force refresh:**
   - Reload page
   - Check booking status trong database

## ✅ Checklist

- [ ] Webhook URL đã được cấu hình trên PayOs
- [ ] Webhook endpoint trả về 200 OK
- [ ] Database có booking với status "Pending"
- [ ] QR code hiển thị đúng amount và booking ID
- [ ] Frontend polling hoạt động (mỗi 2 giây)
- [ ] Webhook logs xuất hiện khi có payment
- [ ] Booking status đổi thành "Paid" sau payment
- [ ] QR code biến mất sau khi thanh toán
- [ ] Success message hiển thị

## 🎯 Kết Quả Mong Đợi

Sau khi cấu hình xong:
- ✅ PayOs gửi webhook tự động khi có payment
- ✅ Backend cập nhật booking status → "Paid"
- ✅ Frontend tự động ẩn QR và hiển thị success
- ✅ User thấy thông báo "Thanh toán thành công"

