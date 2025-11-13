# 📧 Liên Hệ PayOs Support

## 📞 Thông Tin Liên Hệ PayOs

### Email Hỗ Trợ

**Cách tìm email hỗ trợ PayOs:**

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Tìm mục "Liên hệ"** hoặc **"Hỗ trợ"**
3. **Hoặc kiểm tra email từ PayOs** (khi đăng ký merchant)

**Email có thể:**
- support@payos.vn
- help@payos.vn
- contact@payos.vn
- hoặc email trong PayOs Dashboard

### Website & Dashboard

- **Website:** https://payos.vn
- **Dashboard:** https://payos.vn (đăng nhập)
- **Tài liệu API:** https://payos.vn/docs

### Cách Liên Hệ PayOs

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Tìm mục "Hỗ trợ"** hoặc **"Liên hệ"**
3. **Kiểm tra:**
   - Email hỗ trợ
   - Hotline
   - Chat support (nếu có)
   - Ticket system (nếu có)

## 📝 Nội Dung Email Cần Gửi

### Chủ Đề Email

```
Vấn đề verify webhook URL với Railway domain
```

### Nội Dung Email

```
Kính gửi PayOs Support,

Tôi đang gặp vấn đề khi config webhook URL với Railway domain.

Thông tin:
- Client ID: 90ad103f-aa49-4c33-9692-76d739a68b1b
- Webhook URL: https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
- Lỗi: "Request failed with status code 404"

Vấn đề:
1. Railway endpoint hoạt động tốt khi test bằng curl:
   - GET request: Trả về {"status":"active",...}
   - POST request: Trả về {"status":"active",...}

2. PayOs API báo lỗi khi verify:
   - Code: "20"
   - Desc: "Webhook url invalid"
   - Data: "Request failed with status code 404"

3. PayOs không gửi webhook sau khi thanh toán thành công

Yêu cầu:
- Kiểm tra vấn đề với Railway domain (up.railway.app)
- Hỗ trợ config webhook URL với Railway
- Hoặc hướng dẫn cách verify webhook URL đúng cách

Cảm ơn,
[Your Name]
```

## 🔍 Thông Tin Cần Cung Cấp

Khi liên hệ PayOs support, cung cấp:

1. **Client ID:** `90ad103f-aa49-4c33-9692-76d739a68b1b`
2. **Webhook URL:** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
3. **Lỗi:** "Request failed with status code 404"
4. **Test result:** Endpoint hoạt động khi test bằng curl
5. **API call:**
   ```bash
   curl -X POST "https://api-merchant.payos.vn/confirm-webhook" \
     -H "x-client-id: 90ad103f-aa49-4c33-9692-76d739a68b1b" \
     -H "x-api-key: acb138f1-a0f0-4a1f-9692-16d54332a580" \
     -d '{"webhookUrl": "https://quanlyresort-production.up.railway.app/api/simplepayment/webhook"}'
   ```
6. **Response:** `{"code":"20","desc":"Webhook url invalid","data":"Request failed with status code 404"}`

## 📋 Checklist Trước Khi Gửi Email

- [ ] Đã test Railway endpoint hoạt động
- [ ] Đã thử config webhook URL qua API
- [ ] Đã đợi 24-48 giờ và thử lại
- [ ] Đã chuẩn bị thông tin cần cung cấp
- [ ] Đã viết email với nội dung rõ ràng

## 💡 Lưu Ý

- **Gửi email bằng tiếng Việt** (PayOs là công ty Việt Nam)
- **Cung cấp đầy đủ thông tin** để PayOs có thể hỗ trợ nhanh
- **Đính kèm screenshots** nếu có (PayOs Dashboard, Railway logs, etc.)
- **Kiên nhẫn đợi phản hồi** (thường 1-2 ngày làm việc)

## 🎯 Kết Quả Mong Đợi

Sau khi liên hệ PayOs support:
- ✅ PayOs kiểm tra và fix vấn đề Railway domain
- ✅ PayOs hướng dẫn cách config webhook URL đúng
- ✅ Webhook URL được verify thành công
- ✅ PayOs gửi webhook sau khi thanh toán

## 🔗 Links Quan Trọng

- **PayOs Website:** https://payos.vn
- **PayOs Dashboard:** https://payos.vn (đăng nhập để tìm thông tin liên hệ)
- **PayOs API Docs:** https://payos.vn/docs (nếu có)

## 📝 Cách Tìm Email Hỗ Trợ PayOs

1. **Vào PayOs Dashboard:** https://payos.vn
2. **Đăng nhập** với tài khoản merchant
3. **Tìm các mục sau:**
   - "Hỗ trợ" / "Support"
   - "Liên hệ" / "Contact"
   - "Trợ giúp" / "Help"
   - "Ticket" / "Yêu cầu hỗ trợ"
4. **Kiểm tra email từ PayOs:**
   - Email đăng ký merchant
   - Email thông báo từ PayOs
   - Email trong Settings/Account

