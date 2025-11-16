# 📋 Format Webhook Thực Tế Của SePay

## 📥 Dữ Liệu Gửi Qua Webhook

**SePay sẽ gửi một request với phương thức POST, với nội dung như sau:**

```json
{
    "id": 92704,                              // ID giao dịch trên SePay
    "gateway": "Vietcombank",                 // Brand name của ngân hàng
    "transactionDate": "2023-03-25 14:02:37", // Thời gian xảy ra giao dịch phía ngân hàng
    "accountNumber": "0123499999",            // Số tài khoản ngân hàng
    "code": null,                              // Mã code thanh toán (sepay tự nhận diện dựa vào cấu hình tại Công ty -> Cấu hình chung)
    "content": "chuyen tien mua iphone",      // Nội dung chuyển khoản
    "transferType": "in",                      // Loại giao dịch. "in" là tiền vào, "out" là tiền ra
    "transferAmount": 2277000,                 // Số tiền giao dịch
    "accumulated": 19077000,                   // Số dư tài khoản (lũy kế)
    "subAccount": null,                       // Tài khoản ngân hàng phụ (tài khoản định danh)
    "referenceCode": "MBVCB.3278907687",       // Mã tham chiếu của tin nhắn sms
    "description": ""                          // Toàn bộ nội dung tin nhắn sms
}
```

## 🔍 Các Trường Quan Trọng

### 1. **content** (Nội dung chuyển khoản)
- **Vai trò:** Chứa thông tin để xác định booking/order
- **Ví dụ:** `"BOOKING4"`, `"ORDER7"`
- **Cách sử dụng:** Backend sẽ extract booking ID từ content này

### 2. **transferAmount** (Số tiền giao dịch)
- **Vai trò:** Số tiền thực tế được chuyển
- **Ví dụ:** `2277000` (2,277,000 VND)
- **Cách sử dụng:** Backend sẽ verify số tiền này với booking/order amount

### 3. **transferType** (Loại giao dịch)
- **Vai trò:** Xác định tiền vào hay tiền ra
- **Giá trị:** `"in"` (tiền vào) hoặc `"out"` (tiền ra)
- **Cách sử dụng:** Chỉ xử lý khi `transferType == "in"`

### 4. **id** (ID giao dịch trên SePay)
- **Vai trò:** Mã định danh giao dịch trên SePay
- **Ví dụ:** `92704`
- **Cách sử dụng:** Dùng làm transaction ID để tracking

### 5. **referenceCode** (Mã tham chiếu)
- **Vai trò:** Mã tham chiếu của tin nhắn SMS
- **Ví dụ:** `"MBVCB.3278907687"`
- **Cách sử dụng:** Fallback cho transaction ID nếu không có `id`

## 🔧 Cách Backend Xử Lý

### Bước 1: Parse Webhook Request

Backend sẽ deserialize JSON vào `SimpleWebhookRequest`:

```csharp
public class SimpleWebhookRequest
{
    [JsonPropertyName("id")]
    public int? Id { get; set; } // ID giao dịch trên SePay
    
    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; } // Brand name của ngân hàng
    
    [JsonPropertyName("content")]
    public string? Content { get; set; } // Nội dung chuyển khoản
    
    [JsonPropertyName("transferAmount")]
    public decimal? TransferAmount { get; set; } // Số tiền giao dịch
    
    [JsonPropertyName("transferType")]
    public string? TransferType { get; set; } // Loại giao dịch: "in" hoặc "out"
    
    // ... các trường khác
}
```

### Bước 2: Extract Thông Tin

**Extract Content (Booking/Order ID):**
```csharp
// Ưu tiên dùng Content, nếu không có thì dùng Description
if (!string.IsNullOrEmpty(simpleRequest.Content))
{
    content = simpleRequest.Content.Trim(); // Ví dụ: "BOOKING4"
}
```

**Extract Amount:**
```csharp
// Ưu tiên dùng TransferAmount
if (simpleRequest.TransferAmount.HasValue && simpleRequest.TransferAmount.Value > 0)
{
    amount = simpleRequest.TransferAmount.Value; // Ví dụ: 2277000
}
```

**Extract Transaction ID:**
```csharp
// Ưu tiên dùng Id (int), sau đó ReferenceCode
if (simpleRequest.Id.HasValue)
{
    transactionId = simpleRequest.Id.Value.ToString(); // Ví dụ: "92704"
}
else if (!string.IsNullOrEmpty(simpleRequest.ReferenceCode))
{
    transactionId = simpleRequest.ReferenceCode; // Ví dụ: "MBVCB.3278907687"
}
```

### Bước 3: Parse Booking/Order ID

**Từ Content:**
- Format: `"BOOKING4"` → Booking ID = 4
- Format: `"ORDER7"` → Order ID = 7

**Logic:**
```csharp
// Extract booking ID từ content "BOOKING4"
if (content.StartsWith("BOOKING", StringComparison.OrdinalIgnoreCase))
{
    var bookingIdStr = content.Substring(7); // "4"
    if (int.TryParse(bookingIdStr, out var bookingId))
    {
        // Process booking payment
    }
}
```

## 📊 Ví Dụ Webhook Thực Tế

### Ví Dụ 1: Booking Payment

```json
{
    "id": 92704,
    "gateway": "Vietcombank",
    "transactionDate": "2023-03-25 14:02:37",
    "accountNumber": "0123499999",
    "code": null,
    "content": "BOOKING4",
    "transferType": "in",
    "transferAmount": 5000000,
    "accumulated": 19077000,
    "subAccount": null,
    "referenceCode": "MBVCB.3278907687",
    "description": ""
}
```

**Backend sẽ:**
1. Extract `content = "BOOKING4"`
2. Extract `amount = 5000000`
3. Parse booking ID = 4
4. Verify amount với booking 4
5. Update booking status = "Paid"

### Ví Dụ 2: Restaurant Order Payment

```json
{
    "id": 92705,
    "gateway": "MB",
    "transactionDate": "2023-03-25 14:05:12",
    "accountNumber": "0901329227",
    "code": null,
    "content": "ORDER7",
    "transferType": "in",
    "transferAmount": 500000,
    "accumulated": 19577000,
    "subAccount": null,
    "referenceCode": "MBMB.3278907688",
    "description": ""
}
```

**Backend sẽ:**
1. Extract `content = "ORDER7"`
2. Extract `amount = 500000`
3. Parse order ID = 7
4. Verify amount với order 7
5. Update order payment status = "Paid"

## ✅ Response Format

**Backend phải trả về:**

```json
{
    "success": true,
    "message": "Thanh toán thành công",
    "bookingId": 4,
    "bookingCode": "BK-2023-001",
    "webhookId": "abc12345",
    "processedAt": "2023-03-25T14:02:37Z",
    "durationMs": 150
}
```

**HTTP Status Code:** `201` (hoặc `200`)

## 🔍 Logs Mẫu

**Khi nhận webhook, backend sẽ log:**

```
[WEBHOOK] 📥 [WEBHOOK-abc12345] Webhook received at 2023-03-25 14:02:37
[WEBHOOK] 📋 [WEBHOOK-abc12345] Detected Simple/SePay format
[WEBHOOK] 🔍 [WEBHOOK-abc12345] SePay request fields: Id=92704, Gateway=Vietcombank, Content='BOOKING4', TransferAmount=5000000, TransferType=in
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Using Content field (SePay): 'BOOKING4'
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Using TransferAmount field (SePay): 5,000,000 VND
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Using Id field (SePay): 92704
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Bank Gateway: Vietcombank
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Account Number: 0123499999
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Transfer Type: in
[WEBHOOK] 🔍 [WEBHOOK-abc12345] Final extracted: Content='BOOKING4', Amount=5,000,000 VND, TransactionId='92704'
```

## 📋 Checklist

- [x] DTO đã được cập nhật với tất cả các trường từ SePay
- [x] Logic extract content từ field `content` (không phải `description`)
- [x] Logic extract amount từ field `transferAmount` (không phải `amount`)
- [x] Logic extract transaction ID từ field `id` hoặc `referenceCode`
- [x] Logging đầy đủ để debug
- [x] Response format đúng với `success: true` và HTTP 201

## 🔗 Links

- **SePay Dashboard:** https://my.sepay.vn
- **Railway Logs:** Railway Dashboard → Service → Logs
- **Website:** https://quanlyresort-production.up.railway.app

## 💡 Lưu Ý

1. **Content field:** Quan trọng nhất - chứa booking/order ID
2. **TransferAmount:** Số tiền thực tế được chuyển
3. **TransferType:** Chỉ xử lý khi = "in" (tiền vào)
4. **Id/ReferenceCode:** Dùng để tracking transaction
5. **Response:** Phải có `success: true` và HTTP status 201

