# Hướng dẫn cấu hình SMS Notifications

## Tổng quan

Hệ thống hỗ trợ 3 loại SMS provider:
1. **Twilio** - Dịch vụ SMS quốc tế phổ biến
2. **AWS SNS** - Dịch vụ SMS của Amazon
3. **Generic HTTP API** - Tích hợp với bất kỳ SMS gateway nào

---

## 1. Twilio (Khuyến nghị cho quốc tế)

### Bước 1: Đăng ký tài khoản Twilio
1. Truy cập: https://www.twilio.com/
2. Đăng ký tài khoản miễn phí (có $15 credit để test)
3. Xác minh số điện thoại

### Bước 2: Lấy thông tin
1. Vào **Console** → **Account** → **API Keys & Tokens**
2. Copy:
   - **Account SID** (ví dụ: `ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`)
   - **Auth Token** (ví dụ: `your_auth_token_here`)
3. Mua số điện thoại Twilio (hoặc dùng số trial)

### Bước 3: Cấu hình trong appsettings.json
```json
"SmsSettings": {
  "Provider": "twilio",
  "Enabled": "true",
  "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "ApiSecret": "your_auth_token_here",
  "SenderId": "+1234567890",
  "Note": "Twilio configuration"
}
```

**Lưu ý:**
- `SenderId` phải là số điện thoại Twilio của bạn (format: +1234567890)
- Trial account chỉ gửi được đến số đã verify
- Giá: ~$0.0075/SMS (khoảng 180 VNĐ/SMS)

---

## 2. AWS SNS (Khuyến nghị cho production)

### Bước 1: Đăng ký AWS
1. Truy cập: https://aws.amazon.com/
2. Tạo tài khoản AWS
3. Vào **SNS** (Simple Notification Service)

### Bước 2: Cấu hình
1. Tạo **Access Key** và **Secret Key** trong IAM
2. Kích hoạt SMS trong SNS
3. Set spending limit (giới hạn chi phí)

### Bước 3: Cấu hình trong appsettings.json
```json
"SmsSettings": {
  "Provider": "aws",
  "Enabled": "true",
  "ApiKey": "your-aws-access-key",
  "ApiSecret": "your-aws-secret-key",
  "SenderId": "RESORT",
  "Note": "AWS SNS configuration - requires AWS SDK"
}
```

**Lưu ý:**
- Cần cài đặt AWS SDK for .NET
- Giá: ~$0.00645/SMS (khoảng 155 VNĐ/SMS)
- Cần verify số điện thoại trước khi gửi

---

## 3. Generic HTTP API (Cho SMS Gateway Việt Nam)

### Các dịch vụ SMS Việt Nam phổ biến:
- **FPT SMS**: https://sms.fpt.vn/
- **Viettel Solutions**: https://viettel-solutions.vn/
- **VinaPhone**: https://vietnamsms.com/
- **SMS Brandname**: https://smsbrandname.vn/

### Ví dụ: FPT SMS

#### Bước 1: Đăng ký
1. Truy cập: https://sms.fpt.vn/
2. Đăng ký tài khoản
3. Lấy **API Key** và **API Secret**

#### Bước 2: Cấu hình trong appsettings.json
```json
"SmsSettings": {
  "Provider": "generic",
  "Enabled": "true",
  "ApiKey": "your-fpt-api-key",
  "ApiSecret": "your-fpt-api-secret",
  "ApiUrl": "https://api.sms.fpt.vn/send",
  "SenderId": "RESORT",
  "Note": "FPT SMS configuration"
}
```

### Ví dụ: SMS Gateway khác (tùy chỉnh)

Nếu SMS gateway của bạn có format API khác, bạn có thể:
1. Sử dụng `Provider: "generic"`
2. Điều chỉnh `ApiUrl` và format payload trong `SmsService.cs` nếu cần

---

## 4. Test SMS

### Cách test:
1. Set `Enabled: "true"` trong appsettings.json
2. Đảm bảo đã cấu hình đầy đủ thông tin
3. Đặt phòng hoặc thanh toán trên website
4. Kiểm tra log để xem SMS có được gửi không

### Kiểm tra log:
```bash
# Xem log trong console hoặc file log
[SmsService] ✅ SMS sent successfully to +84901234567
```

---

## 5. Chi phí ước tính

| Provider | Giá/SMS | Ghi chú |
|----------|---------|---------|
| Twilio | ~180 VNĐ | Phổ biến, dễ tích hợp |
| AWS SNS | ~155 VNĐ | Tốt cho production |
| FPT SMS | ~200-300 VNĐ | SMS Brandname VN |
| Viettel | ~250-350 VNĐ | SMS Brandname VN |

**Lưu ý:** Giá có thể thay đổi, vui lòng kiểm tra website chính thức của provider.

---

## 6. Troubleshooting

### SMS không được gửi:
1. ✅ Kiểm tra `Enabled: "true"`
2. ✅ Kiểm tra API Key/Secret đúng chưa
3. ✅ Kiểm tra số điện thoại format đúng (phải có +84)
4. ✅ Kiểm tra log để xem lỗi cụ thể
5. ✅ Kiểm tra balance/credit của tài khoản SMS

### Lỗi thường gặp:
- **401 Unauthorized**: API Key/Secret sai
- **402 Payment Required**: Hết credit/balance
- **Invalid phone number**: Format số điện thoại sai
- **Rate limit**: Gửi quá nhiều SMS trong thời gian ngắn

---

## 7. Bảo mật

⚠️ **QUAN TRỌNG:** Không commit file `appsettings.json` có chứa API keys thật lên GitHub!

### Cách bảo vệ:
1. Sử dụng **User Secrets** (development):
   ```bash
   dotnet user-secrets set "SmsSettings:ApiKey" "your-key"
   dotnet user-secrets set "SmsSettings:ApiSecret" "your-secret"
   ```

2. Sử dụng **Environment Variables** (production):
   ```bash
   export SmsSettings__ApiKey="your-key"
   export SmsSettings__ApiSecret="your-secret"
   ```

3. Sử dụng **Azure Key Vault** hoặc **AWS Secrets Manager** (cloud)

---

## 8. Tắt SMS tạm thời

Nếu muốn tắt SMS nhưng giữ cấu hình:
```json
"SmsSettings": {
  "Enabled": "false",
  ...
}
```

Hệ thống sẽ log SMS nhưng không thực sự gửi đi.

---

## Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. Log file để xem lỗi chi tiết
2. Documentation của SMS provider
3. Console output khi chạy ứng dụng

