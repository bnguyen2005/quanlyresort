# 📍 Vị Trí Webhook Logs Trong Terminal

## ✅ Webhook Logs ĐÃ CÓ!

Trong terminal backend, webhook logs nằm ở **dòng 191-273**:

### Chi Tiết Logs:

```
Line 191-193: Webhook endpoint được gọi
─────────────────────────────────────────
info: QuanLyResort.Middleware.JwtAuthorizationMiddleware[0]
      [Authorization] Checking path: /api/simplepayment/webhook, Method: POST
info: QuanLyResort.Middleware.JwtAuthorizationMiddleware[0]
      [Authorization] ✅ Allowing webhook request: /api/simplepayment/webhook

Line 194-207: Webhook received
─────────────────────────────────────────
info: QuanLyResort.Controllers.SimplePaymentController[0]
      ═══════════════════════════════════════════════════════════
info: QuanLyResort.Controllers.SimplePaymentController[0]
      📥 [WEBHOOK-e122feed] Webhook received at 11/06/2025 04:14:08
info: QuanLyResort.Controllers.SimplePaymentController[0]
         Content: BOOKING-6
info: QuanLyResort.Controllers.SimplePaymentController[0]
         Amount: 5,000 VND
info: QuanLyResort.Controllers.SimplePaymentController[0]
         TransactionId: N/A
info: QuanLyResort.Controllers.SimplePaymentController[0]
         IP Address: ::1
info: QuanLyResort.Controllers.SimplePaymentController[0]
         User-Agent: curl/8.7.1

📥 [WEBHOOK-e122feed] Webhook received: BOOKING-6 - 5,000 VND  ← LINE 209

Line 211-214: Extracting booking ID
─────────────────────────────────────────
info: QuanLyResort.Controllers.SimplePaymentController[0]
      🔍 [WEBHOOK-e122feed] Extracting booking ID from content...
✅ [WEBHOOK-e122feed] Booking ID: 6  ← LINE 212
info: QuanLyResort.Controllers.SimplePaymentController[0]
      ✅ [WEBHOOK-e122feed] Extracted booking ID: 6

Line 232-233: Booking found
─────────────────────────────────────────
info: QuanLyResort.Controllers.SimplePaymentController[0]
      ✅ [WEBHOOK-e122feed] Booking found: Code=BKG2025006, Status=Pending, Amount=5,000 VND
✅ [WEBHOOK-e122feed] Booking BKG2025006 - Status: Pending - Amount: 5,000 VND

Line 268-273: Success!
─────────────────────────────────────────
info: QuanLyResort.Controllers.SimplePaymentController[0]
      ✅ [WEBHOOK-e122feed] Booking 6 (BKG2025006) updated to Paid successfully!
info: QuanLyResort.Controllers.SimplePaymentController[0]
      ⏱️ [WEBHOOK-e122feed] Processing time: 15.614ms
info: QuanLyResort.Controllers.SimplePaymentController[0]
      ═══════════════════════════════════════════════════════════
✅ [WEBHOOK-e122feed] SUCCESS! Booking BKG2025006 updated to Paid (16ms)  ← LINE 273
```

## 🔍 Cách Tìm Logs:

### Option 1: Scroll Xuống
Trong terminal backend, scroll xuống đến **dòng 191-273** để thấy webhook logs.

### Option 2: Search Trong Terminal
1. Trong terminal backend, nhấn `Ctrl+F` (hoặc `Cmd+F` trên Mac)
2. Search: `WEBHOOK-e122feed`
3. Hoặc search: `Booking BKG2025006 updated to Paid`

### Option 3: Grep Logs
Nếu bạn đang lưu logs vào file:
```bash
grep "WEBHOOK-e122feed" your-log-file.txt
```

## ✅ Kết Luận:

**Webhook đã hoạt động thành công!** ✅

- ✅ Webhook được nhận (Line 209)
- ✅ Booking ID được extract (Line 212)
- ✅ Booking được tìm thấy (Line 232)
- ✅ Booking được update thành "Paid" (Line 268, 273)
- ✅ Processing time: 15.614ms

**Bây giờ cần kiểm tra frontend:**
- Frontend polling có phát hiện status = "Paid" không?
- QR có biến mất không?
- Success message có hiện không?

