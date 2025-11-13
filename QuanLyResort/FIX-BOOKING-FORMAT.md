# 🔧 Fix Format Nội Dung Chuyển Khoản

## ❌ Vấn Đề

**Code đang dùng format có dấu `-`:**
- ❌ `BOOKING-{id}` (ví dụ: `BOOKING-10`)
- ✅ SePay webhook cần: `BOOKING{id}` (ví dụ: `BOOKING10`)

## ✅ Đã Fix

**Đã sửa 2 files:**
1. ✅ `booking-details.html` - Đổi `BOOKING-${bookingId}` → `BOOKING${bookingId}`
2. ✅ `my-bookings.html` - Đổi `BOOKING-${bookingId}` → `BOOKING${bookingId}`

**File `simple-payment.js` đã đúng:**
- ✅ Dùng `BOOKING${bookingId}` (không có dấu `-`)

## 📋 Format Sau Khi Fix

**Tất cả QR code sẽ dùng format:**
```
BOOKING{id}
```

**Ví dụ:**
- Booking ID = 10 → Nội dung: `BOOKING10`
- Booking ID = 25 → Nội dung: `BOOKING25`

## ✅ Kết Quả

**Sau khi fix:**
- ✅ QR code sẽ có nội dung: `BOOKING{id}` (không có dấu `-`)
- ✅ SePay webhook sẽ extract được booking ID
- ✅ Booking sẽ tự động update thành "Paid"

## 🔗 Links

- **SePay Webhook Guide:** `SEPAY-WEBHOOK-GUIDE.md`
- **SePay QR Code Guide:** `SEPAY-QR-CODE-EXPLAINED.md`

