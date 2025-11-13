# 🧪 Hướng Dẫn Test PayOs Webhook

## 📋 Tổng Quan

Script test webhook PayOs với dữ liệu mẫu từ PayOs API documentation.

## 🚀 Cách Sử Dụng

### Chạy Script

```bash
cd QuanLyResort
./test-payos-webhook.sh
```

Hoặc:

```bash
bash QuanLyResort/test-payos-webhook.sh
```

## 📊 Các Test Cases

### Test 1: Dữ liệu mẫu từ PayOs API

**Payload:**
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 123,
    "amount": 3000,
    "description": "VQRIO123",
    ...
  }
}
```

**Mục đích:**
- Test với dữ liệu mẫu chính thức từ PayOs
- Description = "VQRIO123" (không phải booking ID)

**Kết quả mong đợi:**
- HTTP 200 OK
- Response có thể báo "Không tìm thấy booking ID" (vì VQRIO123 không phải format BOOKING{id})

### Test 2: Booking Payment (BOOKING4)

**Payload:**
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 40043,
    "amount": 5000,
    "description": "BOOKING4",
    ...
  }
}
```

**Mục đích:**
- Test với description = "BOOKING4"
- Verify extract booking ID = 4

**Kết quả mong đợi:**
- HTTP 200 OK
- Response có `bookingId: 4`
- Booking 4 được update thành "Paid" (nếu booking tồn tại)

### Test 3: Restaurant Order Payment (ORDER7)

**Payload:**
```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 20000007,
    "amount": 150000,
    "description": "ORDER7",
    ...
  }
}
```

**Mục đích:**
- Test với description = "ORDER7"
- Verify extract restaurant order ID = 7

**Kết quả mong đợi:**
- HTTP 200 OK
- Response có `orderId: 7` hoặc `orderNumber`
- Restaurant order 7 được update thành "Paid" (nếu order tồn tại)

### Test 4: Payment Failed (Code != "00")

**Payload:**
```json
{
  "code": "01",
  "desc": "Payment failed",
  "success": false,
  "data": {
    ...
  }
}
```

**Mục đích:**
- Test xử lý lỗi khi code != "00"
- Verify webhook không update booking khi payment failed

**Kết quả mong đợi:**
- HTTP 200 OK
- Response có message về payment failed
- Booking không được update

### Test 5: Verification Request (Empty Body)

**Payload:**
```
(empty)
```

**Mục đích:**
- Test PayOs verification request (empty body)
- Verify endpoint trả về status active

**Kết quả mong đợi:**
- HTTP 200 OK
- Response có `status: "active"`
- Response có `endpoint: "/api/simplepayment/webhook"`

## ✅ Kết Quả Mong Đợi

### Tất Cả Tests Thành Công

```
═══════════════════════════════════════════════════════════
📊 TỔNG KẾT
═══════════════════════════════════════════════════════════

✅ Passed: 5/5
❌ Failed: 0/5

🎉 Tất cả tests đều thành công!
```

### Một Số Tests Thất Bại

```
═══════════════════════════════════════════════════════════
📊 TỔNG KẾT
═══════════════════════════════════════════════════════════

✅ Passed: 3/5
❌ Failed: 2/5

⚠️  Một số tests thất bại. Kiểm tra lại webhook endpoint.
```

## 🐛 Troubleshooting

### Lỗi: "Connection refused" hoặc "Failed to connect"

**Nguyên nhân:**
- Railway service không chạy
- Webhook URL sai

**Giải pháp:**
1. Kiểm tra Railway service đang chạy
2. Kiểm tra webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. Test thủ công: `curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

### Lỗi: HTTP 500 Internal Server Error

**Nguyên nhân:**
- Lỗi trong code xử lý webhook
- Database connection error

**Giải pháp:**
1. Kiểm tra Railway logs
2. Kiểm tra database connection
3. Kiểm tra code xử lý webhook

### Test 2/3 Thất Bại: "Booking/Order not found"

**Nguyên nhân:**
- Booking/Order ID không tồn tại trong database

**Giải pháp:**
- Đây là bình thường nếu booking/order chưa được tạo
- Tạo booking/order trước khi test
- Hoặc test với booking/order ID có sẵn

### Test 1 Thất Bại: "Cannot extract booking ID"

**Nguyên nhân:**
- Description = "VQRIO123" không phải format BOOKING{id}

**Giải pháp:**
- Đây là bình thường, test này để verify webhook xử lý đúng format không hợp lệ

## 📋 Checklist

- [ ] Đã chạy script test webhook
- [ ] Tất cả tests đều pass (5/5)
- [ ] Test 2: Extract booking ID thành công
- [ ] Test 3: Extract restaurant order ID thành công
- [ ] Test 4: Xử lý lỗi đúng
- [ ] Test 5: Verification request hoạt động

## 💡 Lưu Ý

1. **Test với dữ liệu thật:**
   - Script test với dữ liệu mẫu
   - Để test với dữ liệu thật, cần thanh toán thật qua PayOs

2. **Booking/Order ID:**
   - Test 2 và 3 cần booking/order ID tồn tại trong database
   - Nếu không có, test sẽ báo "not found" (bình thường)

3. **Signature:**
   - Script dùng signature mẫu (không verify)
   - Vì `VerifySignature=false` nên signature không được kiểm tra

4. **Railway Logs:**
   - Sau khi chạy test, kiểm tra Railway logs để xem chi tiết xử lý
   - Logs sẽ hiển thị: `[WEBHOOK] 📥 Webhook received`, `✅✅✅ SUCCESS: Extracted bookingId...`

## 🔗 Links Quan Trọng

- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
- **Railway Dashboard:** https://railway.app
- **PayOs API Documentation:** https://payos.vn/docs/api/

## 🎯 Kết Quả Mong Đợi

Sau khi test thành công:
- ✅ Webhook endpoint xử lý đúng format PayOs
- ✅ Extract booking ID từ description
- ✅ Extract restaurant order ID từ description
- ✅ Xử lý lỗi đúng khi code != "00"
- ✅ Verification request hoạt động

