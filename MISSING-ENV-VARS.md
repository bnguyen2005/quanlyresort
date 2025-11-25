# ⚠️ Các Environment Variables Còn Thiếu

## 🔴 THIẾU - Cần thêm ngay

### Email Settings (QUAN TRỌNG - Để gửi email notifications)

Thêm các biến sau vào Render:

```
Key: EmailSettings__SmtpUsername
Value: phamthahlam@gmail.com
```

```
Key: EmailSettings__SmtpPassword
Value: mylghnnnbhxowmvb
```

```
Key: EmailSettings__FromEmail
Value: phamthahlam@gmail.com
```

```
Key: EmailSettings__FromName
Value: Resort Deluxe
```

```
Key: EmailSettings__SmtpHost
Value: smtp.gmail.com
```

```
Key: EmailSettings__SmtpPort
Value: 587
```

```
Key: EmailSettings__EnableSsl
Value: true
```

```
Key: EmailSettings__ContactRecipient
Value: phamthahlam@gmail.com
```

---

## ✅ Đã có (Không cần thêm)

- ✅ ASPNETCORE_ENVIRONMENT
- ✅ ConnectionStrings__DefaultConnection
- ✅ JwtSettings__SecretKey
- ✅ JwtSettings__Issuer
- ✅ JwtSettings__Audience
- ✅ JwtSettings__ExpirationHours
- ✅ AIChat settings
- ✅ BankWebhook settings
- ✅ SePay settings
- ✅ VietQR settings

---

## 📋 Checklist

### Bắt buộc cho Email Notifications
- [ ] EmailSettings__SmtpUsername
- [ ] EmailSettings__SmtpPassword
- [ ] EmailSettings__FromEmail
- [ ] EmailSettings__FromName

### Khuyến nghị
- [ ] EmailSettings__SmtpHost (có default nhưng nên set)
- [ ] EmailSettings__SmtpPort (có default nhưng nên set)
- [ ] EmailSettings__EnableSsl (có default nhưng nên set)
- [ ] EmailSettings__ContactRecipient

---

## 🚀 Cách thêm trên Render

1. Vào **Dashboard** → **Web Service** của bạn
2. Click tab **Environment**
3. Click **Add Environment Variable**
4. Thêm từng biến ở trên
5. Click **Save Changes**
6. Service sẽ tự động redeploy

---

## ⚠️ Lưu ý

- `EmailSettings__SmtpPassword` phải là **App Password** của Gmail, không phải mật khẩu thường
- Sau khi thêm, kiểm tra logs để xem email có gửi được không
- Test bằng cách đặt phòng và kiểm tra email inbox

---

## 🧪 Test sau khi thêm

1. Đặt phòng → Kiểm tra email `phamthahlam@gmail.com`
2. Thanh toán → Kiểm tra email xác nhận
3. Xem logs trên Render để kiểm tra `[EmailService]` messages

