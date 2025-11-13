# 📚 PayOs API Documentation - Thông Tin Bổ Ích

**Nguồn:** [PayOs API Documentation](https://payos.vn/docs/api/)

## 🔐 Signature Format - Quan Trọng!

### Theo PayOs API Documentation

> "Bạn cần dùng checksum key từ Kênh thanh toán và HMAC_SHA256 để tạo signature và data theo định dạng được **sort theo alphabet**: `amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl`."

### ✅ Code Hiện Tại

```csharp
// PayOsService.cs - Line 58-61
// PayOs signature format: FIXED ORDER (not alphabetical!)
// Format: amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}
var signatureString = $"amount={amountLong}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
```

### 🔍 Phân Tích

**Thứ tự trong code:**
1. `amount`
2. `cancelUrl`
3. `description`
4. `orderCode`
5. `returnUrl`

**Thứ tự alphabetical:**
1. `amount` ✅
2. `cancelUrl` ✅
3. `description` ✅
4. `orderCode` ✅
5. `returnUrl` ✅

**Kết luận:** ✅ Code đã đúng thứ tự alphabetical!

### 📝 Lưu Ý

- PayOs yêu cầu **sort theo alphabet** (a-z)
- Code hiện tại đã đúng thứ tự
- Không cần thay đổi

## 📋 Các Trường Trong Request Body

### Tạo Link Thanh Toán

Theo [PayOs API Documentation](https://payos.vn/docs/api/):

| Trường | Type | Required | Mô Tả |
|--------|------|----------|-------|
| `orderCode` | integer | ✅ | Mã đơn hàng |
| `amount` | integer | ✅ | Số tiền thanh toán |
| `description` | string | ✅ | Mô tả thanh toán (giới hạn 9 ký tự nếu không dùng PayOs) |
| `buyerName` | string | ❌ | Tên người mua (cho hóa đơn điện tử) |
| `buyerCompanyName` | string | ❌ | Tên đơn vị mua (cho hóa đơn điện tử) |
| `buyerTaxCode` | string | ❌ | MST (cho hóa đơn điện tử) |
| `buyerAddress` | string | ❌ | Địa chỉ (cho hóa đơn điện tử) |
| `buyerEmail` | string | ❌ | Email (cho hóa đơn điện tử) |
| `buyerPhone` | string | ❌ | SĐT (cho hóa đơn điện tử) |
| `items` | Array | ❌ | Danh sách sản phẩm |
| `cancelUrl` | string (URI) | ✅ | URL khi hủy đơn |
| `returnUrl` | string (URI) | ✅ | URL khi thanh toán thành công |
| `invoice` | object | ❌ | Thông tin hóa đơn |
| `expiredAt` | number (Int32 timestamp) | ❌ | Thời gian hết hạn |
| `signature` | string | ✅ | Chữ ký HMAC-SHA256 |

### ✅ Code Hiện Tại

```csharp
// PayOsService.cs - CreatePaymentLinkAsync
var requestBody = new
{
    orderCode = orderCode,
    amount = amountLong,
    description = description,
    cancelUrl = cancelUrl,
    returnUrl = returnUrl,
    expiredAt = expiredAtUnix > 0 ? (long?)expiredAtUnix : null,
    signature = signature
};
```

**✅ Đã đúng:**
- Có đầy đủ các trường required
- Format đúng (integer cho amount, orderCode)
- Signature được tính đúng

**💡 Có thể bổ sung:**
- `buyerName`, `buyerEmail`, `buyerPhone` (nếu cần hóa đơn điện tử)
- `items` (nếu cần chi tiết sản phẩm)

## 🔗 Webhook URL Configuration

### API: Kiểm Tra Và Thêm/Cập Nhật Webhook URL

**Endpoint:** `POST /confirm-webhook`

**Headers:**
- `x-client-id`: Client ID
- `x-api-key`: API Key

**Request Body:**
```json
{
  "webhookUrl": "https://your-domain.com/api/webhook"
}
```

**Response:**
```json
{
  "code": "00",
  "desc": "success",
  "data": {
    "webhookUrl": "https://your-domain.com/api/webhook"
  }
}
```

### ✅ Code Hiện Tại

Script `verify-payos-webhook.sh` đã implement đúng:
```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: $CLIENT_ID" \
  -H "x-api-key: $API_KEY" \
  -d '{"webhookUrl": "$WEBHOOK_URL"}'
```

## ⚠️ Lỗi Thường Gặp

### HTTP 401 - Unauthorized

**Nguyên nhân:**
- Client ID hoặc API Key không đúng
- Headers không được set đúng

**Giải pháp:**
- Kiểm tra `x-client-id` và `x-api-key` headers
- Đảm bảo credentials đúng từ PayOs Dashboard

### HTTP 429 - Too Many Requests

**Nguyên nhân:**
- Gọi API quá nhiều lần trong thời gian ngắn

**Giải pháp:**
- Implement rate limiting
- Đợi một lúc rồi thử lại

### Code 201 - Signature Không Hợp Lệ

**Nguyên nhân:**
- Signature format không đúng
- ChecksumKey không đúng
- Thứ tự các trường không đúng (phải alphabetical)

**Giải pháp:**
- Kiểm tra signature string: `amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}`
- Đảm bảo ChecksumKey đúng từ PayOs Dashboard
- Verify thứ tự alphabetical

## 📊 So Sánh Code Với Documentation

### ✅ Đã Đúng

1. **Signature Format:**
   - ✅ Dùng HMAC-SHA256
   - ✅ Thứ tự alphabetical đúng
   - ✅ Format string đúng

2. **Request Body:**
   - ✅ Có đầy đủ trường required
   - ✅ Type đúng (integer cho amount, orderCode)
   - ✅ Headers đúng (x-client-id, x-api-key)

3. **Webhook:**
   - ✅ Endpoint đúng format
   - ✅ Xử lý đúng PayOs webhook format

### 💡 Có Thể Cải Thiện

1. **Thêm Buyer Information:**
   - Có thể thêm `buyerName`, `buyerEmail`, `buyerPhone` nếu cần hóa đơn điện tử

2. **Thêm Items:**
   - Có thể thêm `items` array nếu cần chi tiết sản phẩm

3. **Error Handling:**
   - Có thể xử lý HTTP 429 (rate limiting) tốt hơn

## 🔍 Kiểm Tra Code

### Signature String Format

**Code hiện tại:**
```csharp
var signatureString = $"amount={amountLong}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
```

**Theo documentation:**
```
amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl
```

**✅ Khớp 100%!**

### Request Body Format

**Code hiện tại:**
```csharp
var requestBody = new
{
    orderCode = orderCode,      // integer ✅
    amount = amountLong,        // integer ✅
    description = description,   // string ✅
    cancelUrl = cancelUrl,      // string (URI) ✅
    returnUrl = returnUrl,      // string (URI) ✅
    expiredAt = expiredAtUnix, // number (Int32 timestamp) ✅
    signature = signature       // string ✅
};
```

**✅ Khớp với documentation!**

## 📋 Checklist

- [x] ✅ Signature format đúng (alphabetical order)
- [x] ✅ Request body có đầy đủ trường required
- [x] ✅ Type đúng (integer cho amount, orderCode)
- [x] ✅ Headers đúng (x-client-id, x-api-key)
- [x] ✅ Webhook endpoint xử lý đúng format
- [ ] 💡 Có thể thêm buyer information (optional)
- [ ] 💡 Có thể thêm items array (optional)

## 🎯 Kết Luận

**Code hiện tại đã đúng với PayOs API Documentation!**

- ✅ Signature format đúng
- ✅ Request body format đúng
- ✅ Headers đúng
- ✅ Webhook xử lý đúng

**Không cần thay đổi gì!**

## 🔗 Links Quan Trọng

- **PayOs API Documentation:** https://payos.vn/docs/api/
- **PayOs Dashboard:** https://payos.vn
- **PayOs Support:** support@payos.vn

