# 🏨 Resort Management System API

**ResortManagementAPI** là hệ thống quản lý resort toàn diện được thiết kế theo các tiêu chuẩn kiến trúc phần mềm hiện đại, an toàn và dễ dàng mở rộng.

Hệ thống bao gồm Backend RESTful API, Customer Frontend (Giao diện khách hàng), Admin Dashboard (Giao diện quản lý), hỗ trợ PWA (Progressive Web App) và hệ thống phân quyền JWT bảo mật cao.

---

## 🌟 Tính năng nổi bật

### 🏗 Kiến trúc & Code Quality
- **Pure Unit Of Work & Repository Pattern:** Toàn bộ Service và Controller tương tác thông qua `IUnitOfWork`. Không chọc thẳng vào `DbContext` (ngăn chặn Leaky Abstraction).
- **Generic Repository Querying:** Hỗ trợ `IQueryable` cho phép đẩy mọi logic tính toán GroupBy, Sum, Count xuống SQL Server xử lý, tiết kiệm RAM tuyệt đối.
- **S.O.L.I.D Principles:** Controller được chia nhỏ theo từng nghiệp vụ độc lập (vd: `PaymentWebhookController`, `BookingPaymentController`, `RestaurantPaymentController`).
- **Unit Testing:** Thiết lập sẵn kiến trúc Test tự động với **xUnit** và **Moq**.

### 🔒 Bảo mật & An toàn dữ liệu
- **Thread-Safety (Chống Race Condition):** Thuật toán sinh mã `BookingCode` và `InvoiceNumber` an toàn tuyệt đối, dùng Temporary-GUID kết hợp ID Auto-Increment, cam kết không trùng lặp khi có hàng ngàn request cùng lúc.
- **Secret Management:** Loại bỏ hoàn toàn chuỗi kết nối và mật khẩu cứng (Hardcoded Secrets). Hỗ trợ đọc từ biến môi trường (Environment Variables) hoặc `.env`.
- **Strict CORS Policy:** Chỉ cho phép các domain được cấp quyền truy cập vào API, chống tấn công mạo danh (CSRF).
- **JWT Authorization:** Xác thực bảo mật, phân quyền nghiêm ngặt giữa Admin, Cashier, FrontDesk và Customer.

---

## 📂 Cấu trúc Project

```text
QuanLyResort/
├── Models/                 # Entities (Room, Booking, Customer, Invoice, v.v.)
├── Data/                   # DbContext & EF Core Configurations
├── Repositories/           # Repository Pattern + Unit of Work
├── Services/               # Business Logic (Booking, Payment, Room, Auth)
├── Controllers/            # RESTful API Controllers
├── wwwroot/
│   ├── customer/           # Deluxe theme - Giao diện đặt phòng
│   ├── admin/              # Sneat theme - Admin Dashboard
│   └── service-worker.js   # PWA Offline support
└── QuanLyResort.Tests/     # Dự án Unit Test (xUnit, Moq)
```

---

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống
- **.NET 8 SDK** (Bắt buộc)
- **SQL Server LocalDB** (Đi kèm Visual Studio) hoặc SQL Server Express
- **Visual Studio 2022** hoặc **VS Code**

### Các bước chạy dự án
**1. Restore dependencies:**
```powershell
dotnet restore
```

**2. Khởi tạo Database & Migration:**
Hệ thống sử dụng EF Core Code-First.
```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update
```

**3. Chạy ứng dụng:**
```powershell
dotnet run
```
Truy cập `https://localhost:7000/swagger` để xem tài liệu API.

**4. Dữ liệu mẫu (Seed Data):**
Vì Database mới sẽ trống, hãy sử dụng Postman hoặc Swagger để gọi API:
`POST /api/admin/seed`
Hệ thống sẽ tự động tạo các loại phòng, khách hàng, nhân viên và dữ liệu đặt phòng mẫu để bạn trải nghiệm.

---

## 🧪 Testing

Dự án đã được thiết lập môi trường Unit Test.

Để chạy bộ Test tự động cho `BookingService` và các luồng quan trọng khác:
```powershell
cd QuanLyResort.Tests
dotnet test
```

---

## 🔗 Liên kết truy cập nhanh

- **Swagger UI (API Docs):** `https://localhost:7000/swagger`
- **Customer Site:** `https://localhost:7000/customer/index.html`
- **Admin Dashboard:** `https://localhost:7000/admin/index.html`

### Tài khoản thử nghiệm (Sau khi Seed)
| Vai trò | Email | Mật khẩu |
|---------|-------|----------|
| **Admin** | admin@resort.test | P@ssw0rd123 |
| **Lễ tân** | frontdesk@resort.test | P@ssw0rd123 |
| **Khách hàng** | customer1@guest.test | Guest@123 |

---

## 📝 TODO & Future Roadmap

- Tích hợp AutoMapper cho các DTOs.
- Thêm Redis/MemoryCache để tối ưu màn hình báo cáo Dashboard.
- Cài đặt Hangfire để xử lý việc gửi Email chạy ngầm (Background Jobs).
- Áp dụng Soft-Delete cho các dữ liệu quan trọng như Hóa đơn, Khách hàng.

---
*Phát triển bởi đội ngũ: Nhựt, Nguyên, Lam, Ninh. Phiên bản hệ thống đã được bảo trì & nâng cấp chuẩn Enterprise.*
