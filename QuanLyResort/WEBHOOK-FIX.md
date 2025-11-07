# Webhook Fix - Hướng Dẫn

## ✅ Đã Sửa

1. **Thêm webhook endpoints vào PublicEndpoints list**
2. **Thêm explicit webhook check TRƯỚC authentication check**
3. **Webhook endpoints không cần JWT token**

## 🔄 Cần Restart Backend

**QUAN TRỌNG:** Sau khi sửa middleware, bạn **PHẢI restart backend** để thay đổi có hiệu lực:

```bash
# Stop backend (Ctrl+C)
# Restart:
cd QuanLyResort
dotnet run
```

## 🧪 Test Webhook

Sau khi restart backend:

```bash
cd QuanLyResort
./debug-webhook.sh 39
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 39,
  "bookingCode": "BKG2025039"
}
```

## 📋 Webhook Endpoints Được Cho Phép

Tất cả các endpoints sau đều không cần JWT token:
- ✅ `/api/simplepayment/webhook`
- ✅ `/api/payment/webhook`
- ✅ `/api/payment/payos-webhook`
- ✅ `/api/payment/vietqr-webhook`
- ✅ `/api/payment/mbbank-webhook`
- ✅ `/api/payment/bank-webhook`

## 🔍 Kiểm Tra Logs

Sau khi test webhook, check backend logs:

```
[Authorization] ✅ Allowing webhook request: /api/simplepayment/webhook
[Information] 📥 Webhook received: Content=BOOKING-39, Amount=15000
[Information] ✅ Booking 39 updated to Paid
```

## ⚠️ Nếu Vẫn Lỗi

1. **Đảm bảo backend đã restart** sau khi sửa middleware
2. **Kiểm tra path có đúng không** (lowercase: `/api/simplepayment/webhook`)
3. **Kiểm tra method có đúng không** (POST)
4. **Xem backend logs** để biết middleware có chạy không

