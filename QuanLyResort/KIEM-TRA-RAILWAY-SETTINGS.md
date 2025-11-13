# 🔍 Kiểm Tra Railway Settings

## ✅ Settings Hiện Tại

### Source
- **Source Repo:** `Lamm123435469898/quanlyresort` ✅
- **Branch:** `main` ✅
- **Root Directory:** Để trống ✅ (Đúng - vì Dockerfile build context là root)

### Build
- **Builder:** Dockerfile ✅
- **Dockerfile Path:** `/QuanLyResort/Dockerfile` ⚠️ (Cần kiểm tra)

### Networking
- **Domain:** `quanlyresort-production.up.railway.app` ✅
- **Port:** `10000` ✅

## ⚠️ Vấn Đề Tiềm Ẩn

### Dockerfile Path

**Hiện tại:** `/QuanLyResort/Dockerfile` (absolute path với dấu `/` ở đầu)

**Vấn đề:**
- Railway có thể không nhận diện đúng path với dấu `/` ở đầu
- Nên dùng relative path từ repo root

**Fix:**
- Đổi thành: `QuanLyResort/Dockerfile` (không có dấu `/` ở đầu)
- Hoặc: `./QuanLyResort/Dockerfile`

## ✅ Cấu Hình Đúng

### Option 1: Root Directory Để Trống (Khuyên Dùng)

**Settings:**
- **Root Directory:** Để trống (không set)
- **Dockerfile Path:** `QuanLyResort/Dockerfile` (relative path, không có dấu `/`)

**Lý do:**
- Dockerfile build context là root của repo
- Dockerfile copy từ `QuanLyResort/QuanLyResort.csproj` (từ root context)
- Entrypoint script ở `QuanLyResort/docker-entrypoint.sh`

### Option 2: Root Directory = QuanLyResort

**Settings:**
- **Root Directory:** `QuanLyResort`
- **Dockerfile Path:** `Dockerfile` (vì đã ở trong QuanLyResort rồi)

**Lưu ý:**
- Cần đảm bảo Dockerfile build context vẫn đúng
- Có thể cần sửa Dockerfile nếu build context thay đổi

## 🔧 Cách Fix

### Bước 1: Vào Railway Settings

1. Railway Dashboard → Service `quanlyresort`
2. Tab **"Settings"**
3. Scroll xuống phần **"Build"**

### Bước 2: Sửa Dockerfile Path

**Hiện tại:**
```
Dockerfile Path: /QuanLyResort/Dockerfile
```

**Đổi thành:**
```
Dockerfile Path: QuanLyResort/Dockerfile
```

**Hoặc:**
```
Dockerfile Path: ./QuanLyResort/Dockerfile
```

### Bước 3: Kiểm Tra Root Directory

**Đảm bảo:**
- **Root Directory:** Để trống (không set)
- Hoặc nếu đã set, xóa đi

### Bước 4: Save Changes

1. Click **"Update"** (hoặc **"Save"**)
2. Railway sẽ tự động trigger deploy mới
3. Đợi 2-3 phút

## 🔍 Kiểm Tra Sau Khi Fix

### Bước 1: Xem Deployments

**Railway Dashboard → Deployments**

**Tìm deployment mới:**
- Status: "Building" → "Deploying" → "Active"
- Xem build logs có lỗi không

### Bước 2: Xem Build Logs

**Railway Dashboard → Logs**

**Tìm:**
```
Building Docker image...
Step 1/10 : FROM mcr.microsoft.com/dotnet/sdk:8.0
...
Successfully built ...
```

**Nếu thấy lỗi:**
```
Error: Dockerfile not found
Error: COPY failed: file not found
```

→ Dockerfile path chưa đúng, cần sửa lại

### Bước 3: Test Service

**Sau khi deploy xong:**
```bash
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

**Kết quả mong đợi:**
```json
{
  "status": "active",
  "endpoint": "/api/simplepayment/webhook",
  "message": "Webhook endpoint is ready"
}
```

## 📋 Checklist

- [ ] Root Directory: Để trống (hoặc `QuanLyResort` nếu cần)
- [ ] Dockerfile Path: `QuanLyResort/Dockerfile` (không có dấu `/` ở đầu)
- [ ] Đã save changes
- [ ] Railway đã trigger deploy mới
- [ ] Build logs không có lỗi
- [ ] Service hoạt động bình thường

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Settings:** Railway Dashboard → Settings
- **Service Deployments:** Railway Dashboard → Deployments
- **Service Logs:** Railway Dashboard → Logs

## 💡 Lưu Ý

1. **Dockerfile path** - Nên dùng relative path từ repo root
2. **Root Directory** - Để trống nếu Dockerfile build context là root
3. **Build context** - Dockerfile copy từ `QuanLyResort/QuanLyResort.csproj` (từ root)
4. **Auto deploy** - Railway sẽ tự động deploy sau khi save settings

## 🎯 Kết Luận

**Settings hiện tại:**
- ✅ Source repo và branch đúng
- ✅ Port và domain đúng
- ⚠️ Dockerfile path có thể cần sửa: `/QuanLyResort/Dockerfile` → `QuanLyResort/Dockerfile`

**Khuyến nghị:**
- Sửa Dockerfile path thành `QuanLyResort/Dockerfile` (không có dấu `/` ở đầu)
- Đảm bảo Root Directory để trống
- Save và đợi Railway deploy lại

