# ⚠️ SePay Webhook Không Gửi Khi Thanh Toán Bằng QR Code

## 📋 Vấn Đề

**Mô tả:**
- ✅ **Thanh toán thủ công bằng terminal:** Webhook hoạt động bình thường
- ❌ **Thanh toán bằng mã QR:** 
  - Ngân hàng đã nhận tiền ✅
  - SePay Dashboard hiển thị nhận tiền ✅
  - **NHƯNG không gửi webhook** ❌

## 🔍 Nguyên Nhân Có Thể

### 1. **Webhook Chỉ Kích Hoạt Cho Một Số Loại Giao Dịch**

**SePay có thể có cấu hình:**
- Webhook chỉ gửi cho giao dịch từ terminal
- Webhook không gửi cho giao dịch từ QR code
- Cần kích hoạt riêng cho QR code payments

### 2. **Nội Dung Chuyển Khoản Không Đúng Format**

**Khi thanh toán bằng QR code:**
- Nội dung chuyển khoản có thể bị thay đổi
- SePay có thể không nhận diện được booking/order ID
- Webhook có thể không được kích hoạt nếu không match pattern

### 3. **Webhook URL Chưa Được Cấu Hình Đúng**

**Trong SePay Dashboard:**
- Webhook URL có thể chỉ được cấu hình cho terminal payments
- Cần cấu hình riêng cho QR code payments
- Hoặc cần cấu hình webhook cho tất cả loại giao dịch

### 4. **SePay Có Cấu Hình Riêng Cho QR Code**

**Có thể cần:**
- Kích hoạt webhook riêng cho QR code payments
- Cấu hình điều kiện webhook khác nhau
- Hoặc dùng webhook endpoint khác cho QR code

## ✅ Giải Pháp

### Bước 1: Kiểm Tra SePay Dashboard - Webhook Settings

1. **Vào SePay Dashboard:** https://my.sepay.vn
2. **Menu:** **Webhooks** hoặc **Cài đặt → Webhooks**
3. **Kiểm tra:**
   - Webhook URL có đúng không?
   - Webhook có được kích hoạt cho QR code payments không?
   - Có điều kiện nào filter webhook không?

### Bước 2: Kiểm Tra Webhook Conditions

**Trong SePay Dashboard → Webhooks:**

**Kiểm tra các điều kiện:**
- **Loại giao dịch:** Terminal, QR Code, Tất cả?
- **Số tiền tối thiểu:** Có giới hạn không?
- **Nội dung chuyển khoản:** Có pattern nào không?
- **Trạng thái:** Chỉ gửi khi nào?

### Bước 3: Kiểm Tra Nội Dung Chuyển Khoản

**Khi thanh toán bằng QR code:**
- Nội dung chuyển khoản phải là: `BOOKING{id}` hoặc `ORDER{id}`
- Không có khoảng trắng
- Không có ký tự đặc biệt
- Format chính xác

**Ví dụ:**
- ✅ `BOOKING4`
- ❌ `BOOKING 4` (có khoảng trắng)
- ❌ `BOOKING-4` (có dấu gạch ngang)
- ❌ `Thanh toán BOOKING4` (có thêm text)

### Bước 4: Kiểm Tra SePay Dashboard - Statistics

**SePay Dashboard → Webhooks → Thống kê:**

**Kiểm tra:**
- **Thống kê gửi:** Có tăng không khi thanh toán bằng QR?
- **Thống kê thành công:** Có tăng không?
- **Thống kê thất bại:** Có lỗi nào không?

**Nếu "Thống kê gửi" = 0:**
- SePay không gửi webhook
- Cần kiểm tra cấu hình webhook

### Bước 5: Liên Hệ SePay Support

**Nếu tất cả đều đúng nhưng vẫn không gửi webhook:**

1. **Liên hệ SePay Support:**
   - Email: support@sepay.vn
   - Hoặc qua SePay Dashboard → Hỗ trợ

2. **Cung cấp thông tin:**
   - Tài khoản SePay: ID 5365, Tên ResortDeluxe
   - Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Vấn đề: Webhook không gửi khi thanh toán bằng QR code
   - Terminal payments: Webhook hoạt động bình thường
   - QR code payments: Webhook không được gửi

## 🔧 Cấu Hình Webhook Trong SePay Dashboard

### Bước 1: Vào Webhook Settings

1. **SePay Dashboard:** https://my.sepay.vn
2. **Menu:** **Webhooks** hoặc **Cài đặt → Webhooks**

### Bước 2: Kiểm Tra Webhook URL

**Webhook URL phải là:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kiểm tra:**
- URL có đúng không?
- Có typo không?
- Có https:// không?

### Bước 3: Kiểm Tra Webhook Conditions

**Tìm các điều kiện:**
- **Loại giao dịch:** Phải chọn "Tất cả" hoặc "QR Code"
- **Số tiền:** Không có giới hạn (hoặc giới hạn phù hợp)
- **Nội dung:** Không có pattern filter (hoặc pattern đúng)

### Bước 4: Kích Hoạt Webhook Cho QR Code

**Nếu có option "Kích hoạt cho QR Code":**
- ✅ Bật option này
- ✅ Lưu cấu hình

## 🧪 Test Webhook

### Test 1: Test Thủ Công

**Dùng curl để test webhook endpoint:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook/1.0" \
  -d '{
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
  }'
```

**Kiểm tra:**
- Response có `success: true` không?
- HTTP status code có = 201 không?
- Booking có được cập nhật không?

### Test 2: Test Với Giao Dịch Thật

1. **Tạo booking mới** (ví dụ: booking 4)
2. **Quét QR code** và chuyển tiền
3. **Nội dung chuyển khoản:** `BOOKING4` (không có khoảng trắng)
4. **Đợi 1-5 phút**
5. **Kiểm tra:**
   - SePay Dashboard → Thống kê có tăng không?
   - Railway logs → Có webhook received không?
   - Booking status → Có = "Paid" không?

## 🔍 Debug Checklist

### SePay Dashboard:
- [ ] Webhook URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] Webhook được kích hoạt cho QR code payments
- [ ] Không có điều kiện filter webhook
- [ ] Thống kê gửi có tăng khi thanh toán bằng QR không?

### Nội Dung Chuyển Khoản:
- [ ] Format đúng: `BOOKING{id}` hoặc `ORDER{id}`
- [ ] Không có khoảng trắng
- [ ] Không có ký tự đặc biệt
- [ ] Booking/Order ID có tồn tại trong database không?

### Railway Backend:
- [ ] Webhook endpoint hoạt động (test thủ công thành công)
- [ ] Response có `success: true` và HTTP 201
- [ ] Logs không có lỗi

### SePay Support:
- [ ] Đã liên hệ SePay support về vấn đề này
- [ ] Đã cung cấp đầy đủ thông tin
- [ ] Đã nhận được phản hồi từ SePay

## 💡 Lưu Ý Quan Trọng

1. **Terminal vs QR Code:** SePay có thể có cấu hình riêng cho từng loại
2. **Webhook Conditions:** Cần kiểm tra điều kiện kích hoạt webhook
3. **Nội Dung:** Format nội dung chuyển khoản rất quan trọng
4. **SePay Support:** Có thể cần liên hệ để kích hoạt webhook cho QR code

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **SePay Support:** support@sepay.vn hoặc qua Dashboard
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

## 🎯 Kết Luận

**Vấn đề:** Webhook không gửi khi thanh toán bằng QR code, nhưng hoạt động với terminal

**Nguyên nhân có thể:**
- Webhook chưa được kích hoạt cho QR code payments
- Cấu hình webhook chỉ cho terminal payments
- Nội dung chuyển khoản không đúng format

**Giải pháp:**
1. Kiểm tra SePay Dashboard → Webhooks → Conditions
2. Kích hoạt webhook cho QR code payments
3. Kiểm tra nội dung chuyển khoản format
4. Liên hệ SePay support nếu cần

**Bước tiếp theo:**
1. Vào SePay Dashboard → Webhooks
2. Kiểm tra cấu hình webhook
3. Kích hoạt webhook cho QR code payments
4. Test lại với giao dịch thật
5. Nếu vẫn không hoạt động → Liên hệ SePay support

