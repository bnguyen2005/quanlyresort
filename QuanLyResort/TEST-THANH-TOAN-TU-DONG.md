# 🧪 Test Thanh Toán Tự Động

## ✅ Điều Kiện

- ✅ App đã deploy lên Render: `https://quanlyresort.onrender.com`
- ✅ Database đã có data
- ✅ Webhook endpoint hoạt động

## 📋 Các Bước Test

### Bước 1: Cấu Hình PayOs Webhook

```bash
cd QuanLyResort
./config-payos-webhook.sh
```

**Hoặc thủ công:**
```bash
curl -X POST https://api.payos.vn/v2/webhook-url \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{
    "webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"
  }'
```

### Bước 2: Test Webhook Endpoint

**Test webhook status:**
```bash
curl https://quanlyresort.onrender.com/api/simplepayment/webhook-status
```

**Test webhook với booking thật:**
```bash
# Lấy bookingId từ database hoặc từ frontend
# Ví dụ: bookingId = 1
curl -X POST https://quanlyresort.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING1",
    "amount": 5000,
    "transactionId": "TEST123"
  }'
```

### Bước 3: Test Thanh Toán Thật

1. **Đăng nhập:**
   - Vào: `https://quanlyresort.onrender.com/customer/login.html`
   - Email: `customer1@guest.test`
   - Password: `Guest@123`

2. **Tạo booking mới:**
   - Vào trang booking
   - Chọn phòng và dates
   - Tạo booking
   - Lưu bookingId (ví dụ: 7)

3. **Mở modal thanh toán:**
   - Vào: `https://quanlyresort.onrender.com/customer/my-bookings.html`
   - Click "Thanh toán" trên booking vừa tạo
   - Modal hiển thị QR code với:
     - Số tiền
     - Mã booking: `BOOKING7`
     - Thông tin ngân hàng

4. **Thanh toán:**
   - Mở app ngân hàng (MB Bank)
   - Quét QR code
   - Xác nhận thanh toán
   - Chờ vài giây

5. **Kiểm tra kết quả:**
   - ✅ QR code tự động biến mất
   - ✅ Hiển thị "Thanh toán thành công"
   - ✅ Modal tự động đóng sau 2 giây
   - ✅ Booking status đổi thành "Paid"

## 🔍 Monitor Webhook

### Xem Logs Trên Render

1. Vào: https://dashboard.render.com
2. Click service `quanlyresort-api`
3. Tab "Logs"
4. Tìm các dòng:
   ```
   📥 [WEBHOOK-xxx] Webhook received
   ✅ [WEBHOOK-xxx] Booking xxx updated to Paid
   ✅ [WEBHOOK-xxx] SUCCESS!
   ```

### Xem Logs Trong Browser Console

1. Mở browser console (F12)
2. Tìm các dòng:
   ```
   🔍 [SimplePolling] Booking status: Paid
   ✅ [SimplePolling] Payment detected!
   🎉 [showPaymentSuccess] Showing payment success...
   ```

## ⚠️ Troubleshooting

### Webhook Không Nhận Được

**Kiểm tra:**
1. Webhook URL đã được cấu hình trên PayOs
2. URL đúng: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
3. Endpoint trả về 200 OK khi test

**Test:**
```bash
curl -X POST https://quanlyresort.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content":"BOOKING1","amount":5000}'
```

### QR Code Không Biến Mất

**Kiểm tra:**
1. Browser console có logs polling không?
2. Booking status có đổi thành "Paid" không?
3. UI elements có tồn tại không? (`spQRImage`, `spSuccess`)

**Debug:**
```javascript
// Trong browser console
console.log('Booking status:', booking.status);
console.log('QR element:', document.getElementById('spQRImage'));
console.log('Success element:', document.getElementById('spSuccess'));
```

### Polling Không Hoạt Động

**Kiểm tra:**
1. `window.paymentPollingInterval` có được set không?
2. API call có trả về đúng không?
3. Có lỗi CORS không?

**Debug:**
```javascript
// Trong browser console
console.log('Polling interval:', window.paymentPollingInterval);
console.log('Current booking ID:', window.currentPaymentBookingId);
```

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
- [ ] Modal tự động đóng sau 2 giây

## 🎯 Kết Quả Mong Đợi

Sau khi test thành công:
- ✅ PayOs gửi webhook tự động khi có payment
- ✅ Backend cập nhật booking status → "Paid"
- ✅ Frontend tự động ẩn QR và hiển thị success
- ✅ User thấy thông báo "Thanh toán thành công"
- ✅ Không cần refresh page
- ✅ Không cần manual update
