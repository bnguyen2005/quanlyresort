# 🔍 Debug: Đã Thanh Toán Nhưng Không Cập Nhật Trạng Thái

## ❓ Vấn Đề

**Đã thanh toán thành công nhưng booking status không được cập nhật thành "Paid".**

## 🔍 Phân Tích Từ Logs

Từ Railway logs, tôi thấy:
- ✅ Frontend đang polling liên tục: `GET /api/bookings/4`
- ❌ **KHÔNG có webhook nào được nhận** từ SePay
- ❌ Booking status vẫn chưa được cập nhật

## 🎯 Nguyên Nhân Có Thể

### 1. SePay Webhook Không Được Gửi

**Vấn đề:**
- VietQR **KHÔNG có webhook** tự động
- Chỉ SePay mới có webhook
- Nếu SePay không detect thanh toán → Không gửi webhook

**Kiểm tra:**
1. ✅ SePay account đã link với tài khoản ngân hàng chưa?
2. ✅ SePay webhook đã được setup trong SePay Dashboard chưa?
3. ✅ Webhook URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### 2. Nội Dung Chuyển Khoản Không Đúng Format

**Format đúng:**
- ✅ `BOOKING4` → Backend extract booking ID = 4
- ❌ `BOOKING-4` → **SAI** (có dấu gạch ngang)
- ❌ `book4` → **SAI** (không có prefix BOOKING)
- ❌ `Thanh toan booking 4` → **SAI** (có khoảng trắng)

**Kiểm tra:**
1. Mở app ngân hàng
2. Xem lịch sử giao dịch
3. Kiểm tra nội dung chuyển khoản có đúng format `BOOKING4` không

### 3. SePay Chưa Detect Thanh Toán

**Vấn đề:**
- SePay cần thời gian để detect thanh toán (có thể 1-5 phút)
- SePay chỉ detect nếu tài khoản đã được link

**Kiểm tra:**
1. Đăng nhập SePay Dashboard: https://my.sepay.vn
2. Xem "Thống kê gửi" trong webhook settings
3. Nếu thấy `0/0` → SePay chưa gửi webhook

## 🧪 Test Webhook Thủ Công

### Bước 1: Test Webhook Endpoint

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
    "id": "TEST-'$(date +%s)'",
    "gateway": "MB",
    "accountNumber": "0901329227"
  }'
```

**Kết quả mong đợi:**
- ✅ HTTP Status: `201`
- ✅ Response: `{"success": true, ...}`
- ✅ Railway logs: `[WEBHOOK] ✅ Booking status updated to Paid`

### Bước 2: Kiểm Tra Booking Status

Sau khi test webhook, kiểm tra booking:

```bash
curl -X GET "https://quanlyresort-production.up.railway.app/api/bookings/4" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Kết quả mong đợi:**
- ✅ `"status": "Paid"`

## 🔧 Giải Pháp

### Giải Pháp 1: Cấu Hình SePay Webhook (Tự Động)

**1. Kiểm tra SePay Dashboard:**
- Đăng nhập: https://my.sepay.vn
- Vào: **Công ty** → **Cấu hình chung** → **Webhook**
- Kiểm tra:
  - ✅ Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
  - ✅ Trạng thái: **Đã kích hoạt**
  - ✅ "Thống kê gửi": Nếu thấy số > 0 → Webhook đã được gửi

**2. Kiểm tra Nội Dung Chuyển Khoản:**
- Format: `BOOKING4` (không có dấu gạch ngang, không có khoảng trắng)
- Khi quét QR, app ngân hàng sẽ tự động điền nội dung

**3. Đợi SePay Detect:**
- SePay cần 1-5 phút để detect thanh toán
- Sau khi detect, SePay sẽ gửi webhook → Backend cập nhật status

### Giải Pháp 2: Cập Nhật Thủ Công (Tạm Thời)

Nếu webhook không hoạt động, có thể cập nhật thủ công:

**1. Qua Website:**
- Đăng nhập admin
- Vào booking details
- Cập nhật status = "Paid"

**2. Qua API:**
```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/bookings/4/pay-online" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

### Giải Pháp 3: Sử Dụng Frontend Polling (Fallback)

Frontend đang polling mỗi 3 giây để check booking status. Nếu webhook không hoạt động:
- Frontend sẽ không tự động detect payment
- Cần refresh trang hoặc đợi polling detect (nếu admin cập nhật thủ công)

## 📋 Checklist Debug

- [ ] **1. Kiểm tra SePay Webhook Configuration**
  - [ ] Webhook URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
  - [ ] Trạng thái: **Đã kích hoạt**
  - [ ] "Thống kê gửi" > 0 (nếu có)

- [ ] **2. Kiểm tra Nội Dung Chuyển Khoản**
  - [ ] Format: `BOOKING4` (không có dấu gạch ngang)
  - [ ] Không có khoảng trắng
  - [ ] Không có ký tự đặc biệt

- [ ] **3. Test Webhook Thủ Công**
  - [ ] Chạy script `test-webhook-booking4.sh`
  - [ ] Kiểm tra Railway logs
  - [ ] Kiểm tra booking status sau khi test

- [ ] **4. Kiểm tra Railway Logs**
  - [ ] Có log `[WEBHOOK] 📥 Webhook received` không?
  - [ ] Có log `[WEBHOOK] ✅ Booking status updated to Paid` không?
  - [ ] Có lỗi nào không?

- [ ] **5. Kiểm tra Booking Status**
  - [ ] Booking status = "Paid"?
  - [ ] Invoice đã được tạo?
  - [ ] Payment method = "Online"?

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app
- **Test Script:** `./test-webhook-booking4.sh`

## 💡 Lưu Ý

1. **VietQR không có webhook** - Chỉ tạo QR code, không detect thanh toán
2. **SePay webhook cần thời gian** - Có thể 1-5 phút sau khi thanh toán
3. **Nội dung chuyển khoản quan trọng** - Phải đúng format `BOOKING{id}`
4. **Frontend polling là fallback** - Chỉ check status, không tự động update

## 🆘 Nếu Vẫn Không Hoạt Động

1. **Kiểm tra SePay Support:**
   - Liên hệ SePay support để verify webhook configuration
   - Hỏi về thời gian detect thanh toán

2. **Kiểm tra Railway:**
   - Railway logs có lỗi không?
   - Webhook endpoint có accessible không?

3. **Test Thủ Công:**
   - Chạy script test webhook
   - Nếu test thành công → Vấn đề ở SePay
   - Nếu test thất bại → Vấn đề ở backend

