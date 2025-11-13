# 🔧 Fix TransferAmount Extraction - SePay Webhook

## 🐛 Vấn Đề

Từ logs Railway, tôi thấy:
- ✅ Code mới đã được deploy và hoạt động
- ✅ Đã extract được content: `ORDER7`
- ✅ Đã extract được restaurant order ID: `7`
- ❌ **Không extract được `transferAmount`**: `Amount=0` (nhưng trong raw request có `"transferAmount": 150000`)

**Logs:**
```
[WEBHOOK] 🔍 [WEBHOOK-6c044259] Simple deserialization result: Content=ORDER7, Amount=0
[WEBHOOK] 📋 [WEBHOOK-6c044259] Detected Simple format
[WEBHOOK] 📥 Webhook received: ORDER7 - 0 VND
```

**Raw request JSON:**
```json
{
  "transferAmount": 150000,
  "description": "ORDER7",
  "content": "ORDER7"
}
```

## ✅ Giải Pháp

### 1. Thêm JsonPropertyName Attributes

**Vấn đề:** JSON property names là camelCase (`transferAmount`), nhưng C# properties là PascalCase (`TransferAmount`). Mặc dù có `PropertyNameCaseInsensitive = true`, nhưng cần đảm bảo mapping chính xác.

**Fix:** Thêm `[JsonPropertyName]` attributes cho các SePay fields:

```csharp
[JsonPropertyName("description")]
public string? Description { get; set; }

[JsonPropertyName("transferAmount")]
public decimal? TransferAmount { get; set; }

[JsonPropertyName("referenceCode")]
public string? ReferenceCode { get; set; }

[JsonPropertyName("transferType")]
public string? TransferType { get; set; }

[JsonPropertyName("id")]
public string? Id { get; set; }
```

### 2. Cải Thiện Logging

**Thêm log để debug TransferAmount extraction:**

```csharp
_logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Simple deserialization result: Content={Content}, Amount={Amount}, TransferAmount={TransferAmount}", 
    webhookId, simpleRequest?.Content ?? "NULL", simpleRequest?.Amount ?? 0, simpleRequest?.TransferAmount?.ToString() ?? "NULL");
```

## 📋 Thay Đổi

### File: `QuanLyResort/Controllers/SimplePaymentController.cs`

1. **Thêm JsonPropertyName attributes** cho SePay fields (dòng 1029-1038)
2. **Cải thiện logging** để debug TransferAmount extraction (dòng 183)

## 🧪 Test Sau Khi Deploy

### Test SePay Webhook với TransferAmount

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN"
  }'
```

**Kết quả mong đợi:**
```json
{
  "message": "Đã thanh toán rồi",
  "bookingId": 4,
  "webhookId": "..."
}
```

**Logs mong đợi:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=NULL, Amount=0, TransferAmount=5000
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 5000
[WEBHOOK] 📥 Webhook received: BOOKING4 - 5,000 VND
```

## 🔍 Kiểm Tra Logs

**Vào Railway Dashboard → Logs**

**Tìm các dòng sau:**

1. **Deserialization result:**
   ```
   [WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=..., TransferAmount=...
   ```

2. **TransferAmount extraction:**
   ```
   [WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 150000
   ```

3. **Final extracted:**
   ```
   [WEBHOOK] 🔍 [WEBHOOK-xxx] Final extracted: Content='ORDER7', Amount=150000, TransactionId='...'
   ```

## 📋 Checklist

- [x] Đã thêm JsonPropertyName attributes
- [x] Đã cải thiện logging
- [x] Đã commit và push code
- [ ] Đợi Railway deploy (2-3 phút)
- [ ] Test SePay webhook với TransferAmount
- [ ] Kiểm tra logs xác nhận TransferAmount được extract
- [ ] Xác nhận booking/order được update với đúng amount

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **Deploy time** - Railway mất 2-3 phút để deploy
2. **Service restart** - Service sẽ restart tự động sau khi deploy
3. **Logs delay** - Logs có thể delay vài giây
4. **Test ngay** - Sau khi deploy xong, test lại SePay webhook

## 🎯 Kết Quả Mong Đợi

Sau khi deploy fix này:
- ✅ TransferAmount sẽ được extract từ SePay webhook
- ✅ Amount sẽ không còn = 0
- ✅ Booking/order sẽ được update với đúng amount
- ✅ Logs sẽ hiển thị TransferAmount value

