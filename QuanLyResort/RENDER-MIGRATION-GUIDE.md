# 🔄 Hướng Dẫn Chuyển Từ Railway Sang Render

## 📋 So Sánh Environment Variables

### ✅ Không Cần Thay Đổi (Copy Nguyên Vẹn)

Các biến sau có thể copy trực tiếp từ Railway sang Render:

1. **ASP.NET Core:**
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ASPNETCORE_URLS=http://0.0.0.0:10000`
   - `PORT=10000`

2. **Database:**
   - `ConnectionStrings__DefaultConnection=Data Source=resort.db`

3. **JWT Settings:**
   - `JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$`
   - `JwtSettings__Issuer=ResortManagementAPI`
   - `JwtSettings__Audience=ResortManagementClient`
   - `JwtSettings__ExpirationHours=24`

4. **PayOs Settings:**
   - `BankWebhook__PayOs__ClientId=90ad103f-aa49-4c33-9692-76d739a68b1b`
   - `BankWebhook__PayOs__ApiKey=acb138f1-a0f0-4a1f-9692-16d54332a580`
   - `BankWebhook__PayOs__ChecksumKey=44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c`
   - `BankWebhook__PayOs__SecretKey=44affe6d08bc7f9b8147ea701413ab2421739b97c69b3cb401d3d31f587cbb1c`
   - `BankWebhook__PayOs__VerifySignature=false`

5. **SePay Settings:**
   - `SePay__AccountId=5365`
   - `SePay__ApiBaseUrl=https://pgapi.sepay.vn`
   - `SePay__ApiToken=PWGH9OZC4OEMDYNDIIGLWRMTQQQZNA49JU3FFY5LXI8STESEJA6EIBYCP7BOQXFH`
   - `SePay__BankAccountNumber=0901329227`
   - `SePay__BankCode=MB`
   - `SePay__MerchantId=SP-LIVE-LT39A334`

6. **VietQR Settings:**
   - `VietQR__BankAccountNumber=0901329227`
   - `VietQR__BankCode=MB`

7. **AI Chat Settings:**
   - `AIChat__Provider=groq`
   - `AIChat__ApiKey=YOUR_GROQ_API_KEY_HERE` (thay bằng API key thực tế)
   - `AIChat__ApiUrl=https://api.groq.com/openai/v1/chat/completions`
   - `AIChat__Model=llama-3.1-8b-instant`

### ⚠️ CẦN THAY ĐỔI (Sau Khi Deploy)

Các biến sau cần cập nhật **SAU KHI** Render deploy xong và có URL:

1. **BankWebhook__PayOs__WebhookUrl**
   - **Hiện tại (Railway):** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - **Cần thay bằng:** `https://your-service-name.onrender.com/api/simplepayment/webhook`
   - **Cách làm:** Sau khi deploy, Render sẽ cung cấp URL (ví dụ: `quanlyresort-api.onrender.com`), thay vào đây

2. **SEPAY_WEBHOOK_URL**
   - **Hiện tại (Railway):** `https://quanlyresort-production.up.railway.app/api/simplepayment/webhook`
   - **Cần thay bằng:** `https://your-service-name.onrender.com/api/simplepayment/webhook`
   - **Cách làm:** Dùng cùng URL như trên

### ❌ KHÔNG CẦN (Có Thể Bỏ Qua)

- `Source=/data/resort.db` - Biến này không cần thiết, có thể bỏ qua

## 📝 Checklist Chuyển Đổi

### Bước 1: Copy Biến Không Cần Thay Đổi
- [ ] Copy tất cả các biến từ phần "Không Cần Thay Đổi" vào Render
- [ ] Đảm bảo format đúng (không có dấu ngoặc kép `"`)

### Bước 2: Deploy Trên Render
- [ ] Deploy service trên Render
- [ ] Chờ deploy hoàn tất
- [ ] Lấy URL của service (ví dụ: `quanlyresort-api.onrender.com`)

### Bước 3: Cập Nhật Webhook URLs
- [ ] Cập nhật `BankWebhook__PayOs__WebhookUrl` với URL Render mới
- [ ] Cập nhật `SEPAY_WEBHOOK_URL` với URL Render mới
- [ ] Redeploy hoặc restart service để áp dụng thay đổi

### Bước 4: Cập Nhật Webhook Trên PayOs/SePay
- [ ] Vào PayOs dashboard → Cập nhật Webhook URL
- [ ] Vào SePay dashboard → Cập nhật Webhook URL
- [ ] Test webhook để đảm bảo hoạt động

## 🔍 Format Lưu Ý

**Trên Render, KHÔNG cần dấu ngoặc kép:**
- ✅ Đúng: `AIChat__ApiKey=YOUR_GROQ_API_KEY_HERE`
- ❌ Sai: `AIChat__ApiKey="YOUR_GROQ_API_KEY_HERE"`

## 📄 File Tham Khảo

Xem file `RENDER-ENV-VARIABLES-COMPLETE.txt` để có danh sách đầy đủ và sẵn sàng copy-paste.

