# 🔧 Cấu Hình SePay Trong Railway - Thông Tin Cụ Thể

## 📋 Thông Tin SePay Của Bạn

- **Account ID:** `5365`
- **Tên:** `ResortDeluxe`
- **API Token:** `PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH`

## 🚀 Các Bước Cấu Hình Trong Railway

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Thêm Các Biến Môi Trường

**Click "New Variable" và thêm từng biến sau:**

#### 1. SePay API Token
```
Name: SePay__ApiToken
Value: PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```

#### 2. SePay Account ID
```
Name: SePay__AccountId
Value: 5365
```

#### 3. SePay Bank Code (Optional - mặc định MB)
```
Name: SePay__BankCode
Value: MB
```

#### 4. SePay API Base URL (Optional - mặc định)
```
Name: SePay__ApiBaseUrl
Value: https://my.sepay.vn/userapi
```

### Bước 3: Kiểm Tra Các Biến Đã Thêm

Sau khi thêm, bạn sẽ thấy trong danh sách Variables:

```
SePay__ApiToken = PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
SePay__AccountId = 5365
SePay__BankCode = MB
SePay__ApiBaseUrl = https://my.sepay.vn/userapi
```

### Bước 4: Redeploy Service

1. **Vào tab "Deployments"**
2. **Click "Redeploy"** hoặc đợi Railway tự động redeploy
3. **Đợi deploy xong** (2-3 phút)

### Bước 5: Kiểm Tra Logs

1. **Vào tab "Logs"**
2. **Tìm dòng log:**
   ```
   [SEPAY] ✅ Service initialized with ApiToken: PWGH9OZC...
   ```
3. **Nếu thấy warning:**
   ```
   [SEPAY] ⚠️ SePay API Token chưa được cấu hình
   ```
   → Kiểm tra lại tên biến (phải có `__` giữa `SePay` và `ApiToken`)

## ✅ Checklist

- [ ] Đã thêm `SePay__ApiToken` = `PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH`
- [ ] Đã thêm `SePay__AccountId` = `5365`
- [ ] Đã thêm `SePay__BankCode` = `MB` (optional)
- [ ] Đã thêm `SePay__ApiBaseUrl` = `https://my.sepay.vn/userapi` (optional)
- [ ] Railway đã redeploy thành công
- [ ] Không còn warning trong logs về SePay configuration

## 🧪 Test Sau Khi Cấu Hình

### Test 1: Tạo QR Code Cho Booking

1. **Tạo booking mới** trong hệ thống
2. **Click "Thanh toán"**
3. **Kiểm tra QR code hiển thị**
4. **Kiểm tra console log:**
   ```
   [FRONTEND] ✅ [updatePaymentModal] SePay QR code created
   ```

### Test 2: Tạo QR Code Cho Restaurant Order

1. **Tạo restaurant order mới**
2. **Click "Thanh toán"**
3. **Kiểm tra QR code hiển thị**
4. **Kiểm tra console log:**
   ```
   [FRONTEND] ✅ [updateRestaurantPaymentModal] SePay QR code created
   ```

## 🐛 Troubleshooting

### Lỗi: "SePay service chưa được cấu hình"

**Nguyên nhân:**
- Tên biến không đúng (thiếu `__`)
- Giá trị có khoảng trắng ở đầu/cuối

**Giải pháp:**
1. Kiểm tra tên biến: `SePay__ApiToken` (không phải `SePay_ApiToken`)
2. Copy chính xác giá trị, không có khoảng trắng
3. Redeploy service

### Lỗi: "SePay API error: Status=401"

**Nguyên nhân:**
- API Token không đúng

**Giải pháp:**
1. Kiểm tra lại API Token trong SePay Dashboard
2. Đảm bảo copy đầy đủ token (không bị cắt)
3. Update `SePay__ApiToken` và redeploy

### Lỗi: "SePay API error: Status=404"

**Nguyên nhân:**
- Account ID không đúng
- Bank Code không đúng

**Giải pháp:**
1. Kiểm tra Account ID: `5365`
2. Kiểm tra Bank Code: `MB` (hoặc ngân hàng khác nếu bạn dùng)
3. Update và redeploy

## 📝 Lưu Ý Quan Trọng

1. **Tên biến:** Phải dùng `__` (2 dấu gạch dưới) giữa `SePay` và tên field
2. **API Token:** Bảo mật, không chia sẻ công khai
3. **Account ID:** Là số `5365`, không phải tên `ResortDeluxe`
4. **Bank Code:** Mặc định `MB`, có thể đổi nếu dùng ngân hàng khác

## 🎯 Kết Quả Mong Đợi

Sau khi cấu hình thành công:
- ✅ Có thể tạo QR code động cho booking
- ✅ Có thể tạo QR code động cho restaurant order
- ✅ QR code chứa sẵn số tiền và nội dung chuyển khoản
- ✅ Webhook tự động nhận thông báo thanh toán
- ✅ Trạng thái booking/order tự động cập nhật khi thanh toán thành công

