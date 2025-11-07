# ⚠️ Vấn Đề: PayOs Verify Webhook URL Trả Về 400

## ❌ Vấn Đề

Khi config PayOs webhook URL qua API, PayOs trả về lỗi:

```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 400"
}
```

## 🔍 Nguyên Nhân

### 1. Ngrok Free Plan Có Warning Page

**Ngrok free plan** hiển thị warning page khi truy cập lần đầu:
- PayOs gửi request verify đến webhook URL
- Ngrok hiển thị warning page (HTML) thay vì response từ backend
- PayOs nhận được HTML thay vì JSON → Trả về 400

### 2. PayOs Verify Request Format

PayOs có thể gửi request verify với format đặc biệt:
- **Method:** GET hoặc POST
- **Format:** Có thể khác với webhook thật
- **Response:** Cần trả về 200 OK

### 3. Endpoint Không Hỗ Trợ Verify Request

Endpoint hiện tại chỉ xử lý webhook thật, không xử lý verify request.

## ✅ Giải Pháp

### Giải Pháp 1: Test Với Thanh Toán Thật (Khuyến Nghị)

**PayOs có thể vẫn gọi webhook tự động** mặc dù config API báo lỗi!

**Các bước:**

1. **Quét QR và thanh toán** với nội dung: `BOOKING9`
2. **Quan sát backend logs** (terminal chạy backend):
   ```
   📥 [WEBHOOK-xxx] Webhook received: BOOKING9 - 5,000 VND
   ✅ [WEBHOOK-xxx] Extracted booking ID: 9
   ✅ [WEBHOOK-xxx] Booking BKG2025009 - Status: Paid
   ```
3. **Nếu thấy logs trên** → Webhook hoạt động! ✅
4. **Nếu không thấy** → PayOs không gọi webhook (do ngrok free plan)

### Giải Pháp 2: Dùng Ngrok Paid Plan

**Ngrok paid plan** không có warning page:
- PayOs có thể verify webhook URL
- Webhook sẽ hoạt động tự động

**Cách dùng:**
1. Đăng ký ngrok paid plan
2. Config ngrok với domain cố định
3. Config PayOs webhook với ngrok URL
4. PayOs verify thành công → Webhook hoạt động

### Giải Pháp 3: Deploy Lên Server Thật

**Deploy backend lên server có domain thật:**
- PayOs có thể verify webhook URL
- Webhook sẽ hoạt động tự động

**Cách deploy:**
1. Deploy backend lên server (Azure, AWS, VPS, etc.)
2. Config domain và SSL
3. Config PayOs webhook với domain thật
4. PayOs verify thành công → Webhook hoạt động

### Giải Pháp 4: Gọi Manual Webhook (Tạm Thời)

**Sau khi thanh toán thành công, gọi manual webhook:**

```bash
# Sau khi thanh toán BOOKING9 với 5,000 VND
curl -X POST https://7866dede24e5.ngrok-free.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING9", "amount": 5000}'
```

**Hoặc dùng script tự động:**

```bash
# Tạo script auto-webhook.sh
#!/bin/bash
BOOKING_ID=$1
AMOUNT=$2
NGROK_URL="https://7866dede24e5.ngrok-free.app"

curl -X POST "$NGROK_URL/api/simplepayment/webhook" \
  -H "Content-Type: application/json" \
  -d "{\"content\": \"BOOKING$BOOKING_ID\", \"amount\": $AMOUNT}"
```

## 🧪 Test Ngay

### Test 1: Thanh Toán Thật

1. **Mở trang web:**
   ```
   https://7866dede24e5.ngrok-free.app/customer/my-bookings.html
   ```

2. **Click "Thanh toán" cho booking "Pending"**

3. **Quét QR và thanh toán** với nội dung: `BOOKING9`

4. **Quan sát backend logs:**
   - Nếu thấy `📥 [WEBHOOK-xxx]` → Webhook hoạt động! ✅
   - Nếu không thấy → PayOs không gọi webhook ❌

### Test 2: Manual Webhook

```bash
# Test manual webhook
curl -X POST https://7866dede24e5.ngrok-free.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING9", "amount": 5000}'
```

**Kiểm tra backend logs:**
```
📥 [WEBHOOK-xxx] Webhook received: BOOKING9 - 5,000 VND
✅ [WEBHOOK-xxx] Extracted booking ID: 9
✅ [WEBHOOK-xxx] Booking BKG2025009 - Status: Paid
```

## 📋 Checklist

- [ ] Ngrok đang chạy
- [ ] Backend đang chạy
- [ ] Test manual webhook thành công
- [ ] Thanh toán thật và quan sát logs
- [ ] Nếu không hoạt động → Dùng ngrok paid plan hoặc deploy lên server thật

## 🎯 Kết Luận

**Ngrok free plan có thể không hoạt động với PayOs** do warning page.

**Giải pháp tốt nhất:**
1. **Test với thanh toán thật** (PayOs có thể vẫn gọi webhook)
2. **Nếu không hoạt động** → Dùng ngrok paid plan hoặc deploy lên server thật
3. **Tạm thời** → Gọi manual webhook sau khi thanh toán

## ⚠️ Lưu Ý

- **PayOs có thể gọi webhook** ngay cả khi config API báo lỗi (tùy PayOs)
- **Ngrok free plan** có thể không hoạt động với PayOs
- **Giải pháp tốt nhất** cho production: Deploy lên server thật hoặc dùng ngrok paid plan

