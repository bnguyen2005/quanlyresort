# ⚙️ Hướng Dẫn Setup Advanced Settings trên Render

## 📋 Cấu Hình Advanced Settings

Trên trang **"Advanced"** của Render, cấu hình như sau:

### 1. Secret Files
**Để trống** - Không cần thiết cho project này.

### 2. Health Check Path ⭐ QUAN TRỌNG
```
/api/health
```

**Giải thích:** Render sẽ gọi endpoint này để kiểm tra service có hoạt động không. Nếu endpoint trả về 200 OK, Render biết service đang healthy.

### 3. Registry Credential
**Chọn:** `No credential` (hoặc để mặc định)

**Giải thích:** Chỉ cần nếu bạn pull private Docker images từ registry. Project này dùng public images nên không cần.

### 4. Docker Build Context Directory
**Để trống** (hoặc để mặc định)

**Giải thích:** Build context là root của repo, không cần chỉ định.

### 5. Dockerfile Path ⭐ QUAN TRỌNG
```
QuanLyResort/Dockerfile
```

**Giải thích:** Dockerfile nằm trong thư mục `QuanLyResort/`, cần chỉ định đường dẫn chính xác.

### 6. Docker Command
**Để trống** (hoặc để mặc định)

**Giải thích:** Dockerfile đã có CMD/ENTRYPOINT, không cần override.

### 7. Pre-Deploy Command
**Để trống** (hoặc để mặc định)

**Giải thích:** 
- Nếu cần chạy database migrations, có thể thêm:
  ```
  dotnet ef database update --project QuanLyResort
  ```
- Nhưng hiện tại không cần vì migrations được chạy tự động khi app start.

### 8. Auto-Deploy
**Chọn:** `On Commit` (mặc định)

**Giải thích:** Render sẽ tự động deploy mỗi khi có commit mới lên branch `main`. Đây là tính năng hữu ích, nên giữ nguyên.

### 9. Build Filters
**Để trống** (hoặc để mặc định)

**Giải thích:** 
- Nếu muốn, có thể thêm **Ignored Paths** để tránh deploy khi chỉ thay đổi file không liên quan:
  - `*.md` (markdown files)
  - `docs/**` (nếu có thư mục docs)
  - `.gitignore`
- Nhưng thường không cần thiết.

## ✅ Tóm Tắt Cấu Hình

| Setting | Giá Trị | Ghi Chú |
|---------|---------|---------|
| **Health Check Path** | `/api/health` | ⭐ Bắt buộc |
| **Dockerfile Path** | `QuanLyResort/Dockerfile` | ⭐ Bắt buộc |
| **Docker Build Context Directory** | (để trống) | Mặc định |
| **Docker Command** | (để trống) | Mặc định |
| **Pre-Deploy Command** | (để trống) | Tùy chọn |
| **Auto-Deploy** | `On Commit` | Khuyến nghị |
| **Registry Credential** | `No credential` | Mặc định |
| **Build Filters** | (để trống) | Tùy chọn |

## 🎯 Các Bước Thực Hiện

1. **Mở phần Advanced** (click "> Advanced" để expand)

2. **Cấu hình Health Check Path:**
   - Tìm "Health Check Path"
   - Nhập: `/api/health`
   - (Mặc định có thể là `/healthz`, cần đổi thành `/api/health`)

3. **Cấu hình Dockerfile Path:**
   - Tìm "Dockerfile Path"
   - Nhập: `QuanLyResort/Dockerfile`

4. **Kiểm tra Auto-Deploy:**
   - Đảm bảo "Auto-Deploy" là `On Commit`

5. **Các mục khác:** Để mặc định hoặc để trống

## ⚠️ Lưu Ý Quan Trọng

1. **Health Check Path phải đúng:** `/api/health`
   - Nếu sai, Render sẽ không thể kiểm tra health của service
   - Service có thể bị đánh dấu là "unhealthy"

2. **Dockerfile Path phải chính xác:**
   - Nếu sai, Render sẽ không tìm thấy Dockerfile
   - Build sẽ fail

3. **Auto-Deploy:**
   - Nếu tắt, bạn phải deploy thủ công mỗi lần có thay đổi
   - Khuyến nghị: Giữ `On Commit` để tự động deploy

## 🔍 Kiểm Tra Sau Khi Deploy

Sau khi deploy, kiểm tra:

1. **Health Check:**
   ```bash
   curl https://your-service.onrender.com/api/health
   ```
   - Kết quả mong đợi: `{"status":"healthy",...}`

2. **Logs:**
   - Vào Render Dashboard → Logs
   - Tìm: `Application started`
   - Tìm: `Now listening on: http://0.0.0.0:10000`

3. **Service Status:**
   - Trên Render Dashboard, service phải hiển thị "Live" (màu xanh)

## 📄 File Tham Khảo

- `Dockerfile` - Xem cấu trúc Dockerfile
- `railway.json` - Xem cấu hình tương tự cho Railway (để tham khảo)

