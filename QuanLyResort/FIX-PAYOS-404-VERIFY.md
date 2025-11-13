# 🔧 Fix PayOs Webhook 404 Verification Error

## ❌ Lỗi Hiện Tại

PayOs API trả về:
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 404"
}
```

**Nguyên nhân:**
- PayOs đang cố verify webhook URL nhưng nhận được 404
- Có thể PayOs đang gọi endpoint khác hoặc có vấn đề với routing

## ✅ Giải Pháp

### Bước 1: Kiểm Tra Endpoint Hoạt Động

Endpoint đã hoạt động tốt (đã test):
```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
# Response: {"status":"active","endpoint":"/api/simplepayment/webhook",...}
```

### Bước 2: Đợi PayOs Verify Tự Động

PayOs có thể cần thời gian để verify webhook URL. Đợi 5-10 phút và thử lại.

### Bước 3: Thử Gọi API Lại

Sau khi đợi, thử gọi API lại:

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
```

### Bước 4: Kiểm Tra Logs Trên Railway

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Logs"**
3. **Tìm requests từ PayOs:**
   - IP addresses từ PayOs
   - User-Agent có chứa "PayOs"
   - Requests đến `/api/simplepayment/webhook`

**Nếu thấy:**
```
[WEBHOOK-VERIFY] PayOs verification request received
```
→ PayOs đã verify thành công

**Nếu không thấy:**
→ PayOs chưa verify được, cần kiểm tra thêm

## 🔍 Debug Steps

### 1. Test Tất Cả Endpoints

```bash
# Test GET webhook
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook

# Test POST webhook (empty body)
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d ''

# Test webhook-status
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status
```

### 2. Kiểm Tra Routing

Đảm bảo routing đúng:
- Path: `/api/simplepayment/webhook`
- Method: GET và POST đều được hỗ trợ
- Không cần authentication

### 3. Kiểm Tra CORS (Nếu Cần)

PayOs có thể cần CORS headers. Kiểm tra `Program.cs` có config CORS cho PayOs không.

## 💡 Giải Pháp Thay Thế

### Option 1: Đợi Và Thử Lại

PayOs có thể cần thời gian để verify. Đợi 10-15 phút và thử lại.

### Option 2: Liên Hệ PayOs Support

Nếu vẫn lỗi sau 15 phút:
1. Liên hệ PayOs support
2. Cung cấp:
   - Webhook URL: `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - Client ID: `c704495b-5984-4ad3-aa23-b2794a02aa83`
   - Lỗi: "Request failed with status code 404"

### Option 3: Test Webhook Thủ Công

Sau khi config, PayOs sẽ gửi test webhook. Kiểm tra logs để xem có nhận được không.

## 📋 Checklist

- [ ] Endpoint hoạt động (đã test GET và POST)
- [ ] Đã gọi PayOs API để config webhook
- [ ] Đã đợi 10-15 phút để PayOs verify
- [ ] Đã kiểm tra logs trên Railway
- [ ] Đã thử gọi API lại
- [ ] Đã test tạo payment link

## 🎯 Kết Quả Mong Đợi

Sau khi PayOs verify thành công:
- ✅ PayOs sẽ chấp nhận webhook URL
- ✅ Có thể tạo payment link thành công
- ✅ Webhook sẽ được gọi khi thanh toán thành công

## ⚠️ Lưu Ý

- PayOs có thể cần thời gian để verify (5-15 phút)
- Nếu vẫn lỗi sau 15 phút, có thể cần liên hệ PayOs support
- Webhook URL phải accessible từ internet (Railway đã có public domain)

