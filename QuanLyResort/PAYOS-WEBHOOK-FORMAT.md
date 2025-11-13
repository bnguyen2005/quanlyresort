# 📋 PayOs Webhook Format Documentation

**Nguồn:** [PayOs API Documentation](https://payos.vn/docs/api/)

## 📥 Format Webhook PayOs

### Request Body Schema

PayOs sẽ gửi POST request đến webhook URL với format JSON:

```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 123,
    "amount": 3000,
    "description": "VQRIO123",
    "accountNumber": "12345678",
    "reference": "TF230204212323",
    "transactionDateTime": "2023-02-04 18:25:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb18",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "",
    "counterAccountBankName": "",
    "counterAccountName": "",
    "counterAccountNumber": "",
    "virtualAccountName": "",
    "virtualAccountNumber": ""
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
```

## 📊 Chi Tiết Các Trường

### Root Level

| Trường | Type | Required | Mô Tả |
|--------|------|----------|-------|
| `code` | string | ✅ | Mã lỗi. `"00"` = thành công |
| `desc` | string | ✅ | Thông tin lỗi. `"success"` = thành công |
| `success` | boolean | ✅ | Trạng thái thành công |
| `data` | object | ✅ | Dữ liệu giao dịch |
| `signature` | string | ✅ | Chữ ký để kiểm tra tính toàn vẹn (HMAC-SHA256) |

### Data Object

| Trường | Type | Required | Mô Tả | Ví Dụ |
|--------|------|----------|-------|-------|
| `orderCode` | long | ✅ | Mã đơn hàng | `123` |
| `amount` | decimal | ✅ | Số tiền | `3000` |
| `description` | string | ✅ | Mô tả đơn hàng | `"BOOKING7"` hoặc `"VQRIO123"` |
| `accountNumber` | string | ✅ | Số tài khoản | `"12345678"` |
| `reference` | string | ✅ | Mã tham chiếu giao dịch | `"TF230204212323"` |
| `transactionDateTime` | string | ✅ | Thời gian giao dịch | `"2023-02-04 18:25:00"` |
| `currency` | string | ✅ | Loại tiền tệ | `"VND"` |
| `paymentLinkId` | string | ✅ | ID của payment link | `"124c33293c43417ab7879e14c8d9eb18"` |
| `code` | string | ❌ | Code trong data | `"00"` |
| `desc` | string | ❌ | Mô tả trong data | `"Thành công"` |
| `counterAccountBankId` | string | ❌ | ID ngân hàng đối tác | `""` |
| `counterAccountBankName` | string | ❌ | Tên ngân hàng đối tác | `""` |
| `counterAccountName` | string | ❌ | Tên chủ tài khoản đối tác | `""` |
| `counterAccountNumber` | string | ❌ | Số tài khoản đối tác | `""` |
| `virtualAccountName` | string | ❌ | Tên tài khoản ảo | `""` |
| `virtualAccountNumber` | string | ❌ | Số tài khoản ảo | `""` |

## 🔐 Signature Verification

### Format

PayOs sử dụng **HMAC-SHA256** để tính signature.

### Cách Verify

1. **Lấy ChecksumKey** từ PayOs Dashboard
2. **Tạo payload** từ các trường trong `data`
3. **Tính HMAC-SHA256** với ChecksumKey
4. **So sánh** với signature nhận được

### Lưu Ý

- Hiện tại code đang **tắt signature verification** (`VerifySignature=false`)
- Có thể bật lại khi cần thiết
- PayOs có thể thay đổi format signature, cần kiểm tra documentation

## ✅ Response Format

Webhook endpoint phải trả về **HTTP 2XX** để xác nhận đã nhận dữ liệu thành công.

### Response Thành Công

```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "bookingCode": "BKG2025004",
  "webhookId": "abc12345",
  "processedAt": "2025-11-13T11:40:00Z"
}
```

### Response Lỗi

```json
{
  "message": "Không tìm thấy booking ID trong nội dung",
  "webhookId": "abc12345",
  "content": "VQRIO123",
  "orderCode": 123
}
```

## 🔍 Xử Lý Trong Code

### SimplePaymentController.cs

Code hiện tại xử lý webhook như sau:

1. **Đọc raw request body**
2. **Deserialize** thành `PayOsWebhookRequest`
3. **Kiểm tra code** = `"00"` (thành công)
4. **Extract booking ID** từ `description` (ví dụ: `"BOOKING7"` → `7`)
5. **Update booking status** thành `"Paid"`
6. **Trả về response** HTTP 200

### PayOsWebhookRequest Model

```csharp
public class PayOsWebhookRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } // "00" = success
    
    [JsonPropertyName("desc")]
    public string Desc { get; set; } // "success"
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("data")]
    public PayOsWebhookData? Data { get; set; }
    
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}
```

### PayOsWebhookData Model

Đã được cập nhật để bao gồm tất cả các trường từ PayOs API documentation.

## 📝 Ví Dụ Webhook Thực Tế

### Webhook Thành Công

```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 40043,
    "amount": 5000,
    "description": "BOOKING4",
    "accountNumber": "0901329227",
    "reference": "TF230204212323",
    "transactionDateTime": "2023-02-04 18:25:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb18",
    "code": "00",
    "desc": "Thành công"
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
```

### Xử Lý

1. **Code = "00"** → Thành công ✅
2. **Description = "BOOKING4"** → Extract booking ID = `4`
3. **Amount = 5000** → Verify với booking amount
4. **Update booking 4** → Status = `"Paid"`

## 🐛 Troubleshooting

### Lỗi: "Cannot extract booking ID"

**Nguyên nhân:**
- `description` không đúng format (ví dụ: `"VQRIO123"` thay vì `"BOOKING4"`)

**Giải pháp:**
- Kiểm tra format description khi tạo payment link
- Đảm bảo description = `"BOOKING{id}"` hoặc `"ORDER{id}"`

### Lỗi: "Invalid signature"

**Nguyên nhân:**
- Signature verification bật nhưng tính toán sai

**Giải pháp:**
- Tắt signature verification (`VerifySignature=false`) nếu không cần
- Hoặc kiểm tra ChecksumKey đúng chưa

### Lỗi: "Code != 00"

**Nguyên nhân:**
- Giao dịch không thành công

**Giải pháp:**
- Kiểm tra `code` và `desc` trong response
- Không update booking status nếu code != "00"

## 🔗 Links Quan Trọng

- **PayOs API Documentation:** https://payos.vn/docs/api/
- **PayOs Webhook Guide:** https://payos.vn/docs/tich-hop-webhook/
- **PayOs Signature Verification:** https://payos.vn/docs/tich-hop-webhook/kiem-tra-du-lieu-voi-signature/
- **Railway Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 📋 Checklist

- [x] ✅ Code đã xử lý đúng format PayOs webhook
- [x] ✅ Model `PayOsWebhookRequest` đầy đủ các trường
- [x] ✅ Model `PayOsWebhookData` đầy đủ các trường
- [x] ✅ Xử lý code "00" = thành công
- [x] ✅ Extract booking ID từ description
- [x] ✅ Update booking status thành "Paid"
- [ ] ⚠️ Signature verification đang tắt (có thể bật khi cần)

