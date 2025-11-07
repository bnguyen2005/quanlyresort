# 🔍 Hướng Dẫn Kiểm Tra Webhook Hoạt Động

## ⚠️ LƯU Ý

Sau khi thêm logging, **cần restart backend** để các thay đổi có hiệu lực!

## 📋 Các Bước Kiểm Tra

### 1. Restart Backend

```bash
# Stop backend hiện tại (Ctrl+C)
# Restart:
cd QuanLyResort
dotnet run
```

### 2. Kiểm Tra Status Endpoint

```bash
curl http://localhost:5130/api/simplepayment/webhook-status
```

**Expected Response:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "2025-11-06T...",
  "supportedFormats": [
    "BOOKING-{id}",
    "BOOKING-BKG{id}",
    "{id} (direct booking ID)"
  ],
  "message": "Webhook system is ready to receive payments"
}
```

### 3. Test Webhook

**Option A: Dùng Script**
```bash
./test-webhook.sh [booking_id] [amount]

# Example với booking ID 41:
./test-webhook.sh 41 15000
```

**Option B: Dùng curl**
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-41",
    "amount": 15000,
    "transactionId": "TEST-123456"
  }'
```

### 4. Xem Console Logs

Khi webhook được gọi, bạn sẽ thấy trong **backend console**:

```
═══════════════════════════════════════════════════════════
📥 [WEBHOOK-abc12345] Webhook received at 2025-11-06 10:30:00
   Content: BOOKING-41
   Amount: 15,000 VND
   TransactionId: TEST-123456
   IP Address: 127.0.0.1
   User-Agent: curl/7.68.0

🔍 [WEBHOOK-abc12345] Extracting booking ID from content...
✅ [WEBHOOK-abc12345] Extracted booking ID: 41
🔍 [WEBHOOK-abc12345] Fetching booking 41...
✅ [WEBHOOK-abc12345] Booking found: Code=BKG2025041, Status=Pending, Amount=15,000 VND
🔄 [WEBHOOK-abc12345] Updating booking 41 to Paid status...
✅ [WEBHOOK-abc12345] Booking 41 (BKG2025041) updated to Paid successfully!
⏱️ [WEBHOOK-abc12345] Processing time: 125ms
═══════════════════════════════════════════════════════════
```

### 5. Kiểm Tra Booking Status

Sau khi webhook xử lý thành công:

```bash
# Kiểm tra booking đã được update chưa
curl http://localhost:5130/api/bookings/41 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kiểm tra:**
- `status` = `"Paid"` ✅
- `invoice` được tạo ✅
- `paidAt` có giá trị ✅

## 🔍 Debug

### Nếu endpoint trả về 404:
1. ✅ Đảm bảo backend đã được restart
2. ✅ Kiểm tra route: `/api/simplepayment/webhook-status`
3. ✅ Kiểm tra backend có đang chạy: `curl http://localhost:5130/api/rooms`

### Nếu webhook không hoạt động:
1. ✅ Xem console logs để tìm lỗi
2. ✅ Kiểm tra booking ID có tồn tại không
3. ✅ Kiểm tra amount có khớp không
4. ✅ Kiểm tra booking status (chưa được paid)

## 📊 Log Format

Mỗi webhook có unique ID để trace:
- `[WEBHOOK-xxxxx]` - Simple payment webhook
- `[BANK-WEBHOOK-xxxxx]` - Bank webhook
- `[PAYOS-WEBHOOK-xxxxx]` - PayOs webhook

## 🎯 Quick Test

```bash
# 1. Check status
curl http://localhost:5130/api/simplepayment/webhook-status

# 2. Test webhook (thay booking_id và amount)
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content":"BOOKING-41","amount":15000,"transactionId":"TEST-123"}'

# 3. Check console logs để xem kết quả
```

