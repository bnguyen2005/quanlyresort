# 📋 Hướng Dẫn Sử Dụng "Add from .env" trên Render

## 🎯 Cách Sử Dụng

### Bước 1: Copy Nội Dung File .env

Mở file `RENDER-ENV-VARIABLES-COMPLETE.txt` hoặc `.env.example` và copy **TOÀN BỘ** nội dung.

### Bước 2: Paste Vào Render

1. Trên trang **"Environment Variables"** của Render
2. Click nút **"Add from .env"**
3. Paste toàn bộ nội dung đã copy vào textarea
4. Click **"Add Variables"** hoặc **"Save"**

### Bước 3: Kiểm Tra

Render sẽ tự động parse và thêm tất cả các biến. Kiểm tra xem:
- Tất cả biến đã được thêm chưa
- Giá trị có đúng không
- Có biến nào bị lỗi format không

## ⚠️ Lưu Ý Quan Trọng

### Format Đúng:
```
KEY=value
KEY2=value2
```

### Format SAI:
```
KEY = value          ❌ Có khoảng trắng quanh dấu =
KEY="value"          ❌ Có dấu ngoặc kép
KEY='value'          ❌ Có dấu ngoặc đơn
```

### Các Trường Hợp Đặc Biệt:

1. **Giá trị có ký tự đặc biệt:**
   ```
   JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
   ```
   ✅ Đúng - Không cần escape

2. **Giá trị có dấu hai chấm:**
   ```
   ASPNETCORE_URLS=http://0.0.0.0:10000
   ```
   ✅ Đúng - URL không cần escape

3. **Giá trị boolean:**
   ```
   BankWebhook__PayOs__VerifySignature=false
   ```
   ✅ Đúng - Dùng `true` hoặc `false` (chữ thường)

4. **Giá trị có dấu gạch dưới:**
   ```
   ConnectionStrings__DefaultConnection=Data Source=resort.db
   ```
   ✅ Đúng - Dấu `__` là separator của .NET

## 📝 File .env Mẫu

Xem file `.env.example` để có format đúng và đầy đủ.

## 🔧 Sau Khi Thêm Xong

### Cần Cập Nhật Thủ Công:

Sau khi deploy, cần cập nhật 2 biến với URL thực tế của Render:

1. **BankWebhook__PayOs__WebhookUrl**
   - Thay `your-service-name.onrender.com` bằng URL thực tế
   - Ví dụ: `https://quanlyresort-api.onrender.com/api/simplepayment/webhook`

2. **SEPAY_WEBHOOK_URL**
   - Thay `your-service-name.onrender.com` bằng URL thực tế
   - Ví dụ: `https://quanlyresort-api.onrender.com/api/simplepayment/webhook`

### Cách Cập Nhật:

1. Vào Render Dashboard → Service → **Variables**
2. Tìm biến cần sửa
3. Click **Edit** (icon bút chì)
4. Cập nhật giá trị
5. Click **Save**
6. Render sẽ tự động redeploy

## ✅ Checklist

- [ ] Đã copy toàn bộ nội dung từ `.env.example`
- [ ] Đã paste vào Render "Add from .env"
- [ ] Đã kiểm tra tất cả biến đã được thêm
- [ ] Đã cập nhật `BankWebhook__PayOs__WebhookUrl` với URL thực tế (sau khi deploy)
- [ ] Đã cập nhật `SEPAY_WEBHOOK_URL` với URL thực tế (sau khi deploy)

