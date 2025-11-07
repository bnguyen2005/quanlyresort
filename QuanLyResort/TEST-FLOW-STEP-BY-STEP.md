# Test Flow: QR → Thanh toán → Webhook → Cập nhật UI

## 🎯 Mục Tiêu

Test flow hoàn chỉnh:
1. User click "Thanh toán" → QR code hiển thị
2. User quét QR và thanh toán (hoặc simulate webhook)
3. Webhook được gọi → Backend cập nhật booking
4. Frontend polling detect → UI tự động cập nhật

## 📋 Chuẩn Bị

### 1. Khởi động Backend

```bash
cd QuanLyResort
dotnet run
```

Đợi đến khi thấy: `Now listening on: http://localhost:5130`

### 2. Chuẩn bị Booking

- Tạo booking mới HOẶC
- Tìm booking có status = "Pending"
- Ghi nhớ Booking ID (ví dụ: 39)

### 3. Mở Frontend

1. Mở browser: `http://localhost:5130/customer/my-bookings.html`
2. Đăng nhập:
   - Email: `customer1@guest.test`
   - Password: `Guest@123`
3. Tìm booking cần test

## 🧪 Test Flow

### Cách 1: Test Bằng Script (Nhanh)

```bash
cd QuanLyResort

# Test với booking ID 39
./quick-test-flow.sh 39

# Hoặc test webhook đơn giản
./test-simple-webhook.sh 39
```

### Cách 2: Test Manual (Từng Bước)

#### Bước 1: Mở Payment Modal

1. Trong browser, click nút **"Thanh toán"** trên một booking
2. **Quan sát:**
   - ✅ Modal hiển thị
   - ✅ QR code hiển thị
   - ✅ Nội dung: `BOOKING-{bookingId}`
   - ✅ Số tiền hiển thị đúng
   - ✅ Console log: `[SimplePayment] Modal opened for booking: {id}`

#### Bước 2: Test Webhook

Mở terminal mới (giữ browser mở):

```bash
cd QuanLyResort

# Test webhook với booking ID 39
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

**Kiểm tra Backend Logs:**
```
[Information] 📥 Webhook received: Content=BOOKING-39, Amount=15000
[Information] ✅ Booking 39 updated to Paid
```

#### Bước 3: Kiểm Tra UI Tự Động Cập Nhật

Trong browser (modal vẫn đang mở):

**Quan sát:**
1. **Trong vòng 5 giây** (polling interval):
   - ✅ QR code tự động ẩn
   - ✅ Hiển thị "✅ Thanh toán thành công!"
   - ✅ Spinner loading biến mất
   - ✅ Console log: `[Polling] Status = Paid, updating UI...`

2. **Sau 2 giây:**
   - ✅ Modal tự đóng
   - ✅ Toast notification: "✅ Thanh toán thành công!"
   - ✅ Booking list tự động reload
   - ✅ Booking hiển thị status = "Paid" với badge xanh

#### Bước 4: Kiểm Tra Database

```bash
# Nếu có token, kiểm tra booking status
curl -X GET "http://localhost:5130/api/bookings/39" \
  -H "Authorization: Bearer $TOKEN" | jq '.status'
```

Expected: `"Paid"`

## 🔍 Debug

### Vấn đề 1: Webhook không hoạt động

**Triệu chứng:**
- Response: `{"message": "Unauthorized..."}`

**Giải pháp:**
- Kiểm tra `SimplePaymentController.cs` có `[AllowAnonymous]` không
- Rebuild: `dotnet build`

### Vấn đề 2: UI không cập nhật

**Triệu chứng:**
- Webhook thành công nhưng QR code không ẩn

**Debug:**
1. Mở browser console (F12)
2. Kiểm tra polling logs:
   ```
   [Polling] Current status: ...
   ```
3. Kiểm tra network tab:
   - Request: `GET /api/bookings/39`
   - Response có `status: "Paid"` không?

**Giải pháp:**
- Đảm bảo polling đang chạy (check console)
- Kiểm tra `simple-payment.js` có load không
- Refresh page và thử lại

### Vấn đề 3: Booking ID không parse được

**Triệu chứng:**
- Webhook response: `"Không tìm thấy booking ID trong nội dung"`

**Giải pháp:**
- Đảm bảo content có format: `BOOKING-39` hoặc `BOOKING-BKG2025039`
- Check backend logs: `⚠️ Cannot extract booking ID from content...`

## ✅ Checklist

- [ ] Backend đang chạy (`dotnet run`)
- [ ] Frontend mở được (`http://localhost:5130/customer/my-bookings.html`)
- [ ] Đăng nhập thành công
- [ ] Có booking với status = "Pending"
- [ ] Click "Thanh toán" → Modal hiển thị QR
- [ ] QR code hiển thị đúng (có nội dung `BOOKING-{id}`)
- [ ] Test webhook → Response success
- [ ] Backend logs: `✅ Booking {id} updated to Paid`
- [ ] UI tự động cập nhật (QR ẩn, success hiển thị)
- [ ] Modal tự đóng sau 2 giây
- [ ] Booking list reload và hiển thị status = "Paid"

## 🎬 Video Flow (Mô Tả)

```
1. User action:
   → Click "Thanh toán" button
   → Modal opens with QR code
   → Polling starts (every 5 seconds)

2. Payment simulation:
   → curl POST /api/simplepayment/webhook
   → Backend processes: Parse ID → Update status = "Paid"
   → Return success

3. Frontend auto-update:
   → Polling detects status = "Paid"
   → Hide QR code
   → Show success message
   → Auto-close modal after 2 seconds
   → Reload booking list
   → Display "Paid" badge
```

## 💡 Tips

1. **Test nhanh:** Dùng script `quick-test-flow.sh`
2. **Test real:** Quét QR bằng app ngân hàng (cần config PayOs webhook)
3. **Debug:** Mở browser console (F12) để xem logs
4. **Backend logs:** Xem terminal nơi chạy `dotnet run`

## 🚀 Next Steps

Sau khi test thành công:
1. Config PayOs webhook URL trong dashboard
2. Test với real payment (quét QR thật)
3. Monitor logs để đảm bảo webhook được gọi đúng

