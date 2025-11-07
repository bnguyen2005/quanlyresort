# 🧪 Hướng Dẫn Test Webhook

## 📋 So Sánh Localhost vs Ngrok

### 🏠 LOCALHOST (http://localhost:5130)

**✅ Ưu điểm:**
- Test nhanh, không cần ngrok
- Dùng để verify code hoạt động
- Không cần internet

**❌ Nhược điểm:**
- PayOs **KHÔNG THỂ** gọi được (vì localhost không truy cập từ internet)
- Chỉ test được **manual webhook** (bằng tay)
- Không test được thanh toán thật từ PayOs

**Khi nào dùng:**
- ✅ Test code mới
- ✅ Verify webhook endpoint hoạt động
- ✅ Debug logic xử lý webhook

### 🌐 NGROK (https://069c46a78b2b.ngrok-free.app)

**✅ Ưu điểm:**
- PayOs **CÓ THỂ** gọi được (truy cập từ internet)
- Test được **thanh toán thật**
- Webhook **tự động** khi thanh toán
- QR code tự động biến mất

**❌ Nhược điểm:**
- Cần ngrok đang chạy
- URL thay đổi mỗi lần restart ngrok (free plan)
- Cần internet

**Khi nào dùng:**
- ✅ Test thanh toán thật từ PayOs
- ✅ Test webhook tự động
- ✅ Demo cho khách hàng

## 🎯 Quy Trình Test Khuyến Nghị

### Bước 1: Test Localhost (Verify Code)

```bash
# 1. Đảm bảo backend đang chạy
cd QuanLyResort
dotnet run

# 2. Test manual webhook
curl -X POST http://localhost:5130/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING7", "amount": 10000}'

# 3. Kiểm tra kết quả
# - Xem backend logs → Sẽ thấy webhook processed
# - Kiểm tra booking status → Sẽ là "Paid"
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 7,
  "bookingCode": "BKG2025007"
}
```

### Bước 2: Test Ngrok (Test Thật)

```bash
# 1. Chạy ngrok (terminal mới)
ngrok http 5130

# 2. Copy URL từ ngrok (ví dụ: https://069c46a78b2b.ngrok-free.app)

# 3. Test webhook qua ngrok
curl -X POST https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING7", "amount": 10000}'

# 4. Test thanh toán thật:
#    - Mở payment modal
#    - Quét QR và thanh toán với nội dung "BOOKING7"
#    - PayOs sẽ tự động gọi webhook
#    - QR sẽ tự động biến mất
```

## 🔍 Kiểm Tra Kết Quả

### 1. Backend Logs

Sẽ thấy:
```
📥 [WEBHOOK-xxx] Webhook received: BOOKING7 - 10,000 VND
✅ [WEBHOOK-xxx] Extracted booking ID: 7
✅ [WEBHOOK-xxx] Booking BKG2025007 - Status: Paid
```

### 2. Frontend

- QR code tự động biến mất
- Hiển thị "✅ Thanh toán thành công!"
- Booking status = "Paid"

### 3. Database

- Booking status = "Paid"
- Invoice được tạo
- Payment reference được lưu

## ⚠️ Lưu Ý

1. **Localhost:** Chỉ test được manual, PayOs không gọi được
2. **Ngrok:** Cần để PayOs tự động gọi webhook
3. **Restart backend:** Sau khi sửa code, cần restart backend
4. **Booking status:** Nếu booking đã "Paid", webhook sẽ trả về "Đã thanh toán rồi"

## 🎉 Kết Luận

**Test cả 2:**
- **Localhost** → Verify code hoạt động
- **Ngrok** → Test với PayOs thật

**Cho production:**
- Deploy backend lên server thật
- Config PayOs webhook với domain thật
- Không dùng ngrok free plan

