# ⚡ Quick Start: Deploy Lên Server Thật (5 Phút)

## 🎯 Mục Đích

Deploy backend lên server có **HTTPS thật** để PayOs webhook hoạt động tự động 100%.

## 🚀 Render (Khuyến Nghị - Free Tier)

### Bước 1: Push Code Lên GitHub

```bash
cd QuanLyResort
git init
git add .
git commit -m "Ready for deployment"
git remote add origin https://github.com/YOUR_USERNAME/quanlyresort.git
git push -u origin main
```

### Bước 2: Deploy Trên Render

1. **Vào:** https://dashboard.render.com
2. **"New +" → "Web Service"**
3. **Connect GitHub** → Chọn repo
4. **Cấu hình:**
   - **Name:** `quanlyresort-api`
   - **Environment:** `.NET`
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/QuanLyResort.dll`
   - **Instance Type:** Free

5. **Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:$PORT
   ConnectionStrings__DefaultConnection=<YOUR_DB_CONNECTION>
   JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration2025!@#$
   BankWebhook__PayOs__ClientId=c704495b-5984-4ad3-aa23-b2794a02aa83
   BankWebhook__PayOs__ApiKey=f6ea421b-a8b7-46b8-92be-209eb1a9b2fb
   BankWebhook__PayOs__ChecksumKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
   BankWebhook__PayOs__SecretKey=429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313
   ```

6. **Click "Create Web Service"**

### Bước 3: Đợi Deploy (5-10 phút)

Render sẽ tự động build và deploy. URL sẽ là:
```
https://quanlyresort-api.onrender.com
```

### Bước 4: Config PayOs Webhook

```bash
cd QuanLyResort
./config-payos-after-deploy.sh https://quanlyresort-api.onrender.com
```

**Kết quả mong đợi:**
```json
{
  "code": 0,
  "desc": "success"
}
```

## ✅ Hoàn Thành!

Bây giờ:
- ✅ **HTTPS thật** → PayOs verify được
- ✅ **Webhook tự động 100%** → Không cần manual
- ✅ **Real-time** → Payment detect ngay lập tức

## 🧪 Test Ngay

1. **Mở:** `https://quanlyresort-api.onrender.com/customer/my-bookings.html`
2. **Click "Thanh toán"** cho booking pending
3. **Quét QR và thanh toán** với nội dung: `BOOKING10`
4. **Quan sát:**
   - PayOs tự động gọi webhook
   - Backend update booking status
   - Frontend tự động ẩn QR và hiện success message

## ⚠️ Lưu Ý Render Free Tier

- Service sẽ **sleep** sau 15 phút không có request
- Lần đầu request sẽ mất **~30 giây** để wake up
- **Giải pháp:** Upgrade lên Starter ($7/tháng) hoặc dùng Railway

## 🔄 Update Code

Mỗi khi push code lên GitHub, Render tự động deploy:

```bash
git add .
git commit -m "Update"
git push
```

## 📋 Checklist

- [ ] Code đã push lên GitHub
- [ ] Tạo service trên Render
- [ ] Config environment variables
- [ ] Deploy thành công
- [ ] Test webhook status endpoint
- [ ] Config PayOs webhook
- [ ] Test thanh toán thật

