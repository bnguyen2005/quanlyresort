# Webhook Troubleshooting Guide

## Vấn Đề: Webhook Không Hoạt Động

### 1. Kiểm Tra Endpoint Có Accessible Không

```bash
# Test webhook endpoint
curl -X POST "http://localhost:5130/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-39",
    "amount": 15000,
    "transactionId": "TEST-123"
  }'
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

### 2. Kiểm Tra Backend Logs

Khi webhook được gọi, backend sẽ log:

```
[Information] 📥 Webhook received: Content=BOOKING-39, Amount=15000
[Information] ✅ Booking 39 updated to Paid
```

**Nếu không thấy logs:**
- Webhook không đến được backend
- PayOs chưa config webhook URL
- Webhook URL không accessible từ internet

### 3. Vấn Đề Thường Gặp

#### Vấn đề 1: Unauthorized (401)
**Nguyên nhân:** Middleware chặn webhook
**Giải pháp:** Đã thêm `/api/simplepayment/webhook` vào public endpoints

#### Vấn đề 2: PayOs Không Gửi Webhook
**Nguyên nhân:** 
- Webhook URL chưa config trong PayOs dashboard
- Webhook URL là localhost (không accessible từ PayOs)

**Giải pháp:**
1. **Development:** Dùng ngrok
   ```bash
   ngrok http 5130
   # Copy URL: https://abc123.ngrok.io
   # Config trong PayOs: https://abc123.ngrok.io/api/simplepayment/webhook
   ```

2. **Production:** Deploy lên server và config webhook URL thật

#### Vấn đề 3: Booking ID Không Parse Được
**Nguyên nhân:** Content chuyển khoản không đúng format
**Giải pháp:** Đảm bảo content có format:
- `BOOKING-39` (recommended)
- `BOOKING-BKG2025039`

**Backend sẽ log:**
```
[Warning] ⚠️ Cannot extract booking ID from content: {Content}
```

#### Vấn đề 4: Amount Mismatch
**Nguyên nhân:** Số tiền chuyển không khớp
**Giải pháp:** Backend cho phép sai số 10%, hoặc amount >= expected amount

**Backend sẽ log:**
```
[Warning] ⚠️ Amount mismatch: Expected={Expected}, Received={Received}
```

### 4. Test Webhook Bằng Script

```bash
cd QuanLyResort
./debug-webhook.sh 39
```

### 5. Kiểm Tra Database

Sau khi webhook được gọi:
```sql
SELECT BookingId, BookingCode, Status, EstimatedTotalAmount, UpdatedAt
FROM Bookings
WHERE BookingId = 39;
```

Nếu `Status = 'Paid'` và `UpdatedAt` mới → ✅ Webhook đã hoạt động

### 6. Flow Hoàn Chỉnh

```
1. User quét QR và thanh toán
   → Nội dung: "BOOKING-39"
   → Số tiền: 15000 VND

2. PayOs xử lý thanh toán
   → Gửi webhook đến: /api/simplepayment/webhook
   → Body: { "content": "BOOKING-39", "amount": 15000 }

3. Backend xử lý webhook
   → Parse booking ID = 39
   → Check booking exists
   → Update status = "Paid"
   → Return OK

4. Frontend polling detect
   → GET /api/bookings/39
   → Status = "Paid"
   → Hide QR, show success
```

### 7. Debug Checklist

- [ ] Backend đang chạy (`dotnet run`)
- [ ] Endpoint `/api/simplepayment/webhook` accessible (test bằng curl)
- [ ] Response status = 200 OK
- [ ] Backend logs có `📥 Webhook received...`
- [ ] Backend logs có `✅ Booking {id} updated to Paid`
- [ ] Database có `Status = 'Paid'`
- [ ] PayOs dashboard có config webhook URL
- [ ] Webhook URL accessible từ internet (không phải localhost)

### 8. Test Manual

1. **Test webhook endpoint:**
   ```bash
   ./debug-webhook.sh 39
   ```

2. **Check booking status:**
   ```bash
   curl -X GET "http://localhost:5130/api/bookings/39" \
     -H "Authorization: Bearer $TOKEN"
   ```

3. **Check backend logs:**
   - Tìm: `📥 Webhook received...`
   - Tìm: `✅ Booking {id} updated to Paid`

### 9. Nếu Vẫn Không Hoạt Động

1. **Kiểm tra PayOs webhook logs:**
   - Vào PayOs dashboard
   - Xem webhook delivery logs
   - Check response status

2. **Kiểm tra CORS:**
   - Webhook từ PayOs có thể bị CORS block
   - Đảm bảo CORS policy cho phép PayOs domain

3. **Kiểm tra Firewall:**
   - Server có cho phép incoming webhook không?
   - Port 5130 có mở không?

4. **Test với ngrok:**
   - Expose localhost qua ngrok
   - Config PayOs webhook URL = ngrok URL
   - Test lại

