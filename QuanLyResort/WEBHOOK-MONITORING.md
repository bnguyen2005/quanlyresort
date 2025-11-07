# Hướng Dẫn Kiểm Tra Webhook Thanh Toán Tự Động

## 📊 Cách Kiểm Tra Webhook Hoạt Động

### 1. Kiểm Tra Trạng Thái Webhook System

**Endpoint:** `GET /api/simplepayment/webhook-status`

```bash
curl http://localhost:5130/api/simplepayment/webhook-status
```

**Response:**
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

### 2. Xem Logs Trong Console

Khi webhook được nhận, bạn sẽ thấy logs trong console với format:

```
═══════════════════════════════════════════════════════════
📥 [WEBHOOK-abc12345] Webhook received at 2025-11-06 10:30:00
   Content: BOOKING-39
   Amount: 15,000 VND
   TransactionId: TXN123456
   IP Address: 192.168.1.1
   User-Agent: PayOs/1.0
🔍 [WEBHOOK-abc12345] Extracting booking ID from content...
✅ [WEBHOOK-abc12345] Extracted booking ID: 39
🔍 [WEBHOOK-abc12345] Fetching booking 39...
✅ [WEBHOOK-abc12345] Booking found: Code=BKG2025039, Status=Pending, Amount=15,000 VND
🔄 [WEBHOOK-abc12345] Updating booking 39 to Paid status...
✅ [WEBHOOK-abc12345] Booking 39 (BKG2025039) updated to Paid successfully!
⏱️ [WEBHOOK-abc12345] Processing time: 125ms
═══════════════════════════════════════════════════════════
```

### 3. Test Webhook Manually

**Endpoint:** `POST /api/simplepayment/webhook`

```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-39",
    "amount": 15000,
    "transactionId": "TEST-123456"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 39,
  "bookingCode": "BKG2025039",
  "webhookId": "abc12345",
  "processedAt": "2025-11-06T10:30:00Z",
  "durationMs": 125
}
```

### 4. Kiểm Tra Logs File

Logs được ghi vào:
- **Console Output** (terminal chạy backend)
- **Application Logs** (nếu có cấu hình file logging)

### 5. Kiểm Tra Booking Status

Sau khi webhook xử lý thành công:

```bash
# Kiểm tra booking đã được update chưa
curl http://localhost:5130/api/bookings/39 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kiểm tra:**
- `status` = `"Paid"`
- `invoice` được tạo
- `paidAt` có giá trị

### 6. Monitor Real-time

**Browser Console:**
- Mở trang `my-bookings.html` hoặc `booking-details.html`
- Mở Developer Console (F12)
- Xem logs khi payment polling phát hiện status change

**Backend Console:**
- Xem logs với format `[WEBHOOK-xxxxx]` hoặc `[BANK-WEBHOOK-xxxxx]`
- Mỗi webhook có unique ID để trace

## 🔍 Debug Checklist

- [ ] Webhook endpoint accessible: `GET /api/simplepayment/webhook-status`
- [ ] Webhook nhận được request (check console logs)
- [ ] Booking ID được extract đúng từ content
- [ ] Booking được tìm thấy trong database
- [ ] Amount verification pass
- [ ] Booking status được update thành "Paid"
- [ ] Invoice được tạo
- [ ] Frontend polling phát hiện status change

## 📝 Log Format

### Webhook Received
```
📥 [WEBHOOK-{id}] Webhook received
   Content: {content}
   Amount: {amount} VND
   TransactionId: {transactionId}
```

### Success
```
✅ [WEBHOOK-{id}] Booking {id} ({code}) updated to Paid successfully!
⏱️ Processing time: {ms}ms
```

### Error
```
❌ [WEBHOOK-{id}] ERROR: {error message}
```

## 🚀 Production Monitoring

Trong production, nên:
1. **Setup logging service** (Serilog, NLog)
2. **Monitor webhook endpoint** (uptime, response time)
3. **Alert on errors** (webhook failures)
4. **Track webhook success rate** (metrics)

