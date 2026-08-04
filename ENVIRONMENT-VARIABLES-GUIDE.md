# 🔐 Hướng dẫn cài đặt Environment Variables

## 📋 Tổng quan

Environment Variables (biến môi trường) dùng để lưu thông tin nhạy cảm như passwords, API keys mà không commit vào code.

---

## 🔑 Danh sách biến môi trường cần cài

### 1. **ASPNETCORE_ENVIRONMENT**
- **Mục đích**: Môi trường chạy ứng dụng
- **Giá trị**: `Production` (cho production) hoặc `Development` (cho dev)
- **Bắt buộc**: ✅ Có
- **Ví dụ**: `Production`

### 2. **ConnectionStrings__DefaultConnection**
- **Mục đích**: Connection string đến database
- **Giá trị**: Chuỗi kết nối database
- **Bắt buộc**: ✅ Có
- **Ví dụ SQLite**: `Data Source=resort.db`
- **Ví dụ SQL Server**: `Server=your-server;Database=ResortDb;User Id=user;Password=pass;`
- **Ví dụ PostgreSQL**: `Host=localhost;Database=resort;Username=user;Password=pass`

### 3. **EmailSettings__SmtpUsername**
- **Mục đích**: Email dùng để gửi email
- **Giá trị**: Email Gmail của bạn
- **Bắt buộc**: ✅ Có (nếu muốn gửi email)
- **Ví dụ**: `phamthahlam@gmail.com`

### 4. **EmailSettings__SmtpPassword**
- **Mục đích**: App Password của Gmail (không phải mật khẩu thường)
- **Giá trị**: App Password 16 ký tự
- **Bắt buộc**: ✅ Có (nếu muốn gửi email)
- **Ví dụ**: `mylghnnnbhxowmvb`
- ⚠️ **Lưu ý**: Phải là App Password, không phải mật khẩu Gmail thường

### 5. **EmailSettings__SmtpHost**
- **Mục đích**: SMTP server
- **Giá trị**: `smtp.gmail.com` (cho Gmail)
- **Bắt buộc**: ❌ Không (có default)
- **Ví dụ**: `smtp.gmail.com`

### 6. **EmailSettings__SmtpPort**
- **Mục đích**: Port SMTP
- **Giá trị**: `587` (cho Gmail)
- **Bắt buộc**: ❌ Không (có default)
- **Ví dụ**: `587`

### 7. **EmailSettings__FromEmail**
- **Mục đích**: Email hiển thị là người gửi
- **Giá trị**: Email của bạn
- **Bắt buộc**: ❌ Không (dùng SmtpUsername nếu không set)
- **Ví dụ**: `phamthahlam@gmail.com`

### 8. **EmailSettings__FromName**
- **Mục đích**: Tên hiển thị khi gửi email
- **Giá trị**: Tên thương hiệu
- **Bắt buộc**: ❌ Không (có default)
- **Ví dụ**: `Resort Deluxe`

### 9. **JwtSettings__SecretKey**
- **Mục đích**: Secret key để tạo JWT tokens
- **Giá trị**: Chuỗi bí mật dài và phức tạp
- **Bắt buộc**: ✅ Có
- **Ví dụ**: `YourSuperSecretKeyForJWTTokenGeneration2025!@#$`
- ⚠️ **Lưu ý**: Phải giữ bí mật, không được tiết lộ

### 10. **JwtSettings__Issuer**
- **Mục đích**: Issuer của JWT token
- **Giá trị**: Tên ứng dụng
- **Bắt buộc**: ❌ Không (có default)
- **Ví dụ**: `ResortManagementAPI`

### 11. **JwtSettings__Audience**
- **Mục đích**: Audience của JWT token
- **Giá trị**: Tên client
- **Bắt buộc**: ❌ Không (có default)
- **Ví dụ**: `ResortManagementClient`

### 12. **JwtSettings__ExpirationHours**
- **Mục đích**: Thời gian hết hạn của JWT token (giờ)
- **Giá trị**: Số giờ
- **Bắt buộc**: ❌ Không (có default: 24)
- **Ví dụ**: `24`

---

## 🚀 Cách cài đặt trên các Platform

### Render.com

1. Vào **Dashboard** → Chọn **Web Service**
2. Click vào service của bạn
3. Vào tab **Environment**
4. Click **Add Environment Variable**
5. Thêm từng biến:

```
Key: ASPNETCORE_ENVIRONMENT
Value: Production
```

```
Key: ConnectionStrings__DefaultConnection
Value: Data Source=resort.db
```

```
Key: EmailSettings__SmtpUsername
Value: phamthahlam@gmail.com
```

```
Key: EmailSettings__SmtpPassword
Value: mylghnnnbhxowmvb
```

```
Key: JwtSettings__SecretKey
Value: YourSuperSecretKeyForJWTTokenGeneration2025!@#$
```

6. Click **Save Changes**
7. Service sẽ tự động redeploy

---

### Railway.app

1. Vào **Project** → Chọn **Service**
2. Click tab **Variables**
3. Click **+ New Variable**
4. Thêm từng biến (tương tự Render)
5. Click **Deploy** để apply

---

### Vercel

1. Vào **Project Settings**
2. Click **Environment Variables**
3. Thêm biến cho:
   - **Production**
   - **Preview**
   - **Development**
4. Click **Save**

---

### Azure App Service

1. Vào **Configuration** → **Application settings**
2. Click **+ New application setting**
3. Thêm từng biến
4. Click **Save**

---

### Heroku

```bash
# Dùng Heroku CLI
heroku config:set ASPNETCORE_ENVIRONMENT=Production
heroku config:set ConnectionStrings__DefaultConnection="your-connection-string"
heroku config:set EmailSettings__SmtpUsername=phamthahlam@gmail.com
heroku config:set EmailSettings__SmtpPassword=mylghnnnbhxowmvb
heroku config:set JwtSettings__SecretKey="YourSuperSecretKeyForJWTTokenGeneration2025!@#$"
```

Hoặc qua Dashboard:
1. Vào **Settings** → **Config Vars**
2. Click **Reveal Config Vars**
3. Thêm từng biến

---

## 📝 Format của Environment Variables

### Cách đặt tên

Trong .NET Core, dùng `__` (double underscore) để phân cấp:

```
EmailSettings__SmtpUsername
EmailSettings__SmtpPassword
JwtSettings__SecretKey
```

Tương đương với trong `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpUsername": "..."
  }
}
```

### Ví dụ đầy đủ

```bash
# Core Settings
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=resort.db

# Email Settings
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=phamthahlam@gmail.com
EmailSettings__SmtpPassword=mylghnnnbhxowmvb
EmailSettings__FromEmail=phamthahlam@gmail.com
EmailSettings__FromName=Resort Deluxe
EmailSettings__EnableSsl=true
EmailSettings__ContactRecipient=phamthahlam@gmail.com

# JWT Settings
JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
JwtSettings__Issuer=ResortManagementAPI
JwtSettings__Audience=ResortManagementClient
JwtSettings__ExpirationHours=24

# SMS Settings (optional, đã tắt)
SmsSettings__Enabled=false
SmsSettings__Provider=generic
SmsSettings__ApiKey=your-sms-api-key
SmsSettings__ApiUrl=https://api.sms-provider.com/send
SmsSettings__SenderId=RESORT
```

---

## ✅ Checklist cài đặt

### Bắt buộc (Minimum)
- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `ConnectionStrings__DefaultConnection=...`
- [ ] `EmailSettings__SmtpUsername=...`
- [ ] `EmailSettings__SmtpPassword=...`
- [ ] `JwtSettings__SecretKey=...`

### Khuyến nghị (Recommended)
- [ ] `EmailSettings__FromEmail=...`
- [ ] `EmailSettings__FromName=...`
- [ ] `JwtSettings__Issuer=...`
- [ ] `JwtSettings__Audience=...`

### Tùy chọn (Optional)
- [ ] `EmailSettings__SmtpHost=...` (default: smtp.gmail.com)
- [ ] `EmailSettings__SmtpPort=...` (default: 587)
- [ ] `SmsSettings__...` (nếu muốn bật SMS)

---

## 🔒 Bảo mật

### ⚠️ QUAN TRỌNG

1. **Không commit** `appsettings.json` có chứa passwords thật
2. **Dùng Environment Variables** trên production
3. **Rotate secrets** định kỳ (đổi mật khẩu, keys)
4. **Giới hạn quyền truy cập** vào environment variables
5. **Log không hiển thị** sensitive data

### Best Practices

1. **Development**: Dùng `appsettings.Development.json` (không commit)
2. **Production**: Dùng Environment Variables
3. **Secrets Management**: Dùng Azure Key Vault, AWS Secrets Manager (nếu có)

---

## 🧪 Test Environment Variables

### Kiểm tra biến đã set chưa

```bash
# Trên server
echo $ASPNETCORE_ENVIRONMENT
echo $EmailSettings__SmtpUsername
```

### Test trong code

```csharp
// Trong Program.cs hoặc Controller
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var email = _configuration["EmailSettings:SmtpUsername"];
Console.WriteLine($"Environment: {env}");
Console.WriteLine($"Email: {email}");
```

---

## 📞 Troubleshooting

### Biến không được load

1. ✅ Kiểm tra tên biến đúng chưa (có `__` không)
2. ✅ Kiểm tra đã save và redeploy chưa
3. ✅ Kiểm tra scope (Production/Development)
4. ✅ Kiểm tra logs để xem giá trị

### Email không gửi được

1. ✅ Kiểm tra `EmailSettings__SmtpPassword` đúng App Password chưa
2. ✅ Kiểm tra `EmailSettings__SmtpUsername` đúng email chưa
3. ✅ Kiểm tra logs: `[EmailService]` trong console

### JWT không hoạt động

1. ✅ Kiểm tra `JwtSettings__SecretKey` đã set chưa
2. ✅ Kiểm tra secret key đủ dài và phức tạp
3. ✅ Kiểm tra logs để xem lỗi cụ thể

---

## 📚 Tham khảo

- [.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment Variables in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable)

