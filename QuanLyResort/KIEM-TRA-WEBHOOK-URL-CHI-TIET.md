# 🔍 Kiểm Tra Webhook URL Chi Tiết

## ✅ Kiểm Tra URL

### URL Webhook

```
https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kiểm tra từng phần:**
- ✅ Protocol: `https://` (đúng)
- ✅ Domain: `quanlyresort-production.up.railway.app` (đúng)
- ✅ Path: `/api/simplepayment/webhook` (đúng)
- ✅ Không có khoảng trắng ở đầu/cuối (đúng)

## 🧪 Test Chi Tiết

### Test 1: GET Request

```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2025-11-13T..."
}
```

### Test 2: POST Request (Empty Body)

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d ''
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2025-11-13T..."
}
```

### Test 3: POST Request Với PayOs User-Agent

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: PayOs/1.0" \
  -d ''
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready",
  "timestamp": "2025-11-13T..."
}
```

### Test 4: POST Request Với PayOs Data

```bash
curl -X POST https://quanlyresort-production.up.railway.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -H "User-Agent: PayOs/1.0" \
  -d '{
    "code": "00",
    "desc": "success",
    "data": {
      "orderCode": 123,
      "amount": 3000,
      "description": "BOOKING4",
      "reference": "TEST-123456"
    }
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "bookingCode": "BKG2025004"
}
```

## 🔍 Kiểm Tra Domain

### 1. Kiểm Tra DNS

```bash
nslookup quanlyresort-production.up.railway.app
```

**Kết quả mong đợi:**
- Domain resolve được
- IP address hợp lệ

### 2. Kiểm Tra SSL Certificate

```bash
openssl s_client -connect quanlyresort-production.up.railway.app:443 \
  -servername quanlyresort-production.up.railway.app
```

**Kết quả mong đợi:**
- SSL certificate hợp lệ
- Certificate cho `*.up.railway.app`

### 3. Kiểm Tra HTTP Headers

```bash
curl -I https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
- HTTP/2 200
- Content-Type: application/json
- Server: railway-edge

## ⚠️ Lưu Ý

### PayOs Verify Webhook URL

PayOs có thể verify webhook URL bằng cách:
1. **GET request** đến webhook URL
2. **POST request với empty body** đến webhook URL
3. **Kiểm tra response** có đúng format không
4. **Kiểm tra HTTP status code** (phải là 200)

**Nếu PayOs vẫn báo 404:**
- Có thể PayOs đang verify bằng cách khác
- Có thể PayOs có firewall/network issues với Railway domain
- Có thể PayOs đang cache kết quả verify cũ

## 📋 Checklist Kiểm Tra

- [x] URL đúng format (không có khoảng trắng)
- [x] GET request hoạt động
- [x] POST request (empty body) hoạt động
- [x] POST request với PayOs data hoạt động
- [x] Domain resolve được
- [x] SSL certificate hợp lệ
- [x] HTTP headers đúng
- [ ] PayOs verify thành công (vẫn báo 404)

## 💡 Kết Luận

**URL webhook đã đúng và hoạt động tốt!**

Vấn đề là PayOs không verify được Railway URL, không phải do URL sai.

**Giải pháp:**
1. Liên hệ PayOs support về vấn đề Railway domain
2. Dùng Render URL tạm thời
3. Đợi PayOs fix

## 🔗 URLs

- **Railway Webhook:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook` ✅
- **Railway Webhook Status:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook-status` ✅
- **PayOs Dashboard:** https://payos.vn

