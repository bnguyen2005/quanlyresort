# 🔍 Kiểm Tra PayOs Webhook Không Hoạt Động

## 📋 Vấn Đề

PayOs chưa gửi webhook sau khi thanh toán, dẫn đến:
- Status booking vẫn là "Pending"
- QR code không ẩn
- Thông báo "Thanh toán thành công" không hiển thị

## 🔍 Nguyên Nhân Có Thể

### 1. **Webhook URL chưa được cấu hình trên PayOs**

PayOs **KHÔNG có dashboard** để config webhook. Bạn phải gọi API để đăng ký webhook URL.

**Kiểm tra:**
```bash
cd QuanLyResort
./config-payos-webhook.sh
```

**Nếu lỗi:** Kiểm tra lại:
- ✅ Webhook URL có thể truy cập: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- ✅ Client ID và API Key đúng trong `appsettings.json`
- ✅ Backend đã deploy và đang chạy

### 2. **Chưa có giao dịch thực tế**

PayOs **CHỈ gửi webhook khi có giao dịch thực tế** (chuyển tiền thật).

**Nếu bạn chỉ test QR code mà chưa chuyển tiền:**
- ❌ PayOs sẽ KHÔNG gửi webhook
- ✅ Cần test với webhook thủ công (xem bên dưới)

### 3. **PayOs có delay trong việc gửi webhook**

PayOs có thể mất **vài phút** để gửi webhook sau khi thanh toán thành công.

**Kiểm tra:**
- Đợi 2-5 phút sau khi chuyển tiền
- Xem logs trên Render để xem có webhook đến không

## 🧪 Test Webhook Thủ Công

### Bước 1: Lấy thông tin từ PayOs response

Từ logs console, bạn sẽ thấy:
```json
{
  "orderCode": 47571,
  "description": "CSMJ4XFPZW3 BOOKING4",
  "amount": 5000
}
```

### Bước 2: Test webhook với script

```bash
cd QuanLyResort
chmod +x test-payos-webhook.sh
./test-payos-webhook.sh 4 47571 CSMJ4XFPZW3
```

**Giải thích:**
- `4` = Booking ID
- `47571` = Order Code từ PayOs
- `CSMJ4XFPZW3` = Description prefix từ PayOs

### Bước 3: Kiểm tra kết quả

**Thành công (HTTP 200):**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 4,
  "bookingCode": "BKG2025004"
}
```

**Sau đó:**
- ✅ Status booking sẽ đổi thành "Paid"
- ✅ QR code sẽ ẩn
- ✅ Thông báo "Thanh toán thành công" sẽ hiển thị

## 🔧 Cấu Hình Webhook Trên PayOs

### Cách 1: Dùng Script (Khuyến Nghị)

```bash
cd QuanLyResort
chmod +x config-payos-webhook.sh
./config-payos-webhook.sh
```

### Cách 2: Gọi API Thủ Công

```bash
curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
  -H "Content-Type: application/json" \
  -H "x-client-id: c704495b-5984-4ad3-aa23-b2794a02aa83" \
  -H "x-api-key: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb" \
  -d '{"webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"}'
```

**Thành công:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://quanlyresort.onrender.com/api/simplepayment/webhook"
  }
}
```

## 📊 Kiểm Tra Logs Trên Render

1. Vào **Render Dashboard**: https://dashboard.render.com
2. Chọn service **quanlyresort-api**
3. Click **Logs**
4. Tìm các dòng có `[WEBHOOK-...]` để xem webhook có đến không

**Nếu thấy:**
```
📥 [WEBHOOK-xxxx] Webhook received
✅ [WEBHOOK-xxxx] Booking 4 updated to Paid successfully!
```
→ Webhook đã hoạt động!

**Nếu KHÔNG thấy:**
→ Webhook chưa được gửi từ PayOs hoặc chưa được cấu hình

## ✅ Checklist

- [ ] Webhook URL đã được cấu hình trên PayOs (dùng script `config-payos-webhook.sh`)
- [ ] Backend đang chạy và có thể truy cập: `https://quanlyresort.onrender.com/api/simplepayment/webhook`
- [ ] Đã test webhook thủ công và thành công
- [ ] Đã chuyển tiền thật (không chỉ test QR code)
- [ ] Đã đợi 2-5 phút sau khi chuyển tiền
- [ ] Đã kiểm tra logs trên Render

## 🚨 Nếu Vẫn Không Hoạt Động

1. **Kiểm tra webhook endpoint:**
   ```bash
   curl https://quanlyresort.onrender.com/api/simplepayment/webhook-status
   ```

2. **Test webhook thủ công:**
   ```bash
   ./test-payos-webhook.sh 4
   ```

3. **Kiểm tra PayOs credentials:**
   - Client ID: `c704495b-5984-4ad3-aa23-b2794a02aa83`
   - API Key: `f6ea421b-a8b7-46b8-92be-209eb1a9b2fb`
   - Webhook URL: `https://quanlyresort.onrender.com/api/simplepayment/webhook`

4. **Liên hệ PayOs support** nếu vẫn không hoạt động

