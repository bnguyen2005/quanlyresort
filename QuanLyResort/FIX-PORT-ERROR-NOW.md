# 🚨 FIX LỖI PORT NGAY LẬP TỨC

## ❌ Lỗi Hiện Tại
```
PORT variable must be integer between 0 and 65535
```

## ✅ Giải Pháp (Làm NGAY)

### Bước 1: Vào Railway Variables Tab

1. Mở Railway Dashboard
2. Chọn service `quanlyresort`
3. Click tab **"Variables"**

### Bước 2: Set PORT Environment Variable

**Thêm hoặc sửa biến:**

- **Key:** `PORT`
- **Value:** `10000` 
  - ⚠️ **QUAN TRỌNG:** Chỉ nhập số `10000`
  - ❌ KHÔNG nhập `"10000"` (có dấu ngoặc kép)
  - ❌ KHÔNG nhập `'10000'` (có dấu nháy đơn)
  - ✅ CHỈ nhập: `10000`

### Bước 3: Xóa PORT Cũ (Nếu Có)

Nếu PORT đã tồn tại với giá trị sai:
1. Click vào biến PORT
2. Click **"Delete"** hoặc **"Remove"**
3. Tạo lại với giá trị `10000` (số nguyên)

### Bước 4: Redeploy

1. Vào tab **"Deployments"**
2. Click **"Redeploy"** trên deployment mới nhất
3. Chọn **"Deploy"**

## 🔍 Kiểm Tra

Sau khi deploy, vào tab **"Logs"** và tìm:

✅ **Thành công:**
```
=== PORT Debug Info ===
PORT env var: '10000'
Using PORT: 10000
ASPNETCORE_URLS: http://0.0.0.0:10000
```

❌ **Vẫn lỗi:**
- Kiểm tra lại giá trị PORT trong Variables tab
- Đảm bảo PORT là số `10000`, không phải string
- Xóa và tạo lại biến PORT

## 📝 Lưu Ý

- Railway có thể tự động inject PORT, nhưng format có thể không đúng
- **PHẢI set PORT=10000 thủ công** để đảm bảo format đúng
- Entrypoint script đã được cải thiện để xử lý các edge cases, nhưng Railway validate PORT trước khi chạy container

## 🎯 Kết Quả

Sau khi fix:
- ✅ PORT được set đúng format (số nguyên)
- ✅ Railway không còn báo lỗi validation
- ✅ Container start thành công
- ✅ Application chạy trên port 10000

