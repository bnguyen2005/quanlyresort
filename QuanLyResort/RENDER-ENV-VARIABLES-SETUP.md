# 🔧 Hướng Dẫn Setup Environment Variables trên Render

## 📋 Danh Sách Environment Variables Cần Thêm

Trên trang **"Environment Variables"** của Render, click **"+ Add Environment Variable"** và thêm từng biến sau:

### 1. ASP.NET Core Settings

```
NAME: ASPNETCORE_ENVIRONMENT
VALUE: Production
```

```
NAME: ASPNETCORE_URLS
VALUE: http://0.0.0.0:$PORT
```

```
NAME: PORT
VALUE: 10000
```

### 2. Database Connection

```
NAME: ConnectionStrings__DefaultConnection
VALUE: Data Source=/data/resort.db
```

**Lưu ý:** Nếu dùng PostgreSQL trên Render, thay bằng:
```
NAME: ConnectionStrings__DefaultConnection
VALUE: Server=your-postgres-host;Database=resortdb;User Id=your-user;Password=your-password;
```

### 3. JWT Settings

```
NAME: JwtSettings__SecretKey
VALUE: YourSuperSecretKeyForJWTTokenGeneration2025!@#$
```

```
NAME: JwtSettings__Issuer
VALUE: ResortManagementAPI
```

```
NAME: JwtSettings__Audience
VALUE: ResortManagementClient
```

```
NAME: JwtSettings__ExpirationHours
VALUE: 24
```

### 4. PayOs Settings

```
NAME: BankWebhook__PayOs__ClientId
VALUE: c704495b-5984-4ad3-aa23-b2794a02aa83
```

```
NAME: BankWebhook__PayOs__ApiKey
VALUE: f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
```

```
NAME: BankWebhook__PayOs__ChecksumKey
VALUE: 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

```
NAME: BankWebhook__PayOs__SecretKey
VALUE: 429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
```

```
NAME: BankWebhook__PayOs__VerifySignature
VALUE: false
```

```
NAME: BankWebhook__PayOs__WebhookUrl
VALUE: https://your-service-name.onrender.com/api/simplepayment/webhook
```

**⚠️ QUAN TRỌNG:** Thay `your-service-name` bằng tên service thực tế của bạn trên Render!

### 5. AI Chat Settings (Groq)

```
NAME: AIChat__Provider
VALUE: groq
```

```
NAME: AIChat__ApiKey
VALUE: gsk_your_new_groq_api_key_here
```

**⚠️ QUAN TRỌNG:** 
- Thay `gsk_your_new_groq_api_key_here` bằng API key mới từ Groq console
- API key cũ đã bị revoke, cần tạo key mới!

```
NAME: AIChat__ApiUrl
VALUE: https://api.groq.com/openai/v1/chat/completions
```

```
NAME: AIChat__Model
VALUE: llama-3.1-8b-instant
```

## 📝 Cách Thêm Từng Biến

1. Trên trang **"Environment Variables"** của Render
2. Click **"+ Add Environment Variable"**
3. Nhập **NAME** (tên biến)
4. Nhập **VALUE** (giá trị)
5. Click **Save** hoặc **Add**
6. Lặp lại cho tất cả các biến trên

## ✅ Checklist

Sau khi thêm xong, kiểm tra:

- [ ] Đã thêm tất cả 15 biến môi trường
- [ ] `AIChat__ApiKey` đã được thay bằng API key mới từ Groq
- [ ] `BankWebhook__PayOs__WebhookUrl` đã được cập nhật với URL thực tế của Render service
- [ ] `ConnectionStrings__DefaultConnection` phù hợp với database bạn đang dùng

## 🚀 Sau Khi Thêm Xong

1. Click **"Deploy Web Service"** (nút màu đen ở dưới cùng)
2. Render sẽ bắt đầu build và deploy
3. Kiểm tra logs để xác nhận deploy thành công
4. Test API endpoints để đảm bảo mọi thứ hoạt động

## 🔍 Kiểm Tra Logs

Sau khi deploy, kiểm tra logs để xác nhận:

- ✅ `[AI Chat] ✅ API Key configured (length: XX, provider: groq)` - API key đã được cấu hình
- ✅ `Application started` - Ứng dụng đã khởi động thành công
- ✅ `Now listening on: http://0.0.0.0:10000` - Server đang chạy

## ⚠️ Lưu Ý

1. **KHÔNG** commit API keys vào code
2. **LUÔN** sử dụng Environment Variables trên Render
3. Nếu cần thay đổi, chỉ cần update trên Render dashboard, không cần commit code

