# 🔧 Hướng Dẫn Setup SePay Webhook

## 📋 Form "Thêm Webhook" - Hướng Dẫn Chi Tiết

### Bước 1: Đặt Tên ✅

**Trường:** "Đặt tên"

**Giá trị:**
```
ResortDeluxe
```

**Hoặc có thể đặt:**
```
Resort Payment Webhook
QuanLyResort Webhook
```

**Lưu ý:** Tên này chỉ để phân biệt các webhook với nhau, không ảnh hưởng đến hoạt động.

---

### Bước 2: Chọn Sự Kiện ✅

**Trường:** "Bắn WebHooks khi"

**Giá trị đã chọn:**
```
Có tiền vào
```

**✅ Đúng rồi!** - Chọn "Có tiền vào" để nhận webhook khi khách hàng thanh toán.

**Các lựa chọn khác:**
- "Có tiền ra" - Khi bạn chuyển tiền ra
- "Cả hai" - Cả tiền vào và tiền ra

**Khuyến nghị:** Giữ "Có tiền vào" ✅

---

### Bước 3: Chọn Điều Kiện

#### 3.1. Tài Khoản Ngân Hàng

**Trường:** "Khi tài khoản ngân hàng là"

**Giá trị:**
```
0901329227
```

**Hoặc để trống** nếu muốn nhận webhook từ tất cả tài khoản.

**Khuyến nghị:** 
- ✅ **Điền số tài khoản** nếu chỉ muốn nhận webhook từ tài khoản cụ thể
- ✅ **Để trống** nếu muốn nhận từ tất cả tài khoản

#### 3.2. Bỏ Qua Nếu Không Có Code Thanh Toán

**Trường:** "Bỏ qua nếu nội dung giao dịch không có Code thanh toán?"

**Giá trị hiện tại:**
```
Không
```

**Giải thích:**
- **"Có"** = Chỉ nhận webhook nếu nội dung chuyển khoản có code thanh toán (ví dụ: BOOKING4)
- **"Không"** = Nhận webhook cho tất cả giao dịch (kể cả không có code)

**Khuyến nghị:**
- ✅ **Chọn "Có"** nếu muốn chỉ nhận webhook khi có code thanh toán (ví dụ: BOOKING4)
- ⚠️ **Chọn "Không"** nếu muốn nhận tất cả giao dịch (có thể có nhiều webhook không liên quan)

**💡 Khuyến nghị:** Chọn **"Có"** để chỉ nhận webhook khi có code thanh toán.

---

### Bước 4: Thuộc Tính WebHooks

#### 4.1. Gọi Đến URL ⭐ QUAN TRỌNG

**Trường:** "Gọi đến URL"

**Giá trị cần điền:**
```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**⚠️ LƯU Ý:**
- Phải là URL **HTTPS** (không phải HTTP)
- Phải là URL **public** (không phải localhost)
- Phải chính xác từng ký tự

**Test URL trước:**
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

#### 4.2. Là WebHooks Xác Thực Thanh Toán?

**Trường:** "Là WebHooks xác thực thanh toán?"

**Giá trị hiện tại:**
```
Không
```

**Giải thích:**
- **"Có"** = Webhook này dùng để xác thực thanh toán tự động
- **"Không"** = Webhook này chỉ để nhận thông báo

**Khuyến nghị:**
- ✅ **Chọn "Có"** vì bạn cần xác thực thanh toán tự động cho booking

**💡 Khuyến nghị:** Chọn **"Có"** ✅

#### 4.3. Gọi Lại Webhooks Khi?

**Trường:** "Gọi lại Webhooks khi?"

**Checkbox:**
```
☑ HTTP Status Code không nằm trong phạm vi từ 200 đến 299.
```

**Giải thích:**
- Nếu check = SePay sẽ gọi lại webhook nếu server trả về lỗi (không phải 200-299)
- Nếu không check = SePay chỉ gọi 1 lần, không retry

**Khuyến nghị:**
- ✅ **Nên check** để SePay tự động retry nếu server lỗi tạm thời

**💡 Khuyến nghị:** **Check** ✅

---

### Bước 5: Cấu Hình Chứng Thực WebHooks

#### 5.1. Kiểu Chứng Thực

**Trường:** "Kiểu chứng thực"

**Giá trị hiện tại:**
```
Không cần chứng thực
```

**Các lựa chọn:**
- **"Không cần chứng thực"** - Không cần xác thực (đơn giản nhất)
- **"OAuth 2.0"** - Xác thực bằng OAuth 2.0 (bảo mật cao)
- **"API Key"** - Xác thực bằng API Key (bảo mật trung bình)

**Khuyến nghị:**
- ✅ **"Không cần chứng thực"** - Để test nhanh
- 💡 **"API Key"** - Nên dùng khi production (bảo mật hơn)

**💡 Khuyến nghị:** 
- **Test:** Chọn "Không cần chứng thực"
- **Production:** Chọn "API Key" (sau khi test thành công)

#### 5.2. Request Content Type

**Trường:** "Request Content type"

**Giá trị hiện tại:**
```
application/json
```

**✅ Đúng rồi!** - Giữ nguyên "application/json"

---

### Bước 6: Trạng Thái ✅

**Trường:** "Trạng thái"

**Giá trị hiện tại:**
```
Kích hoạt
```

**✅ Đúng rồi!** - Giữ nguyên "Kích hoạt"

---

## 📋 Tóm Tắt Các Giá Trị Nên Điền

| Trường | Giá Trị Khuyến Nghị |
|--------|---------------------|
| **Đặt tên** | `ResortDeluxe` hoặc `Resort Payment Webhook` |
| **Bắn WebHooks khi** | `Có tiền vào` ✅ |
| **Khi tài khoản ngân hàng là** | `0901329227` (hoặc để trống) |
| **Bỏ qua nếu không có Code thanh toán?** | `Có` ⭐ (quan trọng) |
| **Gọi đến URL** | `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ⭐ |
| **Là WebHooks xác thực thanh toán?** | `Có` ⭐ |
| **Gọi lại Webhooks khi?** | ☑ Check (HTTP Status Code không 200-299) |
| **Kiểu chứng thực** | `Không cần chứng thực` (test) hoặc `API Key` (production) |
| **Request Content type** | `application/json` ✅ |
| **Trạng thái** | `Kích hoạt` ✅ |

## 🎯 Các Bước Thực Hiện

### 1. Điền Form

1. **Đặt tên:** `ResortDeluxe` (hoặc tên khác)
2. **Bắn WebHooks khi:** `Có tiền vào` ✅
3. **Khi tài khoản ngân hàng là:** `0901329227` (hoặc để trống)
4. **Bỏ qua nếu không có Code thanh toán?:** Chọn **"Có"** ⭐
5. **Gọi đến URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ⭐
6. **Là WebHooks xác thực thanh toán?:** Chọn **"Có"** ⭐
7. **Gọi lại Webhooks khi?:** ☑ **Check** checkbox
8. **Kiểu chứng thực:** `Không cần chứng thực` (để test)
9. **Request Content type:** `application/json` ✅
10. **Trạng thái:** `Kích hoạt` ✅

### 2. Click "Thêm"

Sau khi điền xong, click nút **"Thêm"** (màu xanh, góc dưới bên phải).

### 3. Kiểm Tra Kết Quả

1. **Xem danh sách webhook** trong dashboard
2. **Kiểm tra trạng thái** webhook vừa tạo
3. **Xem logs** (nếu có) để biết SePay có verify được URL không

### 4. Test Webhook

1. **Tạo giao dịch thử nghiệm** (nếu có)
2. **Kiểm tra Railway logs** xem có nhận được webhook không
3. **Kiểm tra SePay logs** (nếu có) để xem webhook có được gửi không

## ⚠️ Lưu Ý Quan Trọng

### 1. URL Phải Chính Xác

- ✅ Phải là HTTPS
- ✅ Phải là URL public (không phải localhost)
- ✅ Không có khoảng trắng ở đầu/cuối

### 2. Code Thanh Toán

Nếu chọn "Có" cho "Bỏ qua nếu không có Code thanh toán", bạn cần:
- Cấu hình cấu trúc mã thanh toán tại: **Cấu hình công ty → Cấu hình chung → Cấu trúc mã thanh toán**
- Format code: `BOOKING{id}` (ví dụ: `BOOKING4`)

### 3. Chứng Thực

- **Test:** Dùng "Không cần chứng thực" để test nhanh
- **Production:** Nên dùng "API Key" để bảo mật

## 🔍 Sau Khi Setup

### Kiểm Tra Webhook Hoạt Động

1. **Vào danh sách webhook** trong SePay dashboard
2. **Xem trạng thái** webhook vừa tạo
3. **Xem logs** (nếu có) để biết webhook có được gửi không

### Test Với Giao Dịch Thật

1. **Tạo booking mới** trên website
2. **Thanh toán** với nội dung: `BOOKING{id}`
3. **Kiểm tra Railway logs** xem có nhận được webhook không
4. **Kiểm tra booking status** có tự động update thành "Paid" không

## 🐛 Troubleshooting

### Webhook Không Được Gửi

**Nguyên nhân:**
- URL không đúng
- Server không trả về 200 OK
- Code thanh toán không khớp (nếu chọn "Có")

**Giải pháp:**
1. Kiểm tra URL chính xác
2. Test endpoint: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. Kiểm tra code thanh toán format

### Webhook Được Gửi Nhưng Không Xử Lý

**Nguyên nhân:**
- Webhook format không đúng
- Server lỗi khi xử lý

**Giải pháp:**
1. Kiểm tra Railway logs
2. Xem webhook format từ SePay
3. Cập nhật code xử lý webhook

## 📋 Checklist

- [ ] Đã điền "Đặt tên"
- [ ] Đã chọn "Có tiền vào"
- [ ] Đã điền tài khoản ngân hàng (hoặc để trống)
- [ ] Đã chọn "Có" cho "Bỏ qua nếu không có Code thanh toán"
- [ ] Đã điền Railway URL chính xác
- [ ] Đã chọn "Có" cho "Là WebHooks xác thực thanh toán"
- [ ] Đã check "Gọi lại Webhooks khi HTTP Status Code không 200-299"
- [ ] Đã chọn "Không cần chứng thực" (test) hoặc "API Key" (production)
- [ ] Đã chọn "application/json"
- [ ] Đã chọn "Kích hoạt"
- [ ] Đã click "Thêm"
- [ ] Đã kiểm tra webhook trong dashboard
- [ ] Đã test với giao dịch thử nghiệm

## 🔗 Links Quan Trọng

- **SePay Dashboard:** https://my.sepay.vn
- **Webhook Management:** https://my.sepay.vn/webhooks
- **Documentation:** https://docs.sepay.vn
- **Railway Webhook:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

