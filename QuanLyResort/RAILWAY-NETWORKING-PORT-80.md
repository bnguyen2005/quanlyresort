# 🔧 Railway Networking - Port 80 Configuration

## 📊 Hiểu Về Railway Networking

### Railway Port Mapping

Railway tự động route traffic như sau:
- **Public Port 80 (HTTP)** → **Container Port** (từ `PORT` environment variable)
- **Public Port 443 (HTTPS)** → **Container Port** (từ `PORT` environment variable)

**Quan trọng:**
- Railway domain (`quanlyresort-production.up.railway.app`) route đến **port 80** (public)
- Railway tự động map port 80 → container port (từ `PORT` env var)
- Container vẫn cần listen trên port được set trong `PORT` env var

## ✅ Cấu Hình Đúng

### Option 1: Container Listen Port 10000 (Khuyến Nghị)

**Cách hoạt động:**
- Railway route: `port 80 (public)` → `port 10000 (container)`
- Container listen: `0.0.0.0:10000`
- URL: `https://quanlyresort-production.up.railway.app` (Railway tự động route)

**Environment Variables:**
```env
PORT=10000
ASPNETCORE_URLS=http://0.0.0.0:10000
```

### Option 2: Container Listen Port 80

**Cách hoạt động:**
- Railway route: `port 80 (public)` → `port 80 (container)`
- Container listen: `0.0.0.0:80`
- URL: `https://quanlyresort-production.up.railway.app` (Railway tự động route)

**Environment Variables:**
```env
PORT=80
ASPNETCORE_URLS=http://0.0.0.0:80
```

**⚠️ Lưu ý:** Port 80 có thể cần quyền root trong container (không khuyến nghị)

## 🔧 Cấu Hình Railway

### Bước 1: Kiểm Tra Networking Settings

1. **Vào Railway Dashboard** → Service `quanlyresort`
2. **Tab "Settings"** → **"Networking"**
3. **Kiểm tra:**
   - **Public Domain:** `quanlyresort-production.up.railway.app`
   - **Port:** `80` (HTTP) - Đây là port public, Railway tự động route

### Bước 2: Cấu Hình Environment Variables

**Vào tab "Variables" và set:**

#### Nếu dùng Port 10000 (Khuyến nghị):
```env
PORT=10000
ASPNETCORE_URLS=http://0.0.0.0:10000
```

#### Nếu dùng Port 80:
```env
PORT=80
ASPNETCORE_URLS=http://0.0.0.0:80
```

### Bước 3: Kiểm Tra Container Port

Railway sẽ tự động route port 80 (public) đến container port (từ `PORT` env var).

**Không cần cấu hình thêm** - Railway tự động xử lý!

## 🔍 Kiểm Tra

### 1. Kiểm Tra Logs

Sau khi deploy, vào tab "Logs" và tìm:

✅ **Thành công (Port 10000):**
```
Using PORT: 10000
ASPNETCORE_URLS: http://0.0.0.0:10000
Now listening on: http://0.0.0.0:10000
```

✅ **Thành công (Port 80):**
```
Using PORT: 80
ASPNETCORE_URLS: http://0.0.0.0:80
Now listening on: http://0.0.0.0:80
```

### 2. Test URL

```bash
# Test Railway domain (port 80 public → container port)
curl https://quanlyresort-production.up.railway.app/api/health

# Test webhook endpoint
curl https://quanlyresort-production.up.railway.app/api/simplepayment/webhook
```

## ⚠️ Lưu Ý Quan Trọng

### Railway Tự Động Route Port

- **Không cần** cấu hình port mapping thủ công
- Railway tự động route port 80 (public) → container port
- Container chỉ cần listen trên port được set trong `PORT` env var

### Port 80 vs Port 10000

**Port 10000 (Khuyến nghị):**
- ✅ Không cần quyền root
- ✅ Tránh conflict với system services
- ✅ Railway tự động route port 80 → 10000

**Port 80:**
- ⚠️ Có thể cần quyền root trong container
- ⚠️ Có thể conflict với system services
- ✅ Railway vẫn tự động route port 80 → 80

## 🐛 Troubleshooting

### Lỗi: "Port 10000 không chạy được"

**Nguyên nhân:**
- Railway đang route port 80 → container, nhưng container listen port khác
- `PORT` env var không được set đúng

**Giải pháp:**
1. Kiểm tra `PORT` env var trong Railway Variables
2. Đảm bảo container listen trên port được set trong `PORT`
3. Railway sẽ tự động route port 80 → container port

### Lỗi: "Cannot bind to port 80"

**Nguyên nhân:**
- Port 80 có thể cần quyền root
- Container không có quyền bind port 80

**Giải pháp:**
- Dùng port 10000 thay vì port 80
- Railway vẫn route port 80 (public) → 10000 (container)

## 📋 Checklist

- [ ] Railway domain: `quanlyresort-production.up.railway.app` (port 80 public)
- [ ] `PORT` env var đã được set (10000 hoặc 80)
- [ ] Container listen trên port được set trong `PORT`
- [ ] Railway tự động route port 80 → container port
- [ ] Test URL hoạt động: `https://quanlyresort-production.up.railway.app/api/health`

## 💡 Khuyến Nghị

**Dùng Port 10000:**
- ✅ Không cần quyền root
- ✅ Tránh conflict
- ✅ Railway tự động route port 80 → 10000
- ✅ URL vẫn là: `https://quanlyresort-production.up.railway.app`

**Cấu hình:**
```env
PORT=10000
ASPNETCORE_URLS=http://0.0.0.0:10000
```

## 🎯 Kết Luận

- Railway domain route đến port 80 (public)
- Railway tự động route port 80 → container port (từ `PORT` env var)
- Container chỉ cần listen trên port được set trong `PORT`
- Không cần cấu hình port mapping thủ công
- Khuyến nghị dùng port 10000 trong container

