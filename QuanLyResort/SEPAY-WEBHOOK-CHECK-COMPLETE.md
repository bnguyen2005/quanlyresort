# ✅ Kiểm Tra & Cấu Hình SePay Webhook Hoàn Chỉnh

## 🎯 Mục Tiêu

**Đảm bảo SePay webhook hoạt động để tự động cập nhật booking status khi thanh toán thành công.**

## 📋 Checklist Kiểm Tra

### ✅ Bước 1: Kiểm Tra SePay Dashboard

**1.1. Đăng Nhập SePay Dashboard:**
- **URL:** https://my.sepay.vn
- **Đăng nhập** với tài khoản của bạn

**1.2. Kiểm Tra Webhook Configuration:**
- **Vào:** **Công ty** → **Cấu hình chung** → **Webhook**
- **Kiểm tra:**
  - ✅ **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
  - ✅ **Trạng thái:** **Đã kích hoạt** (Active)
  - ✅ **Thống kê gửi:** Nếu thấy số > 0 → Webhook đã được gửi
  - ✅ **Thống kê thành công:** Nếu thấy số > 0 → Webhook đã được nhận thành công

**1.3. Kiểm Tra Tài Khoản Ngân Hàng:**
- **Vào:** **Tài khoản** → **Danh sách tài khoản**
- **Kiểm tra:**
  - ✅ Tài khoản `0901329227` (MB Bank) đã được link với SePay
  - ✅ Trạng thái: **Đã kích hoạt**

### ✅ Bước 2: Kiểm Tra Railway Variables

**2.1. Vào Railway Dashboard:**
- **URL:** https://railway.app
- **Chọn project:** `quanlyresort`
- **Vào tab:** **Variables**

**2.2. Kiểm Tra Các Biến Bắt Buộc:**

#### ✅ Biến 1: CLIENT_ID (Account ID)
```
Name:  SePay__AccountId
Value: 5365
```
**Hoặc:**
```
Name:  SEPAY_CLIENT_ID
Value: 5365
```

#### ✅ Biến 2: API_TOKEN (API Key)
```
Name:  SePay__ApiToken
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```
**Hoặc:**
```
Name:  SEPAY_API_KEY
Value: spsk_live_eofJdy5CA7gcyDAVe9xev5HhrZvFcGGb
```

#### ✅ Biến 3: MERCHANT_ID (Nếu có)
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```
**Lưu ý:** Phải có **2 dấu gạch dưới** (`__`)!

#### ✅ Biến 4: BANK_ACCOUNT_NUMBER
```
Name:  SePay__BankAccountNumber
Value: 0901329227
```

#### ✅ Biến 5: BANK_CODE
```
Name:  SePay__BankCode
Value: MB
```

#### ✅ Biến 6: API_BASE_URL (Optional)
```
Name:  SePay__ApiBaseUrl
Value: https://pgapi.sepay.vn
```

### ✅ Bước 3: Test Webhook Endpoint

**3.1. Test Webhook Thủ Công:**

Chạy script test:
```bash
cd QuanLyResort
./test-webhook-booking4.sh
```

Hoặc test thủ công:
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "in",
    "id": "TEST-123",
    "gateway": "MB",
    "accountNumber": "0901329227"
  }'
```

**Kết quả mong đợi:**
- ✅ HTTP Status: `201`
- ✅ Response: `{"success": true, ...}`
- ✅ Railway logs: `[WEBHOOK] ✅ Booking status updated to Paid`

**3.2. Kiểm Tra Booking Status Sau Test:**

```bash
curl -X GET "https://quanlyresort-production.up.railway.app/api/bookings/4" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kết quả mong đợi:**
- ✅ `"status": "Paid"`

### ✅ Bước 4: Kiểm Tra Nội Dung Chuyển Khoản

**4.1. Format Đúng:**
- ✅ `BOOKING4` → Backend extract booking ID = 4
- ✅ `ORDER7` → Backend extract order ID = 7

**4.2. Format Sai (Sẽ Không Hoạt Động):**
- ❌ `BOOKING-4` → **SAI** (có dấu gạch ngang)
- ❌ `book4` → **SAI** (không có prefix BOOKING)
- ❌ `Thanh toan booking 4` → **SAI** (có khoảng trắng)

**4.3. Cách Kiểm Tra:**
1. Mở app ngân hàng (MB Bank)
2. Vào **Lịch sử giao dịch**
3. Xem **Nội dung chuyển khoản**
4. Kiểm tra có đúng format `BOOKING{id}` không

### ✅ Bước 5: Kiểm Tra Railway Logs

**5.1. Vào Railway Logs:**
- **Railway Dashboard** → **Service** → **Logs**

**5.2. Tìm Logs Webhook:**
- ✅ `[WEBHOOK] 📥 Webhook received` → Webhook đã được nhận
- ✅ `[WEBHOOK] ✅ Booking status updated to Paid` → Status đã được cập nhật
- ❌ `[WEBHOOK] ⚠️ Booking not found` → Booking ID không đúng
- ❌ `[WEBHOOK] ⚠️ Amount mismatch` → Số tiền không khớp

## 🔧 Cấu Hình Lại SePay Webhook (Nếu Cần)

### Bước 1: Vào SePay Dashboard

1. **Đăng nhập:** https://my.sepay.vn
2. **Vào:** **Công ty** → **Cấu hình chung** → **Webhook**

### Bước 2: Cập Nhật Webhook URL

**Webhook URL:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Lưu ý:**
- ✅ URL phải bắt đầu bằng `https://`
- ✅ URL phải trỏ đúng endpoint `/api/simplepayment/webhook`
- ✅ Không có dấu `/` ở cuối

### Bước 3: Kích Hoạt Webhook

1. **Chọn:** **Đã kích hoạt** (Active)
2. **Chọn phương thức xác thực:** **Không cần chứng thực** (hoặc **API Key** nếu có)
3. **Lưu** cấu hình

### Bước 4: Test Webhook

1. **SePay Dashboard** → **Webhook** → **Test Webhook**
2. **Gửi test webhook** với nội dung: `BOOKING4`
3. **Kiểm tra Railway logs** xem có nhận được không

## 🐛 Troubleshooting

### Vấn Đề 1: Webhook Không Được Gửi

**Nguyên nhân:**
- SePay chưa detect thanh toán
- Tài khoản ngân hàng chưa được link với SePay
- Nội dung chuyển khoản không đúng format

**Giải pháp:**
1. Kiểm tra tài khoản ngân hàng đã được link với SePay chưa
2. Kiểm tra nội dung chuyển khoản có đúng format `BOOKING{id}` không
3. Đợi 1-5 phút sau khi thanh toán (SePay cần thời gian để detect)

### Vấn Đề 2: Webhook Được Gửi Nhưng Không Cập Nhật Status

**Nguyên nhân:**
- Booking ID không đúng
- Số tiền không khớp
- Backend lỗi khi xử lý webhook

**Giải pháp:**
1. Kiểm tra Railway logs để xem lỗi cụ thể
2. Test webhook thủ công với booking ID đúng
3. Kiểm tra số tiền có khớp với booking amount không

### Vấn Đề 3: "Thống kê gửi" = 0/0

**Nguyên nhân:**
- SePay chưa gửi webhook nào
- Webhook chưa được kích hoạt

**Giải pháp:**
1. Kiểm tra webhook đã được kích hoạt chưa
2. Kiểm tra tài khoản ngân hàng đã được link chưa
3. Thử thanh toán lại với nội dung đúng format

## 📊 Kiểm Tra Trạng Thái Webhook

### SePay Dashboard

**Vào:** **Công ty** → **Cấu hình chung** → **Webhook**

**Thông tin cần kiểm tra:**
- ✅ **Webhook URL:** Đúng URL
- ✅ **Trạng thái:** Đã kích hoạt
- ✅ **Thống kê gửi:** Số webhook đã gửi
- ✅ **Thống kê thành công:** Số webhook thành công

### Railway Logs

**Tìm logs:**
```
[WEBHOOK] 📥 Webhook received
[WEBHOOK] ✅ Booking status updated to Paid
```

**Nếu không thấy logs:**
- Webhook chưa được gửi từ SePay
- Hoặc webhook bị lỗi khi gửi

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app
- **Test Script:** `./test-webhook-booking4.sh`

## 💡 Lưu Ý Quan Trọng

1. **VietQR không có webhook** - Chỉ tạo QR code, không detect thanh toán
2. **SePay webhook cần thời gian** - Có thể 1-5 phút sau khi thanh toán
3. **Nội dung chuyển khoản quan trọng** - Phải đúng format `BOOKING{id}`
4. **Webhook URL phải đúng** - Phải trỏ đúng endpoint `/api/simplepayment/webhook`
5. **Tài khoản ngân hàng phải được link** - SePay chỉ detect nếu tài khoản đã được link

## ✅ Checklist Tổng Hợp

- [ ] SePay Dashboard: Webhook URL đúng
- [ ] SePay Dashboard: Webhook đã kích hoạt
- [ ] SePay Dashboard: Tài khoản ngân hàng đã được link
- [ ] Railway Variables: Tất cả biến đã được cấu hình
- [ ] Test Webhook: Endpoint hoạt động đúng
- [ ] Nội dung chuyển khoản: Đúng format `BOOKING{id}`
- [ ] Railway Logs: Có logs webhook được nhận
- [ ] Booking Status: Được cập nhật thành "Paid" sau khi thanh toán

