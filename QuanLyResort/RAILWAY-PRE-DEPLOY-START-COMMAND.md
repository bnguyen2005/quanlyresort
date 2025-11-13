# ⚙️ Railway Pre-deploy và Start Command

## ❌ Không Cần Điền Các Trường Này

**Cho project này (dùng Dockerfile):**
- ❌ **Pre-deploy Command:** Không cần (để trống)
- ❌ **Start Command:** Không cần (để trống)

## 🔍 Giải Thích

### Pre-deploy Command

**Là gì:**
- Chạy command trước khi deploy (ví dụ: `npm run migrate`, `dotnet ef database update`)
- Chạy trong Docker image trước khi start application

**Khi nào cần:**
- ✅ Nếu không dùng Dockerfile (dùng .NET native build)
- ✅ Nếu cần chạy migration trước khi start
- ✅ Nếu cần setup database trước khi deploy

**Cho project này:**
- ❌ **Không cần** - Vì dùng Dockerfile
- ❌ **Không cần** - Database migration đã được xử lý trong `Program.cs` (dòng 287-343)
- ❌ **Không cần** - Dockerfile đã có ENTRYPOINT

### Start Command

**Là gì:**
- Command để start application (ví dụ: `dotnet QuanLyResort.dll`, `npm start`)
- Override command mặc định từ Dockerfile

**Khi nào cần:**
- ✅ Nếu không dùng Dockerfile (dùng .NET native build)
- ✅ Nếu muốn override ENTRYPOINT từ Dockerfile
- ✅ Nếu cần custom start command

**Cho project này:**
- ❌ **Không cần** - Vì dùng Dockerfile
- ❌ **Không cần** - Dockerfile đã có ENTRYPOINT: `exec dotnet QuanLyResort.dll`
- ❌ **Không cần** - Railway sẽ tự động dùng ENTRYPOINT từ Dockerfile

## ✅ Cấu Hình Đúng Cho Project Này

### Build Section
- **Builder:** Dockerfile ✅
- **Dockerfile Path:** `QuanLyResort/Dockerfile` ✅
- **Pre-deploy Command:** Để trống ✅
- **Start Command:** Để trống ✅

### Deploy Section
- **Custom Start Command:** Để trống ✅
- **Pre-deploy step:** Để trống ✅

## 🔍 Railway Tự Động Detect

**Railway sẽ tự động:**
1. Detect Dockerfile
2. Build Docker image từ Dockerfile
3. Dùng ENTRYPOINT từ Dockerfile để start application
4. Không cần Pre-deploy hoặc Start Command

## 📋 Dockerfile ENTRYPOINT

**File:** `QuanLyResort/docker-entrypoint.sh`

**Nội dung:**
```bash
#!/bin/sh
# Entrypoint script để đọc PORT từ environment variable
# ... validation và setup ...
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
exec dotnet QuanLyResort.dll
```

**Railway sẽ tự động:**
- Chạy `docker-entrypoint.sh` khi start container
- Script sẽ set PORT và chạy `dotnet QuanLyResort.dll`
- Không cần Start Command

## ⚠️ Nếu Điền Các Trường Này

### Nếu Điền Pre-deploy Command

**Ví dụ:** `npm run migrate`

**Kết quả:**
- ❌ Lỗi: `npm: command not found` (vì đây là .NET project, không phải Node.js)
- ❌ Không cần thiết (database migration đã được xử lý trong code)

### Nếu Điền Start Command

**Ví dụ:** `dotnet QuanLyResort.dll`

**Kết quả:**
- ⚠️ Override ENTRYPOINT từ Dockerfile
- ⚠️ Mất logic xử lý PORT từ `docker-entrypoint.sh`
- ❌ Có thể gây lỗi PORT validation

## ✅ Kết Luận

**Cho project này:**
- ✅ **Pre-deploy Command:** Để trống (không cần)
- ✅ **Start Command:** Để trống (không cần)
- ✅ **Railway sẽ tự động:** Detect Dockerfile và dùng ENTRYPOINT

**Railway sẽ tự động deploy nếu:**
- ✅ Auto Deploy được bật
- ✅ Có Dockerfile
- ✅ Có commit mới trên GitHub

**Không cần điền Pre-deploy hoặc Start Command để tự động deploy!**

## 🔗 Links

- **Railway Dashboard:** https://railway.app
- **Service Settings:** Railway Dashboard → Settings
- **Dockerfile:** `QuanLyResort/Dockerfile`

## 💡 Lưu Ý

1. **Dockerfile** - Railway tự động detect và dùng ENTRYPOINT
2. **Pre-deploy** - Chỉ cần nếu không dùng Dockerfile hoặc cần custom setup
3. **Start Command** - Chỉ cần nếu muốn override ENTRYPOINT
4. **Auto Deploy** - Phụ thuộc vào Auto Deploy setting, không phụ thuộc vào Pre-deploy/Start Command

