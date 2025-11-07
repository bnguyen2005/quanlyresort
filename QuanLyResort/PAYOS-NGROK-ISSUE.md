# ⚠️ Vấn Đề: PayOs Không Thể Config Webhook Với Ngrok Free

## 🔍 Vấn Đề

Khi config PayOs webhook với ngrok free plan, PayOs trả về lỗi:
```json
{
  "code": "20",
  "desc": "Webhook url invalid",
  "data": "Request failed with status code 400"
}
```

## 📋 Nguyên Nhân

1. **Ngrok free plan có warning page:**
   - Khi PayOs test webhook URL, nó gặp ngrok warning page
   - PayOs không thể verify webhook URL hoạt động
   - → PayOs từ chối config webhook

2. **Ngrok free plan có thể chặn request từ server:**
   - PayOs server có thể bị ngrok chặn
   - → PayOs không thể gọi webhook

## ✅ Giải Pháp

### Option 1: Dùng Ngrok Paid Plan (Khuyến Nghị)

1. **Đăng ký ngrok paid plan:**
   - Vào https://dashboard.ngrok.com/
   - Upgrade lên paid plan
   - Không có warning page
   - PayOs có thể verify webhook dễ dàng

2. **Config webhook:**
   ```bash
   ./config-payos-webhook.sh https://your-ngrok-url.ngrok.io/api/simplepayment/webhook
   ```

### Option 2: Deploy Backend Lên Server Thật

1. **Deploy backend:**
   - Azure, AWS, Heroku, etc.
   - Dùng domain thật (không phải ngrok)
   - Ví dụ: `https://api.yourdomain.com/api/simplepayment/webhook`

2. **Config webhook:**
   ```bash
   ./config-payos-webhook.sh https://api.yourdomain.com/api/simplepayment/webhook
   ```

### Option 3: Test Thanh Toán Thật (Có Thể Hoạt Động)

**Mặc dù config API báo lỗi, PayOs vẫn có thể gọi webhook khi thanh toán thật!**

1. **Bỏ qua lỗi config:**
   - PayOs có thể vẫn gọi webhook khi thanh toán thành công
   - Mặc dù config API báo lỗi

2. **Test thanh toán thật:**
   - Mở payment modal
   - Quét QR và thanh toán
   - Kiểm tra backend logs
   - Nếu thấy webhook → PayOs đã gọi được!

### Option 4: Dùng LocalTunnel (Thay Thế Ngrok)

1. **Cài đặt LocalTunnel:**
   ```bash
   npm install -g localtunnel
   ```

2. **Chạy LocalTunnel:**
   ```bash
   lt --port 5130
   ```

3. **Config webhook với LocalTunnel URL:**
   ```bash
   ./config-payos-webhook.sh https://your-localtunnel-url.loca.lt/api/simplepayment/webhook
   ```

## 🧪 Test Webhook Sau Khi Config

### Test 1: Manual Webhook (Để Verify)

```bash
curl -X POST https://your-webhook-url/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING-6", "amount": 5000}'
```

### Test 2: Thanh Toán Thật

1. Mở payment modal
2. Quét QR và thanh toán
3. Xem backend logs → Sẽ thấy webhook received
4. QR tự động biến mất trong 5 giây

## 📝 Lưu Ý

1. **Ngrok free plan:**
   - ⚠️ Có warning page
   - ⚠️ PayOs có thể không verify được
   - ✅ Nhưng vẫn có thể gọi webhook khi thanh toán thật

2. **Ngrok paid plan:**
   - ✅ Không có warning page
   - ✅ PayOs verify dễ dàng
   - ✅ Hoạt động tốt cho production

3. **Deploy backend:**
   - ✅ Giải pháp tốt nhất cho production
   - ✅ Không phụ thuộc vào ngrok
   - ✅ Domain thật, ổn định

## 🎯 Khuyến Nghị

**Cho Development/Test:**
- Dùng ngrok free plan
- Test thanh toán thật
- Nếu PayOs gọi được webhook → OK!

**Cho Production:**
- Deploy backend lên server thật
- Dùng domain thật
- Config PayOs webhook với domain thật

## ✅ Kết Luận

Mặc dù config API báo lỗi với ngrok free, **PayOs vẫn có thể gọi webhook khi thanh toán thật!**

**Cách test:**
1. Bỏ qua lỗi config
2. Test thanh toán thật
3. Kiểm tra backend logs
4. Nếu thấy webhook → Thành công! 🎉

