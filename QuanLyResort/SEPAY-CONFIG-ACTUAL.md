# 🔧 Cấu Hình SePay Với Thông Tin Thực Tế

## 📋 Thông Tin SePay Từ Dashboard

**Từ SePay Dashboard:**
- **ID:** `5365`
- **Tên:** `ResortDeluxe`
- **API Token:** `PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH`

## 🔧 Cấu Hình Railway Variables

### Bước 1: Vào Railway Dashboard

1. **Mở Railway:** https://railway.app
2. **Chọn project** `quanlyresort`
3. **Vào tab "Variables"**

### Bước 2: Thêm/Cập Nhật Các Biến

#### ✅ Biến 1: CLIENT_ID (ID từ SePay Dashboard)

**Format 1 (Khuyến nghị):**
```
Name:  SEPAY_CLIENT_ID
Value: 5365
```

**Format 2 (Format cũ - vẫn hỗ trợ):**
```
Name:  SePay__AccountId
Value: 5365
```

**Hoặc:**
```
Name:  SePay__ClientId
Value: 5365
```

#### ✅ Biến 2: API_TOKEN (API Token từ SePay Dashboard)

**Format 1 (Khuyến nghị):**
```
Name:  SEPAY_API_KEY
Value: PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```

**Format 2 (Format cũ - vẫn hỗ trợ):**
```
Name:  SePay__ApiToken
Value: PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```

**Lưu ý:** 
- API Token này có format khác với `spsk_live_...` (có thể là format cũ hoặc format khác)
- Code sẽ tự động xử lý cả 2 format

#### ✅ Biến 3: MERCHANT_ID (Nếu có)

**Nếu bạn có MERCHANT_ID từ SePay Dashboard:**
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```

**Hoặc:**
```
Name:  SEPAY_MERCHANT_ID
Value: SP-LIVE-LT39A334
```

#### ✅ Biến 4: SECRET_KEY (Cho webhook verification)

**Nếu bạn có SECRET_KEY từ SePay Dashboard:**
```
Name:  SEPAY_SECRET_KEY
Value: {SECRET_KEY từ SePay Dashboard}
```

**Hoặc:**
```
Name:  SePay__SecretKey
Value: {SECRET_KEY từ SePay Dashboard}
```

#### ✅ Biến 5: WEBHOOK_URL

```
Name:  SEPAY_WEBHOOK_URL
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Hoặc:**
```
Name:  SePay__WebhookUrl
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

#### ✅ Biến 6: BANK_ACCOUNT_NUMBER (Cho static QR code)

```
Name:  SePay__BankAccountNumber
Value: 0901329227
```

#### ✅ Biến 7: BANK_CODE

```
Name:  SePay__BankCode
Value: MB
```

#### ✅ Biến 8: API_BASE_URL

```
Name:  SePay__ApiBaseUrl
Value: https://pgapi.sepay.vn
```

**Hoặc:**
```
Name:  SEPAY_API_BASE_URL
Value: https://pgapi.sepay.vn
```

## 📊 Tóm Tắt Các Biến Cần Cấu Hình

### Bắt Buộc:
- ✅ `SEPAY_CLIENT_ID` = `5365`
- ✅ `SEPAY_API_KEY` = `PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH`
- ✅ `SEPAY_WEBHOOK_URL` = `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### Quan Trọng (Nếu có):
- ⚠️ `SePay__MerchantId` = `SP-LIVE-LT39A334` (nếu có từ SePay Dashboard)
- ⚠️ `SEPAY_SECRET_KEY` = `{SECRET_KEY}` (cho webhook verification)

### Tùy Chọn:
- `SePay__BankAccountNumber` = `0901329227` (cho static QR code)
- `SePay__BankCode` = `MB`
- `SePay__ApiBaseUrl` = `https://pgapi.sepay.vn`

## 🔍 Lưu Ý Về API Token

**API Token bạn cung cấp:**
```
PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```

**Format này khác với:**
- `spsk_live_...` (Production token format mới)
- `spsk_test_...` (Test token format)

**Code sẽ tự động xử lý:**
- Code đã được cập nhật để hỗ trợ cả 2 format
- Authorization header sẽ luôn dùng `Bearer {token}`

## ✅ Checklist Cấu Hình

- [ ] `SEPAY_CLIENT_ID` = `5365` đã được thêm vào Railway
- [ ] `SEPAY_API_KEY` = `PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH` đã được thêm vào Railway
- [ ] `SePay__MerchantId` đã được thêm (nếu có)
- [ ] `SEPAY_SECRET_KEY` đã được thêm (nếu có)
- [ ] `SEPAY_WEBHOOK_URL` đã được thêm
- [ ] `SePay__BankAccountNumber` đã được thêm (cho static QR code)
- [ ] Code đã được deploy với các biến mới
- [ ] SePay webhook đã được setup với URL đúng trong SePay Dashboard

## 🧪 Test Sau Khi Cấu Hình

### Bước 1: Kiểm Tra Logs

**Railway Dashboard → Service → Logs**

**Tìm các dòng:**
- `[SEPAY] 🔍 Client ID configured: 5365`
- `[SEPAY] 🔍 API Key configured: PWGH9OZC...`
- `[SEPAY] 🔍 Merchant ID configured: ...` (nếu có)
- `[SEPAY] 🔄 Thử endpoint: ...`

### Bước 2: Test Tạo QR Code

1. **Vào website:** https://quanlyresort-production.up.railway.app
2. **Đăng nhập** với tài khoản customer
3. **Tạo booking mới**
4. **Click "Thanh toán"**
5. **Kiểm tra logs:**
   - Endpoint nào được thử?
   - Endpoint nào thành công?
   - Có lỗi 404 không?
   - Có lỗi 429 (rate limit) không?

### Bước 3: Test Webhook

1. **Quét QR code** bằng app ngân hàng
2. **Chuyển tiền:**
   - **Nội dung:** `BOOKING{id}` (ví dụ: `BOOKING4`)
   - **Số tiền:** Đúng với booking
3. **Đợi 1-5 phút**
4. **Kiểm tra:**
   - SePay dashboard → Thống kê có tăng không?
   - Railway logs → Có webhook received không?
   - Booking status → Có = "Paid" không?
   - QR code → Có tự động ẩn không?

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Variables:** Railway Dashboard → Variables
- **Website:** https://quanlyresort-production.up.railway.app

## 💡 Lưu Ý

1. **API Token Format:** Token bạn cung cấp có format khác, nhưng code sẽ tự động xử lý
2. **Merchant ID:** Có thể BẮT BUỘC cho Production API, cần kiểm tra SePay Dashboard
3. **Rate Limiting:** Code đã implement rate limiting (2 requests/second)
4. **Multiple Endpoints:** Code sẽ tự động thử nhiều endpoint nếu endpoint đầu tiên không hoạt động
5. **Fallback:** Nếu tất cả endpoints đều thất bại, sẽ fallback sang static QR code

## 🎯 Kết Luận

**Với thông tin bạn cung cấp:**
- ✅ ID: `5365` → Dùng làm `SEPAY_CLIENT_ID`
- ✅ API Token: `PWGH9OZC...` → Dùng làm `SEPAY_API_KEY`
- ✅ Code đã sẵn sàng xử lý format token này
- ✅ Rate limiting đã được implement
- ✅ Multiple endpoint fallback đã được implement

**Bước tiếp theo:**
1. Cấu hình các biến trên Railway
2. Deploy code mới
3. Test tạo QR code
4. Kiểm tra logs để xem endpoint nào hoạt động

