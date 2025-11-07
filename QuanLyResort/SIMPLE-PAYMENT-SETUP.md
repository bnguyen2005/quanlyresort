# Hệ Thống Thanh Toán Đơn Giản - Setup Guide

## ✅ Đã Tạo

1. **Backend Controller:** `SimplePaymentController.cs`
   - Endpoint: `POST /api/simplepayment/webhook`
   - Chỉ xử lý: Parse booking ID → Update status = "Paid"

2. **Frontend Script:** `simple-payment.js`
   - QR code generation
   - Polling check booking status (5 giây/lần)
   - Auto hide QR khi paid

## 🚀 Cách Sử Dụng

### Bước 1: Đăng ký Controller trong Program.cs

Thêm vào `Program.cs` (sau các controllers khác):

```csharp
// Simple Payment Controller (không cần thêm gì, đã tự động map)
```

Controller đã có `[ApiController]` và `[Route]` nên sẽ tự động được đăng ký.

### Bước 2: Thêm Modal Đơn Giản vào my-bookings.html

Thêm **SAU** modal `payListModal` hiện tại (dòng ~290):

```html
<!-- Modal Thanh Toán Đơn Giản -->
<div class="modal fade" id="simplePaymentModal" tabindex="-1">
  <div class="modal-dialog modal-dialog-centered modal-lg">
    <div class="modal-content" style="border-radius: 20px;">
      <div class="modal-header" style="background: linear-gradient(135deg, #c8a97e 0%, #b89968 100%); color: white;">
        <h5 class="modal-title">💳 Thanh Toán</h5>
        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body" style="padding: 30px;">
        <div class="text-center mb-4">
          <h6>Mã đặt phòng: <strong id="spBookingCode">-</strong></h6>
          <h4 class="text-primary">Số tiền: <span id="spAmount">0 ₫</span></h4>
        </div>

        <div id="spQRSection">
          <p class="text-center mb-3">
            <strong>Nội dung chuyển khoản:</strong><br>
            <code id="spContent" style="background: #f8f9fa; padding: 8px 12px; border-radius: 8px; font-size: 16px; font-weight: 600;">BOOKING-</code>
          </p>
          <div class="text-center mb-4">
            <img id="spQRImage" alt="QR Code" style="max-width: 300px; border: 4px solid #e9ecef; border-radius: 15px; padding: 15px;">
          </div>
          <div class="card" style="background: #f8f9fa; padding: 20px; border-radius: 12px;">
            <p class="mb-2"><strong>Ngân hàng:</strong> MBBank</p>
            <p class="mb-2"><strong>Số tài khoản:</strong> <span id="spBankAccount">0901329227</span></p>
            <p class="mb-0"><strong>Chủ tài khoản:</strong> <span id="spBankName">Resort Deluxe</span></p>
          </div>
        </div>

        <div id="spWaiting" class="text-center mt-4" style="display: block;">
          <div class="spinner-border text-primary" role="status"></div>
          <p class="mt-2">Đang chờ thanh toán...</p>
        </div>

        <div id="spSuccess" class="text-center mt-4" style="display: none;">
          <div class="alert alert-success">
            <h5>✅ Thanh toán thành công!</h5>
            <p>Đang cập nhật thông tin...</p>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
      </div>
    </div>
  </div>
</div>
```

### Bước 3: Thêm Script vào my-bookings.html

Thêm **TRƯỚC** thẻ `</body>`:

```html
<script src="/customer/js/simple-payment.js"></script>
```

### Bước 4: Cập Nhật Nút "Thanh toán"

Thay đổi dòng 965 trong `renderBookings`:

```javascript
// Từ:
<button class=\"btn btn-primary\" onclick=\"payBooking(${booking.bookingId})\">

// Thành:
<button class=\"btn btn-primary\" onclick=\"openSimplePayment(${booking.bookingId})\">
```

## 🧪 Test Webhook

### Test bằng curl:

```bash
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING-39",
    "amount": 15000,
    "transactionId": "TEST-123"
  }'
```

### Test bằng script:

```bash
cd QuanLyResort
./test-simple-webhook.sh 39
```

## 📋 Flow Hoàn Chỉnh

1. **User click "Thanh toán"**
   - Gọi `openSimplePayment(bookingId)`
   - Hiển thị modal với QR code
   - Content: `BOOKING-{bookingId}`

2. **User quét QR và thanh toán**
   - Nội dung chuyển khoản: `BOOKING-39`
   - Số tiền: bất kỳ (>= estimated amount)

3. **PayOs/VietQR gửi webhook**
   - POST `/api/simplepayment/webhook`
   - Body: `{ "content": "BOOKING-39", "amount": 15000 }`

4. **Backend xử lý**
   - Parse booking ID = 39
   - Update booking status = "Paid"
   - Return OK

5. **Frontend polling detect**
   - Check booking status mỗi 5 giây
   - Khi status = "Paid" → Ẩn QR, hiển thị success
   - Đóng modal sau 2 giây

## ✨ Ưu Điểm

- ✅ **Đơn giản:** Chỉ 1 endpoint webhook, 1 script JS
- ✅ **Không cần SignalR:** Chỉ dùng polling
- ✅ **Không cần Payment Session:** Chỉ check booking status
- ✅ **Dễ debug:** Log rõ ràng
- ✅ **Dễ test:** Có script test

## 🔧 Troubleshooting

### Webhook không hoạt động?
- Kiểm tra logs: `[Information] 📥 Webhook received...`
- Test endpoint: `curl -X POST http://localhost:5130/api/simplepayment/webhook ...`

### QR không ẩn sau khi thanh toán?
- Kiểm tra polling có chạy không (console.log)
- Kiểm tra booking status có = "Paid" không
- Kiểm tra network tab xem API call có thành công không

### Booking ID không parse được?
- Đảm bảo content có format: `BOOKING-39` hoặc `BOOKING-BKG2025039`
- Check logs: `[Warning] ⚠️ Cannot extract booking ID...`

