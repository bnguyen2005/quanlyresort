# 📊 Phân Tích Logs Backend

## 🔍 Từ Logs Bạn Cung Cấp

### ✅ Những Gì Đang Hoạt Động:

1. **Polling đang chạy tốt:**
   ```
   GET /api/bookings/6 (lặp lại nhiều lần)
   ```
   - Frontend đang polling mỗi 5 giây ✅
   - Authorization OK ✅
   - API trả về dữ liệu thành công ✅

2. **Authorization hoạt động:**
   ```
   [Authorization] ✅ Access granted for role 'Customer'
   ```

### ❌ Vấn Đề:

**KHÔNG thấy webhook logs!**

Trong logs, bạn KHÔNG thấy:
- `📥 [WEBHOOK-xxxx] Webhook received`
- `✅ [WEBHOOK-xxxx] Booking updated to Paid`

**Điều này có nghĩa:**
- ❌ Webhook từ PayOs/VietQR **chưa được gọi** đến backend
- ❌ Hoặc webhook được gọi nhưng **không đến được backend** (firewall, network, etc.)

## 🔧 Nguyên Nhân Có Thể:

### 1. PayOs/VietQR Chưa Config Webhook
- Webhook URL chưa được set trong PayOs dashboard
- Webhook URL không đúng format

**Giải pháp:**
- Vào PayOs dashboard
- Kiểm tra Webhook URL: `http://localhost:5130/api/simplepayment/webhook`
- ⚠️ **Lưu ý**: `localhost` chỉ hoạt động trong môi trường local. Nếu deploy, cần URL public (dùng ngrok)

### 2. Webhook URL Không Accessible
- Backend đang chạy trên `localhost` - không thể truy cập từ internet
- PayOs không thể gọi đến `localhost`

**Giải pháp:**
- Dùng **ngrok** để expose localhost:
  ```bash
  ngrok http 5130
  ```
- Copy URL từ ngrok (ví dụ: `https://abc123.ngrok.io`)
- Update webhook URL trong PayOs: `https://abc123.ngrok.io/api/simplepayment/webhook`

### 3. Webhook Được Gọi Nhưng Bị Block
- Firewall blocking
- CORS issues
- Middleware blocking

**Kiểm tra:**
- Xem logs có 403/401 errors không
- Kiểm tra `JwtAuthorizationMiddleware` có cho phép webhook endpoint không

## 🧪 Cách Test Ngay:

### Test 1: Kiểm Tra Webhook Endpoint
```bash
cd QuanLyResort
./check-booking-status.sh 6
```

### Test 2: Manual Call Webhook
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-6",
    "amount": 10000
  }'
```

Sau đó kiểm tra:
- Backend logs có hiển thị `📥 [WEBHOOK-xxxx]` không?
- Frontend polling có phát hiện status = "Paid" không?
- QR có biến mất không?

### Test 3: Kiểm Tra Booking Status
Mở browser Console (F12) và chạy:
```javascript
const token = localStorage.getItem('token');
fetch('/api/bookings/6', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => console.log('Status:', data.status));
```

## 📝 Checklist Debug:

- [ ] Backend logs có `📥 [WEBHOOK-xxxx]` khi thanh toán không?
- [ ] PayOs dashboard có config webhook URL không?
- [ ] Webhook URL có accessible từ internet không? (không phải localhost)
- [ ] Manual webhook call có hoạt động không?
- [ ] Booking status có update thành "Paid" không?
- [ ] Frontend polling có phát hiện status change không?

## 🎯 Kết Luận:

**Vấn đề chính:** Webhook từ PayOs/VietQR không đến được backend.

**Giải pháp:**
1. **Nếu đang test local:** Dùng ngrok để expose localhost
2. **Nếu đã deploy:** Kiểm tra webhook URL trong PayOs config
3. **Test manual webhook** để đảm bảo code hoạt động đúng

## 🚀 Next Steps:

1. **Test manual webhook** với script `check-booking-status.sh`
2. **Nếu manual hoạt động:** Vấn đề ở PayOs config → Cần ngrok hoặc deploy public
3. **Nếu manual không hoạt động:** Kiểm tra code/webhook endpoint

