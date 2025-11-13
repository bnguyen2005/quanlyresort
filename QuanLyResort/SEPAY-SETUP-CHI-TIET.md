# 📋 Hướng Dẫn Setup SePay Webhook - Chi Tiết Từng Bước

## 🎯 Mục Tiêu

Setup webhook SePay để tự động nhận thông báo khi khách hàng thanh toán và tự động update booking status thành "Paid".

## 📋 Bước 1: Đăng Nhập SePay Dashboard

1. **Mở trình duyệt** và vào: https://my.sepay.vn
2. **Đăng nhập** với tài khoản SePay của bạn
3. **Vào trang Webhooks:** https://my.sepay.vn/webhooks
4. **Click nút "Thêm Webhook"** (thường ở góc trên bên phải)

## 📋 Bước 2: Điền Form "Thêm Webhook"

### 2.1. Đặt Tên

**Trường:** "Đặt tên"

**Giá trị:**
```
ResortDeluxe
```

**Hoặc:**
```
Resort Payment Webhook
QuanLyResort Webhook
```

**Lưu ý:** Tên này chỉ để phân biệt các webhook, không ảnh hưởng đến hoạt động.

---

### 2.2. Chọn Sự Kiện

**Trường:** "Bắn WebHooks khi"

**Chọn:**
```
☑ Có tiền vào
```

**Giải thích:**
- **"Có tiền vào"** = Nhận webhook khi khách hàng chuyển tiền vào tài khoản
- **"Có tiền ra"** = Nhận webhook khi bạn chuyển tiền ra
- **"Cả hai"** = Nhận webhook cho cả tiền vào và tiền ra

**✅ Khuyến nghị:** Chọn **"Có tiền vào"**

---

### 2.3. Điều Kiện - Tài Khoản Ngân Hàng

**Trường:** "Khi tài khoản ngân hàng là"

**Giá trị:**
```
0901329227
```

**Hoặc để trống** nếu muốn nhận webhook từ tất cả tài khoản.

**Khuyến nghị:**
- ✅ **Điền số tài khoản** nếu chỉ muốn nhận webhook từ tài khoản cụ thể
- ✅ **Để trống** nếu muốn nhận từ tất cả tài khoản

---

### 2.4. Điều Kiện - Code Thanh Toán ⭐ QUAN TRỌNG

**Trường:** "Bỏ qua nếu nội dung giao dịch không có Code thanh toán?"

**Chọn:**
```
☑ Có
```

**Giải thích:**
- **"Có"** = Chỉ nhận webhook nếu nội dung chuyển khoản có code thanh toán (ví dụ: `BOOKING4`)
- **"Không"** = Nhận webhook cho tất cả giao dịch (kể cả không có code)

**✅ Khuyến nghị:** Chọn **"Có"** để chỉ nhận webhook khi có code thanh toán.

**Lưu ý:** Nếu chọn "Có", bạn cần cấu hình cấu trúc mã thanh toán tại:
- **Cấu hình công ty → Cấu hình chung → Cấu trúc mã thanh toán**
- Format code: `BOOKING{id}` (ví dụ: `BOOKING4`)

---

### 2.5. Thuộc Tính - URL Webhook ⭐ QUAN TRỌNG NHẤT

**Trường:** "Gọi đến URL"

**Giá trị:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**⚠️ LƯU Ý QUAN TRỌNG:**
- ✅ Phải là URL **HTTPS** (không phải HTTP)
- ✅ Phải là URL **public** (không phải localhost)
- ✅ Phải chính xác từng ký tự
- ✅ Không có khoảng trắng ở đầu/cuối
- ✅ Không có dấu `/` ở cuối (trừ khi cần)

**Test URL trước khi điền:**
```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

---

### 2.6. Thuộc Tính - Xác Thực Thanh Toán ⭐ QUAN TRỌNG

**Trường:** "Là WebHooks xác thực thanh toán?"

**Chọn:**
```
☑ Có
```

**Giải thích:**
- **"Có"** = Webhook này dùng để xác thực thanh toán tự động (booking sẽ tự động update thành "Paid")
- **"Không"** = Webhook này chỉ để nhận thông báo (không tự động update booking)

**✅ Khuyến nghị:** Chọn **"Có"** vì bạn cần xác thực thanh toán tự động cho booking.

---

### 2.7. Thuộc Tính - Gọi Lại Webhook

**Trường:** "Gọi lại Webhooks khi?"

**Checkbox:**
```
☑ HTTP Status Code không nằm trong phạm vi từ 200 đến 299.
```

**Giải thích:**
- **Check** = SePay sẽ gọi lại webhook nếu server trả về lỗi (không phải 200-299)
- **Không check** = SePay chỉ gọi 1 lần, không retry

**✅ Khuyến nghị:** **Check** để SePay tự động retry nếu server lỗi tạm thời.

---

### 2.8. Cấu Hình Chứng Thực - Kiểu Chứng Thực

**Trường:** "Kiểu chứng thực"

**Chọn:**
```
Không cần chứng thực
```

**Các lựa chọn:**
- **"Không cần chứng thực"** - Không cần xác thực (đơn giản nhất, để test)
- **"OAuth 2.0"** - Xác thực bằng OAuth 2.0 (bảo mật cao)
- **"API Key"** - Xác thực bằng API Key (bảo mật trung bình)

**Khuyến nghị:**
- ✅ **Test:** Chọn "Không cần chứng thực"
- 💡 **Production:** Nên chọn "API Key" (sau khi test thành công)

**Nếu chọn "API Key":**
- SePay sẽ yêu cầu bạn nhập API Key
- Bạn cần cấu hình API Key trong code để verify webhook

---

### 2.9. Cấu Hình Chứng Thực - Request Content Type

**Trường:** "Request Content type"

**Chọn:**
```
application/json
```

**✅ Đúng rồi!** - Giữ nguyên "application/json"

---

### 2.10. Trạng Thái

**Trường:** "Trạng thái"

**Chọn:**
```
☑ Kích hoạt
```

**✅ Đúng rồi!** - Giữ nguyên "Kích hoạt"

---

## 📋 Bước 3: Click "Thêm"

Sau khi điền xong tất cả các trường, **click nút "Thêm"** (thường ở góc dưới bên phải, màu xanh).

**SePay sẽ:**
- Tạo webhook mới
- Verify URL (kiểm tra xem URL có hoạt động không)
- Hiển thị kết quả

---

## 📋 Bước 4: Kiểm Tra Kết Quả

### 4.1. Xem Danh Sách Webhook

**Sau khi click "Thêm":**
1. SePay sẽ hiển thị danh sách webhook
2. Tìm webhook vừa tạo (tên: `ResortDeluxe`)
3. Kiểm tra **trạng thái:**
   - ✅ **"Kích hoạt"** = Webhook đã được tạo thành công
   - ❌ **"Lỗi"** = Có vấn đề, cần kiểm tra lại

### 4.2. Kiểm Tra URL Verification

**SePay sẽ tự động verify URL:**
- ✅ **Thành công** = URL hoạt động, webhook sẵn sàng
- ❌ **Thất bại** = URL không hoạt động hoặc không trả về 200 OK

**Nếu verification thất bại:**
1. Kiểm tra URL có đúng không
2. Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. Kiểm tra Railway service có đang chạy không

---

## 📋 Bước 5: Test Webhook

### 5.1. Test Với Script

**Chạy script test:**
```bash
./QuanLyResort/test-sepay-webhook-production.sh
```

**Kết quả mong đợi:**
- ✅ Test 1: Empty Body - PASSED
- ✅ Test 2-5: SePay Format - PASSED (hoặc 404 nếu booking không tồn tại)

### 5.2. Test Với Giao Dịch Thật

**Sau khi setup webhook:**
1. **Tạo booking mới** trên website
2. **Thanh toán** với nội dung: `BOOKING{id}` (ví dụ: `BOOKING10`)
3. **Kiểm tra Railway logs:**
   - Railway Dashboard → Service → Logs
   - Tìm: `[WEBHOOK] 📥 Webhook received`
   - Tìm: `[WEBHOOK] 📋 Detected Simple/SePay format`
   - Tìm: `[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId`
4. **Kiểm tra booking status:**
   - Vào website → Booking details
   - Status có tự động update thành "Paid" không
   - Invoice có được tạo không

---

## 🐛 Troubleshooting

### Webhook Không Được Tạo

**Nguyên nhân:**
- Form chưa điền đầy đủ
- URL không hợp lệ
- SePay server lỗi

**Giải pháp:**
1. Kiểm tra lại tất cả các trường
2. Kiểm tra URL có đúng không
3. Thử lại sau vài phút

### URL Verification Thất Bại

**Nguyên nhân:**
- URL không đúng
- Server không trả về 200 OK
- Railway service không chạy

**Giải pháp:**
1. Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
2. Kiểm tra Railway service có đang chạy không
3. Kiểm tra Railway logs xem có lỗi gì không

### Webhook Không Được Gửi

**Nguyên nhân:**
- Code thanh toán không khớp (nếu chọn "Có" cho "Bỏ qua nếu không có Code thanh toán")
- Tài khoản ngân hàng không khớp (nếu đã điền)
- Webhook chưa được kích hoạt

**Giải pháp:**
1. Kiểm tra code thanh toán format: `BOOKING{id}`
2. Kiểm tra tài khoản ngân hàng có đúng không
3. Kiểm tra webhook có được kích hoạt không

### Webhook Được Gửi Nhưng Không Xử Lý

**Nguyên nhân:**
- Webhook format không đúng
- Server lỗi khi xử lý
- Booking ID không được extract

**Giải pháp:**
1. Kiểm tra Railway logs để xem webhook format
2. Xem có lỗi gì trong logs không
3. Kiểm tra booking ID có được extract không

---

## 📋 Checklist Hoàn Chỉnh

- [ ] Đã đăng nhập SePay dashboard
- [ ] Đã vào trang Webhooks
- [ ] Đã click "Thêm Webhook"
- [ ] Đã điền "Đặt tên": `ResortDeluxe`
- [ ] Đã chọn "Có tiền vào"
- [ ] Đã điền tài khoản ngân hàng (hoặc để trống)
- [ ] Đã chọn "Có" cho "Bỏ qua nếu không có Code thanh toán"
- [ ] Đã điền URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- [ ] Đã chọn "Có" cho "Là WebHooks xác thực thanh toán"
- [ ] Đã check "Gọi lại Webhooks khi HTTP Status Code không 200-299"
- [ ] Đã chọn "Không cần chứng thực" (test) hoặc "API Key" (production)
- [ ] Đã chọn "application/json"
- [ ] Đã chọn "Kích hoạt"
- [ ] Đã click "Thêm"
- [ ] Đã kiểm tra webhook trong danh sách
- [ ] Đã kiểm tra URL verification thành công
- [ ] Đã test với script
- [ ] Đã test với giao dịch thật
- [ ] Đã kiểm tra Railway logs
- [ ] Đã kiểm tra booking status tự động update

---

## 🔗 Links Quan Trọng

- **SePay Dashboard:** https://my.sepay.vn
- **Webhook Management:** https://my.sepay.vn/webhooks
- **Railway Dashboard:** https://railway.app
- **Railway Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Test Script:** `./QuanLyResort/test-sepay-webhook-production.sh`

---

## 💡 Lưu Ý Quan Trọng

1. **URL phải chính xác:** Copy-paste URL để tránh lỗi typo
2. **Code thanh toán:** Format `BOOKING{id}` (ví dụ: `BOOKING4`)
3. **Test trước:** Test với script trước khi test với giao dịch thật
4. **Logs:** Luôn kiểm tra Railway logs để debug
5. **Chứng thực:** Dùng "Không cần chứng thực" để test, sau đó chuyển sang "API Key" cho production

---

## 🎯 Kết Luận

**Sau khi setup xong:**
- ✅ SePay sẽ tự động gửi webhook khi có giao dịch
- ✅ Railway sẽ tự động nhận và xử lý webhook
- ✅ Booking sẽ tự động update thành "Paid"
- ✅ Invoice sẽ tự động được tạo

**Không cần làm gì thêm!** Webhook sẽ tự động hoạt động. 🎉

