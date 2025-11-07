# Quick Webhook Test Guide

## 🚀 Cách Kiểm Tra Webhook Nhanh

### 1. Kiểm Tra Webhook Status

```bash
curl http://localhost:5130/api/simplepayment/webhook-status
```

**Expected Response:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "2025-11-06T10:30:00Z",
  "supportedFormats": [
    "BOOKING-{id}",
    "BOOKING-BKG{id}",
    "{id} (direct booking ID)"
  ],
  "message": "Webhook system is ready to receive payments"
}
```

### 2. Test Webhook Manually

**Option A: Dùng Script**
```bash
./test-webhook.sh [booking_id] [amount]

# Example:
./test-webhook.sh 39 15000
```

**Option B: Dùng curl trực tiếp**
```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-39",
    "amount": 15000,
    "transactionId": "TEST-123456"
  }'
```

### 3. Xem Console Logs

Khi webhook được gọi, bạn sẽ thấy trong **backend console**:

```
═══════════════════════════════════════════════════════════
📥 [WEBHOOK-abc12345] Webhook received at 2025-11-06 10:30:00
   Content: BOOKING-39
   Amount: 15,000 VND
   TransactionId: TEST-123456
   IP Address: 127.0.0.1
   User-Agent: curl/7.68.0

🔍 [WEBHOOK-abc12345] Extracting booking ID from content...
✅ [WEBHOOK-abc12345] Extracted booking ID: 39
🔍 [WEBHOOK-abc12345] Fetching booking 39...
✅ [WEBHOOK-abc12345] Booking found: Code=BKG2025039, Status=Pending, Amount=15,000 VND
🔄 [WEBHOOK-abc12345] Updating booking 39 to Paid status...
✅ [WEBHOOK-abc12345] Booking 39 (BKG2025039) updated to Paid successfully!
⏱️ [WEBHOOK-abc12345] Processing time: 125ms
═══════════════════════════════════════════════════════════
```

### 4. Kiểm Tra Booking Status

**Sau khi webhook xử lý, kiểm tra booking:**

```bash
curl http://localhost:5130/api/bookings/39 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kiểm tra:**
- `status` = `"Paid"` ✅
- `invoice` được tạo ✅
- `paidAt` có giá trị ✅

### 5. Monitor Real-time

**Frontend:**
- Mở trang `my-bookings.html` hoặc `booking-details.html`
- Mở Developer Console (F12)
- Xem logs khi payment polling phát hiện status change

**Backend:**
- Xem console terminal chạy backend
- Tìm logs với prefix `[WEBHOOK-xxxxx]` hoặc `[BANK-WEBHOOK-xxxxx]`
- Mỗi webhook có unique ID để trace

## 🔍 Debug Checklist

- [ ] Webhook endpoint accessible (`GET /api/simplepayment/webhook-status`)
- [ ] Webhook nhận được request (check console logs)
- [ ] Booking ID được extract đúng từ content
- [ ] Booking được tìm thấy trong database
- [ ] Amount verification pass
- [ ] Booking status được update thành "Paid"
- [ ] Invoice được tạo
- [ ] Frontend polling phát hiện status change

## 📊 Log Format

### Simple Payment Webhook
- Prefix: `[WEBHOOK-xxxxx]`
- Log từng bước: extract → fetch → verify → update

### Bank Webhook
- Prefix: `[BANK-WEBHOOK-xxxxx]`
- Log thêm: bank name, transaction details

### PayOs Webhook
- Prefix: `[PAYOS-WEBHOOK-xxxxx]`
- Log thêm: PayOs code, signature verification

## 🎯 Production Monitoring

Trong production, nên:
1. **Setup logging service** (Serilog, NLog) để lưu logs vào file
2. **Monitor webhook endpoint** (uptime, response time)
3. **Alert on errors** (webhook failures, timeout)
4. **Track metrics** (success rate, average processing time)

