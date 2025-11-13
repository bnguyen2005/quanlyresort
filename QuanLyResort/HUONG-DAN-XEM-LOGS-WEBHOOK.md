# 🔍 Hướng Dẫn Xem Logs Webhook SePay

## 📊 Tình Trạng Hiện Tại

Từ logs bạn gửi:
- ✅ Service hoạt động bình thường
- ✅ API endpoints đang được gọi (GET `/api/bookings/4`, `/api/bookings/my`)
- ❌ **Chưa thấy webhook SePay nào** - Cần test để kiểm tra code mới

## 🧪 Test Webhook Để Kiểm Tra Code Mới

### Bước 1: Test SePay Webhook

**Chạy lệnh này:**
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

### Bước 2: Xem Logs Ngay Sau Khi Test

**Vào Railway Dashboard → Logs**

**Tìm các dòng sau (ngay sau khi test):**

**Nếu code mới đã hoạt động (commit `42e8ab3`):**
```
[WEBHOOK] 📥 [WEBHOOK-xxx] Webhook received at ...
[WEBHOOK]    Raw request JSON: {"description":"BOOKING4","transferAmount":5000,...}
[WEBHOOK] 🔍 [WEBHOOK-xxx] Attempting to deserialize as PayOs format...
[WEBHOOK] 🔍 [WEBHOOK-xxx] PayOs deserialization result: Code=, Desc=, Success=False, Data=False
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
[WEBHOOK] 📥 [WEBHOOK-xxx] Webhook received at ...
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=NULL
[WEBHOOK] 🔍 [WEBHOOK-xxx] PayOs verification request (empty data)
```

## 🔍 Dấu Hiệu Code Mới Đã Hoạt Động

### Dấu Hiệu 1: TransferAmount Được Extract

**Tìm trong logs:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: ..., TransferAmount=5000
```

**Nếu thấy `TransferAmount=5000` (không phải NULL):**
- ✅ Code mới đã hoạt động
- ✅ JsonPropertyName attributes đã được áp dụng

### Dấu Hiệu 2: Using TransferAmount Field

**Tìm trong logs:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 5000
```

**Nếu thấy dòng này:**
- ✅ Code mới đã hoạt động
- ✅ TransferAmount được extract và sử dụng

### Dấu Hiệu 3: Final Extracted Amount

**Tìm trong logs:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Final extracted: Content='BOOKING4', Amount=5000, TransactionId='...'
[WEBHOOK] 📥 Webhook received: BOOKING4 - 5,000 VND
```

**Nếu thấy `Amount=5000` (không phải 0):**
- ✅ Code mới đã hoạt động
- ✅ Webhook sẽ xử lý thành công (không còn verification response)

## ❌ Dấu Hiệu Code Cũ (Chưa Deploy)

### Dấu Hiệu 1: TransferAmount = NULL

**Tìm trong logs:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: ..., TransferAmount=NULL
```

**Nếu thấy `TransferAmount=NULL`:**
- ❌ Code mới chưa được deploy
- ❌ JsonPropertyName attributes chưa được áp dụng

### Dấu Hiệu 2: Verification Response

**Response từ API:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

**Nếu thấy response này:**
- ❌ TransferAmount không được extract
- ❌ Code mới chưa được deploy

## 📋 Checklist

- [ ] Đã test SePay webhook
- [ ] Đã xem logs ngay sau khi test
- [ ] Đã tìm dòng "Simple deserialization result"
- [ ] Đã kiểm tra TransferAmount có giá trị không (5000 hay NULL?)
- [ ] Đã kiểm tra có dòng "Using TransferAmount field" không
- [ ] Đã xác nhận code mới đã hoạt động

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **Test ngay** - Test webhook và xem logs ngay sau đó
2. **Tìm đúng dòng** - Tìm dòng "Simple deserialization result" để xem TransferAmount
3. **Code mới** - Cần commit `42e8ab3` đã được deploy
4. **JsonPropertyName** - Đã thêm attributes cho SePay fields

## 🎯 Kết Luận

**Để xác nhận code mới đã hoạt động:**
1. Test SePay webhook với `transferAmount: 5000`
2. Xem logs Railway ngay sau khi test
3. Tìm dòng "Simple deserialization result" và kiểm tra `TransferAmount=5000` hay `TransferAmount=NULL`

**Nếu thấy `TransferAmount=5000`:**
- ✅ Code mới đã hoạt động
- ✅ SePay webhook sẽ xử lý thành công

**Nếu thấy `TransferAmount=NULL`:**
- ❌ Code mới chưa được deploy
- ❌ Cần redeploy lại

