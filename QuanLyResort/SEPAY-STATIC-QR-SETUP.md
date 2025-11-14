# 📸 Cấu Hình SePay Static QR Code (Số Tiền Động)

## 📋 Tổng Quan

SePay hỗ trợ 2 cách tạo QR code:

1. **Dynamic QR Code (API):** Tạo qua SePay API - QR code động hoàn toàn
2. **Static QR Code (URL):** Tạo từ URL SePay - QR code tĩnh nhưng **số tiền vẫn động**

## 🔄 Cách Hoạt Động

### QR Code Tĩnh Nhưng Số Tiền Động

QR code được tạo từ URL:
```
https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4
```

**Đặc điểm:**
- ✅ **Số tiền động:** Tham số `amount` thay đổi theo từng booking/order
- ✅ **Nội dung động:** Tham số `des` thay đổi theo booking ID
- ✅ **Không cần API:** Không cần gọi SePay API
- ✅ **Luôn hoạt động:** Không phụ thuộc vào API status

## 🔧 Cấu Hình Railway Variables

### Biến Cần Thiết

1. **SePay__BankAccountNumber** (Bắt buộc cho static QR):
   ```
   Name:  SePay__BankAccountNumber
   Value: 0901329227
   ```

2. **SePay__BankCode** (Optional - mặc định MB):
   ```
   Name:  SePay__BankCode
   Value: MB
   ```

### Các Biến Khác (Optional - cho API)

3. **SePay__ApiBaseUrl** (Optional):
   ```
   Name:  SePay__ApiBaseUrl
   Value: https://pgapi.sepay.vn
   ```

4. **SePay__ApiToken** (Optional - nếu muốn dùng API):
   ```
   Name:  SePay__ApiToken
   Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
   ```

5. **SePay__AccountId** (Optional - nếu muốn dùng API):
   ```
   Name:  SePay__AccountId
   Value: 5365
   ```

6. **SePay__MerchantId** (Optional - nếu muốn dùng API):
   ```
   Name:  SePay__MerchantId
   Value: SP-LIVE-LT39A334
   ```

## 🎯 Cách Hoạt Động

### Ưu Tiên 1: SePay API (Nếu Có)

1. **Kiểm tra có API credentials:**
   - Có `SePay__ApiToken` và `SePay__AccountId`?
   - → Gọi SePay API để tạo QR code động

2. **Nếu API thành công:**
   - Trả về QR code từ API response
   - QR code động hoàn toàn

### Ưu Tiên 2: Static QR Code (Fallback)

1. **Nếu API không hoạt động hoặc chưa cấu hình:**
   - Tạo QR code từ URL SePay
   - URL có tham số `amount` động theo booking/order

2. **Format URL:**
   ```
   https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING4
   ```

3. **QR code vẫn động về số tiền:**
   - Booking 1: `amount=5000` → QR code cho 5,000 VND
   - Booking 2: `amount=10000` → QR code cho 10,000 VND
   - Booking 3: `amount=15000` → QR code cho 15,000 VND

## 📝 Ví Dụ URL QR Code

### Booking 1 (5,000 VND):
```
https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=5000&des=BOOKING1
```

### Booking 2 (10,000 VND):
```
https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=10000&des=BOOKING2
```

### Restaurant Order 1 (50,000 VND):
```
https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=50000&des=ORDER1
```

## ✅ Checklist Cấu Hình Tối Thiểu

**Để QR code hoạt động (static fallback):**

- [ ] Đã thêm `SePay__BankAccountNumber` = `0901329227`
- [ ] Đã thêm `SePay__BankCode` = `MB` (optional, default: MB)

**Để QR code động hoàn toàn (API):**

- [ ] Đã thêm `SePay__ApiBaseUrl` = `https://pgapi.sepay.vn`
- [ ] Đã thêm `SePay__ApiToken` = `spsk_live_...`
- [ ] Đã thêm `SePay__AccountId` = `5365`
- [ ] Đã thêm `SePay__MerchantId` = `SP-LIVE-LT39A334`

## 🧪 Test

1. **Cấu hình tối thiểu:** Chỉ cần `SePay__BankAccountNumber`
2. **Tạo booking mới** → Click "Thanh toán"
3. **Kiểm tra QR code hiển thị:**
   - QR code URL: `https://qr.sepay.vn/img?acc=0901329227&bank=MB&amount=...`
   - Số tiền trong URL phải khớp với booking amount

## 📝 Lưu Ý

1. **QR code tĩnh nhưng số tiền động:**
   - URL thay đổi theo `amount` và `des`
   - Mỗi booking/order có QR code riêng với số tiền riêng

2. **Fallback tự động:**
   - Nếu API không hoạt động → Tự động dùng static QR
   - Nếu API hoạt động → Dùng API QR (ưu tiên)

3. **Bank Account Number:**
   - Phải là số tài khoản ngân hàng thực tế
   - Ví dụ: `0901329227` (MB Bank)

## 🎯 Kết Quả

Sau khi cấu hình:
- ✅ QR code hiển thị với số tiền đúng theo booking/order
- ✅ Mỗi booking/order có QR code riêng
- ✅ Số tiền trong QR code thay đổi theo từng phòng/order
- ✅ Hoạt động ngay cả khi API không hoạt động

