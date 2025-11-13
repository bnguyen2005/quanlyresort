# 🧪 Kết Quả Test SePay Webhook

## 📊 Tình Trạng Hiện Tại

### Test Results

**✅ Passed: 2/5**
- Test 3: Format với description → 200 OK (nhưng trả về verification response)
- Test 5: Empty body (verification) → 200 OK

**❌ Failed: 3/5**
- Test 1: SePay format với transferAmount → 404 (Booking không tồn tại - đúng vì test data)
- Test 2: Simple format → 404 (Booking không tồn tại - đúng vì test data)
- Test 4: Restaurant Order → 404 (Order không tồn tại - đúng vì test data)

## ⚠️ Vấn Đề

### Test 3: TransferAmount Không Được Extract

**Request:**
```json
{
  "description": "BOOKING4",
  "transferAmount": 5000,
  "transferType": "IN"
}
```

**Response:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

**Vấn đề:**
- Trả về verification response thay vì xử lý webhook
- `TransferAmount` không được extract (Amount vẫn = 0)
- Code mới (commit `42e8ab3`) có thể chưa được deploy

## 🔍 Cách Kiểm Tra

### Bước 1: Xem Logs Railway

**Vào Railway Dashboard → Logs**

**Tìm khi test SePay webhook:**

**Nếu code mới đã hoạt động:**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=5000
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 5000
[WEBHOOK] 📥 Webhook received: BOOKING4 - 5,000 VND
```

**Nếu code cũ (chưa có JsonPropertyName):**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result: Content=..., Amount=0, TransferAmount=NULL
[WEBHOOK] 🔍 [WEBHOOK-xxx] PayOs verification request (empty data)
```

### Bước 2: Kiểm Tra Deployment

**Railway Dashboard → Deployments**

**Tìm deployment mới nhất:**
- Commit: `42e8ab3` - "fix: Add JsonPropertyName attributes..."
- Hoặc: `4bae202` - "trigger: Force Railway redeploy..."
- Status: "Active"

**Nếu không thấy:**
- Code mới chưa được deploy
- Cần trigger deploy lại

### Bước 3: Test Với Booking Thật

**Nếu có booking thật trong database:**

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING1",
    "transferAmount": 1000000,
    "transferType": "IN",
    "id": "sepay-real-123",
    "referenceCode": "REF-REAL-456"
  }'
```

**Kết quả mong đợi:**
```json
{
  "message": "Đã thanh toán rồi",
  "bookingId": 1,
  "webhookId": "..."
}
```

## 🔧 Giải Pháp

### Nếu Code Mới Chưa Được Deploy

**Option 1: Trigger Redeploy**
```bash
cd QuanLyResort
./trigger-redeploy.sh
```

**Option 2: Manual Redeploy**
1. Railway Dashboard → Deployments
2. Click "Redeploy" trên deployment mới nhất
3. Đợi 2-3 phút

### Nếu Code Đã Deploy Nhưng Vẫn Không Hoạt Động

**Kiểm tra logs để tìm lỗi:**
- JSON deserialization error
- TransferAmount vẫn NULL
- Logic processing issue

**Fix code và redeploy**

## 📋 Checklist

- [ ] Đã xem logs Railway (code mới đã hoạt động?)
- [ ] Đã kiểm tra deployment (commit `42e8ab3` đã deploy?)
- [ ] Đã test với booking thật (nếu có)
- [ ] TransferAmount được extract đúng
- [ ] Webhook xử lý thành công

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Service Deployments:** Railway Dashboard → Deployments
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **Test data** - Booking 4 không tồn tại nên 404 là đúng
2. **TransferAmount** - Cần code mới (commit `42e8ab3`) để extract đúng
3. **Logs** - Xem logs để xác nhận code mới đã hoạt động
4. **Deploy time** - Railway mất 2-3 phút để deploy

## 🎯 Kết Luận

**Tình trạng:**
- ✅ Webhook endpoint hoạt động (200 OK)
- ⚠️ TransferAmount chưa được extract (có thể code mới chưa deploy)
- ⚠️ Test 3 vẫn trả về verification response

**Bước tiếp theo:**
1. Xem logs Railway để xác nhận code mới
2. Nếu code mới chưa deploy → Trigger redeploy
3. Test lại với booking thật (nếu có)

