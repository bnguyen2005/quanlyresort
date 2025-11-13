# 🌐 Cách Generate Public Domain Trên Railway

## ⚠️ Vấn Đề Hiện Tại

Bạn đang thấy: `quanlyresort.railway.internal`

**Đây là internal domain** - chỉ dùng để các service trong cùng project giao tiếp với nhau, **KHÔNG thể truy cập từ internet**.

## ✅ Giải Pháp: Generate Public Domain

### Cách 1: Từ Service Settings (Khuyến nghị)

1. **Vào Railway Dashboard**
2. **Click vào service `quanlyresort`**
3. **Click tab "Settings"** (ở trên cùng, bên cạnh "Deployments", "Variables", "Metrics")
4. **Scroll xuống tìm section "Networking"** hoặc **"Public Domain"**
5. **Tìm nút "Generate Domain"** hoặc **"Generate Public URL"**
6. **Click nút đó**

### Cách 2: Từ Service Overview

1. **Vào Railway Dashboard**
2. **Click vào service `quanlyresort`**
3. **Ở phần service details**, tìm section hiển thị domain
4. **Nếu thấy "Unexposed service"** hoặc chỉ có internal domain
5. **Click "Generate Domain"** hoặc **"Expose"**

### Cách 3: Từ Networking Tab (Nếu có)

1. **Vào service `quanlyresort`**
2. **Tìm tab "Networking"** (có thể ở trên cùng hoặc trong Settings)
3. **Click "Generate Domain"**

## 🎯 Kết Quả

Sau khi generate, bạn sẽ có URL dạng:

```
https://quanlyresort-production-XXXX.up.railway.app
```

Hoặc:

```
https://quanlyresort.up.railway.app
```

**URL này có thể truy cập từ internet!**

## 📋 Các Bước Tiếp Theo

### 1. Copy Public URL

Sau khi generate, copy URL public (không phải `.railway.internal`)

### 2. Test URL

Mở trình duyệt và vào:
```
https://YOUR_PUBLIC_URL.up.railway.app/swagger
```

### 3. Test Health Check

```bash
curl https://YOUR_PUBLIC_URL.up.railway.app/api/health
```

## 🔍 Nếu Không Tìm Thấy Nút "Generate Domain"

### Kiểm Tra:

1. **Bạn đang ở đúng service chưa?**
   - Đảm bảo đang ở service `quanlyresort`, không phải project level

2. **Service đã deploy thành công chưa?**
   - Vào tab "Deployments" → Kiểm tra có deployment "ACTIVE" không

3. **Kiểm tra Settings tab:**
   - Scroll xuống tất cả các sections
   - Tìm "Networking", "Public Domain", hoặc "Expose"

### Alternative: Dùng Railway CLI

Nếu không tìm thấy trên UI, có thể dùng Railway CLI:

```bash
# Cài Railway CLI (nếu chưa có)
npm i -g @railway/cli

# Login
railway login

# Generate domain
railway domain
```

## ⚠️ Lưu Ý Quan Trọng

- **Internal domain** (`*.railway.internal`): Chỉ dùng trong Railway network
- **Public domain** (`*.up.railway.app`): Có thể truy cập từ internet
- **HTTPS tự động**: Railway tự động cung cấp HTTPS cho public domain
- **Miễn phí**: Generate public domain là miễn phí trên Railway

## 🎉 Sau Khi Có Public URL

1. ✅ Test Swagger: `https://YOUR_URL/swagger`
2. ✅ Test API endpoints
3. ✅ Cập nhật PayOs webhook URL (nếu dùng)
4. ✅ Cập nhật frontend API base URL (nếu có)

## 🐛 Troubleshooting

### Không thấy nút "Generate Domain"

**Giải pháp:**
- Đảm bảo service đã deploy thành công
- Kiểm tra bạn có quyền admin trên project
- Thử refresh trang hoặc đăng nhập lại

### Domain đã tồn tại nhưng không truy cập được

**Kiểm tra:**
1. Service đang chạy (xem Logs)
2. PORT đã được set đúng (PORT=10000)
3. Application đã start (xem logs có "Application started")

### Lỗi 404 hoặc "Service not found"

**Giải pháp:**
- Đảm bảo đang dùng public URL (`*.up.railway.app`), không phải internal (`*.railway.internal`)
- Kiểm tra service đã expose chưa

