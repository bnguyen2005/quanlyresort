# 🚂 Hướng Dẫn Deploy Lên Railway

## 🎯 Tổng Quan

Railway là platform tốt cho .NET Core với:
- ✅ Free tier ($5 credit/tháng)
- ✅ Auto-deploy từ GitHub
- ✅ Hỗ trợ Docker và .NET native
- ✅ Database tích hợp (PostgreSQL, MySQL, MongoDB)
- ✅ HTTPS tự động

## 📋 Bước 1: Đăng Ký Railway

1. **Vào:** https://railway.app
2. **Đăng ký** bằng GitHub account
3. **Chọn plan:** Hobby (Free tier)

## 🚀 Bước 2: Tạo Project Mới

1. **Click:** "New Project"
2. **Chọn:** "Deploy from GitHub repo"
3. **Chọn repository:** `Lamm123435469898/quanlyresort`
4. **Chọn branch:** `main`

## ⚙️ Bước 3: Cấu Hình Service

Railway sẽ tự động detect `.NET` hoặc `Dockerfile`. Nếu không:

### Option A: Dùng Dockerfile (Khuyến nghị)

1. **Service Settings** → **Source**
   - **Root Directory:** `QuanLyResort`
   - **Dockerfile Path:** `Dockerfile` (hoặc để trống nếu ở root)

2. **Service Settings** → **Deploy**
   - Railway sẽ tự động build từ Dockerfile

### Option B: Dùng .NET Native

1. **Service Settings** → **Source**
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/QuanLyResort.dll`

## 🔐 Bước 4: Environment Variables

Vào **Variables** tab và thêm:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
PORT=10000

# Database (SQLite - file-based)
ConnectionStrings__DefaultConnection=Data Source=/data/resort.db

# JWT Settings
JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
JwtSettings__Issuer=ResortManagementAPI
JwtSettings__Audience=ResortManagementClient
JwtSettings__ExpirationHours=24

# PayOs Settings
BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
BankWebhook__PayOs__VerifySignature=false
BankWebhook__PayOs__WebhookUrl=https://YOUR_RAILWAY_URL.up.railway.app/api/simplepayment/webhook

# AI Chat Settings
AIChat__Provider=groq
AIChat__ApiKey=YOUR_GROQ_API_KEY_HERE
AIChat__ApiUrl=https://api.groq.com/openai/v1/chat/completions
AIChat__Model=llama-3.1-8b-instant
```

## 💾 Bước 5: Persistent Storage (Cho SQLite)

Nếu dùng SQLite, cần persistent volume:

1. **Service Settings** → **Volumes**
2. **Click:** "Add Volume"
3. **Mount Path:** `/data`
4. **Size:** 1GB (đủ cho SQLite)

## 🌐 Bước 6: Lấy URL

Sau khi deploy thành công:

1. **Service Settings** → **Networking**
2. **Generate Domain** (nếu chưa có)
3. URL sẽ là: `https://YOUR_SERVICE_NAME.up.railway.app`

## 🔄 Bước 7: Auto-Deploy

Railway tự động deploy khi:
- Push code lên GitHub
- Merge PR vào `main` branch

Có thể tắt/bật trong **Settings** → **Deployments**

## 📝 Bước 8: Cập Nhật PayOs Webhook

Sau khi có URL Railway:

1. **Copy URL:** `https://YOUR_SERVICE_NAME.up.railway.app/api/simplepayment/webhook`
2. **Cập nhật trong PayOs Dashboard:**
   - Vào: https://payos.vn
   - Settings → Webhook URL
   - Paste URL Railway

## ✅ Kiểm Tra

1. **Health Check:**
   ```bash
   curl https://YOUR_SERVICE_NAME.up.railway.app/api/health
   ```

2. **Swagger:**
   ```
   https://YOUR_SERVICE_NAME.up.railway.app/swagger
   ```

3. **Test Webhook:**
   ```bash
   curl https://YOUR_SERVICE_NAME.up.railway.app/api/simplepayment/webhook-status
   ```

## 🐛 Troubleshooting

### Lỗi: "Port not found"
→ Thêm `PORT` environment variable

### Lỗi: "Database locked"
→ SQLite không phù hợp cho production, nên dùng PostgreSQL

### Lỗi: "Build failed"
→ Kiểm tra `railway.json` và `Dockerfile`

## 💡 Tips

- **Free tier:** $5 credit/tháng, đủ cho development
- **Sleep mode:** Railway không sleep như Render
- **Logs:** Xem real-time logs trong Railway dashboard
- **Metrics:** Xem CPU, Memory, Network usage

## 🔗 Links Hữu Ích

- Railway Docs: https://docs.railway.app
- .NET on Railway: https://docs.railway.app/languages/dotnet
- Railway Discord: https://discord.gg/railway

