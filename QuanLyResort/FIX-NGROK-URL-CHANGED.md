# 🔧 Fix: Ngrok URL Đã Thay Đổi

## ❌ Vấn Đề

PayOs trả về lỗi **404** khi verify webhook URL:
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 404"
}
```

## 🔍 Nguyên Nhân

**Ngrok URL đã thay đổi!**

- **URL cũ:** `https://069c46a78b2b.ngrok-free.app` (không còn hoạt động)
- **URL mới:** `https://7866dede24e5.ngrok-free.app`

**Lý do:**
- Mỗi lần restart ngrok, URL sẽ thay đổi
- URL cũ không còn hoạt động hoặc trả về warning page
- PayOs không thể verify được webhook URL

## ✅ Giải Pháp

### Bước 1: Lấy Ngrok URL Mới

```bash
# Kiểm tra ngrok URL hiện tại
curl -s http://localhost:4040/api/tunnels | grep -o '"public_url":"[^"]*"' | head -1
```

**Hoặc xem trong terminal chạy ngrok:**
```
Forwarding: https://7866dede24e5.ngrok-free.app -> http://localhost:5130
```

### Bước 2: Config PayOs Webhook Với URL Mới

```bash
cd QuanLyResort
./config-payos-webhook.sh https://7866dede24e5.ngrok-free.app/api/simplepayment/webhook
```

### Bước 3: Test Webhook Endpoint

```bash
# Test webhook với URL mới
curl -X POST https://7866dede24e5.ngrok-free.app/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING9", "amount": 5000}'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "bookingId": 9,
  "bookingCode": "BKG2025009"
}
```

## ⚠️ Lưu Ý

### Ngrok Free Plan

- **URL thay đổi mỗi lần restart ngrok**
- **Có warning page** → PayOs có thể không verify được
- **Giải pháp:** Dùng ngrok paid plan hoặc deploy lên server thật

### Ngrok Paid Plan

- **URL cố định** (có thể config custom domain)
- **Không có warning page** → PayOs verify được
- **Giải pháp tốt nhất** cho production

### Deploy Lên Server Thật

- **URL cố định** (domain thật)
- **Không có warning page** → PayOs verify được
- **Giải pháp tốt nhất** cho production

## 🔄 Quy Trình Khi Ngrok URL Thay Đổi

1. **Lấy ngrok URL mới:**
   ```bash
   curl -s http://localhost:4040/api/tunnels | grep -o '"public_url":"[^"]*"' | head -1
   ```

2. **Config lại PayOs webhook:**
   ```bash
   ./config-payos-webhook.sh <NGROK_URL_MỚI>/api/simplepayment/webhook
   ```

3. **Test webhook:**
   ```bash
   curl -X POST <NGROK_URL_MỚI>/api/simplepayment/webhook \
     -H "Content-Type: application/json" \
     -d '{"content": "BOOKING9", "amount": 5000}'
   ```

## 📋 Checklist

- [ ] Ngrok đang chạy
- [ ] Lấy ngrok URL mới
- [ ] Config PayOs webhook với URL mới
- [ ] Test webhook endpoint
- [ ] Kiểm tra backend logs
- [ ] Test thanh toán thật

## 🎯 Kết Quả

Sau khi config thành công:

1. **PayOs verify webhook URL thành công**
2. **PayOs tự động gọi webhook khi thanh toán**
3. **Backend logs hiển thị webhook received**
4. **Frontend tự động cập nhật (QR biến mất, success message)**

