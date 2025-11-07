# ⚡ Quick Config PayOs Webhook (3 Bước)

## 🚀 Bước 1: Chạy Ngrok

```bash
# Cài ngrok (nếu chưa có)
brew install ngrok  # macOS
# hoặc download từ https://ngrok.com

# Chạy ngrok
ngrok http 5130
```

**Copy URL từ output:**
```
Forwarding: https://abc123.ngrok.io -> http://localhost:5130
```
→ URL của bạn: `https://abc123.ngrok.io`

## 🚀 Bước 2: Config PayOs

1. **Đăng nhập** PayOs dashboard
2. **Vào Settings** → **Webhook Configuration**
3. **Nhập Webhook URL:**
   ```
   https://abc123.ngrok.io/api/simplepayment/webhook
   ```
   (Thay bằng URL ngrok của bạn)
4. **Save**

## 🚀 Bước 3: Test

### Test 1: Test Webhook Endpoint
```bash
./test-webhook-ngrok.sh https://abc123.ngrok.io 6 5000
```

### Test 2: Test Thanh Toán Thật
1. Mở payment modal
2. Quét QR và thanh toán
3. Xem backend logs → Sẽ thấy webhook received
4. QR tự động biến mất trong 5 giây

## ✅ Xong!

Sau khi config xong, mỗi khi user thanh toán:
- PayOs tự động gọi webhook
- QR tự động biến mất
- Success message tự động hiện ra

## 📝 Lưu Ý

⚠️ **Ngrok free plan:** URL thay đổi mỗi lần restart
- Giải pháp: Dùng ngrok paid plan hoặc deploy backend

📚 Xem chi tiết: `HUONG-DAN-CONFIG-PAYOS-WEBHOOK.md`

