# 🔧 Fix: Booking Not Found - Webhook Không Tìm Thấy Booking

## 📋 Vấn Đề

**Từ logs:**
```
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 5
[WEBHOOK] 🔍 Fetching booking 5...
[WEBHOOK] ⚠️ Booking 5 not found
```

**Webhook hoạt động tốt:**
- ✅ Extract được booking ID: 5
- ✅ Extract được amount: 5000
- ✅ Format đúng

**Nhưng:**
- ❌ Booking 5 không tồn tại trong database
- ❌ Không thể update status
- ❌ QR code không ẩn

## 🎯 Nguyên Nhân

**Booking 5 không tồn tại trong database!**

Có thể do:
1. Booking đã bị xóa
2. Booking ID trong nội dung chuyển khoản sai
3. Database không có booking này
4. Đang test với booking ID không tồn tại

## 🔍 Cách Kiểm Tra

### Bước 1: Kiểm Tra Booking ID Thực Tế

**Vào website → My Bookings → Xem booking ID thực tế**

Hoặc kiểm tra database:
```sql
SELECT BookingId, BookingCode, Status, EstimatedTotalAmount 
FROM Bookings 
ORDER BY BookingId DESC 
LIMIT 10;
```

### Bước 2: Kiểm Tra Nội Dung Chuyển Khoản

**Khi thanh toán, nội dung chuyển khoản phải là:**
```
BOOKING{id}
```

**Ví dụ:**
- Booking ID = 4 → Nội dung: `BOOKING4`
- Booking ID = 10 → Nội dung: `BOOKING10`
- Booking ID = 5 → Nội dung: `BOOKING5` ✅

### Bước 3: Test Với Booking ID Có Thật

**Thay vì test với booking 5 (không tồn tại), test với booking ID có thật:**

1. **Tìm booking ID có thật:**
   - Vào website → My Bookings
   - Xem booking ID (ví dụ: 4, 6, 10...)

2. **Test webhook với booking ID có thật:**
   ```bash
   curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{
       "description": "BOOKING4",
       "transferAmount": 150000,
       "transferType": "IN"
     }'
   ```

3. **Kiểm tra logs:**
   - Phải thấy: `✅ Booking found: Code=BOOKING4, Status=...`
   - Không thấy: `⚠️ Booking not found`

## 🔧 Giải Pháp

### Giải Pháp 1: Sử Dụng Booking ID Có Thật

**Thay vì test với booking 5, dùng booking ID có thật:**

1. **Tạo booking mới:**
   - Vào website → Đặt phòng
   - Tạo booking mới
   - Lưu booking ID (ví dụ: 11)

2. **Thanh toán với nội dung đúng:**
   - Nội dung: `BOOKING11`
   - Số tiền: Đúng với booking

3. **Kiểm tra webhook:**
   - Railway logs phải thấy: `✅ Booking found`
   - Booking status tự động update = "Paid"
   - QR code tự động ẩn

### Giải Pháp 2: Kiểm Tra Database

**Nếu booking 5 thực sự tồn tại nhưng không tìm thấy:**

1. **Kiểm tra database connection:**
   - Railway logs có lỗi database không?
   - Database có đang chạy không?

2. **Kiểm tra booking có bị xóa không:**
   ```sql
   SELECT * FROM Bookings WHERE BookingId = 5;
   ```

3. **Kiểm tra booking có bị soft delete không:**
   - Một số hệ thống dùng soft delete
   - Booking vẫn tồn tại nhưng bị đánh dấu deleted

### Giải Pháp 3: Tạo Booking Mới Để Test

**Nếu không có booking nào, tạo booking mới:**

1. **Tạo booking:**
   - Vào website → Đặt phòng
   - Chọn phòng → Đặt phòng
   - Lưu booking ID mới

2. **Thanh toán:**
   - Click "Thanh toán"
   - Quét QR code
   - Chuyển khoản với nội dung: `BOOKING{id}`

3. **Kiểm tra:**
   - Webhook sẽ nhận được
   - Booking sẽ được update
   - QR code sẽ tự động ẩn

## 📊 Checklist

- [ ] Booking ID trong nội dung chuyển khoản = Booking ID thực tế?
- [ ] Booking có tồn tại trong database không?
- [ ] Format nội dung chuyển khoản = `BOOKING{id}`?
- [ ] Test với booking ID có thật?
- [ ] Database connection OK?

## 🎯 Kết Luận

**Vấn đề:** Booking 5 không tồn tại trong database

**Giải pháp:** 
1. Sử dụng booking ID có thật để test
2. Hoặc tạo booking mới để test
3. Đảm bảo nội dung chuyển khoản = `BOOKING{id}` với id đúng

**Sau khi fix:**
- ✅ Webhook sẽ tìm thấy booking
- ✅ Booking status sẽ được update = "Paid"
- ✅ QR code sẽ tự động ẩn

## 🔗 Links

- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app
- **My Bookings:** https://quanlyresort-production.up.railway.app/customer/my-bookings.html

