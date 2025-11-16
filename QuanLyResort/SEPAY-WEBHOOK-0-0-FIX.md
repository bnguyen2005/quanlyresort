# 🔧 Fix: SePay Webhook "Thống kê gửi" = 0/0

## 📊 Tình Trạng Hiện Tại

**Webhook của bạn:**
- ✅ **Trạng thái:** Kích hoạt
- ✅ **Tài khoản:** 0901329227 (MBBank)
- ✅ **Loại:** Xác thực thanh toán (Tiền vào và Tiền ra)
- ❌ **Thống kê:** Hôm nay: 0/0, Tổng: 0/0

**Vấn đề:** SePay chưa gửi webhook nào → Backend không nhận được thông báo thanh toán.

## 🔍 Nguyên Nhân Có Thể

### 1. SePay Chưa Detect Thanh Toán

**Nguyên nhân:**
- SePay cần thời gian để detect thanh toán (1-5 phút)
- SePay chỉ detect nếu nội dung chuyển khoản đúng format
- SePay chỉ detect nếu tài khoản đã được link đúng

**Kiểm tra:**
1. ✅ Tài khoản `0901329227` đã được link với SePay (đã có trong webhook config)
2. ⚠️ Kiểm tra nội dung chuyển khoản có đúng format không

### 2. Nội Dung Chuyển Khoản Không Đúng Format

**Format đúng:**
- ✅ `BOOKING4` → SePay detect và gửi webhook
- ✅ `ORDER7` → SePay detect và gửi webhook

**Format sai (SePay không detect):**
- ❌ `BOOKING-4` → **SAI** (có dấu gạch ngang)
- ❌ `book4` → **SAI** (không có prefix BOOKING)
- ❌ `Thanh toan booking 4` → **SAI** (có khoảng trắng)
- ❌ `Chuyen tien` → **SAI** (không có booking ID)

**Cách kiểm tra:**
1. Mở app ngân hàng (MB Bank)
2. Vào **Lịch sử giao dịch**
3. Xem **Nội dung chuyển khoản** của giao dịch vừa thanh toán
4. Kiểm tra có đúng format `BOOKING{id}` không

### 3. Webhook URL Chưa Đúng

**Kiểm tra Webhook URL trong SePay Dashboard:**

**URL đúng:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**URL sai (sẽ không hoạt động):**
- ❌ `http://...` (không có SSL)
- ❌ `.../webhook/` (có dấu `/` ở cuối)
- ❌ `.../api/simplepayment` (thiếu `/webhook`)

**Cách kiểm tra:**
1. Vào SePay Dashboard: https://my.sepay.vn
2. Vào: **Công ty** → **Cấu hình chung** → **Webhook**
3. Kiểm tra **Webhook URL** có đúng không

## 🔧 Giải Pháp

### Bước 1: Kiểm Tra Webhook URL

**1.1. Vào SePay Dashboard:**
- **URL:** https://my.sepay.vn
- **Vào:** **Công ty** → **Cấu hình chung** → **Webhook**

**1.2. Kiểm Tra Webhook URL:**
- **URL phải là:**
  ```
  https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
  ```
- **Nếu sai, sửa lại và lưu**

**1.3. Test Webhook URL:**
- SePay Dashboard có nút **"Test Webhook"** hoặc **"Gửi test"**
- Click để test xem webhook có hoạt động không
- Kiểm tra Railway logs xem có nhận được không

### Bước 2: Kiểm Tra Nội Dung Chuyển Khoản

**2.1. Format Đúng:**
- Khi tạo QR code, nội dung phải là: `BOOKING{id}`
- Ví dụ: `BOOKING4`, `BOOKING5`, `BOOKING6`

**2.2. Kiểm Tra QR Code:**
- QR code được tạo từ VietQR hoặc SePay
- Nội dung trong QR code phải là: `BOOKING{id}` (không có dấu gạch ngang)

**2.3. Kiểm Tra Giao Dịch:**
- Sau khi thanh toán, mở app ngân hàng
- Xem lịch sử giao dịch
- Kiểm tra nội dung chuyển khoản có đúng format không

### Bước 3: Test Webhook Thủ Công

**3.1. Test Webhook Endpoint:**

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

**3.2. Kiểm Tra Booking Status:**
- Sau khi test, kiểm tra booking status có được cập nhật thành "Paid" không

### Bước 4: Thanh Toán Thử Nghiệm

**4.1. Tạo Booking Mới:**
- Tạo booking mới trên website
- Lưu booking ID (ví dụ: `BOOKING5`)

**4.2. Thanh Toán:**
- Quét QR code
- Chuyển khoản với nội dung: `BOOKING5` (không có dấu gạch ngang)
- Đợi 1-5 phút

**4.3. Kiểm Tra:**
- Vào SePay Dashboard → Webhook → Xem "Thống kê gửi"
- Nếu thấy số > 0 → Webhook đã được gửi
- Kiểm tra Railway logs xem có nhận được không

## 🐛 Troubleshooting

### Vấn Đề 1: "Thống kê gửi" Vẫn = 0/0 Sau Khi Thanh Toán

**Nguyên nhân:**
- Nội dung chuyển khoản không đúng format
- SePay chưa detect thanh toán (cần thời gian)
- Tài khoản ngân hàng chưa được link đúng

**Giải pháp:**
1. Kiểm tra nội dung chuyển khoản có đúng format `BOOKING{id}` không
2. Đợi 5-10 phút sau khi thanh toán (SePay cần thời gian)
3. Kiểm tra tài khoản ngân hàng đã được link với SePay chưa

### Vấn Đề 2: Webhook URL Đúng Nhưng Vẫn Không Hoạt Động

**Nguyên nhân:**
- Railway endpoint không accessible
- Backend lỗi khi nhận webhook

**Giải pháp:**
1. Test webhook thủ công (xem Bước 3)
2. Kiểm tra Railway logs xem có lỗi không
3. Kiểm tra Railway service có đang chạy không

### Vấn Đề 3: SePay Detect Nhưng Không Gửi Webhook

**Nguyên nhân:**
- Webhook chưa được kích hoạt (nhưng bạn đã kích hoạt rồi)
- Webhook URL sai
- SePay có vấn đề về phía họ

**Giải pháp:**
1. Kiểm tra lại webhook URL trong SePay Dashboard
2. Thử disable và enable lại webhook
3. Liên hệ SePay support nếu vẫn không hoạt động

## 📋 Checklist

- [ ] **Webhook URL đúng:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] **Webhook đã kích hoạt:** Trạng thái = "Kích hoạt"
- [ ] **Tài khoản đã link:** 0901329227 đã được link với SePay
- [ ] **Nội dung chuyển khoản:** Đúng format `BOOKING{id}` (không có dấu gạch ngang)
- [ ] **Test webhook:** Endpoint hoạt động đúng
- [ ] **Thanh toán thử nghiệm:** Đã thanh toán và đợi 5-10 phút
- [ ] **Kiểm tra thống kê:** "Thống kê gửi" > 0 sau khi thanh toán

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app
- **Test Script:** `./test-webhook-booking4.sh`

## 💡 Lưu Ý Quan Trọng

1. **SePay cần thời gian:** Có thể 1-5 phút sau khi thanh toán mới detect
2. **Nội dung chuyển khoản quan trọng:** Phải đúng format `BOOKING{id}` (không có dấu gạch ngang)
3. **Webhook URL phải đúng:** Phải trỏ đúng endpoint `/api/simplepayment/webhook`
4. **Test thủ công trước:** Test webhook endpoint trước khi thanh toán thật
5. **Kiểm tra logs:** Luôn kiểm tra Railway logs để xem webhook có được nhận không

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Liên hệ SePay Support:**
   - Email: support@sepay.vn
   - Hoặc qua SePay Dashboard → Hỗ trợ
   - Hỏi về: "Webhook không gửi, thống kê = 0/0"

2. **Kiểm tra Railway:**
   - Railway logs có lỗi không?
   - Webhook endpoint có accessible không?

3. **Test thủ công:**
   - Test webhook thủ công với script
   - Nếu test thành công → Vấn đề ở SePay
   - Nếu test thất bại → Vấn đề ở backend

