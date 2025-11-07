# 🔍 Debug: Webhook Không Tự Động

## ❌ Vấn Đề

Từ logs terminal, **KHÔNG có webhook POST request nào** đến server:
- ✅ Có GET requests: `/api/bookings/9`, `/api/bookings/my`
- ❌ **KHÔNG có POST request**: `/api/simplepayment/webhook`

**Kết luận:** PayOs **KHÔNG gọi webhook tự động** sau khi thanh toán thành công.

## 🔍 Nguyên Nhân

### 1. PayOs Chưa Config Webhook URL

**PayOs KHÔNG có dashboard** để config webhook. Phải config qua **API**.

### 2. Ngrok Free Plan Có Warning Page

Ngrok free plan hiển thị warning page khi PayOs verify webhook URL:
- PayOs không thể verify được
- PayOs không kích hoạt webhook

### 3. PayOs Đang Gọi Endpoint Khác

Có thể PayOs đang gọi endpoint khác (không phải `/api/simplepayment/webhook`).

## ✅ Giải Pháp

### Bước 1: Kiểm Tra Ngrok Đang Chạy

```bash
# Kiểm tra ngrok có đang chạy không
curl http://localhost:4040/api/tunnels 2>/dev/null | jq '.tunnels[0].public_url' || echo "Ngrok không chạy"
```

**Nếu ngrok không chạy:**
```bash
ngrok http 5130
```

**Copy URL từ ngrok:**
```
Forwarding: https://069c46a78b2b.ngrok-free.app -> http://localhost:5130
```

### Bước 2: Config PayOs Webhook URL Qua API

**Option A: Dùng Script (Khuyến Nghị)**

```bash
cd QuanLyResort
./config-payos-webhook.sh https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook
```

**Option B: Gọi API Thủ Công**

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook"}'
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://069c46a78b2b.ngrok.io/api/simplepayment/webhook"
  }
}
```

**Nếu lỗi "Webhook url invalid":**
- Ngrok free plan có warning page
- PayOs không thể verify được
- **Giải pháp:** Dùng ngrok paid plan hoặc deploy lên server thật

### Bước 3: Test Webhook Endpoint

**Test 1: Kiểm Tra Endpoint Hoạt Động**

```bash
# Test webhook status
curl http://localhost:5130/api/simplepayment/webhook-status
```

**Kết quả:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "timestamp": "2025-11-07T00:00:00Z",
  "supportedFormats": [
    "BOOKING-{id}",
    "BOOKING-BKG{id}",
    "{id} (direct booking ID)"
  ]
}
```

**Test 2: Test Manual Webhook**

```bash
curl -X POST https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING9", "amount": 5000}'
```

**Kiểm tra backend logs:**
```
📥 [WEBHOOK-xxx] Webhook received: BOOKING9 - 5,000 VND
✅ [WEBHOOK-xxx] Extracted booking ID: 9
✅ [WEBHOOK-xxx] Booking BKG2025009 - Status: Paid
```

### Bước 4: Thanh Toán Thật và Quan Sát

1. **Quét QR và thanh toán** với nội dung: `BOOKING9`
2. **Quan sát backend logs** (terminal chạy backend):
   - Nếu thấy `📥 [WEBHOOK-xxx]` → Webhook hoạt động! ✅
   - Nếu KHÔNG thấy → PayOs vẫn chưa gọi webhook ❌

## 🔧 Nếu PayOs Vẫn Không Gọi Webhook

### Giải Pháp 1: Dùng Ngrok Paid Plan

Ngrok paid plan không có warning page:
- PayOs có thể verify webhook URL
- Webhook sẽ hoạt động tự động

### Giải Pháp 2: Deploy Lên Server Thật

Deploy backend lên server có domain thật:
- PayOs có thể verify webhook URL
- Webhook sẽ hoạt động tự động

### Giải Pháp 3: Gọi Manual Webhook (Tạm Thời)

Sau khi thanh toán thành công, gọi manual webhook:

```bash
# Sau khi thanh toán BOOKING9 với 5,000 VND
curl -X POST https://069c46a78b2b.ngrok-free.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING9", "amount": 5000}'
```

**Hoặc dùng script tự động:**

```bash
# Tạo script auto-webhook.sh
#!/bin/bash
BOOKING_ID=$1
AMOUNT=$2
NGROK_URL="https://069c46a78b2b.ngrok-free.app"

curl -X POST "$NGROK_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "{\"content\": \"BOOKING$BOOKING_ID\", \"amount\": $AMOUNT}"
```

## 📋 Checklist Debug

- [ ] Ngrok đang chạy
- [ ] Backend đang chạy trên port 5130
- [ ] Webhook endpoint hoạt động (`/api/simplepayment/webhook-status`)
- [ ] PayOs webhook URL đã được config qua API
- [ ] Test manual webhook thành công
- [ ] Backend logs hiển thị webhook received
- [ ] Thanh toán thật và quan sát logs

## 🎯 Kết Quả Mong Đợi

Sau khi config thành công:

1. **Thanh toán thành công** → PayOs tự động gọi webhook
2. **Backend logs:**
   ```
   📥 [WEBHOOK-xxx] Webhook received: BOOKING9 - 5,000 VND
   ✅ [WEBHOOK-xxx] Extracted booking ID: 9
   ✅ [WEBHOOK-xxx] Booking BKG2025009 - Status: Paid
   ```
3. **Frontend tự động:**
   - QR code biến mất
   - Hiện "✅ Thanh toán thành công!"
   - Modal tự động đóng

## ⚠️ Lưu Ý

- **Ngrok free plan** có thể không hoạt động với PayOs
- **PayOs cần verify webhook URL** trước khi kích hoạt
- **Nếu config API báo lỗi**, vẫn có thể test với thanh toán thật
- **PayOs có thể gọi webhook** ngay cả khi config API báo lỗi (tùy PayOs)

