# 🔍 Kiểm Tra Booking Status Sau Khi Nhận Webhook

## 📋 Tình Huống

**SePay webhook đã được nhận và đang xử lý:**
- ✅ Webhook received từ SePay
- ✅ Extract được booking ID = 4
- ✅ Booking found: Code=BKG2025004, Status=Pending
- ✅ Amount verified: Expected=5000, Received=5000
- ✅ Đang update booking status...

**Logs bị cắt ở phần "Fetching booking 4..."**

## 🔍 Cách Kiểm Tra Booking Status

### Cách 1: Kiểm Tra Trực Tiếp Trên Website

1. **Vào website:**
   - https://quanlyresort-production.up.railway.app
   - Đăng nhập với tài khoản customer

2. **Vào "My Bookings":**
   - Click vào menu "My Bookings"
   - Hoặc truy cập: https://quanlyresort-production.up.railway.app/customer/my-bookings.html

3. **Kiểm tra booking 4:**
   - Tìm booking có Code: `BKG2025004`
   - Xem Status:
     - ✅ **"Paid"** → Webhook đã update thành công!
     - ⚠️ **"Pending"** → Webhook chưa update hoặc có lỗi

### Cách 2: Kiểm Tra Qua API

**Test API trực tiếp:**

```bash
curl -X GET https://quanlyresort-production.up.railway.app/api/bookings/4 \
  -H "Authorization: Bearer {token}"
```

**Kiểm tra response:**
- `"status": "Paid"` → ✅ Đã update thành công
- `"status": "Pending"` → ⚠️ Chưa update

### Cách 3: Kiểm Tra Railway Logs Tiếp Theo

**Railway Dashboard → Service → Logs**

**Tìm các dòng sau (sau phần "Fetching booking 4..."):**
```
[WEBHOOK] ✅ Booking found: Code=BKG2025004, Status=Pending
[WEBHOOK] ✅ Amount verified: Expected=5000, Received=5000
[WEBHOOK] 🔄 Starting BOOKING STATUS UPDATE
[WEBHOOK] 🔄 Updating booking 4 to Paid status...
[WEBHOOK] 🔄 ProcessOnlinePaymentAsync returned: True
[WEBHOOK] ✅✅✅ SUCCESS: Booking status is 'Paid'!
[WEBHOOK] ✅ Booking 4 (BKG2025004) updated to Paid successfully!
```

**Nếu thấy các dòng này:**
→ ✅ Booking đã được update thành công!

**Nếu KHÔNG thấy:**
→ Có thể có lỗi khi update
→ Kiểm tra logs có lỗi gì không

## 🎯 Các Trường Hợp

### Trường Hợp 1: Booking Status = "Paid"

**Triệu chứng:**
- Booking status = "Paid"
- QR code đã tự động ẩn
- Thông báo "Thanh toán thành công" hiển thị

**Kết luận:**
→ ✅ Webhook đã hoạt động hoàn hảo!

### Trường Hợp 2: Booking Status = "Pending"

**Triệu chứng:**
- Booking status vẫn = "Pending"
- QR code vẫn hiển thị
- Không có thông báo "Thanh toán thành công"

**Nguyên nhân có thể:**
1. Webhook chưa update được (có lỗi)
2. Frontend polling chưa detect được status "Paid"
3. Database chưa được update

**Giải pháp:**
1. Kiểm tra Railway logs có lỗi gì không
2. Kiểm tra `ProcessOnlinePaymentAsync` có return true không
3. Kiểm tra database có được update không

## 🔧 Debug Nếu Booking Chưa Update

### Bước 1: Kiểm Tra Railway Logs

**Tìm các dòng:**
```
[WEBHOOK] 🔄 ProcessOnlinePaymentAsync returned: True
[WEBHOOK] ✅✅✅ SUCCESS: Booking status is 'Paid'!
[WEBHOOK] ✅ Booking 4 updated to Paid successfully!
```

**Nếu KHÔNG thấy:**
→ Có thể có lỗi khi update
→ Kiểm tra logs có lỗi gì không

### Bước 2: Kiểm Tra ProcessOnlinePaymentAsync

**Nếu thấy:**
```
[WEBHOOK] 🔄 ProcessOnlinePaymentAsync returned: False
[WEBHOOK] ❌❌❌ CRITICAL: Failed to update booking
```
→ Có lỗi khi update booking
→ Kiểm tra `ProcessOnlinePaymentAsync` method

### Bước 3: Kiểm Tra Database

**Nếu logs không có lỗi nhưng booking vẫn = "Pending":**
→ Có thể database chưa được update
→ Kiểm tra database connection
→ Kiểm tra transaction có commit không

## 📊 Checklist

- [ ] Booking status có = "Paid" không?
- [ ] Railway logs có `✅ Booking 4 updated to Paid successfully!` không?
- [ ] QR code có tự động ẩn không?
- [ ] Frontend polling có detect được status "Paid" không?

## 🔗 Links

- **Website:** https://quanlyresort-production.up.railway.app
- **My Bookings:** https://quanlyresort-production.up.railway.app/customer/my-bookings.html
- **Railway Logs:** Railway Dashboard → Service → Logs

## 💡 Lưu Ý

1. **Thời gian xử lý:** Webhook có thể mất vài giây để update booking
2. **Frontend polling:** Frontend polling mỗi 2 giây, sẽ detect ngay khi status = "Paid"
3. **Database:** Đảm bảo database connection OK và transaction được commit

