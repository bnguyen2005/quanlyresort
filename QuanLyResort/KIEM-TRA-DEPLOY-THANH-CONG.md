# 🔍 Kiểm Tra Deploy Thành Công

## 📊 Tình Trạng Hiện Tại

Từ Railway Dashboard:
- ✅ **Deployment successful** - 7 minutes ago
- ✅ **Status: ACTIVE** - Service đang chạy
- ✅ **Commit:** `8472ecd` - Code mới đã được deploy

## 🔍 Cách Kiểm Tra Code Mới Đã Hoạt Động

### Test 1: Test Webhook Endpoint

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

### Test 2: Test SePay Format (Description Field)

```bash
curl -X POST "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "BOOKING4",
    "transferAmount": 5000,
    "transferType": "IN"
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

### Test 3: Chạy Script Test SePay

```bash
cd QuanLyResort
./test-sepay-webhook.sh
```

**Kiểm tra Test 3:**
- ✅ **Thành công (code mới):** Extract được booking ID từ description
- ⚠️ **Thất bại (code cũ):** Trả về verification response

## 🔍 Kiểm Tra Logs

### Vào Railway Dashboard → Logs

Tìm các dòng sau để xác nhận code mới:

**Code mới (có SePay support):**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple request fields: Content='NULL', Description='BOOKING4', Amount=0, TransferAmount=5000
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using Description field (SePay format): 'BOOKING4'
[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field (SePay format): 5000
[WEBHOOK] ✅✅✅ SUCCESS: Extracted bookingId from description: 4
```

**Code cũ (không có SePay support):**
```
[WEBHOOK] 🔍 [WEBHOOK-xxx] PayOs verification request (empty data)
```

## 🐛 Nếu Không Thấy Thay Đổi

### Nguyên Nhân Có Thể

1. **Service chưa restart** - Code mới chưa được load
2. **Cache** - Browser/Service đang cache code cũ
3. **Deploy chưa hoàn tất** - Service đang restart

### Giải Pháp

#### 1. Restart Service

**Cách 1: Railway Dashboard**
1. Vào Railway Dashboard
2. Service `quanlyresort`
3. Tab "Settings"
4. Click "Restart" hoặc "Redeploy"

**Cách 2: Redeploy**
1. Tab "Deployments"
2. Click "Redeploy" trên deployment mới nhất
3. Chọn "Deploy"

#### 2. Kiểm Tra Logs

Vào Railway Dashboard → Logs và tìm:
- Service startup logs
- Code initialization logs
- Webhook processing logs

#### 3. Test Lại

Sau khi restart, test lại:
```bash
./test-sepay-webhook.sh
```

## ✅ Xác Nhận Code Mới Đã Hoạt Động

### Dấu Hiệu Code Mới:

1. **Test 3 thành công:**
   - Extract được booking ID từ description
   - Response có `bookingId: 4`

2. **Logs có SePay format:**
   - `Using Description field (SePay format)`
   - `Using TransferAmount field (SePay format)`

3. **SimpleWebhookRequest có thêm fields:**
   - `Description`, `TransferAmount`, `Id`, `ReferenceCode`

## 📋 Checklist

- [ ] Service status: ACTIVE
- [ ] Deployment successful
- [ ] Đã test webhook endpoint (trả về 200 OK)
- [ ] Đã test SePay format với description (extract được booking ID)
- [ ] Đã xem logs (có SePay format messages)
- [ ] Code mới đã hoạt động

## 🔗 Links Quan Trọng

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

