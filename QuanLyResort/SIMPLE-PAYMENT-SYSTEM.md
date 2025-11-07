# Hệ Thống Thanh Toán Đơn Giản

## Flow Đơn Giản

### 1. Frontend (my-bookings.html)
- User click "Thanh toán"
- Hiển thị QR code với nội dung: `BOOKING-{bookingId}`
- Bắt đầu polling: Check booking status mỗi 5 giây
- Khi status = "Paid" → Ẩn QR, hiển thị "Thanh toán thành công"

### 2. Backend (PaymentController)
- Webhook endpoint: `/api/payment/webhook`
- Nhận webhook từ PayOs/VietQR
- Parse booking ID từ content
- Cập nhật booking status = "Paid"
- Trả về OK

### 3. Không Cần
- ❌ SignalR/WebSocket (phức tạp)
- ❌ Payment Session phức tạp (chỉ cần check booking status)
- ❌ Nhiều fallback logic
- ❌ Error handling phức tạp

## Endpoints Cần Thiết

1. `POST /api/payment/webhook` - Webhook từ ngân hàng
2. `GET /api/bookings/{id}` - Check booking status (đã có sẵn)

## Flow Hoàn Chỉnh

```
1. User click "Thanh toán"
   → Hiển thị QR code với BOOKING-39
   
2. User quét QR và thanh toán
   → Nội dung chuyển khoản: "BOOKING-39"
   
3. Ngân hàng gửi webhook
   → POST /api/payment/webhook
   → Content: "BOOKING-39"
   
4. Backend xử lý
   → Parse booking ID = 39
   → Update booking status = "Paid"
   → Return OK
   
5. Frontend polling detect
   → GET /api/bookings/39
   → Status = "Paid"
   → Ẩn QR, hiển thị success
```

