# 🚀 Hướng Dẫn Deploy Lên Server Thật

## 🎯 Mục Đích

Deploy backend lên server có **HTTPS thật** để PayOs webhook hoạt động tự động 100%.

## 📋 Các Platform Hỗ Trợ .NET

### ✅ Khuyến Nghị (Hỗ Trợ .NET Tốt):

1. **Azure App Service** ⭐ (Tốt nhất cho .NET)
2. **Render** (Free tier, dễ dùng)
3. **Railway** (Free tier, dễ dùng)
4. **Google Cloud Run** (Pay-as-you-go)

### ❌ Không Phù Hợp:

- **Vercel** - Chủ yếu cho Node.js, không hỗ trợ .NET backend tốt

## 🚀 Option 1: Deploy Lên Render (Khuyến Nghị - Free)

### Bước 1: Chuẩn Bị

1. **Đăng ký tài khoản Render:**
   - Truy cập: https://render.com
   - Đăng ký bằng GitHub

2. **Push code lên GitHub:**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin https://github.com/Lamm123435469898/quanlyresort.git
   git push -u origin main
   ```

### Bước 2: Tạo Web Service Trên Render

1. **Vào Render Dashboard** → Click "New +" → "Web Service"
2. **Connect GitHub repository**
3. **Cấu hình:**
   - **Name:** `quanlyresort-api`
   - **Environment:** `.NET`
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/QuanLyResort.dll`
   - **Instance Type:** Free (hoặc Starter nếu cần)

4. **Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=<YOUR_DATABASE_CONNECTION_STRING>
   JwtSettings__SecretKey=<YOUR_JWT_SECRET>
   BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
   BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
   BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
   BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
   ```

5. **Click "Create Web Service"**

### Bước 3: Lấy URL

Sau khi deploy thành công, bạn sẽ có URL:
```
https://quanlyresort-api.onrender.com
```

### Bước 4: Config PayOs Webhook

```bash
cd QuanLyResort
./config-payos-webhook.sh https://quanlyresort-api.onrender.com/api/simplepayment/webhook
```

## 🚀 Option 2: Deploy Lên Railway (Free Tier)

### Bước 1: Chuẩn Bị

1. **Đăng ký Railway:**
   - Truy cập: https://railway.app
   - Đăng ký bằng GitHub

2. **Push code lên GitHub** (nếu chưa có)

### Bước 2: Tạo Project

1. **Vào Railway Dashboard** → "New Project"
2. **"Deploy from GitHub repo"**
3. **Chọn repository**

### Bước 3: Cấu Hình

Railway tự động detect .NET và cấu hình. Bạn chỉ cần:

1. **Thêm Environment Variables:**
   - Vào "Variables" tab
   - Thêm các biến như Render (xem trên)

2. **Lấy URL:**
   - Railway tự động tạo URL: `https://your-app.railway.app`

### Bước 4: Config PayOs Webhook

```bash
./config-payos-webhook.sh https://your-app.railway.app/api/simplepayment/webhook
```

## 🚀 Option 3: Deploy Lên Azure App Service (Tốt Nhất Cho .NET)

### Bước 1: Cài Azure CLI

```bash
# macOS
brew install azure-cli

# Hoặc download từ: https://aka.ms/installazurecliwindows
```

### Bước 2: Login Azure

```bash
az login
```

### Bước 3: Tạo App Service

```bash
# Tạo resource group
az group create --name quanlyresort-rg --location eastus

# Tạo App Service plan (Free tier)
az appservice plan create \
  --name quanlyresort-plan \
  --resource-group quanlyresort-rg \
  --sku FREE

# Tạo Web App
az webapp create \
  --name quanlyresort-api \
  --resource-group quanlyresort-rg \
  --plan quanlyresort-plan \
  --runtime "DOTNET|8.0"

# Deploy code
az webapp deployment source config-local-git \
  --name quanlyresort-api \
  --resource-group quanlyresort-rg
```

### Bước 4: Config Environment Variables

```bash
az webapp config appsettings set \
  --name quanlyresort-api \
  --resource-group quanlyresort-rg \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="<YOUR_CONNECTION_STRING>" \
    JwtSettings__SecretKey="<YOUR_SECRET>"
```

### Bước 5: Deploy Code

```bash
# Add Azure remote
git remote add azure https://quanlyresort-api.scm.azurewebsites.net/quanlyresort-api.git

# Deploy
git push azure main
```

### Bước 6: Lấy URL

URL sẽ là: `https://quanlyresort-api.azurewebsites.net`

## 🔧 Cấu Hình Database

### Option A: SQL Server trên Azure (Khuyến Nghị)

1. **Tạo SQL Database trên Azure:**
   ```bash
   az sql server create \
     --name quanlyresort-sql \
     --resource-group quanlyresort-rg \
     --location eastus \
     --admin-user adminuser \
     --admin-password <YOUR_PASSWORD>

   az sql db create \
     --resource-group quanlyresort-rg \
     --server quanlyresort-sql \
     --name ResortManagementDb \
     --service-objective Basic
   ```

2. **Connection String:**
   ```
   Server=tcp:quanlyresort-sql.database.windows.net,1433;Initial Catalog=ResortManagementDb;Persist Security Info=False;User ID=adminuser;Password=<YOUR_PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

### Option B: SQLite (Đơn Giản - Chỉ Cho Test)

SQLite có thể dùng cho development, nhưng production nên dùng SQL Server.

## ✅ Sau Khi Deploy

### Bước 1: Test Backend

```bash
# Test webhook status
curl https://your-domain.com/api/simplepayment/webhook-status
```

### Bước 2: Config PayOs Webhook

```bash
cd QuanLyResort
./config-payos-webhook.sh https://your-domain.com/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success",
  "data": {
    "webhookUrl": "https://your-domain.com/api/simplepayment/webhook"
  }
}
```

### Bước 3: Test Thanh Toán

1. **Mở trang web:** `https://your-domain.com/customer/my-bookings.html`
2. **Click "Thanh toán"** cho booking pending
3. **Quét QR và thanh toán** với nội dung: `BOOKING10`
4. **Quan sát:**
   - PayOs tự động gọi webhook
   - Backend update booking status
   - Frontend tự động ẩn QR và hiện success message

## 📋 Checklist Deploy

- [ ] Code đã push lên GitHub
- [ ] Tạo service trên platform (Render/Railway/Azure)
- [ ] Config environment variables
- [ ] Deploy thành công
- [ ] Test webhook status endpoint
- [ ] Config PayOs webhook URL
- [ ] Test thanh toán thật
- [ ] Verify webhook hoạt động

## 🎯 Kết Quả

Sau khi deploy:
- ✅ **HTTPS thật** → PayOs verify được webhook URL
- ✅ **Webhook tự động 100%** → Không cần manual
- ✅ **Real-time** → Payment được detect ngay lập tức
- ✅ **Ổn định** → Không phụ thuộc vào ngrok

## 💡 Tips

1. **Render Free Tier:**
   - Service sẽ sleep sau 15 phút không có request
   - Lần đầu request sẽ mất ~30 giây để wake up
   - Upgrade lên Starter ($7/tháng) để tránh sleep

2. **Railway Free Tier:**
   - Có $5 credit miễn phí mỗi tháng
   - Sau đó pay-as-you-go

3. **Azure Free Tier:**
   - App Service Free tier có giới hạn
   - SQL Database Basic tier ~$5/tháng

## 🔗 Links

- **Render:** https://render.com
- **Railway:** https://railway.app
- **Azure:** https://azure.microsoft.com
- **PayOs API Docs:** https://payos.vn/docs

