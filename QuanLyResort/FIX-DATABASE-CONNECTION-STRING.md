# 🔧 Fix Lỗi Database Connection String

## ❌ Lỗi Hiện Tại

```
Format of the initialization string does not conform to specification starting at index 0.
Database can connect: False
Database provider: Microsoft.EntityFrameworkCore.SqlServer
```

**Nguyên nhân:**
- Connection string không đúng format
- Railway đang cố dùng SQL Server nhưng connection string không hợp lệ
- Railway chạy trên Linux, không hỗ trợ LocalDB

## ✅ Giải Pháp: Đổi Sang SQLite

### Bước 1: Vào Railway Variables

1. **Vào Railway Dashboard**
2. **Click vào service `quanlyresort`**
3. **Click tab "Variables"**

### Bước 2: Tìm Và Sửa Connection String

**Tìm biến:**
```
ConnectionStrings__DefaultConnection
```

### Bước 3: Cập Nhật Giá Trị

**XÓA giá trị cũ** (nếu có):
```
Server=(localdb)\mssqllocaldb;Database=ResortManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

**THÊM giá trị mới:**
```
Data Source=resort.db
```

**Hoặc nếu muốn dùng persistent volume:**
```
Data Source=/data/resort.db
```

### Bước 4: Save Và Redeploy

1. **Click "Save"** hoặc **"Update"**
2. **Vào tab "Deployments"**
3. **Click "Redeploy"**
4. **Chọn "Deploy"**

## 📋 Connection String Đúng

### Option 1: SQLite (Khuyến nghị cho Railway)

```
Data Source=resort.db
```

**Hoặc với persistent volume:**
```
Data Source=/data/resort.db
```

### Option 2: SQL Server Thật (Nếu có)

Nếu bạn có SQL Server database thật (Azure SQL, AWS RDS, etc.):

```
Server=your-server.database.windows.net,1433;Database=ResortManagementDb;User Id=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=False
```

## 🔍 Kiểm Tra Sau Khi Fix

### 1. Xem Logs

Vào tab **"Logs"** và tìm:

✅ **Thành công:**
```
🔧 Checking database connection...
   Database can connect: True
   Database provider: Microsoft.EntityFrameworkCore.Sqlite
📦 Using SQLite - creating database with EnsureCreated...
✅ Database created using EnsureCreated
✅ Data seeded successfully
```

❌ **Vẫn lỗi:**
- Kiểm tra lại connection string
- Đảm bảo không có ký tự đặc biệt
- Không có dấu ngoặc kép thừa

### 2. Test Endpoint

```bash
curl https://quanlyresort-production.up.railway.app/api/health
```

## 📝 Environment Variables Đầy Đủ

Sau khi fix, đảm bảo có các biến sau:

```env
# Database (SQLite)
ConnectionStrings__DefaultConnection=Data Source=resort.db

# Environment
ASPNETCORE_ENVIRONMENT=Production
PORT=10000

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
```

## 🐛 Troubleshooting

### Lỗi: "Format of the initialization string does not conform"

**Nguyên nhân:**
- Connection string có ký tự đặc biệt
- Có dấu ngoặc kép thừa
- Format không đúng

**Giải pháp:**
1. Xóa biến cũ
2. Tạo lại với giá trị: `Data Source=resort.db` (không có dấu ngoặc kép)
3. Save và redeploy

### Lỗi: "Database can connect: False"

**Nguyên nhân:**
- Connection string vẫn sai
- Database chưa được tạo

**Giải pháp:**
1. Kiểm tra lại connection string
2. Đảm bảo dùng SQLite format: `Data Source=resort.db`
3. Redeploy để app tự tạo database

### Lỗi: "LocalDB is not supported"

**Nguyên nhân:**
- Đang dùng LocalDB connection string
- Railway chạy trên Linux, không hỗ trợ LocalDB

**Giải pháp:**
- Đổi sang SQLite: `Data Source=resort.db`

## 💡 Lưu Ý

- **SQLite** là lựa chọn tốt cho Railway free tier
- Database file sẽ được tạo tự động khi app start
- Nếu dùng persistent volume, mount path: `/data`
- Code đã tự động detect SQLite và tạo database

## ✅ Sau Khi Fix

1. ✅ Database connection thành công
2. ✅ Database được tạo tự động
3. ✅ Data được seed (nếu chưa có)
4. ✅ Tất cả endpoints hoạt động bình thường

