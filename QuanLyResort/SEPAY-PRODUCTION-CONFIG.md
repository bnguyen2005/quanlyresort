# 🚀 Cấu Hình SePay Production Trong Railway

## 📋 Thông Tin SePay Production

Bạn đã khởi tạo môi trường **production** thành công! Đây là thông tin credentials:

- **MERCHANT ID:** `SP-LIVE-LT39A334`
- **SECRET KEY:** `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`

⚠️ **QUAN TRỌNG:** Đây là thông tin production, hãy lưu trữ an toàn và không chia sẻ công khai!

## 🔧 Cấu Hình Trong Railway

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Cập Nhật/Cập Nhật Các Biến Môi Trường

**Xóa hoặc cập nhật các biến test cũ (nếu có), sau đó thêm các biến production:**

#### 1. SePay API Token (SECRET KEY)
```
Name: SePay__ApiToken
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

#### 2. SePay Account ID (MERCHANT ID)
```
Name: SePay__AccountId
Value: SP-LIVE-LT39A334
```

#### 3. SePay Bank Code (Optional - mặc định MB)
```
Name: SePay__BankCode
Value: MB
```

#### 4. SePay API Base URL (Optional - production URL)
```
Name: SePay__ApiBaseUrl
Value: https://my.sepay.vn/userapi
```

### Bước 3: Kiểm Tra Các Biến Đã Thêm

Sau khi thêm, bạn sẽ thấy trong danh sách Variables:

```
SePay__ApiToken = spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
SePay__AccountId = SP-LIVE-LT39A334
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
   [SEPAY] ✅ Service initialized with ApiToken: spsk_live...
   ```
3. **Nếu thấy warning:**
   ```
   [SEPAY] ⚠️ SePay API Token chưa được cấu hình
   ```
   → Kiểm tra lại tên biến (phải có `__` giữa `SePay` và `ApiToken`)

## ✅ Checklist Cấu Hình Production

- [ ] Đã xóa/cập nhật các biến test cũ
- [ ] Đã thêm `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`
- [ ] Đã thêm `SePay__AccountId` = `SP-LIVE-LT39A334`
- [ ] Đã thêm `SePay__BankCode` = `MB` (optional)
- [ ] Đã thêm `SePay__ApiBaseUrl` = `https://my.sepay.vn/userapi` (optional)
- [ ] Railway đã redeploy thành công
- [ ] Không còn warning trong logs về SePay configuration
- [ ] Đã test tạo QR code cho booking
- [ ] Đã test tạo QR code cho restaurant order

## 🧪 Test Production

### Test 1: Tạo QR Code Cho Booking (Production)

1. **Tạo booking mới** trong hệ thống
2. **Click "Thanh toán"**
3. **Kiểm tra QR code hiển thị** (phải là QR code production, không phải test)
4. **Kiểm tra console log:**
   ```
   [FRONTEND] ✅ [updatePaymentModal] SePay QR code created
   [SEPAY] ✅ Đơn hàng tạo thành công: OrderId=..., OrderCode=BOOKING...
   ```

### Test 2: Tạo QR Code Cho Restaurant Order (Production)

1. **Tạo restaurant order mới**
2. **Click "Thanh toán"**
3. **Kiểm tra QR code hiển thị**
4. **Kiểm tra console log:**
   ```
   [FRONTEND] ✅ [updateRestaurantPaymentModal] SePay QR code created
   [SEPAY] ✅ Đơn hàng tạo thành công: OrderId=..., OrderCode=ORDER...
   ```

### Test 3: Thanh Toán Thật (Production)

1. **Quét QR code** bằng app ngân hàng
2. **Thanh toán số tiền nhỏ** để test (ví dụ: 10,000 VND)
3. **Kiểm tra webhook** nhận được từ SePay
4. **Kiểm tra trạng thái booking/order** tự động cập nhật thành "Paid"

## 🔒 Bảo Mật Production Credentials

### ⚠️ QUAN TRỌNG:

1. **Không commit credentials vào git**
   - Đã có `.gitignore` để bỏ qua `appsettings.Production.json`
   - Chỉ dùng Railway Variables

2. **Không chia sẻ credentials công khai**
   - MERCHANT ID và SECRET KEY là thông tin nhạy cảm
   - Chỉ chia sẻ với team member cần thiết

3. **Rotate credentials định kỳ**
   - Nếu nghi ngờ bị lộ, tạo credentials mới ngay lập tức
   - Update trong Railway Variables

4. **Monitor logs**
   - Kiểm tra logs thường xuyên để phát hiện lỗi bất thường
   - Nếu thấy nhiều request 401/403, có thể credentials bị lộ

## 🐛 Troubleshooting Production

### Lỗi: "SePay API error: Status=401"

**Nguyên nhân:**
- SECRET KEY không đúng
- Credentials test đang được dùng thay vì production

**Giải pháp:**
1. Kiểm tra `SePay__ApiToken` = `spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb`
2. Đảm bảo không có biến test cũ
3. Redeploy service

### Lỗi: "SePay API error: Status=404"

**Nguyên nhân:**
- MERCHANT ID không đúng
- Account ID test đang được dùng

**Giải pháp:**
1. Kiểm tra `SePay__AccountId` = `SP-LIVE-LT39A334`
2. Đảm bảo không có biến test cũ
3. Redeploy service

### Lỗi: "QR code không hiển thị"

**Nguyên nhân:**
- API trả về lỗi nhưng không log rõ
- Credentials chưa được cập nhật

**Giải pháp:**
1. Kiểm tra logs trong Railway
2. Kiểm tra console browser (F12)
3. Đảm bảo tất cả biến đã được set đúng

## 📝 Lưu Ý Quan Trọng

1. **MERCHANT ID:** `SP-LIVE-LT39A334` (có prefix `SP-LIVE-`)
2. **SECRET KEY:** `spsk_live_...` (có prefix `spsk_live_`)
3. **Tên biến:** Phải dùng `__` (2 dấu gạch dưới): `SePay__ApiToken`
4. **Production vs Test:** 
   - Production: `SP-LIVE-...`, `spsk_live_...`
   - Test: `SP-TEST-...`, `spsk_test_...`

## 🎯 Kết Quả Mong Đợi

Sau khi cấu hình production thành công:
- ✅ Có thể tạo QR code động cho booking (production)
- ✅ Có thể tạo QR code động cho restaurant order (production)
- ✅ QR code chứa sẵn số tiền và nội dung chuyển khoản
- ✅ Webhook tự động nhận thông báo thanh toán thật
- ✅ Trạng thái booking/order tự động cập nhật khi thanh toán thành công
- ✅ **Bắt đầu nhận thanh toán thật từ khách hàng!** 🎉

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Hướng dẫn chi tiết:** Xem file `SEPAY-API-SETUP.md`

