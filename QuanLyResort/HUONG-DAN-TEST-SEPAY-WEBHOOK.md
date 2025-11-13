# 🧪 Hướng Dẫn Test SePay Webhook

## 📊 Tình Trạng Hiện Tại

Từ logs Railway, tôi thấy:
- ✅ Service đang hoạt động bình thường
- ✅ API endpoints đang được gọi (GET `/api/bookings/my`, `/api/rooms`, etc.)
- ❌ **Không thấy webhook SePay nào** - SePay chưa gửi webhook đến

## 🔍 Cách Kiểm Tra

### Bước 1: Xem Logs Railway

**Vào Railway Dashboard → Logs**

**Tìm webhook SePay:**
- Tìm: `[WEBHOOK] 📥 [WEBHOOK-xxx] Webhook received`
- Tìm: `[WEBHOOK] 🔍 [WEBHOOK-xxx] Simple deserialization result`
- Tìm: `[WEBHOOK] 🔍 [WEBHOOK-xxx] Using TransferAmount field`

**Nếu không thấy:**
- SePay chưa gửi webhook
- Hoặc webhook bị lỗi trước khi đến server

### Bước 2: Test Webhook Thủ Công

**Test với format SePay:**

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

### Bước 3: Kiểm Tra Deployment

**Railway Dashboard → Deployments**

**Tìm deployment mới nhất:**
- Commit: `42e8ab3` - "fix: Add JsonPropertyName attributes..."
- Status: "Active"

**Nếu không thấy:**
- Code mới chưa được deploy
- Cần trigger redeploy

## 🧪 Test Script

**Chạy script test:**

```bash
cd QuanLyResort
./test-sepay-webhook.sh
```

**Kết quả mong đợi:**
- Test 3: Format với description → Extract được booking ID và TransferAmount
- Logs hiển thị: `Using TransferAmount field (SePay format): 5000`

## 📋 Checklist

- [ ] Đã xem logs Railway (có webhook SePay không?)
- [ ] Đã test webhook thủ công (TransferAmount được extract?)
- [ ] Đã kiểm tra deployment (code mới đã deploy?)
- [ ] Đã chạy test script (kết quả như mong đợi?)
- [ ] SePay đã gửi webhook thật (từ SePay dashboard)

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Logs:** Railway Dashboard → Logs
- **Service Deployments:** Railway Dashboard → Deployments
- **Webhook Endpoint:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`

## 💡 Lưu Ý

1. **SePay webhook** - SePay sẽ tự động gửi webhook khi có giao dịch
2. **Test thủ công** - Có thể test webhook thủ công để kiểm tra code
3. **Code mới** - Cần commit `42e8ab3` đã được deploy để extract TransferAmount
4. **Logs** - Xem logs để xác nhận webhook đã được xử lý

## 🎯 Kết Luận

**Từ logs hiện tại:**
- ✅ Service hoạt động bình thường
- ❌ Chưa thấy webhook SePay nào

**Bước tiếp theo:**
1. Test webhook thủ công để kiểm tra code
2. Kiểm tra SePay dashboard xem có giao dịch không
3. Xem logs Railway khi SePay gửi webhook thật
