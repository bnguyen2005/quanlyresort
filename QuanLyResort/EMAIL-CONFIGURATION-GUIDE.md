# 📧 Hướng Dẫn Cấu Hình Email Service

## 🎯 Tổng Quan

Email Service được sử dụng để gửi email liên hệ từ form trên website đến địa chỉ `phamthahlam@gmail.com`.

## 📋 Cấu Hình Environment Variables

Thêm các biến sau vào **Render Environment Variables** hoặc `appsettings.json`:

### Gmail SMTP (Khuyến Nghị)

```env
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=your-email@gmail.com
EmailSettings__SmtpPassword=your-app-password
EmailSettings__FromEmail=your-email@gmail.com
EmailSettings__FromName=Resort Deluxe
EmailSettings__EnableSsl=true
EmailSettings__ContactRecipient=phamthahlam@gmail.com
```

### Outlook/Hotmail SMTP

```env
EmailSettings__SmtpHost=smtp-mail.outlook.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=your-email@outlook.com
EmailSettings__SmtpPassword=your-password
EmailSettings__FromEmail=your-email@outlook.com
EmailSettings__FromName=Resort Deluxe
EmailSettings__EnableSsl=true
EmailSettings__ContactRecipient=phamthahlam@gmail.com
```

### SMTP Server Khác

```env
EmailSettings__SmtpHost=your-smtp-server.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=your-username
EmailSettings__SmtpPassword=your-password
EmailSettings__FromEmail=noreply@yourdomain.com
EmailSettings__FromName=Resort Deluxe
EmailSettings__EnableSsl=true
EmailSettings__ContactRecipient=phamthahlam@gmail.com
```

## 🔐 Tạo App Password cho Gmail

Nếu dùng Gmail, bạn cần tạo **App Password** (không dùng mật khẩu thường):

1. **Bật 2-Step Verification:**
   - Vào: https://myaccount.google.com/security
   - Bật "2-Step Verification"

2. **Tạo App Password:**
   - Vào: https://myaccount.google.com/apppasswords
   - Chọn "Mail" và "Other (Custom name)"
   - Nhập tên: "Resort Deluxe"
   - Click "Generate"
   - **Copy password** (16 ký tự, không có khoảng trắng)

3. **Sử dụng App Password:**
   - `EmailSettings__SmtpPassword` = App Password vừa tạo (không phải mật khẩu Gmail thường)

## 📝 Cấu Hình Trên Render

### Bước 1: Vào Environment Variables

1. Render Dashboard → Service → **Environment**
2. Click **"+ Add Environment Variable"**

### Bước 2: Thêm Các Biến

Thêm từng biến theo format trên (dùng `__` thay vì `:` cho nested config).

**Ví dụ:**
- `EmailSettings__SmtpHost` = `smtp.gmail.com`
- `EmailSettings__SmtpPort` = `587`
- `EmailSettings__SmtpUsername` = `your-email@gmail.com`
- `EmailSettings__SmtpPassword` = `your-app-password`
- `EmailSettings__FromEmail` = `your-email@gmail.com`
- `EmailSettings__FromName` = `Resort Deluxe`
- `EmailSettings__EnableSsl` = `true`
- `EmailSettings__ContactRecipient` = `phamthahlam@gmail.com`

### Bước 3: Save và Redeploy

1. Click **"Save Changes"**
2. Render sẽ tự động redeploy service

## 🧪 Test Email Service

### Test 1: Test API Endpoint

```bash
curl -X POST https://your-service.onrender.com/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "email": "test@example.com",
    "subject": "Test Contact",
    "message": "This is a test message"
  }'
```

**Kết quả mong đợi:**
```json
{
  "success": true,
  "message": "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể."
}
```

### Test 2: Test Trên Website

1. Vào: `https://your-service.onrender.com/customer/contact.html`
2. Điền form liên hệ
3. Click "Gửi liên hệ"
4. Kiểm tra email `phamthahlam@gmail.com` xem có nhận được email không

## ⚠️ Lưu Ý Quan Trọng

1. **Gmail App Password:**
   - ⚠️ **KHÔNG** dùng mật khẩu Gmail thường
   - Phải tạo **App Password** từ Google Account settings
   - App Password là 16 ký tự, không có khoảng trắng

2. **Security:**
   - ⚠️ **KHÔNG** commit email credentials vào git
   - Luôn dùng Environment Variables
   - File `.gitignore` đã được cấu hình để bỏ qua `appsettings.json`

3. **Fallback:**
   - Nếu SMTP không được cấu hình, API vẫn trả về `200 OK` nhưng email sẽ không được gửi
   - Logs sẽ hiển thị warning: `[EmailService] ⚠️ SMTP credentials not configured`

## 🔍 Kiểm Tra Logs

Vào Render Dashboard → **Logs** và tìm:

- ✅ **Success:** `[EmailService] ✅ Email sent successfully to phamthahlam@gmail.com`
- ⚠️ **Warning:** `[EmailService] ⚠️ SMTP credentials not configured`
- ❌ **Error:** `[EmailService] ❌ Failed to send email`

## 📧 Format Email

Email được gửi sẽ có format HTML đẹp với:
- Header màu vàng (#c8a97e) - màu brand của resort
- Thông tin người gửi (tên, email)
- Chủ đề và nội dung
- Timestamp

## ✅ Checklist

- [ ] Đã thêm tất cả EmailSettings environment variables
- [ ] Đã tạo Gmail App Password (nếu dùng Gmail)
- [ ] Đã test API endpoint
- [ ] Đã test form trên website
- [ ] Đã kiểm tra email nhận được tại `phamthahlam@gmail.com`
- [ ] Đã kiểm tra logs trên Render

