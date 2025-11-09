# 🔍 Debug: QR Code Không Ẩn Sau Khi Thanh Toán

## 📋 Vấn Đề

Sau khi thanh toán thành công (webhook đã được nhận), QR code không ẩn và thông báo "Thanh toán thành công" không hiển thị.

## 🔍 Các Trường Hợp Cần Kiểm Tra

### Trường Hợp 1: Webhook Không Parse Được PayOs Format

**Triệu chứng:**
- Logs chỉ có: `[WEBHOOK] 📥 Webhook received...`
- Không có log: `[WEBHOOK] 📋 Detected PayOs format`
- Không có log: `[WEBHOOK] 🔍 Extracting booking ID...`

**Nguyên nhân có thể:**
- PayOsWebhookRequest deserialization thất bại
- `payOsRequest.Data` là null
- `payOsRequest.Code` là null hoặc empty

**Cách kiểm tra:**
1. Xem logs trên Render, tìm dòng có `[WEBHOOK] 🔍 Attempting to deserialize as PayOs format...`
2. Kiểm tra log: `[WEBHOOK] 🔍 PayOs deserialization result`
3. Kiểm tra log: `[WEBHOOK] 🔍 Checking PayOs format conditions`

**Giải pháp:**
- Nếu deserialization thất bại: Kiểm tra JSON format từ PayOs
- Nếu `Data` là null: Kiểm tra PayOsWebhookData class structure
- Nếu `Code` là null: PayOs có thể gửi format khác

### Trường Hợp 2: Không Extract Được Booking ID

**Triệu chứng:**
- Có log: `[WEBHOOK] 📋 Detected PayOs format`
- Có log: `[WEBHOOK] 🔍 Attempting to extract bookingId from content`
- Có log: `[WEBHOOK] ⚠️ Failed to extract bookingId from content`

**Nguyên nhân có thể:**
- Description format không match với pattern
- Ví dụ: "CSHAX0QC6D9 BOOKING4" - pattern2 nên match được "BOOKING4"

**Cách kiểm tra:**
1. Xem log: `[WEBHOOK] ExtractBookingId: Normalized content`
2. Xem log: `[WEBHOOK] ExtractBookingId: ✅ Matched pattern2 'BOOKING{Id}'`

**Giải pháp:**
- Nếu không match: Thêm pattern mới hoặc sửa pattern hiện tại
- Kiểm tra description từ PayOs có đúng format không

### Trường Hợp 3: Booking Không Được Update

**Triệu chứng:**
- Có log: `[WEBHOOK] ✅ Extracted booking ID: 4`
- Có log: `[WEBHOOK] 🔄 Updating booking 4 to Paid status...`
- Có log: `[WEBHOOK] ✅ Booking updated to Paid successfully!`
- Nhưng booking status vẫn là "Pending"

**Nguyên nhân có thể:**
- `ProcessOnlinePaymentAsync` trả về `true` nhưng không update thực sự
- Database transaction rollback
- Có lỗi trong BookingService

**Cách kiểm tra:**
1. Xem log: `[WEBHOOK] 🔄 Current booking status before update`
2. Xem log: `[WEBHOOK] ✅ Booking status after update`
3. Kiểm tra database trực tiếp: `SELECT Status FROM Bookings WHERE BookingId = 4`

**Giải pháp:**
- Kiểm tra `ProcessOnlinePaymentAsync` implementation
- Kiểm tra database transaction
- Kiểm tra có exception nào không

### Trường Hợp 4: Frontend Polling Không Phát Hiện Status "Paid"

**Triệu chứng:**
- Backend logs cho thấy booking đã được update thành "Paid"
- Frontend polling vẫn thấy status là "Pending"
- Không có log: `[FRONTEND] ✅ Payment detected!`

**Nguyên nhân có thể:**
- API `/api/bookings/4` trả về status cũ (cache)
- Frontend polling không hoạt động
- Status format không match (case sensitivity)

**Cách kiểm tra:**
1. Mở browser console (F12)
2. Tìm logs: `[FRONTEND] 🔍 [SimplePolling] Poll #X - Status: ...`
3. Kiểm tra: `[FRONTEND] 🔍 isPaid check: true/false`
4. Kiểm tra: `[FRONTEND] 🔍 Raw status: '...', Normalized: '...'`

**Giải pháp:**
- Kiểm tra API response có đúng không
- Kiểm tra polling có đang chạy không
- Kiểm tra status comparison logic

### Trường Hợp 5: showPaymentSuccess() Không Hoạt Động

**Triệu chứng:**
- Có log: `[FRONTEND] ✅ Payment detected!`
- Có log: `[FRONTEND] 🎉 [showPaymentSuccess] Showing payment success...`
- Nhưng QR code vẫn hiển thị

**Nguyên nhân có thể:**
- CSS override
- Element không tồn tại
- Modal state issue

**Cách kiểm tra:**
1. Xem logs: `[FRONTEND] ✅ [showPaymentSuccess] Hidden QR image`
2. Xem logs: `[FRONTEND] ✅ [showPaymentSuccess] Showed success message`
3. Kiểm tra computed styles trong browser DevTools

**Giải pháp:**
- Kiểm tra element IDs có đúng không
- Kiểm tra CSS có override không
- Force update với `!important` hoặc remove/add classes

## 📊 Checklist Debug

Sau khi deploy code mới với logging chi tiết, kiểm tra logs theo thứ tự:

### Backend Logs (Render Dashboard)

1. ✅ `[WEBHOOK] 📥 Webhook received` - Webhook đã được nhận
2. ✅ `[WEBHOOK] 🔍 Attempting to deserialize as PayOs format...` - Bắt đầu parse
3. ✅ `[WEBHOOK] 🔍 PayOs deserialization result` - Parse thành công/thất bại
4. ✅ `[WEBHOOK] 📋 Detected PayOs format` - Đã nhận diện PayOs format
5. ✅ `[WEBHOOK] 🔍 Attempting to extract bookingId from content` - Bắt đầu extract
6. ✅ `[WEBHOOK] ExtractBookingId: Normalized content` - Content đã normalize
7. ✅ `[WEBHOOK] ExtractBookingId: ✅ Matched pattern2` - Đã match pattern
8. ✅ `[WEBHOOK] ✅ Extracted bookingId from description: 4` - Đã extract được
9. ✅ `[WEBHOOK] ✅ Booking found: Code=..., Status=...` - Booking tồn tại
10. ✅ `[WEBHOOK] 🔄 Current booking status before update: Pending` - Status trước update
11. ✅ `[WEBHOOK] 🔄 Updating booking 4 to Paid status...` - Đang update
12. ✅ `[WEBHOOK] ✅ Booking status after update: Paid` - Status sau update
13. ✅ `[WEBHOOK] ✅ Booking updated to Paid successfully!` - Update thành công

### Frontend Logs (Browser Console)

1. ✅ `[FRONTEND] 🔄 [SimplePolling] Starting polling for booking: 4` - Polling đã bắt đầu
2. ✅ `[FRONTEND] 🔍 [SimplePolling] Poll #1 - Status: ...` - Poll đang chạy
3. ✅ `[FRONTEND] 🔍 Raw status: '...', Normalized: '...'` - Status đã normalize
4. ✅ `[FRONTEND] 🔍 isPaid check: true` - Đã phát hiện "Paid"
5. ✅ `[FRONTEND] ✅ Payment detected!` - Đã phát hiện thanh toán
6. ✅ `[FRONTEND] 🎉 [showPaymentSuccess] Showing payment success...` - Bắt đầu show success
7. ✅ `[FRONTEND] ✅ [showPaymentSuccess] Hidden QR image` - QR đã ẩn
8. ✅ `[FRONTEND] ✅ [showPaymentSuccess] Showed success message` - Success message đã hiển thị

## 🚨 Nếu Thiếu Log Nào

- **Thiếu log #2-4**: Webhook không parse được → Kiểm tra JSON format
- **Thiếu log #5-8**: Không extract được booking ID → Kiểm tra description format
- **Thiếu log #9-10**: Booking không tồn tại → Kiểm tra booking ID
- **Thiếu log #11-13**: Booking không được update → Kiểm tra ProcessOnlinePaymentAsync
- **Thiếu log Frontend #1-2**: Polling không chạy → Kiểm tra startSimplePolling
- **Thiếu log Frontend #3-4**: Status không phát hiện → Kiểm tra status comparison
- **Thiếu log Frontend #5-6**: showPaymentSuccess không được gọi → Kiểm tra polling logic
- **Thiếu log Frontend #7-8**: UI không update → Kiểm tra showPaymentSuccess implementation

## 🔧 Test Thủ Công

Nếu webhook không hoạt động, test thủ công:

```bash
cd QuanLyResort
./test-payos-webhook.sh 4
```

Script sẽ gửi webhook giả lập và bạn sẽ thấy toàn bộ logs từ backend.

## 📝 Ghi Chú

- Tất cả logs đã có prefix `[WEBHOOK]`, `[BACKEND]`, `[PAYOS]`, `[FRONTEND]` để dễ filter
- Logs chi tiết sẽ giúp xác định chính xác điểm dừng trong flow
- Sau khi xác định được điểm dừng, sẽ dễ dàng fix hơn

