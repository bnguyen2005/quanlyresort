# ✅ Fix: SePay Webhook Response Format

## 📋 Vấn Đề

**SePay yêu cầu webhook response phải:**
- ✅ JSON có `success: true`
- ✅ HTTP Status Code phải là **201** (hoặc 200 cho API Key/Không chứng thực)

**Nếu không thỏa mãn:** SePay sẽ xem là webhook thất bại và không gửi webhook tiếp theo.

## ✅ Đã Sửa

**Tất cả webhook responses đã được cập nhật:**

### 1. Response Thành Công (Booking Payment)
```csharp
return StatusCode(201, new
{
    success = true,
    message = "Thanh toán thành công",
    bookingId = bookingId.Value,
    bookingCode = booking.BookingCode,
    webhookId = webhookId,
    processedAt = DateTime.UtcNow,
    durationMs = duration
});
```

### 2. Response Thành Công (Restaurant Order Payment)
```csharp
return StatusCode(201, new
{
    success = true,
    message = "Thanh toán thành công",
    orderId = restaurantOrderId.Value,
    orderNumber = order.OrderNumber,
    type = "restaurant",
    webhookId = webhookId,
    processedAt = DateTime.UtcNow,
    durationMs = restaurantDuration
});
```

### 3. Response "Already Paid"
```csharp
return StatusCode(201, new 
{ 
    success = true, 
    message = "Đã thanh toán rồi", 
    bookingId = bookingId.Value, 
    webhookId = webhookId 
});
```

### 4. Response Verification Request
```csharp
return StatusCode(201, new
{
    success = true,
    status = "active",
    endpoint = "/api/simplepayment/webhook",
    message = "Webhook endpoint is ready",
    timestamp = DateTime.UtcNow
});
```

## 🎯 Yêu Cầu SePay

### Với Chứng Thực OAuth 2.0:
- ✅ JSON có `success: true`
- ✅ HTTP Status Code = **201**

### Với Chứng Thực API Key:
- ✅ JSON có `success: true`
- ✅ HTTP Status Code = **201** hoặc **200**

### Với Không Chứng Thực:
- ✅ JSON có `success: true`
- ✅ HTTP Status Code = **201** hoặc **200**

## 📊 Trước và Sau

### Trước (Không Đúng):
```csharp
return Ok(new
{
    success = true,
    message = "Thanh toán thành công",
    ...
});
```
- HTTP Status Code: **200** ✅
- Có `success: true` ✅
- **Nhưng SePay có thể yêu cầu 201**

### Sau (Đúng):
```csharp
return StatusCode(201, new
{
    success = true,
    message = "Thanh toán thành công",
    ...
});
```
- HTTP Status Code: **201** ✅
- Có `success: true` ✅
- **Tuân thủ đúng yêu cầu SePay**

## 🧪 Test Sau Khi Fix

### Bước 1: Deploy Code Mới

**Code đã được commit và push:**
- ✅ Tất cả responses đã có `success: true`
- ✅ Tất cả responses đã dùng HTTP Status Code **201**

### Bước 2: Test Webhook Thủ Công

**Test xem response có đúng format không:**

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: SePay-Webhook/1.0" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN"
  }' -v
```

**Kiểm tra response:**
- HTTP Status Code phải = **201**
- Response body phải có `"success": true`

### Bước 3: Test Với Giao Dịch Thật

1. **Tạo booking mới:**
   - Vào website → Đặt phòng
   - Tạo booking mới (ví dụ: booking 4)
   - Click "Thanh toán"

2. **Quét QR code và chuyển tiền:**
   - Quét QR code bằng app ngân hàng
   - **Nội dung chuyển khoản:** `BOOKING4` (không có khoảng trắng)
   - Số tiền: Đúng với booking

3. **Đợi 1-5 phút:**
   - SePay cần thời gian để xử lý và gửi webhook

4. **Kiểm tra:**
   - SePay dashboard → Thống kê có tăng không?
   - Railway logs → Có webhook received không?
   - Booking status → Có = "Paid" không?
   - QR code → Có tự động ẩn không?

## 🔍 Kiểm Tra Response

**Railway Dashboard → Service → Logs**

**Sau khi nhận webhook, kiểm tra response:**
- HTTP Status Code phải = **201**
- Response body phải có `"success": true`

**Nếu SePay gửi webhook thật:**
- SePay sẽ nhận được response với status 201 và `success: true`
- SePay sẽ xem là webhook thành công
- SePay sẽ tiếp tục gửi webhook cho các giao dịch tiếp theo

## 📋 Checklist

- [x] Tất cả responses đã có `success: true`
- [x] Tất cả responses đã dùng HTTP Status Code **201**
- [ ] Code đã được deploy lên Railway
- [ ] Test webhook thủ công → Response có status 201 và success: true
- [ ] Test với giao dịch thật → SePay có gửi webhook không?
- [ ] QR code có tự động ẩn không?

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

## 💡 Lưu Ý

1. **HTTP Status Code 201:** SePay yêu cầu status 201 (hoặc 200) để xem là thành công
2. **success: true:** Bắt buộc phải có trong response JSON
3. **Nếu không đúng:** SePay sẽ xem là webhook thất bại và không gửi webhook tiếp theo
4. **Deploy:** Cần deploy code mới lên Railway để áp dụng thay đổi

## 🎉 Kết Luận

**Đã sửa tất cả webhook responses để tuân thủ yêu cầu SePay:**
- ✅ Tất cả responses đã có `success: true`
- ✅ Tất cả responses đã dùng HTTP Status Code **201**
- ✅ Code đã được commit và push

**Bước tiếp theo:**
- Deploy code mới lên Railway
- Test với giao dịch thật
- Kiểm tra SePay có gửi webhook không

