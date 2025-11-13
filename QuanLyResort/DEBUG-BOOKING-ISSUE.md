# 🔍 Debug Vấn Đề Đặt Phòng

## 📊 Phân Tích Logs

### ✅ Đang Hoạt Động

Từ logs, tôi thấy:
- ✅ Authorization hoạt động đúng
- ✅ User `customer1` với role `Customer` đang truy cập
- ✅ Database queries chạy thành công
- ✅ GET `/api/bookings/4` và `/api/bookings/my` trả về dữ liệu

### ⚠️ Vấn Đề Tiềm Ẩn

**Không thấy request POST `/api/bookings` trong logs:**
- Có nghĩa là form đặt phòng chưa được submit
- Hoặc request POST bị lỗi trước khi đến server

## 🔍 Cách Kiểm Tra

### Bước 1: Kiểm Tra Browser Console

**Mở Browser Console (F12) khi đặt phòng:**

**Tìm các dòng sau:**

**Nếu form được submit:**
```javascript
🔵 [submitBooking] Submitting: {...}
🔵 [submitBooking] Response status: 200
✅ [submitBooking] Booking created: {...}
```

**Nếu có lỗi:**
```javascript
❌ [submitBooking] API Error: ...
```

**Nếu form không được submit:**
- Không thấy log `[submitBooking]`
- Có thể validation failed hoặc button không trigger

### Bước 2: Kiểm Tra Network Tab

**Mở Browser DevTools → Network tab:**

**Khi đặt phòng, tìm request:**
- Method: `POST`
- URL: `/api/bookings`
- Status: `200` (thành công) hoặc `400/401/500` (lỗi)

**Nếu không thấy request POST:**
- Form chưa được submit
- JavaScript có lỗi
- Button không trigger event

### Bước 3: Kiểm Tra Logs Railway

**Vào Railway Dashboard → Logs**

**Tìm khi user đặt phòng:**

**Nếu có request POST:**
```
[Authorization] API Request: POST /api/bookings
[Authorization] User: customer1 accessing: /api/bookings
❌ [CreateBooking] Error: ...
```

**Nếu không thấy request POST:**
- Request chưa đến server
- Có thể bị chặn bởi CORS hoặc network issue

## 🔧 Các Vấn Đề Thường Gặp

### Vấn Đề 1: Form Validation Failed

**Triệu chứng:**
- Click "Đặt phòng" nhưng không có gì xảy ra
- Không thấy request POST trong Network tab

**Nguyên nhân:**
- Validation failed (thiếu thông tin)
- Date không hợp lệ
- Số khách vượt quá sức chứa

**Fix:**
- Kiểm tra form có đầy đủ thông tin không
- Kiểm tra date có hợp lệ không
- Kiểm tra số khách có vượt quá maxOccupancy không

### Vấn Đề 2: JavaScript Error

**Triệu chứng:**
- Click "Đặt phòng" nhưng không có gì xảy ra
- Browser Console có lỗi JavaScript

**Nguyên nhân:**
- JavaScript code có lỗi
- Function không được định nghĩa
- Variable không tồn tại

**Fix:**
- Xem Browser Console để tìm lỗi
- Fix JavaScript error
- Reload trang

### Vấn Đề 3: Token Không Được Gửi

**Triệu chứng:**
- Request POST được gửi nhưng trả về `401 Unauthorized`
- Logs: `[Authorization] ❌ Unauthorized request to: /api/bookings`

**Nguyên nhân:**
- Token không có trong localStorage
- Token không được gửi trong Authorization header
- Token hết hạn

**Fix:**
- Đảm bảo user đã đăng nhập
- Kiểm tra token có trong localStorage không
- Refresh token nếu hết hạn

### Vấn Đề 4: CustomerId Không Tồn Tại

**Triệu chứng:**
- Request POST được gửi nhưng trả về `400 Bad Request`
- Message: `CustomerId X không tồn tại trong hệ thống`

**Nguyên nhân:**
- CustomerId không tồn tại trong database
- CustomerId không đúng format

**Fix:**
- Kiểm tra CustomerId có tồn tại không
- Tạo customer trước khi đặt phòng
- Dùng CustomerId từ JWT token

## 📋 Checklist Debug

- [ ] Mở Browser Console (F12)
- [ ] Click "Đặt phòng" và xem Console logs
- [ ] Kiểm tra Network tab có request POST không
- [ ] Kiểm tra request POST có status code gì
- [ ] Kiểm tra request POST có Authorization header không
- [ ] Kiểm tra logs Railway có request POST không
- [ ] Kiểm tra form có đầy đủ thông tin không

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **API Endpoint:** `https://quanlyresort-production.up.railway.app/api/bookings`

## 💡 Lưu Ý

1. **Form validation** - Kiểm tra form có đầy đủ thông tin không
2. **JavaScript errors** - Xem Browser Console để tìm lỗi
3. **Network requests** - Kiểm tra Network tab để xem request có được gửi không
4. **Authorization** - Đảm bảo token được gửi trong request

## 🎯 Bước Tiếp Theo

1. **Mở Browser Console** - Xem có lỗi JavaScript không
2. **Kiểm tra Network tab** - Xem có request POST không
3. **Kiểm tra logs Railway** - Xem có request POST đến server không
4. **Test với token** - Đảm bảo user đã đăng nhập

