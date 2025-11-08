# 🔧 Cấu Hình Render trên GitHub

## 📋 Trang Hiện Tại

Bạn đang ở trang **"Render for GitHub"** trong GitHub Settings.

## ✅ Bước 1: Chọn Repository Access

Trong phần **"Repository access"**, bạn có 2 lựa chọn:

### Option 1: All repositories (Đã chọn)
- ✅ Render có quyền truy cập tất cả repositories
- ✅ Tự động áp dụng cho repositories mới
- ⚠️ Có thể quá rộng nếu bạn có nhiều repos

### Option 2: Only select repositories (Khuyến nghị)
- ✅ Chỉ cho phép Render truy cập repository `quanlyresort`
- ✅ Bảo mật hơn
- ✅ Kiểm soát tốt hơn

**Cách chọn:**
1. Click radio button **"Only select repositories"**
2. Chọn repository **"quanlyresort"** từ danh sách
3. Click **"Save"**

## ✅ Bước 2: Xác Nhận Cấu Hình

Sau khi click **"Save"**, bạn sẽ thấy:
- ✅ Repository `quanlyresort` đã được chọn
- ✅ Render có quyền truy cập repository này

## 🚀 Bước 3: Vào Render Dashboard

Sau khi cấu hình xong, vào Render Dashboard:

**Link:** https://dashboard.render.com

## 📋 Bước 4: Tạo Web Service

1. Click **"New +"** → **"Web Service"**
2. **Connect GitHub** → Chọn repository `quanlyresort`
3. **Cấu hình:**
   - **Name:** `quanlyresort-api`
   - **Environment:** `.NET`
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/QuanLyResort.dll`
   - **Instance Type:** Free
4. **Environment Variables:** (xem `QUICK-DEPLOY-RENDER.md`)
5. Click **"Create Web Service"**

## ✅ Hoàn Thành!

Sau khi deploy xong, bạn sẽ có URL:
```
https://quanlyresort-api.onrender.com
```

## 📖 Tài Liệu Tham Khảo

- **Deploy Render:** `QUICK-DEPLOY-RENDER.md`
- **Config PayOs:** `HUONG-DAN-CONFIG-PAYOS-API.md`
- **Hướng dẫn đầy đủ:** `HUONG-DAN-DAY-DU.md`

