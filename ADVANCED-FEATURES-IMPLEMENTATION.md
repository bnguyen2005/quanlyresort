# 🚀 Advanced Features Implementation Guide

Tài liệu này mô tả các tính năng nâng cao đã được triển khai và hướng dẫn sử dụng.

## 📋 Danh sách tính năng

1. ✅ **Multi-language Support (i18n)** - Hỗ trợ đa ngôn ngữ
2. ✅ **2FA Authentication** - Xác thực 2 yếu tố
3. 🔄 **Loyalty Program nâng cao** - Đang phát triển
4. 🔄 **Advanced Analytics & ML** - Đang phát triển
5. 🔄 **Calendar Integration** - Đang phát triển
6. 🔄 **CRM Integration** - Đang phát triển

---

## 1. 🌐 Multi-language Support (i18n)

### Tổng quan
Hệ thống hỗ trợ đa ngôn ngữ với khả năng chuyển đổi giữa tiếng Việt và tiếng Anh.

### Cách sử dụng

#### Backend (C#)
```csharp
// Inject service
private readonly ILocalizationService _localization;

// Sử dụng
var message = _localization.GetString("booking.title");
var welcomeMessage = _localization.GetString("welcome.message", new { Name = "John" });
```

#### Frontend (JavaScript)
```javascript
// API endpoint để lấy translations
GET /api/localization/strings?lang=vi

// Set language
POST /api/localization/set-language
Body: { "language": "en" }
```

### Cấu hình
- **Default language**: `vi` (Tiếng Việt)
- **Supported languages**: `vi`, `en`
- **Storage**: Cookie (`language`)

### Thêm ngôn ngữ mới
1. Thêm translations vào `LocalizationService.cs`
2. Thêm vào `GetSupportedLanguages()`
3. Tạo resource files (tùy chọn)

---

## 2. 🔐 2FA Authentication

### Tổng quan
Xác thực 2 yếu tố sử dụng TOTP (Time-based One-Time Password) với Google Authenticator hoặc các app tương tự.

### Cài đặt

#### 1. Thêm NuGet packages
```bash
dotnet add package Otp.NET
dotnet add package QRCoder  # Optional: for QR code generation
```

#### 2. Thêm fields vào User model
Cần migration để thêm:
- `TwoFactorSecret` (string, nullable)
- `TwoFactorEnabled` (bool)
- `TwoFactorEnabledAt` (DateTime?, nullable)
- `TwoFactorRecoveryCodes` (string, nullable)

#### 3. Đăng ký service
```csharp
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
```

### API Endpoints

#### Generate Secret & QR Code
```
POST /api/auth/2fa/generate
Response: { "secret": "...", "qrCodeUri": "otpauth://..." }
```

#### Enable 2FA
```
POST /api/auth/2fa/enable
Body: { "code": "123456" }
```

#### Verify Code (Login)
```
POST /api/auth/2fa/verify
Body: { "code": "123456" }
```

#### Disable 2FA
```
POST /api/auth/2fa/disable
Body: { "password": "..." }
```

#### Recovery Codes
```
GET /api/auth/2fa/recovery-codes
POST /api/auth/2fa/verify-recovery
Body: { "code": "12345678" }
```

### Flow đăng nhập với 2FA
1. User đăng nhập với email/password
2. Nếu 2FA enabled → yêu cầu nhập code
3. User nhập code từ authenticator app
4. Verify code → đăng nhập thành công

### Recovery Codes
- 10 mã recovery được tạo khi enable 2FA
- Mỗi mã chỉ dùng 1 lần
- Lưu mã ở nơi an toàn!

---

## 3. 🎁 Loyalty Program nâng cao

### Tính năng hiện có
- ✅ Loyalty Points trong Customer model
- ✅ API thêm điểm thủ công (Admin)
- ✅ Hiển thị điểm trong account page

### Tính năng nâng cao (Đang phát triển)
- 🔄 Tự động tích điểm khi thanh toán
- 🔄 Hệ thống hạng thành viên (Bronze, Silver, Gold, Platinum)
- 🔄 Đổi điểm lấy ưu đãi/voucher
- 🔄 Lịch sử tích điểm/đổi điểm
- 🔄 Thông báo điểm thưởng

### Cấu hình tích điểm
```json
"LoyaltySettings": {
  "PointsPerVND": 1,  // 1 điểm / 1000 VNĐ
  "TierBronze": { "MinPoints": 0, "Discount": 0 },
  "TierSilver": { "MinPoints": 1000, "Discount": 5 },
  "TierGold": { "MinPoints": 5000, "Discount": 10 },
  "TierPlatinum": { "MinPoints": 10000, "Discount": 15 }
}
```

---

## 4. 📊 Advanced Analytics & ML

### Tính năng dự kiến
- 🔄 Phân tích xu hướng đặt phòng
- 🔄 Dự đoán doanh thu
- 🔄 Phân tích hành vi khách hàng
- 🔄 Gợi ý phòng/dịch vụ (Recommendation Engine)
- 🔄 Phát hiện gian lận (Fraud Detection)
- 🔄 Tối ưu giá phòng (Dynamic Pricing)

### Công nghệ
- **ML.NET** cho machine learning
- **Python scripts** cho advanced analytics
- **Chart.js / D3.js** cho visualization

---

## 5. 📅 Calendar Integration

### Tính năng dự kiến
- 🔄 Đồng bộ booking với Google Calendar
- 🔄 Đồng bộ với Outlook Calendar
- 🔄 Gửi lời mời calendar khi đặt phòng
- 🔄 Nhắc nhở check-in/check-out
- 🔄 Quản lý lịch nhân viên

### API Integration
- **Google Calendar API**
- **Microsoft Graph API** (Outlook)

---

## 6. 👥 CRM Integration

### Tính năng dự kiến
- 🔄 Quản lý quan hệ khách hàng nâng cao
- 🔄 Phân loại khách hàng tự động
- 🔄 Lịch sử tương tác
- 🔄 Email marketing campaigns
- 🔄 Chăm sóc khách hàng tự động
- 🔄 Phân tích customer journey

### Tích hợp với
- **Salesforce** (nếu có)
- **HubSpot** (nếu có)
- **Custom CRM** (built-in)

---

## 🛠️ Cài đặt và Cấu hình

### 1. Đăng ký Services trong Program.cs
```csharp
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
builder.Services.AddHttpContextAccessor(); // Required for LocalizationService
```

### 2. Thêm Migration cho 2FA
```bash
dotnet ef migrations add AddTwoFactorAuth
dotnet ef database update
```

### 3. Cài đặt NuGet packages
```bash
dotnet add package Otp.NET
dotnet add package QRCoder  # Optional
```

---

## 📝 Notes

- **2FA**: Cần thêm fields vào User model trước khi sử dụng
- **i18n**: Có thể mở rộng thêm ngôn ngữ dễ dàng
- **Loyalty**: Đang cải thiện từ hệ thống hiện có
- **Analytics/ML**: Cần nghiên cứu thêm về requirements
- **Calendar/CRM**: Cần API keys từ providers

---

## 🔗 Tài liệu tham khảo

- [Otp.NET Documentation](https://github.com/kspearrin/Otp.NET)
- [Google Calendar API](https://developers.google.com/calendar)
- [ML.NET Documentation](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)

