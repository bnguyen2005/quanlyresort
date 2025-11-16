# 🔄 Cập Nhật Trạng Thái Tự Động Khi Thanh Toán Thành Công

## ❓ Câu Hỏi

**Khi quét QR thanh toán thành công có cập nhật trạng thái tự động không?**

## 📊 Phân Tích

### ✅ Có Tự Động (Nếu SePay Webhook Hoạt Động)

**Cách hoạt động:**
1. ✅ Khách hàng quét QR code VietQR và chuyển khoản
2. ✅ SePay detect thanh toán (nếu đã link tài khoản với SePay)
3. ✅ SePay gửi webhook đến `/api/simplepayment/webhook`
4. ✅ Backend extract booking ID từ content: `BOOKING{id}`
5. ✅ Backend gọi `ProcessOnlinePaymentAsync` → Cập nhật status = "Paid"
6. ✅ Frontend polling detect status = "Paid" → Hiển thị success

**Điều kiện:**
- ✅ SePay đã được cấu hình và link với tài khoản ngân hàng
- ✅ SePay webhook đã được setup trong SePay Dashboard
- ✅ Nội dung chuyển khoản đúng format: `BOOKING{id}` hoặc `ORDER{id}`

### ❌ Không Tự Động (Nếu Chỉ Dùng VietQR)

**Vấn đề:**
- ❌ VietQR **KHÔNG có webhook** tự động
- ❌ VietQR chỉ tạo QR code, không detect thanh toán
- ❌ Backend không biết khi nào thanh toán thành công

**Giải pháp:**
1. ⚠️ **SePay Webhook** (nếu đã cấu hình) - Tự động detect và cập nhật
2. ⚠️ **Frontend Polling** - Chỉ check status, không tự động update
3. ⚠️ **Manual Verification** - Admin verify thủ công

## 🔄 Cơ Chế Hiện Tại

### 1. SePay Webhook (Tự Động)

**Endpoint:** `/api/simplepayment/webhook`

**Khi SePay gửi webhook:**
```json
{
  "content": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "in"
}
```

**Backend xử lý:**
1. Extract booking ID từ `content`: `BOOKING4` → `bookingId = 4`
2. Verify amount (nếu có)
3. Gọi `ProcessOnlinePaymentAsync(bookingId, "Webhook-...")`
4. Cập nhật booking status = "Paid"
5. Return HTTP 201 với `{"success": true}`

**Frontend:**
- Polling mỗi 2 giây check booking status
- Khi detect status = "Paid" → Hiển thị success và reload trang

### 2. Frontend Polling (Check Status)

**Cách hoạt động:**
```javascript
// Polling mỗi 2 giây
setInterval(async () => {
  const booking = await fetchBookingStatus(bookingId);
  if (booking.status === 'Paid') {
    showPaymentSuccess();
    reloadPage();
  }
}, 2000);
```

**Lưu ý:**
- ⚠️ Polling chỉ **check status**, không tự động update
- ⚠️ Cần webhook để **update status** trước
- ⚠️ Nếu không có webhook, status sẽ không tự động update

### 3. Manual Verification (Thủ Công)

**Cách hoạt động:**
1. Admin kiểm tra tài khoản ngân hàng
2. Xác nhận thanh toán thành công
3. Cập nhật booking status = "Paid" thủ công

## 🎯 Giải Pháp Tốt Nhất: VietQR + SePay Webhook

### Cách Hoạt Động:

**1. Tạo QR Code bằng VietQR:**
```
https://img.vietqr.io/image/MB-0901329227-compact2.png?amount=5000&addInfo=BOOKING4
```

**2. Khách hàng quét QR và chuyển khoản:**
- App ngân hàng tự động điền thông tin
- Chuyển khoản thành công

**3. SePay Detect Thanh Toán:**
- SePay đã link với tài khoản ngân hàng
- SePay detect thanh toán (nếu nội dung chuyển khoản đúng format)
- SePay gửi webhook → Backend cập nhật booking

**4. Frontend Polling:**
- Frontend polling backend mỗi 2 giây
- Khi detect status = "Paid" → Hiển thị success

### ✅ Ưu Điểm:
- ✅ **HOÀN TOÀN MIỄN PHÍ** (VietQR)
- ✅ **Tự động cập nhật** (SePay webhook)
- ✅ **QR code động** (số tiền thay đổi)
- ✅ **Polling fallback** (nếu webhook không hoạt động)

## ⚠️ Lưu Ý Quan Trọng

### 1. SePay Webhook Phải Hoạt Động

**Để tự động cập nhật, cần:**
- ✅ SePay account đã link với tài khoản ngân hàng
- ✅ SePay webhook đã được setup trong SePay Dashboard
- ✅ Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- ✅ Nội dung chuyển khoản đúng format: `BOOKING{id}` hoặc `ORDER{id}`

### 2. Nội Dung Chuyển Khoản Phải Đúng Format

**Format đúng:**
- ✅ `BOOKING4` → Backend extract booking ID = 4
- ✅ `ORDER7` → Backend extract order ID = 7

**Format sai:**
- ❌ `BOOKING 4` (có khoảng trắng)
- ❌ `booking4` (chữ thường)
- ❌ `BOOKING-4` (có dấu gạch ngang - vẫn OK nhưng không khuyến nghị)

### 3. SePay Webhook Có Thể Không Hoạt Động

**Nguyên nhân:**
- ⚠️ SePay không detect được thanh toán (nội dung không đúng format)
- ⚠️ SePay webhook chưa được kích hoạt trong SePay Dashboard
- ⚠️ SePay webhook không gửi cho QR code payments (chỉ gửi cho terminal payments)

**Giải pháp:**
- ⚠️ Kiểm tra SePay Dashboard → Webhooks → Statistics
- ⚠️ Kiểm tra nội dung chuyển khoản có đúng format không
- ⚠️ Liên hệ SePay support nếu webhook không gửi

## 📊 Tóm Tắt

| Tình Huống | Tự Động Cập Nhật? | Cách Hoạt Động |
|------------|-------------------|----------------|
| **VietQR + SePay Webhook** | ✅ **CÓ** | SePay detect → Webhook → Backend update → Frontend polling |
| **VietQR (không SePay)** | ❌ **KHÔNG** | Chỉ có polling check, không tự động update |
| **SePay API + Webhook** | ✅ **CÓ** | SePay API tạo order → Webhook → Backend update |
| **Manual Verification** | ⚠️ **THỦ CÔNG** | Admin verify và update thủ công |

## 🎯 Kết Luận

**Trả lời câu hỏi:**

**CÓ tự động cập nhật** nếu:
- ✅ SePay webhook đã được cấu hình và hoạt động
- ✅ Nội dung chuyển khoản đúng format: `BOOKING{id}` hoặc `ORDER{id}`
- ✅ SePay detect được thanh toán

**KHÔNG tự động cập nhật** nếu:
- ❌ Chỉ dùng VietQR (không có SePay webhook)
- ❌ SePay webhook chưa được cấu hình
- ❌ Nội dung chuyển khoản không đúng format

## 💡 Khuyến Nghị

**Để đảm bảo tự động cập nhật:**
1. ✅ Dùng VietQR để tạo QR code (miễn phí)
2. ✅ Cấu hình SePay webhook (để tự động detect thanh toán)
3. ✅ Đảm bảo nội dung chuyển khoản đúng format: `BOOKING{id}`
4. ✅ Kiểm tra SePay Dashboard → Webhooks → Statistics

**Nếu SePay webhook không hoạt động:**
- ⚠️ Dùng polling + manual verification
- ⚠️ Hoặc liên hệ SePay support để fix webhook

