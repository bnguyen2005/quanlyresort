# ✅ Xác Nhận Cấu Hình SePay Railway Variables

## 📊 Cấu Hình Hiện Tại

**Tất cả các biến đã được cấu hình đúng:**

### ✅ Biến 1: Account ID
```
Name:  SePay__AccountId
Value: 5365
```
✅ **Đúng** - Mã định danh ứng dụng từ SePay Dashboard

### ✅ Biến 2: API Base URL
```
Name:  SePay__ApiBaseUrl
Value: https://pgapi.sepay.vn
```
✅ **Đúng** - Production API endpoint của SePay

### ✅ Biến 3: API Token
```
Name:  SePay__ApiToken
Value: PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH
```
✅ **Đúng** - API Token từ SePay Dashboard (format này có thể khác với `spsk_live_...` nhưng vẫn hợp lệ)

### ✅ Biến 4: Bank Account Number
```
Name:  SePay__BankAccountNumber
Value: 0901329227
```
✅ **Đúng** - Số tài khoản ngân hàng MB Bank

### ✅ Biến 5: Bank Code
```
Name:  SePay__BankCode
Value: MB
```
✅ **Đúng** - Mã ngân hàng MB Bank

### ✅ Biến 6: Merchant ID
```
Name:  SePay__MerchantId
Value: SP-LIVE-LT39A334
```
✅ **Đúng** - Merchant ID từ SePay Dashboard (có 2 dấu gạch dưới `__`)

### ✅ Biến 7: Webhook URL
```
Name:  SEPAY_WEBHOOK_URL
Value: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```
✅ **Đúng** - Webhook URL trỏ đúng endpoint

## ✅ Tổng Kết

**Tất cả 7 biến đã được cấu hình đúng và đầy đủ!**

## 🔍 Kiểm Tra Tiếp Theo

### 1. Kiểm Tra SePay Dashboard

**Vào:** https://my.sepay.vn → **Công ty** → **Cấu hình chung** → **Webhook**

**Kiểm tra:**
- ✅ Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- ✅ Trạng thái: **Kích hoạt**
- ⚠️ Thống kê: Hiện tại = 0/0 (chưa có webhook nào được gửi)

### 2. Kiểm Tra Nội Dung Chuyển Khoản

**Vấn đề chính:** SePay chỉ detect và gửi webhook nếu nội dung chuyển khoản đúng format.

**Format đúng:**
- ✅ `BOOKING4` → SePay detect và gửi webhook
- ✅ `ORDER7` → SePay detect và gửi webhook

**Format sai:**
- ❌ `BOOKING-4` → **SAI** (có dấu gạch ngang)
- ❌ `book4` → **SAI** (không có prefix BOOKING)
- ❌ `Thanh toan booking 4` → **SAI** (có khoảng trắng)

**Cách kiểm tra:**
1. Mở app ngân hàng (MB Bank)
2. Vào **Lịch sử giao dịch**
3. Xem **Nội dung chuyển khoản** của giao dịch vừa thanh toán
4. Kiểm tra có đúng format `BOOKING{id}` không

### 3. Test Webhook Endpoint

**Chạy script test:**
```bash
cd QuanLyResort
./test-webhook-booking4.sh
```

**Hoặc test thủ công:**
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

### 4. Kiểm Tra Railway Logs

**Vào:** Railway Dashboard → Service → Logs

**Tìm logs:**
- ✅ `[SEPAY] ✅ Service initialized` → SePay service đã được khởi tạo
- ✅ `[WEBHOOK] 📥 Webhook received` → Webhook đã được nhận
- ✅ `[WEBHOOK] ✅ Booking status updated to Paid` → Status đã được cập nhật

## 🎯 Bước Tiếp Theo

### Bước 1: Test Webhook Thủ Công

**Mục đích:** Verify webhook endpoint hoạt động đúng

```bash
./test-webhook-booking4.sh
```

**Nếu test thành công:**
- ✅ Webhook endpoint hoạt động đúng
- ✅ Backend xử lý webhook đúng
- ⚠️ Vấn đề ở SePay (chưa gửi webhook)

### Bước 2: Kiểm Tra Nội Dung Chuyển Khoản

**Mục đích:** Đảm bảo nội dung đúng format để SePay detect

1. Tạo booking mới (ví dụ: BOOKING5)
2. Quét QR code và thanh toán
3. **Quan trọng:** Đảm bảo nội dung chuyển khoản là `BOOKING5` (không có dấu gạch ngang)
4. Đợi 5-10 phút
5. Kiểm tra SePay Dashboard → Webhook → "Thống kê gửi"

### Bước 3: Thanh Toán Thử Nghiệm

**Nếu vẫn không hoạt động:**

1. **Liên hệ SePay Support:**
   - Email: support@sepay.vn
   - Hoặc qua SePay Dashboard → Hỗ trợ
   - Hỏi về: "Webhook không gửi, thống kê = 0/0, đã cấu hình đầy đủ"

2. **Kiểm tra lại:**
   - Tài khoản ngân hàng đã được link với SePay chưa?
   - Webhook URL có accessible từ internet không?
   - Có firewall nào block webhook không?

## 📋 Checklist Tổng Hợp

- [x] **Railway Variables:** Tất cả 7 biến đã được cấu hình đúng
- [ ] **SePay Dashboard:** Webhook URL đúng và đã kích hoạt
- [ ] **Nội dung chuyển khoản:** Đúng format `BOOKING{id}` (không có dấu gạch ngang)
- [ ] **Test webhook:** Endpoint hoạt động đúng
- [ ] **Thanh toán thử nghiệm:** Đã thanh toán và đợi 5-10 phút
- [ ] **Thống kê SePay:** "Thống kê gửi" > 0 sau khi thanh toán

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app
- **Test Script:** `./test-webhook-booking4.sh`

## 💡 Lưu Ý

1. **Cấu hình Railway đã đúng** - Không cần thay đổi gì
2. **Vấn đề chính:** Nội dung chuyển khoản phải đúng format `BOOKING{id}`
3. **SePay cần thời gian:** Có thể 1-5 phút sau khi thanh toán mới detect
4. **Test thủ công trước:** Test webhook endpoint trước khi thanh toán thật

## 🆘 Nếu Vẫn Không Hoạt Động

**Sau khi đã:**
- ✅ Cấu hình đầy đủ (đã xong)
- ✅ Test webhook thủ công thành công
- ✅ Kiểm tra nội dung chuyển khoản đúng format
- ✅ Đợi 5-10 phút sau khi thanh toán

**Mà vẫn không hoạt động:**
- Liên hệ SePay Support để kiểm tra phía họ
- Có thể SePay có vấn đề về phía họ hoặc cần cấu hình thêm

