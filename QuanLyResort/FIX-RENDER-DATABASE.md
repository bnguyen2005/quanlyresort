# 🔧 Fix Lỗi Database Trên Render

## ❌ Vấn Đề

```
System.PlatformNotSupportedException: LocalDB is not supported on this platform.
```

**Nguyên nhân:**
- Render chạy trên **Linux**
- SQL Server **LocalDB** chỉ hỗ trợ **Windows**
- Connection string đang dùng LocalDB

## ✅ Giải Pháp

### Option 1: Dùng SQLite (Khuyến Nghị - Đơn Giản)

SQLite hoạt động tốt trên Linux và không cần setup database server.

**Cập nhật Environment Variable trong Render:**

```
ConnectionStrings__DefaultConnection = Data Source=resort.db
```

### Option 2: Dùng SQL Server Thật (Nếu Cần)

Nếu cần SQL Server, phải setup SQL Server database thật (không phải LocalDB):

1. **Tạo SQL Server Database:**
   - Azure SQL Database
   - AWS RDS SQL Server
   - Hoặc SQL Server trên VPS

2. **Connection String:**
   ```
   Server=your-server.database.windows.net,1433;Database=ResortManagementDb;User Id=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=False
   ```

## 🔧 Đã Sửa Code

`Program.cs` đã được cập nhật để:
- ✅ Tự động detect SQLite connection string
- ✅ Fallback sang SQLite nếu detect LocalDB trên Linux
- ✅ Hoạt động trên cả Windows và Linux

## 📋 Cập Nhật Render Environment Variables

### Bước 1: Vào Render Dashboard

1. Vào service `quanlyresort-api`
2. Click **"Environment"** tab
3. Tìm biến `ConnectionStrings__DefaultConnection`

### Bước 2: Cập Nhật Connection String

**Xóa giá trị cũ:**
```
Server=(localdb)\mssqllocaldb;Database=ResortManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

**Thêm giá trị mới:**
```
Data Source=resort.db
```

### Bước 3: Redeploy

1. Click **"Manual Deploy"** → **"Deploy latest commit"**
2. Hoặc push code mới lên GitHub (tự động deploy)

## ✅ Sau Khi Fix

App sẽ:
- ✅ Tạo file `resort.db` trong container
- ✅ Tự động seed data (nếu chưa có)
- ✅ Hoạt động bình thường

## 💡 Lưu Ý

- **SQLite file:** Sẽ được tạo trong container
- **Persistent storage:** Nếu cần lưu data lâu dài, nên dùng Render Persistent Disk
- **Backup:** Nên backup database file định kỳ

## 🔄 Nếu Cần Persistent Disk

1. Vào Render Dashboard → Service → **"Disks"**
2. Click **"Link Disk"**
3. Mount path: `/app/data`
4. Update connection string: `Data Source=/app/data/resort.db`

