# 🔧 Sửa Lỗi QR Không Biến Mất Sau Thanh Toán

## 🔍 Bước 1: Kiểm Tra Backend Logs

Mở terminal backend và kiểm tra xem webhook có được gọi không:

```bash
# Tìm logs có dạng:
📥 [WEBHOOK-xxxx] Webhook received: BOOKING-4 - 10000 VND
✅ [WEBHOOK-xxxx] Booking ID: 4
✅ [WEBHOOK-xxxx] Booking BKG2025004 - Status: Paid - Amount: 10000 VND
```

**Nếu KHÔNG thấy logs:**
- ❌ Webhook không được gọi → Vấn đề ở ngân hàng/PayOs
- ✅ Cần kiểm tra webhook URL trong PayOs config

**Nếu THẤY logs nhưng status vẫn "Pending":**
- ❌ Backend không update được → Kiểm tra database/ProcessOnlinePaymentAsync

## 🔍 Bước 2: Kiểm Tra Booking Status

Mở browser Console (F12) và chạy:

```javascript
// Lấy token
const token = localStorage.getItem('token');

// Kiểm tra booking status
fetch(`/api/bookings/4`, {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => {
  console.log('Booking Status:', data.status);
  console.log('Full Booking:', data);
});
```

**Nếu status = "Paid":**
- ✅ Backend đã update thành công
- ❌ Vấn đề ở frontend polling

**Nếu status ≠ "Paid":**
- ❌ Backend chưa update → Kiểm tra webhook xử lý

## 🔍 Bước 3: Kiểm Tra Frontend Polling

Mở Console (F12) và tìm logs:

```
🔄 [SimplePolling] Starting polling for booking: 4
🔍 [SimplePolling] Booking status: Pending for booking: 4
🔍 [SimplePolling] Booking status: Pending for booking: 4
🔍 [SimplePolling] Booking status: Paid for booking: 4
✅ [SimplePolling] Payment detected! Status = Paid, stopping polling...
🎉 [showPaymentSuccess] Showing payment success...
```

**Nếu KHÔNG thấy logs polling:**
- ❌ Polling không chạy → Modal chưa được mở đúng cách
- ✅ Kiểm tra `startSimplePolling()` có được gọi không

**Nếu thấy polling nhưng status vẫn "Pending":**
- ❌ Backend chưa update → Webhook chưa được xử lý

**Nếu thấy "Payment detected!" nhưng QR không biến mất:**
- ❌ `showPaymentSuccess()` không hoạt động → Kiểm tra elements

## 🔍 Bước 4: Kiểm Tra Modal Elements

Mở Console (F12) và chạy:

```javascript
// Kiểm tra modal có tồn tại không
console.log('Modal:', document.getElementById('simplePaymentModal'));

// Kiểm tra các elements
console.log('QR Image:', document.getElementById('spQRImage'));
console.log('QR Section:', document.getElementById('spQRSection'));
console.log('Waiting:', document.getElementById('spWaiting'));
console.log('Success:', document.getElementById('spSuccess'));
```

**Nếu elements = null:**
- ❌ Modal không đúng → Có thể đang dùng modal cũ (`paymentModal`)
- ✅ Kiểm tra HTML có đúng ID không

## 🔍 Bước 5: Manual Test Webhook

Nếu webhook không được gọi tự động, test manual:

```bash
cd QuanLyResort
./debug-qr-not-hiding.sh 4
```

Hoặc curl trực tiếp:

```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-4",
    "amount": 10000
  }'
```

Sau đó quan sát:
1. Backend logs có nhận webhook không?
2. Booking status có update thành "Paid" không?
3. Frontend polling có phát hiện không?

## 🐛 Các Lỗi Thường Gặp

### 1. Webhook Không Được Gọi

**Nguyên nhân:**
- PayOs/VietQR chưa config webhook URL
- Webhook URL không accessible từ internet (localhost)
- Firewall/Network blocking

**Giải pháp:**
- Dùng ngrok để expose localhost: `ngrok http 5130`
- Update webhook URL trong PayOs config
- Test với manual webhook trước

### 2. Backend Không Update Booking

**Nguyên nhân:**
- `ProcessOnlinePaymentAsync` bị lỗi
- Booking status không phải "Pending"/"Confirmed"
- Amount mismatch

**Giải pháp:**
- Kiểm tra backend logs chi tiết
- Kiểm tra booking status trước khi update
- Kiểm tra amount verification logic

### 3. Frontend Polling Không Phát Hiện

**Nguyên nhân:**
- Polling không chạy (modal chưa mở)
- API `/api/bookings/{id}` trả về status cũ
- Cache issue

**Giải pháp:**
- Đảm bảo `startSimplePolling()` được gọi
- Thêm cache buster `?_=${Date.now()}`
- Kiểm tra response từ API

### 4. showPaymentSuccess() Không Hoạt Động

**Nguyên nhân:**
- Elements không tồn tại (wrong modal)
- CSS display bị override
- JavaScript error

**Giải pháp:**
- Kiểm tra modal ID đúng `simplePaymentModal`
- Kiểm tra elements có đúng ID không
- Kiểm tra Console có error không

## ✅ Quick Fix

Nếu vội, có thể manual trigger:

```javascript
// Trong Console (F12), sau khi thanh toán:
// 1. Manual update booking status (nếu có quyền)
// 2. Hoặc trigger showPaymentSuccess() trực tiếp:
if (window.showPaymentSuccess) {
  window.showPaymentSuccess();
}
```

## 📝 Checklist Debug

- [ ] Backend logs có nhận webhook không?
- [ ] Booking status đã thành "Paid" chưa?
- [ ] Frontend polling có chạy không? (Console logs)
- [ ] Polling có phát hiện status = "Paid" không?
- [ ] `showPaymentSuccess()` có được gọi không?
- [ ] Modal elements có tồn tại không?
- [ ] Console có JavaScript errors không?

## 🎯 Kết Luận

Nếu tất cả đều OK nhưng vẫn không hoạt động:
1. Thử refresh page và mở modal lại
2. Kiểm tra có conflict với code cũ không
3. Clear browser cache
4. Kiểm tra network tab xem API calls có thành công không

