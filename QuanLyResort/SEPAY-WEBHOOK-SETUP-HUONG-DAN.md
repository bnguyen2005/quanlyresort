# 🔧 Hướng Dẫn Setup SePay Webhook

## 📋 Vấn Đề Hiện Tại

**QR code đã hiển thị ✅** nhưng khi thanh toán:
- ❌ Chưa ẩn QR code
- ❌ Chưa cập nhật trạng thái thanh toán thành công

**Nguyên nhân:** SePay webhook chưa được setup trong dashboard, nên backend không nhận được thông báo thanh toán.

## 🎯 Giải Pháp

### Bước 1: Setup SePay Webhook trong Dashboard

1. **Đăng nhập SePay Dashboard:**
   - Truy cập: https://my.sepay.vn
   - Đăng nhập với tài khoản của bạn

2. **Vào phần Webhook:**
   - Menu: **Webhooks** hoặc **Cài đặt → Webhook**
   - Hoặc truy cập trực tiếp: https://my.sepay.vn/webhooks

3. **Thêm Webhook URL:**
   ```
   https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
   ```

4. **Cấu hình Webhook:**
   - **URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - **Events:** Chọn tất cả events (hoặc ít nhất: `transfer.in`, `transfer.success`)
   - **Status:** Bật (Active/Enabled)
   - **Cấu hình chứng thực WebHooks:** Có 3 tùy chọn:
     - ✅ **Không cần chứng thực** (Đơn giản nhất - Khuyến nghị cho test)
     - ✅ **API Key** (An toàn hơn - Cần tạo API Token)
     - ✅ **OAuth 2.0** (An toàn nhất - Phức tạp hơn)

5. **Lưu cấu hình**

### 🔐 Tùy Chọn: Tạo API Token (Nếu chọn "API Key")

**API Token là TÙY CHỌN - không bắt buộc!**

Nếu bạn chọn phương thức "Không cần chứng thực" → **Bỏ qua bước này**

Nếu bạn chọn phương thức "API Key" → Làm theo các bước sau:

1. **Vào phần API Access:**
   - SePay Dashboard → **Cấu hình Công ty** → **API Access**
   - Hoặc truy cập: https://my.sepay.vn/api-access

2. **Tạo API Token:**
   - Click **"+ Thêm API"** (góc trên bên phải)
   - Điền thông tin:
     - **Tên:** `Resort Payment Webhook` (hoặc tên bất kỳ)
     - **Trạng thái:** Chọn **Hoạt động**
   - Click **"Thêm"**

3. **Copy API Token:**
   - Sau khi tạo, API Token sẽ hiển thị trong danh sách
   - **Copy token này** (chỉ hiển thị 1 lần, lưu lại cẩn thận!)

4. **Cấu hình trong Webhook:**
   - Khi thêm webhook, chọn phương thức: **"API Key"**
   - Nhập API Token vào trường **"API Key"**

**Lưu ý:**
- API Token có toàn quyền truy cập (SePay chưa hỗ trợ phân quyền)
- Nếu mất token, phải tạo lại
- Backend hiện tại hỗ trợ cả 3 phương thức (không cần code thay đổi)

### Bước 2: Kiểm Tra Webhook Hoạt Động

1. **Test Webhook:**
   - Trong SePay Dashboard, tìm nút **"Test Webhook"** hoặc **"Gửi thử"**
   - Click để gửi test webhook

2. **Kiểm tra Railway Logs:**
   - Railway Dashboard → Service → Logs
   - Tìm các dòng:
     ```
     [WEBHOOK] 📥 Webhook received
     [WEBHOOK] 📋 Detected Simple/SePay format
     [WEBHOOK] 🔍 Using Description field (SePay format)
     [WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId
     ```

### Bước 3: Test Với Thanh Toán Thật

1. **Tạo booking mới:**
   - Đăng nhập → Tạo booking
   - Click "Thanh toán"

2. **Thanh toán:**
   - Quét QR code
   - Chuyển khoản với nội dung: `BOOKING{id}` (ví dụ: `BOOKING4`)
   - Số tiền: Đúng với booking

3. **Kiểm tra tự động:**
   - Sau khi thanh toán, webhook sẽ gửi đến Railway
   - Backend sẽ tự động cập nhật booking status = "Paid"
   - Frontend polling sẽ detect và:
     - ✅ Ẩn QR code
     - ✅ Hiển thị "Thanh toán thành công"
     - ✅ Cập nhật trạng thái booking

## 🔍 Format SePay Webhook

**SePay gửi webhook với format:**
```json
{
  "description": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "IN",
  "id": "TXN123456",
  "referenceCode": "REF123456",
  "accountNumber": "0901329227",
  "bankCode": "MB"
}
```

**Backend đã hỗ trợ:**
- ✅ Extract `description` → Booking ID (`BOOKING4` → `4`)
- ✅ Extract `transferAmount` → Amount
- ✅ Update booking status = "Paid"
- ✅ Log chi tiết để debug

## 📊 Kiểm Tra Logs

### ✅ Nếu Webhook Hoạt Động:

```
[WEBHOOK] 📥 Webhook received: BOOKING4 - 5,000 VND
[WEBHOOK] 📋 Detected Simple/SePay format
[WEBHOOK] 🔍 Using Description field (SePay format): 'BOOKING4'
[WEBHOOK] 🔍 Using TransferAmount field (SePay format): 5000
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 4
[WEBHOOK] ✅ Booking found: Code=BOOKING4, Status=Pending
[WEBHOOK] ✅ Booking 4 updated to Paid successfully!
```

### ❌ Nếu Webhook Không Hoạt Động:

**Không thấy logs** → Webhook chưa được setup hoặc URL sai

**Thấy logs nhưng không extract được booking ID:**
```
[WEBHOOK] ⚠️ ❌ FAILED: Could not extract bookingId from content: '...'
```
→ Kiểm tra format description trong QR code (phải là `BOOKING{id}`)

## 🎯 Checklist

- [ ] SePay webhook URL đã được setup trong dashboard
- [ ] Webhook status = Active/Enabled
- [ ] Test webhook đã gửi thành công
- [ ] Railway logs hiển thị webhook received
- [ ] Booking ID được extract thành công
- [ ] Booking status được update thành "Paid"
- [ ] Frontend polling detect được status "Paid"
- [ ] QR code tự động ẩn
- [ ] Thông báo "Thanh toán thành công" hiển thị

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **SePay Webhooks:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs

## 💡 Lưu Ý

1. **Format nội dung chuyển khoản:** Phải là `BOOKING{id}` (ví dụ: `BOOKING4`)
2. **Webhook delay:** SePay có thể mất vài giây đến vài phút để gửi webhook
3. **Polling:** Frontend polling mỗi 2 giây, sẽ detect ngay khi status = "Paid"
4. **Test:** Luôn test với booking thật sau khi setup webhook

## 🆘 Troubleshooting

### Webhook không nhận được

1. **Kiểm tra URL:**
   - Đảm bảo URL đúng: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Không có dấu `/` ở cuối

2. **Kiểm tra Railway:**
   - Service đang chạy
   - Logs không có lỗi

3. **Kiểm tra SePay Dashboard:**
   - Webhook status = Active
   - Webhook URL đúng

### Webhook nhận được nhưng không update status

1. **Kiểm tra logs:**
   - Xem có extract được booking ID không
   - Xem có update status không

2. **Kiểm tra format description:**
   - Phải là `BOOKING{id}` (ví dụ: `BOOKING4`)
   - Không có khoảng trắng thừa

3. **Kiểm tra booking:**
   - Booking tồn tại
   - Booking status chưa là "Paid"

