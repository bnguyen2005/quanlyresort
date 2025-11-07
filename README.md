# Resort Management API - Complete System

## Tổng quan
**ResortManagementAPI** là hệ thống quản lý resort hoàn chỉnh bao gồm:
- Backend: ASP.NET Core Web API (.NET 8) + EF Core + SQL Server LocalDB
- Frontend Customer: Deluxe theme (responsive)
- Frontend Admin: Sneat Admin Dashboard
- Authentication: JWT (phân quyền Admin vs Customer)
- PWA Support với Service Worker
- Mobile & Desktop responsive
- Audit logs, Reports, Notifications

---

## Cấu trúc Project

```
QuanLyResort/
├── Models/                 # Entities (Room, Booking, Customer, Invoice, etc.)
├── Data/                   # DbContext, DataSeeder
├── Repositories/           # Repository pattern + Unit of Work
├── Services/               # Business logic (BookingService, RoomService, etc.)
├── Controllers/            # API Controllers
├── wwwroot/
│   ├── customer/          # Deluxe theme - Customer frontend
│   ├── admin/             # Sneat theme - Admin dashboard
│   ├── js/                # API helpers, auth, booking integration
│   ├── manifest.json      # PWA manifest
│   └── service-worker.js  # PWA service worker
└── postman_resort_frontend.json  # Postman collection
```

---

## Yêu cầu hệ thống

### Phần mềm cần thiết

| Phần mềm | Version | Link Download | Ghi chú |
|----------|---------|---------------|---------|
| **.NET 8 SDK** | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) | ⚠️ **BẮT BUỘC** |
| **SQL Server LocalDB** | 2019+ | Đi kèm Visual Studio hoặc [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) | ⚠️ **BẮT BUỘC** |
| **Visual Studio 2022** | Community+ | [Download](https://visualstudio.microsoft.com/) | Khuyên dùng (có LocalDB) |
| **VS Code** | Latest | [Download](https://code.visualstudio.com/) | Alternative (cần cài LocalDB riêng) |
| **Postman** | Latest | [Download](https://www.postman.com/) | Optional (test API) |

### Kiểm tra hệ thống

Trước khi cài đặt, kiểm tra các tools đã có chưa:

```powershell
# Kiểm tra .NET SDK
dotnet --version
# Expected: 8.0.x hoặc cao hơn

# Kiểm tra SQL Server LocalDB
sqllocaldb info
# Expected: Hiển thị danh sách instances (ví dụ: MSSQLLocalDB)

# Kiểm tra EF Core Tools
dotnet ef --version
# Expected: 8.0.x hoặc cao hơn
```

---

## 🚀 Hướng dẫn cài đặt chi tiết (Máy mới)

### ✅ Bước 1: Cài đặt Prerequisites

#### 1.1. Cài đặt .NET 8 SDK

1. Truy cập: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download **".NET 8.0 SDK"** (Windows x64)
3. Chạy file installer → Next → Install
4. Xác nhận cài đặt thành công:
   ```powershell
   dotnet --version
   # Output: 8.0.x
   ```

#### 1.2. Cài đặt Visual Studio 2022 (Khuyên dùng)

**Cách 1: Visual Studio 2022 (Có SQL Server LocalDB tích hợp)**

1. Download: https://visualstudio.microsoft.com/downloads/
2. Chạy installer
3. Chọn workload: **"ASP.NET and web development"**
4. Trong tab "Individual components", đảm bảo chọn:
   - ✅ SQL Server Express LocalDB
   - ✅ .NET 8.0 Runtime
5. Click Install (khoảng 5-10GB)

**Cách 2: SQL Server LocalDB riêng (Nếu dùng VS Code)**

1. Download SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
2. Chọn "Download now" → Custom installation
3. Chọn "LocalDB" trong Features
4. Cài đặt xong, kiểm tra:
   ```powershell
   sqllocaldb create MSSQLLocalDB
   sqllocaldb start MSSQLLocalDB
   sqllocaldb info
   ```

#### 1.3. Cài đặt EF Core Tools

```powershell
dotnet tool install --global dotnet-ef
```

Xác nhận:
```powershell
dotnet ef --version
# Output: Entity Framework Core .NET Command-line Tools 8.0.x
```

---

### ✅ Bước 2: Clone/Download Project

**Cách 1: Clone từ Git (nếu có)**
```powershell
git clone <repository-url>
cd QuanLyResort/QuanLyResort
```

**Cách 2: Extract từ ZIP**
1. Extract file ZIP vào thư mục (ví dụ: `D:\CNPM_NC_TH_2025\QuanLyResort`)
2. Mở PowerShell/CMD tại thư mục project:
   ```powershell
   cd D:\CNPM_NC_TH_2025\QuanLyResort\QuanLyResort
   ```

---

### ✅ Bước 3: Restore Dependencies

```powershell
dotnet restore
```

**Output mong đợi:**
```
Restore completed in X.XX sec for ...
```

**❌ Nếu lỗi:** Kiểm tra kết nối internet và .NET SDK version

---

### ✅ Bước 4: Kiểm tra Connection String

Mở file `appsettings.json` và kiểm tra:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResortManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**✏️ Tùy chỉnh (nếu cần):**
- Nếu dùng SQL Server thật, thay đổi:
  ```
  Server=YOUR_SERVER_NAME;Database=ResortManagementDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true
  ```

---

### ✅ Bước 5: Tạo Database

#### 5.1. Kiểm tra Migration đã có chưa

```powershell
ls Migrations/
```

**Nếu thấy file `*.cs`** trong thư mục `Migrations/` → **Bỏ qua 5.2**, chuyển sang 5.3

#### 5.2. Tạo Migration (nếu chưa có)

```powershell
dotnet ef migrations add InitialCreate
```

**Output mong đợi:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'dotnet ef migrations remove'
```

#### 5.3. Apply Migration (Tạo Database)

```powershell
dotnet ef database update
```

**Output mong đợi:**
```
Build started...
Build succeeded.
Applying migration '20241018145811_InitialCreate'.
Done.
```

**✅ Xác nhận database đã tạo:**

**Cách 1: Visual Studio**
- View → SQL Server Object Explorer
- Expand: (localdb)\MSSQLLocalDB → Databases
- Tìm `ResortManagementDb`

**Cách 2: Command Line**
```powershell
sqllocaldb info MSSQLLocalDB
```

**❌ Troubleshooting:**

| Lỗi | Giải pháp |
|-----|-----------|
| `Cannot open database` | Chạy: `sqllocaldb start MSSQLLocalDB` |
| `dotnet ef không được nhận dạng` | Chạy: `dotnet tool install --global dotnet-ef` |
| `Build failed` | Chạy: `dotnet build` để xem lỗi chi tiết |

---

### ✅ Bước 6: Chạy Project

```powershell
dotnet run
```

**Output mong đợi:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

**⚠️ Lưu ý PORT:**
- Port có thể khác (7001, 7002, etc.)
- Kiểm tra trong output hoặc file `Properties/launchSettings.json`

**✅ Xác nhận API hoạt động:**

Mở trình duyệt, truy cập:
```
https://localhost:7000/swagger
```

Bạn sẽ thấy Swagger UI với danh sách API endpoints.

---

### ✅ Bước 7: Seed Data (Dữ liệu mẫu)

**QUAN TRỌNG:** Database mới tạo sẽ **rỗng**, cần seed dữ liệu mẫu.

#### 7.1. Mở Swagger

```
https://localhost:7000/swagger
```

#### 7.2. Login Admin để lấy Token

1. Tìm endpoint: `POST /api/auth/login`
2. Click "Try it out"
3. Nhập:
   ```json
   {
     "email": "admin@resort.test",
     "password": "P@ssw0rd123"
   }
   ```
   **⚠️ CHÚ Ý:** Tài khoản admin này được tạo tự động trong `DataSeeder.cs`, ngay cả khi database rỗng, bạn có thể login ngay.
   
4. Click "Execute"
5. Copy `token` từ Response

#### 7.3. Authorize với Token

1. Click nút **"Authorize"** (icon ổ khóa) ở góc phải trên
2. Nhập: `Bearer YOUR_TOKEN_HERE`
   - Ví dụ: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click "Authorize" → Close

#### 7.4. Gọi API Seed

1. Tìm endpoint: `POST /api/admin/seed`
2. Click "Try it out" → "Execute"
3. Đợi 5-10 giây
4. Response `200 OK`:
   ```json
   {
     "message": "Database seeded successfully",
     "data": { ... }
   }
   ```

**✅ Kết quả:** Database giờ có:
- 6 Staff accounts (Admin, FrontDesk, Cashier, Manager, Accounting, Inventory)
- 10 Rooms (các loại phòng khác nhau)
- 5 Customers mẫu
- 3 Bookings mẫu
- 2 Invoices mẫu

---

### ✅ Bước 8: Truy cập Frontend

Mở trình duyệt, truy cập:

#### 📱 Customer Frontend (Khách hàng)
```
https://localhost:7000/customer/index.html
```

**Test login:**
- Email: `customer1@guest.test`
- Password: `Guest@123`

#### 🖥️ Admin Dashboard (Nhân viên)
```
https://localhost:7000/admin/index.html
```

**Test login:**
- Email: `admin@resort.test`
- Password: `P@ssw0rd123`

---

## 🎯 Tóm tắt Commands (Cheat Sheet)

```powershell
# 1. Check prerequisites
dotnet --version
sqllocaldb info

# 2. Navigate to project
cd D:\CNPM_NC_TH_2025\QuanLyResort\QuanLyResort

# 3. Restore packages
dotnet restore

# 4. Create database
dotnet ef database update

# 5. Run project
dotnet run

# 6. Open browser
# https://localhost:7000/swagger
# https://localhost:7000/customer/index.html
# https://localhost:7000/admin/index.html
```

---

## 🔍 URLs Quan trọng

| Mô tả | URL |
|-------|-----|
| **API Swagger** | `https://localhost:7000/swagger` |
| **Customer Frontend** | `https://localhost:7000/customer/index.html` |
| **Admin Dashboard** | `https://localhost:7000/admin/index.html` |
| **API Base** | `https://localhost:7000/api` |

---

## Tài khoản mặc định (sau khi seed)

### Staff/Admin Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@resort.test | P@ssw0rd123 | Admin |
| frontdesk@resort.test | P@ssw0rd123 | FrontDesk |
| cashier@resort.test | P@ssw0rd123 | Cashier |
| manager@resort.test | P@ssw0rd123 | Manager |
| accounting@resort.test | P@ssw0rd123 | Accounting |
| inventory@resort.test | P@ssw0rd123 | Inventory |

### Customer Accounts

| Email | Password | Role |
|-------|----------|------|
| customer1@guest.test | Guest@123 | Customer |

---

## 📚 API Endpoints & Luồng nghiệp vụ

### Nhóm API chính

| Nhóm | Endpoints | Mô tả |
|------|-----------|-------|
| **Authentication** | `/api/auth/*` | Login, Register (Admin/Customer) |
| **Rooms** | `/api/rooms/*` | Quản lý phòng, kiểm tra availability |
| **Bookings** | `/api/bookings/*` | Tạo booking, check-in, check-out |
| **Invoices** | `/api/invoices/*` | Quản lý hóa đơn, thanh toán |
| **Reports** | `/api/reports/*` | Báo cáo doanh thu, công suất |
| **Audit** | `/api/audit/*` | Audit logs, reconciliation |
| **Admin** | `/api/admin/*` | Seed data, statistics |
| **Alerts** | `/api/alerts/*` | Thông báo hệ thống |

### Luồng nghiệp vụ cơ bản

```
1. Customer → Login → Create Booking → Transfer to Front Desk
2. FrontDesk → Assign Room → Check-in → Add Charges → Checkout
3. Cashier → View Invoice → Process Payment
4. Manager → View Reports & Audit Logs
```

**📄 Chi tiết:** Xem file `CLIENT_API_MAP.md` để biết đầy đủ endpoints và parameters.

---

## 🧪 Testing

### Postman Collection
1. Import file `postman_resort_frontend.json`
2. Set biến `base_url` = `https://localhost:7000`
3. Test các flows: Authentication → Bookings → Invoices → Reports

### Manual Testing
- **Swagger UI:** `https://localhost:7000/swagger`
- **Customer Frontend:** Login với `customer1@guest.test`
- **Admin Dashboard:** Login với `admin@resort.test`

---

## 📱 Tính năng bổ sung

### PWA (Progressive Web App)
- ✅ Có thể install như native app trên mobile
- ✅ Offline support với Service Worker
- ⚙️ Config: `wwwroot/service-worker.js` → `ENABLE_PWA = true/false`

### Mobile Responsive
- ✅ Auto detect và redirect mobile
- ✅ Responsive design cho tất cả màn hình
- ⚙️ Tắt auto-redirect: `localStorage.setItem('force_desktop_view', 'true')`

### Business Rules
- ✅ Double-booking prevention
- ✅ Room status validation
- ✅ Audit logging tự động
- ✅ Notifications real-time

---

## 🔧 Troubleshooting thường gặp

| Lỗi | Giải pháp |
|-----|-----------|
| **Database connection error** | `sqllocaldb start MSSQLLocalDB` |
| **Migration failed** | `dotnet build` → `dotnet ef migrations add InitialCreate --force` |
| **Port already in use** | Đổi port trong `Properties/launchSettings.json` |
| **JWT Invalid Token** | Token hết hạn (24h) → Login lại |
| **CORS Error** | Frontend và API phải cùng origin hoặc config CORS trong `Program.cs` |
| **Swagger 404** | Kiểm tra port trong terminal output |
| **Frontend không load** | Kiểm tra `wwwroot/` folder có đầy đủ files |

**💡 Tip:** Xem terminal output khi chạy `dotnet run` để biết port chính xác!

---

## 🚀 Deployment

### Production (SQL Server)
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ResortManagementDb;User Id=sa;Password=YOUR_PASS;TrustServerCertificate=true"
  }
}
```

```powershell
dotnet ef database update --environment Production
# Deploy: IIS / Azure / Docker
```

---

## 📋 TODO & Future Enhancements

- [ ] Payment gateway (Momo, ZaloPay, VNPay)
- [ ] Email/SMS notifications
- [ ] Multi-language support
- [ ] Advanced reporting (charts, Excel export)
- [ ] Real-time updates (SignalR)

---

## 👥 Team & Contact

- **Developers:** Nhựt, Nguyên, Lam, Ninh
- **Email:** mhnhwt205@gmail.com
- **Docs:** `CLIENT_API_MAP.md`, `README_CLIENT.md`

---

## 📝 Ghi chú quan trọng

### ✅ Đã hoàn thành
- ✅ Backend API hoàn chỉnh (.NET 8 + EF Core)
- ✅ JWT Authentication với phân quyền
- ✅ Frontend Customer (Deluxe theme)
- ✅ Frontend Admin (Sneat dashboard)
- ✅ PWA support
- ✅ Mobile responsive
- ✅ Audit logs & Reports
- ✅ **Navbar alignment fix** (User email ngang hàng hoàn hảo)

### 🔗 Files quan trọng
- `README.md` (file này) - Hướng dẫn cài đặt chi tiết
- `CLIENT_API_MAP.md` - Mapping Frontend ↔ API endpoints
- `postman_resort_frontend.json` - Postman collection
- `wwwroot/customer/` - Customer frontend
- `wwwroot/admin/` - Admin dashboard

### 🎓 Best Practices
- ✅ Clean code, DRY principle
- ✅ Repository pattern + Unit of Work
- ✅ Dependency Injection
- ✅ Async/await cho tất cả DB operations
- ✅ Validation & Error handling
- ✅ Audit logging cho security

---

**🎉 Chúc bạn thành công với Resort Management System!**

_Last updated: October 20, 2025_

