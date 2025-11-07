# ⚡ Quick Deploy Lên Render (5 Phút)

## 🚀 Các Bước Nhanh

### Bước 1: Push Code Lên GitHub

```bash
cd QuanLyResort
git init
git add .
git commit -m "Ready for deployment"
git remote add origin https://github.com/YOUR_USERNAME/quanlyresort.git
git push -u origin main
```

### Bước 2: Tạo Service Trên Render

1. **Vào:** https://dashboard.render.com
2. **Click:** "New +" → "Web Service"
3. **Connect GitHub** → Chọn repository
4. **Cấu hình:**
   - **Name:** `quanlyresort-api`
   - **Environment:** `.NET`
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/QuanLyResort.dll`
   - **Instance Type:** Free

5. **Environment Variables:**
   - Copy từ `render.yaml` hoặc thêm thủ công:
     ```
     ASPNETCORE_ENVIRONMENT=Production
     ASPNETCORE_URLS=http://0.0.0.0:10000
     ConnectionStrings__DefaultConnection=<YOUR_DB_CONNECTION>
     JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
     BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
     BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
     BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
     BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
     ```

6. **Click:** "Create Web Service"

### Bước 3: Đợi Deploy (5-10 phút)

Render sẽ tự động:
- Build project
- Deploy lên server
- Tạo HTTPS URL

### Bước 4: Lấy URL

Sau khi deploy xong, bạn sẽ có URL:
```
https://quanlyresort-api.onrender.com
```

### Bước 5: Config PayOs Webhook

```bash
cd QuanLyResort
./config-payos-webhook.sh https://quanlyresort-api.onrender.com/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success"
}
```

### Bước 6: Test

```bash
# Test webhook status
curl https://quanlyresort-api.onrender.com/api/simplepayment/webhook-status

# Test webhook
curl -X POST https://quanlyresort-api.onrender.com/api/simplepayment/webhook \
  -H "Content-Type: application/json" \
  -d '{"content": "BOOKING10", "amount": 5000}'
```

## ✅ Hoàn Thành!

Bây giờ PayOs webhook sẽ hoạt động tự động 100%!

## ⚠️ Lưu Ý Render Free Tier

- **Service sẽ sleep** sau 15 phút không có request
- **Lần đầu request** sẽ mất ~30 giây để wake up
- **Giải pháp:** Upgrade lên Starter ($7/tháng) hoặc dùng Railway

## 🔄 Update Code

Mỗi khi push code lên GitHub, Render sẽ tự động:
1. Build lại
2. Deploy lại
3. Restart service

```bash
git add .
git commit -m "Update code"
git push
```

