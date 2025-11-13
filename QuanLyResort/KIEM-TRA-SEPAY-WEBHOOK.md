# ✅ Kiểm Tra SePay Webhook - TransferAmount Extraction

## 🎯 Mục Tiêu

Kiểm tra xem webhook SePay đã hoạt động và `TransferAmount` đã được extract đúng chưa sau khi deploy code mới (commit `42e8ab3`).

## 🧪 Test SePay Webhook

### Test 1: Format với description và transferAmount

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN",
    "id": "sepay-test-123",
    "referenceCode": "REF-TEST-456"
  }'
```

**Kết quả mong đợi (code mới):**
```json
{
  "message": "Đã thanh toán rồi",
  "bookingId": 4,
  "webhookId": "..."
}
```

**Kết quả nếu code cũ:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

### Test 2: Format SePay đầy đủ

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "sepay-1763051618",
    "referenceCode": "REF-1763051618",
    "transferType": "IN",
    "transferAmount": 150000,
    "description": "BOOKING4",
    "content": "BOOKING4",
    "accountNumber": "0901329227",
    "accountName": "Resort Deluxe",
    "bankName": "MB",
    "transactionDate": "2025-11-13T12:35:00Z"
  }'
```

## 🔍 Kiểm Tra Logs Railway

### Vào Railway Dashboard → Logs

**Tìm khi test SePay webhook:**

**Nếu code mới đã hoạt động:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=5000
[WEBHOOK] 📋 [WEBHOOK-xxx] Detected Simple/SePay format
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple request fields: Content='...', Description='BOOKING4', Amount=0, TransferAmount=5000
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using Description field (SePay format): 'BOOKING4'
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 5000
[WEBHOOK] 🔍 [WEBHOOK-xxx] Final extracted: Content='BOOKING4', Amount=5000, TransactionId='...'
[WEBHOOK] 📥 Webhook received: BOOKING4 - 5,000 VND
```

**Nếu code cũ (chưa có JsonPropertyName):**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=NULL
[WEBHOOK] 🔍 [WEBHOOK-xxx] PayOs verification request (empty data)
```

## 📋 Checklist

- [ ] Đã test SePay webhook với transferAmount
- [ ] Đã xem logs Railway (TransferAmount được extract?)
- [ ] Đã kiểm tra deployment (commit `42e8ab3` đã deploy?)
- [ ] TransferAmount được extract đúng (không còn = 0)
- [ ] Webhook xử lý thành công (không còn verification response)

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Service Deployments:** Railway Dashboard → Deployments
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **Code mới** - Cần commit `42e8ab3` đã được deploy
2. **JsonPropertyName** - Đã thêm attributes cho SePay fields
3. **TransferAmount** - Sẽ được extract từ `transferAmount` field
4. **Logs** - Xem logs để xác nhận code mới đã hoạt động

## 🎯 Kết Quả Mong Đợi

Sau khi deploy code mới:
- ✅ TransferAmount sẽ được extract từ SePay webhook
- ✅ Amount sẽ không còn = 0
- ✅ Webhook sẽ xử lý thành công (không còn verification response)
- ✅ Booking sẽ được update với đúng amount

