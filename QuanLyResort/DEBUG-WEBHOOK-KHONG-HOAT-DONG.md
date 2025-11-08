# 🔍 DEBUG: Webhook Không Hoạt Động

## 📋 Tình Trạng
- ✅ PayOs đã hiển thị "Đã thanh toán" (orderCode: 43843, 42347)
- ❌ Website chưa cập nhật status thành "Paid"
- ❌ QR code chưa biến mất
- ❌ Chưa hiển thị "Thanh toán thành công"

## 🔍 Các Nguyên Nhân Có Thể

### 1. ❌ PayOs Không Gửi Webhook
**Triệu chứng:**
- PayOs hiển thị "Đã thanh toán" nhưng backend không nhận được webhook
- Logs trên Render không có entry `[WEBHOOK-xxx]`

**Kiểm tra:**
```bash
# 1. Kiểm tra webhook URL có được config trong PayOs không
curl -X POST "https://api-merchant.payos.vn/v2/webhook-url" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb"

# 2. Xem logs trên Render
# https://dashboard.render.com -> Logs
# Tìm: [WEBHOOK-xxx] hoặc "Webhook received"
```

**Giải pháp:**
- Chạy lại script config webhook:
```bash
./config-payos-webhook.sh
```

---

### 2. ❌ Webhook Format Không Đúng
**Triệu chứng:**
- Backend nhận được webhook nhưng không parse được
- Logs có: `⚠️ Cannot extract booking ID`

**Kiểm tra:**
- Xem logs trên Render:
```
📥 [WEBHOOK-xxx] Webhook received
   Raw request: {...}
   PayOs - Description: CSCOK68MZC1 BOOKING4
```

**Logic Extract BookingId:**
- Description từ PayOs: `"CSCOK68MZC1 BOOKING4"`
- Pattern match: `@"BOOKING(\d+)"` → Extract `4` ✅
- **Nếu description không có "BOOKING4" → Không extract được**

**Giải pháp:**
- Kiểm tra description trong PayOs có đúng format không
- Nếu PayOs gửi description khác, cần update logic extract

---

### 3. ❌ Webhook Được Gửi Nhưng Có Lỗi Khi Xử Lý
**Triệu chứng:**
- Logs có: `❌ [WEBHOOK-xxx] Error processing webhook`
- Booking không được update

**Kiểm tra:**
```bash
# Xem logs trên Render để tìm lỗi:
# - Database error
# - Booking not found
# - ProcessOnlinePaymentAsync failed
```

**Giải pháp:**
- Xem chi tiết exception trong logs
- Kiểm tra booking có tồn tại không
- Kiểm tra database connection

---

### 4. ❌ PayOs Gửi Webhook Nhưng Backend Không Nhận Được
**Triệu chứng:**
- PayOs đã gửi webhook (theo PayOs dashboard)
- Backend logs không có entry

**Nguyên nhân:**
- CORS issue
- Firewall/Network blocking
- Webhook URL không accessible từ PayOs server

**Kiểm tra:**
```bash
# Test webhook endpoint manually
curl -X POST "https://quanlyresort.onrender.com/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "00",
    "desc": "success",
    "data": {
      "orderCode": 43843,
      "amount": 5000,
      "description": "CSCOK68MZC1 BOOKING4",
      "accountNumber": "0901329227"
    }
  }'
```

---

## 🧪 TEST WEBHOOK THỦ CÔNG

### Test với format PayOs thực tế:
```bash
# Sử dụng script test
./test-payos-webhook.sh 4

# Hoặc test trực tiếp:
curl -X POST "https://quanlyresort.onrender.com/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -H "User-Agent: PayOs-Webhook/1.0" \
  -d '{
    "code": "00",
    "desc": "success",
    "data": {
      "orderCode": 43843,
      "amount": 5000,
      "description": "CSCOK68MZC1 BOOKING4",
      "accountNumber": "0901329227",
      "accountName": "PHAM THANH LAM",
      "reference": "REF123456",
      "transactionDateTime": "2025-11-09T00:44:06Z",
      "currency": "VND",
      "paymentLinkId": "d0496972015547f9a78af3a3847474b4"
    },
    "signature": "test-signature"
  }'
```

**Kết quả mong đợi:**
- HTTP 200 OK
- Logs: `✅ [WEBHOOK-xxx] Booking 4 updated to Paid`
- Booking status = "Paid"

---

## 📊 CHECKLIST DEBUG

### Bước 1: Kiểm Tra Webhook URL
- [ ] Webhook URL đã được config trong PayOs
- [ ] Webhook URL accessible: `curl https://quanlyresort.onrender.com/api/simplepayment/webhook`
- [ ] Webhook URL trả về 200 OK (verification request)

### Bước 2: Kiểm Tra Logs
- [ ] Xem logs trên Render: https://dashboard.render.com -> Logs
- [ ] Tìm: `[WEBHOOK-xxx]` entries
- [ ] Kiểm tra có lỗi không: `❌` hoặc `⚠️`

### Bước 3: Kiểm Tra PayOs Dashboard
- [ ] PayOs hiển thị "Đã thanh toán"
- [ ] PayOs có gửi webhook không (xem webhook logs trong PayOs dashboard)
- [ ] Description có đúng format: `"CSCOK68MZC1 BOOKING4"`

### Bước 4: Test Webhook Thủ Công
- [ ] Chạy `./test-payos-webhook.sh 4`
- [ ] Kiểm tra response có 200 OK không
- [ ] Kiểm tra booking 4 có update thành "Paid" không

### Bước 5: Kiểm Tra Database
- [ ] Booking 4 có tồn tại không
- [ ] Booking 4 status hiện tại là gì
- [ ] Có invoice được tạo không

---

## 🔧 GIẢI PHÁP TẠM THỜI

Nếu webhook không hoạt động, có thể manually update booking:

```bash
# Sử dụng endpoint manual update (cần Admin token)
curl -X POST "https://quanlyresort.onrender.com/api/simplepayment/manual-update-paid/4" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

---

## 📝 THÔNG TIN QUAN TRỌNG

### PayOs Webhook Format:
```json
{
  "code": "00",  // "00" = success
  "desc": "success",
  "data": {
    "orderCode": 43843,
    "amount": 5000,
    "description": "CSCOK68MZC1 BOOKING4",  // ← Quan trọng: phải có "BOOKING4"
    "accountNumber": "0901329227",
    "reference": "REF123456",
    "transactionDateTime": "2025-11-09T00:44:06Z"
  },
  "signature": "..."
}
```

### Logic Extract BookingId:
1. Ưu tiên extract từ `description`: `"CSCOK68MZC1 BOOKING4"` → `4` ✅
2. Fallback từ `orderCode` nếu `orderCode < 10000` (chỉ cho bookingId cũ)

### Webhook URL:
- Production: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- Config script: `./config-payos-webhook.sh`

---

## 🎯 KẾT LUẬN

**Nguyên nhân phổ biến nhất:**
1. ❌ **PayOs không gửi webhook** (webhook URL chưa được config hoặc PayOs không gửi tự động)
2. ❌ **Webhook format không đúng** (description không có "BOOKING4")
3. ❌ **Backend không nhận được webhook** (network/firewall issue)

**Giải pháp:**
1. Chạy lại `./config-payos-webhook.sh` để đảm bảo webhook URL được config
2. Kiểm tra logs trên Render để xem webhook có được nhận không
3. Test webhook thủ công với `./test-payos-webhook.sh 4`
4. Nếu vẫn không hoạt động, dùng endpoint manual update

