# 🔧 Hướng Dẫn Cấu Hình SePay IPN (Instant Payment Notification)

## 📋 Thông Tin IPN

**IPN (Instant Payment Notification)** là cơ chế thông báo tức thì từ SePay đến website của bạn khi khách hàng hoàn tất giao dịch thanh toán.

## ✅ Cấu Hình IPN Trong SePay Dashboard

### Bước 1: Vào Cấu Hình IPN

1. **Đăng nhập SePay Dashboard:** https://my.sepay.vn
2. **Vào:** **Công ty** → **Cấu hình chung** → **IPN** (hoặc **Cấu hình IPN**)

### Bước 2: Điền Thông Tin

#### ✅ IPN URL *

**Điền:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Lưu ý:**
- ✅ Phải bắt đầu bằng `https://` (không dùng `http://`)
- ✅ URL phải trỏ đúng endpoint `/api/simplepayment/webhook`
- ✅ Không có dấu `/` ở cuối
- ✅ URL phải accessible từ internet (Railway domain đã public)

#### ✅ Auth Type

**Chọn:** `Không có` (hoặc `None`)

**Giải thích:**
- Backend hiện tại chưa implement signature verification
- Có thể chọn "Không có" để đơn giản
- Nếu muốn bảo mật hơn, có thể chọn "API Key" hoặc "OAuth 2.0" sau

#### ✅ Secret Key

**Để trống** (nếu Auth Type = "Không có")

**Hoặc điền Secret Key nếu có:**
- Nếu bạn có Secret Key từ SePay Dashboard
- Format: `spsk_live_...` hoặc tương tự
- Secret Key này sẽ được dùng để verify signature (chưa implement)

#### ✅ Content Type

**Chọn:** `application/json`

**Giải thích:**
- Backend expect JSON format
- SePay sẽ gửi webhook dưới dạng JSON

#### ✅ Trạng thái

**Chọn:** `Kích hoạt IPN` (hoặc `Active`)

**Lưu ý:**
- Phải kích hoạt để SePay gửi IPN notifications
- Nếu không kích hoạt, SePay sẽ không gửi webhook

## 📋 Tóm Tắt Cấu Hình

```
IPN URL:        https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
Auth Type:      Không có
Secret Key:     (để trống)
Content Type:   application/json
Trạng thái:    Kích hoạt IPN
```

## ✅ Sau Khi Cấu Hình

### 1. Lưu Cấu Hình

- Click **"Lưu"** hoặc **"Save"** để lưu cấu hình IPN

### 2. Test IPN (Nếu Có)

- SePay Dashboard có thể có nút **"Test IPN"** hoặc **"Gửi test"**
- Click để test xem IPN có hoạt động không
- Kiểm tra Railway logs xem có nhận được không

### 3. Kiểm Tra Railway Logs

**Vào:** Railway Dashboard → Service → Logs

**Tìm logs:**
- ✅ `[WEBHOOK] 📥 Webhook received` → IPN đã được nhận
- ✅ `[WEBHOOK] ✅ Booking status updated to Paid` → Status đã được cập nhật

## 🔍 Kiểm Tra IPN Hoạt Động

### Cách 1: Test Thủ Công

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

### Cách 2: Thanh Toán Thử Nghiệm

1. Tạo booking mới (ví dụ: BOOKING5)
2. Quét QR code và thanh toán
3. Đảm bảo nội dung chuyển khoản là: `BOOKING5` (không có dấu gạch ngang)
4. Đợi 1-5 phút
5. Kiểm tra Railway logs xem có nhận được IPN không

## 🐛 Troubleshooting

### Vấn Đề 1: IPN Không Được Gửi

**Nguyên nhân:**
- IPN URL sai
- IPN chưa được kích hoạt
- Nội dung chuyển khoản không đúng format

**Giải pháp:**
1. Kiểm tra IPN URL có đúng không
2. Kiểm tra trạng thái = "Kích hoạt IPN"
3. Kiểm tra nội dung chuyển khoản có đúng format `BOOKING{id}` không

### Vấn Đề 2: IPN Được Gửi Nhưng Backend Không Nhận

**Nguyên nhân:**
- Railway endpoint không accessible
- Backend lỗi khi xử lý IPN

**Giải pháp:**
1. Test webhook endpoint thủ công (xem Cách 1)
2. Kiểm tra Railway logs xem có lỗi không
3. Kiểm tra Railway service có đang chạy không

### Vấn Đề 3: IPN Nhận Được Nhưng Không Cập Nhật Status

**Nguyên nhân:**
- Booking ID không đúng
- Số tiền không khớp
- Backend lỗi khi xử lý

**Giải pháp:**
1. Kiểm tra Railway logs để xem lỗi cụ thể
2. Kiểm tra booking ID có đúng không
3. Kiểm tra số tiền có khớp với booking amount không

## 📊 Format IPN Request

**SePay sẽ gửi IPN với format:**

```json
{
  "id": 92704,
  "gateway": "MB",
  "transactionDate": "2023-03-25 14:02:37",
  "accountNumber": "0901329227",
  "code": null,
  "content": "BOOKING4",
  "transferType": "in",
  "transferAmount": 5000,
  "accumulated": 19077000,
  "subAccount": null,
  "referenceCode": "MBMB.3278907687",
  "description": ""
}
```

**Backend sẽ:**
1. Extract `content = "BOOKING4"` → `bookingId = 4`
2. Extract `transferAmount = 5000`
3. Verify amount với booking amount
4. Update booking status = "Paid"
5. Return HTTP 201 với `{"success": true}`

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app
- **Test Script:** `./test-webhook-booking4.sh`

## ✅ Checklist

- [ ] **IPN URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] **Auth Type:** `Không có`
- [ ] **Secret Key:** (để trống)
- [ ] **Content Type:** `application/json`
- [ ] **Trạng thái:** `Kích hoạt IPN`
- [ ] **Đã lưu cấu hình**
- [ ] **Test IPN:** Đã test và hoạt động
- [ ] **Railway Logs:** Có logs nhận được IPN

## 💡 Lưu Ý

1. **IPN URL phải đúng:** Phải trỏ đúng endpoint `/api/simplepayment/webhook`
2. **Phải kích hoạt:** Trạng thái phải = "Kích hoạt IPN"
3. **Content Type:** Phải là `application/json`
4. **Auth Type:** Có thể chọn "Không có" để đơn giản (hoặc "API Key" nếu muốn bảo mật hơn)
5. **Nội dung chuyển khoản:** Phải đúng format `BOOKING{id}` để SePay detect và gửi IPN

