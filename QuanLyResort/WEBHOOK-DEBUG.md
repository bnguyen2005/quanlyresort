# Hướng Dẫn Debug Webhook

## Kiểm Tra Webhook Có Hoạt Động

### 1. Kiểm Tra Endpoint Webhook

Các endpoint webhook đã được cấu hình:
- ✅ `/api/payment/payos-webhook` - PayOs (MB Bank)
- ✅ `/api/payment/vietqr-webhook` - VietQR
- ✅ `/api/payment/mbbank-webhook` - MB Bank
- ✅ `/api/payment/bank-webhook` - Generic webhook

Tất cả đều có `[AllowAnonymous]` nên không cần JWT token.

### 2. Test Webhook Bằng Script

```bash
cd QuanLyResort
./test-webhook-backend.sh [bookingId]

# Ví dụ:
./test-webhook-backend.sh 39
```

### 3. Test Webhook Bằng Curl

#### Test PayOs Webhook:
```bash
curl -X POST http://localhost:5130/api/payment/payos-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "code": 0,
    "desc": "Success",
    "id": "PAYOS-TEST-123",
    "data": {
      "transactionId": "TXN123456",
      "amount": 15000,
      "description": "BOOKING-39",
      "accountNumber": "0901329227",
      "transactionDateTime": "2025-11-04T10:00:00Z"
    },
    "signature": "test-signature"
  }'
```

#### Test Generic Bank Webhook:
```bash
curl -X POST http://localhost:5130/api/payment/bank-webhook \
  -H "Content-Type: application/json" \
  -d '{
    "bankName": "MB",
    "transactionId": "TXN123456",
    "amount": 15000,
    "content": "BOOKING-39",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "transactionDate": "2025-11-04T10:00:00Z"
  }'
```

### 4. Kiểm Tra Backend Logs

Khi webhook được gọi, backend sẽ log:

```
[Information] Processing webhook from {BankName}, TransactionId: {TransactionId}, Amount: {Amount}, Content: {Content}
[Information] Successfully processed payment for booking {BookingId} from {BankName}, TransactionId: {TransactionId}. Broadcasted to {SessionCount} sessions
```

**Cách xem logs:**
- Nếu chạy từ terminal: logs sẽ hiển thị trong console
- Nếu chạy từ Visual Studio: xem Output window
- Nếu chạy từ dotnet run: logs trong terminal

### 5. Kiểm Tra Database

Sau khi webhook được xử lý:
```sql
-- Kiểm tra booking status
SELECT BookingId, BookingCode, Status, EstimatedTotalAmount, UpdatedAt
FROM Bookings
WHERE BookingId = 39;

-- Nếu Status = 'Paid' và UpdatedAt mới, webhook đã hoạt động
```

Hoặc dùng API:
```bash
curl -X GET "http://localhost:5130/api/payment/test/db-check?bookingId=39" \
  -H "Authorization: Bearer $TOKEN"
```

### 6. Vấn Đề Thường Gặp

#### Vấn đề 1: Signature Verification Fail
**Nguyên nhân:** PayOs/VietQR gửi signature nhưng backend verify fail
**Giải pháp:** Tạm thời disable signature verification trong `appsettings.json`:
```json
{
  "BankWebhook": {
    "PayOs": {
      "VerifySignature": false
    },
    "VietQR": {
      "VerifySignature": false
    }
  }
}
```

#### Vấn đề 2: Webhook Không Được Gọi
**Nguyên nhân:** 
- PayOs/VietQR chưa config webhook URL trong dashboard
- Webhook URL không accessible từ internet (localhost không nhận được webhook từ PayOs)

**Giải pháp:**
1. **Development:** Dùng ngrok hoặc localtunnel để expose localhost:
   ```bash
   # Cài ngrok: https://ngrok.com/
   ngrok http 5130
   
   # Copy URL (ví dụ: https://abc123.ngrok.io)
   # Config trong PayOs dashboard: https://abc123.ngrok.io/api/payment/payos-webhook
   ```

2. **Production:** Deploy lên server và config webhook URL thật

#### Vấn đề 3: Booking ID Không Parse Được
**Nguyên nhân:** Content chuyển khoản không đúng format
**Giải pháp:** Đảm bảo content có format:
- `BOOKING-BKG2025039` (recommended)
- `BOOKING-39`
- `BOOKING-BKG39`

Backend sẽ log:
```
[Warning] Could not extract booking ID from content: {Content}
```

#### Vấn đề 4: Amount Mismatch
**Nguyên nhân:** Số tiền chuyển không khớp với booking amount
**Giải pháp:** Backend sẽ log warning nhưng vẫn chấp nhận nếu amount >= expected amount

Backend sẽ log:
```
[Warning] Amount mismatch for booking {BookingId}. Expected: {Expected}, Received: {Received}
```

### 7. Kiểm Tra Webhook URL Trong PayOs Dashboard

1. Đăng nhập PayOs dashboard
2. Vào **Settings** → **Webhooks**
3. Kiểm tra webhook URL:
   - **Development:** `https://your-ngrok-url.ngrok.io/api/payment/payos-webhook`
   - **Production:** `https://your-domain.com/api/payment/payos-webhook`

4. Test webhook từ dashboard (nếu có nút Test)

### 8. Debug Webhook Step by Step

**Bước 1:** Kiểm tra endpoint có accessible không
```bash
curl -X POST http://localhost:5130/api/payment/payos-webhook \
  -H "Content-Type: application/json" \
  -d '{}'
```

Nếu có response (dù là error), endpoint đang hoạt động.

**Bước 2:** Test với payload đúng format
```bash
./test-webhook-backend.sh 39
```

**Bước 3:** Kiểm tra backend logs xem có log không

**Bước 4:** Kiểm tra database xem booking status có update không

**Bước 5:** Kiểm tra SignalR có broadcast không (xem browser console)

### 9. Logs Để Theo Dõi

Backend sẽ log các thông tin sau:
- ✅ `Processing webhook from...` - Webhook được nhận
- ✅ `Processing PayOs webhook...` - PayOs webhook được xử lý
- ✅ `Successfully processed payment...` - Payment đã được xử lý
- ✅ `Broadcasted via SignalR...` - SignalR đã broadcast
- ⚠️ `Could not extract booking ID...` - Không parse được booking ID
- ⚠️ `Amount mismatch...` - Số tiền không khớp
- ❌ `Error processing webhook...` - Lỗi xử lý webhook

### 10. Test Real Payment Flow

1. **Tạo booking mới** (status = "Pending")
2. **Mở payment modal** → QR code hiển thị
3. **Quét QR và thanh toán** từ ngân hàng với nội dung: `BOOKING-{bookingId}`
4. **PayOs gửi webhook** đến backend
5. **Backend xử lý webhook:**
   - Parse booking ID từ content
   - Verify amount
   - Update booking status = "Paid"
   - Broadcast SignalR
6. **Frontend nhận SignalR message** → Ẩn QR, hiển thị success

**Nếu bước 5 không xảy ra:**
- PayOs chưa config webhook URL
- Webhook URL không accessible
- Signature verification fail

**Nếu bước 6 không xảy ra:**
- SignalR không kết nối
- Frontend không join booking group
- Polling không chạy

