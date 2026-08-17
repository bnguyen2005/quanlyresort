<div align="center">

# 🏨 QuanLyResort — Resort Management System

### *Enterprise-grade Hotel & Resort Operations Platform*

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-68217A?style=for-the-badge&logo=nuget)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens)](https://jwt.io/)
[![xUnit](https://img.shields.io/badge/xUnit-Moq-brightgreen?style=for-the-badge)](https://xunit.net/)

**Hệ thống quản lý vận hành resort toàn diện — từ đặt phòng đến kết toán — được xây dựng theo các tiêu chuẩn kiến trúc phần mềm Enterprise.**

[📖 API Docs (Swagger)](#-urls-nhanh) · [🧑‍💻 Hướng dẫn cài đặt](#-hướng-dẫn-cài-đặt) · [🏗 Kiến trúc](#-kiến-trúc-hệ-thống) · [🔒 Bảo mật](#-bảo-mật)

</div>

---

## 🌟 Tổng Quan

**QuanLyResort** là một nền tảng quản lý khách sạn và khu nghỉ dưỡng full-stack được phát triển với tư duy **Production-Ready** từ ngày đầu. Hệ thống bao gồm:

- 🔌 **Backend RESTful API** — .NET 8 Web API với kiến trúc phân tầng nghiêm ngặt.
- 🖥️ **Admin Dashboard** — Giao diện quản trị toàn diện (Sneat Admin Theme).
- 📱 **Customer Portal** — Giao diện đặt phòng trực tuyến (Deluxe Theme, PWA-enabled).
- ✅ **Unit Test Suite** — Bộ kiểm thử tự động với xUnit & Moq.
- 🔒 **Secure by Design** — CORS Policy, JWT RBAC, không có bất kỳ Secret nào trong mã nguồn.

---

## 🏗 Kiến Trúc Hệ Thống

Dự án tuân thủ nghiêm ngặt kiến trúc **N-Tier** kết hợp **Repository + Unit of Work Pattern**, đảm bảo tính tách biệt rõ ràng giữa các tầng.

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT LAYER                              │
│   Admin Dashboard (Sneat)  |  Customer Portal (PWA)         │
└───────────────────┬─────────────────────────────────────────┘
                    │ HTTP / WebSocket (SignalR)
┌───────────────────▼─────────────────────────────────────────┐
│                   API LAYER (Controllers)                     │
│   Auth | Booking | Payment | Room | Report | Restaurant...   │
└───────────────────┬─────────────────────────────────────────┘
                    │ IUnitOfWork (Pure Abstraction)
┌───────────────────▼─────────────────────────────────────────┐
│                 SERVICE LAYER (Business Logic)                │
│   BookingService | InvoiceService | AIChatService | ...      │
└───────────────────┬─────────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────────┐
│         REPOSITORY LAYER (IUnitOfWork / IRepository<T>)      │
│   Generic Repo + IQueryable Support (SQL-side Evaluation)    │
└───────────────────┬─────────────────────────────────────────┘
                    │ Entity Framework Core
┌───────────────────▼─────────────────────────────────────────┐
│                DATABASE LAYER (SQL Server / SQLite)           │
│   20+ Tables | Migrations | DataSeeder                       │
└─────────────────────────────────────────────────────────────┘
```

### Cấu trúc thư mục

```text
QuanLyResort/
├── Controllers/            # 15+ API Controllers (SOLID, single-purpose)
│   ├── BookingsController.cs
│   ├── PaymentWebhookController.cs
│   ├── BookingPaymentController.cs
│   └── RestaurantPaymentController.cs
├── Services/               # Business logic, hoàn toàn tách biệt với DbContext
├── Repositories/
│   ├── IRepository.cs      # Generic Interface + IQueryable Query()
│   ├── Repository.cs
│   ├── IUnitOfWork.cs      # 20+ typed Repositories
│   └── UnitOfWork.cs
├── Models/                 # EF Core Entities
├── Data/                   # DbContext + DataSeeder
├── wwwroot/
│   ├── customer/           # Customer Portal (PWA)
│   └── admin/              # Admin Dashboard
└── QuanLyResort.Tests/     # 🧪 Unit Test Project
    └── Services/
        └── BookingServiceTests.cs
```

---

## ✨ Tính Năng Nổi Bật

### 📦 Quản lý Nghiệp vụ Cốt lõi

| Module | Tính năng |
|--------|-----------|
| **Đặt phòng (Booking)** | Kiểm tra availability real-time, tránh double-booking, phê duyệt & check-in/out |
| **Hóa đơn (Invoice)** | Tạo hóa đơn tổng hợp (phòng + dịch vụ + nhà hàng), hỗ trợ thanh toán từng phần |
| **Thanh toán (Payment)** | Webhook xử lý giao dịch thành công/thất bại, cập nhật trạng thái qua SignalR real-time |
| **Nhà hàng (Restaurant)** | Quản lý order, thực đơn, tự động gắn vào hóa đơn lưu trú |
| **Kho (Inventory)** | Quản lý hàng tồn kho, nhập hàng, cảnh báo thiếu hụt |
| **Báo cáo (Reports)** | Doanh thu theo ngày/tháng, Occupancy Rate, RevPAR, Audit Trail |

### 🎯 Điểm Kỹ Thuật Nổi Bật

**1. Xử lý Race Condition (Concurrency Safety)**

Cơ chế sinh mã Booking và Invoice an toàn 100% khi có hàng nghìn request đồng thời:
```csharp
// ❌ Cách cũ — Rất dễ trùng mã khi có 2+ luồng đồng thời
var lastId = await _context.Bookings.MaxAsync(b => b.BookingId);
booking.BookingCode = $"BKG{lastId + 1:D7}"; // 💣 Race Condition!

// ✅ Cách mới — An toàn tuyệt đối, dựa vào ACID của Database
booking.BookingCode = $"TEMP-{Guid.NewGuid()}"; // Gán tạm để DB nhận record
await _unitOfWork.SaveChangesAsync();           // DB cấp ID thật (Auto-Increment)
booking.BookingCode = $"BKG{booking.BookingId:D7}"; // ID từ DB là duy nhất 100%
await _unitOfWork.SaveChangesAsync();           // Cập nhật mã chính thức
```

**2. IQueryable Repository — Chống RAM Overflow**

```csharp
// ❌ Cách cũ — Kéo toàn bộ 1 triệu records lên RAM rồi mới Sum
var total = _context.Invoices.ToList().Sum(i => i.Amount);

// ✅ Cách mới — SQL Server thực hiện Sum trực tiếp, chỉ trả về 1 con số
var total = await _unitOfWork.Invoices.Query().SumAsync(i => i.Amount);
```

**3. Clean Separation of Concerns (SRP)**

```csharp
// ❌ Cách cũ — 1 "God-Class" SimplePaymentController với 1600+ dòng
// class SimplePaymentController { /* Tất cả trong 1 */ }

// ✅ Cách mới — 3 controller chuyên biệt, mỗi cái < 300 dòng
// PaymentWebhookController     — Xử lý webhook VNPay/MoMo
// BookingPaymentController     — Thanh toán đặt phòng
// RestaurantPaymentController  — Thanh toán dịch vụ nhà hàng
```

---

## 🔒 Bảo Mật

| Hạng mục | Trạng thái | Chi tiết |
|----------|------------|---------|
| **JWT Authentication** | ✅ Áp dụng | Role-Based Access Control (6 roles) |
| **CORS Policy** | ✅ Thắt chặt | Chỉ chấp nhận origin được whitelist |
| **Secret Management** | ✅ An toàn | Toàn bộ Secret đọc từ Environment Variables |
| **Input Validation** | ✅ Áp dụng | Model Validation + Custom Guards |
| **Audit Logging** | ✅ Áp dụng | Ghi log toàn bộ thao tác nhạy cảm |
| **SQL Injection** | ✅ Miễn dịch | EF Core Parameterized Queries |

### Phân quyền (RBAC)

```
Admin       → Toàn quyền hệ thống
Manager     → Xem báo cáo, duyệt yêu cầu
FrontDesk   → Quản lý phòng, Check-in/out
Cashier     → Xử lý thanh toán, xuất hóa đơn
Inventory   → Quản lý kho hàng
Customer    → Đặt phòng, xem lịch sử cá nhân
```

---

## 🧪 Testing

```powershell
# Chạy toàn bộ bộ test
cd QuanLyResort.Tests
dotnet test

# Ví dụ test đang có: BookingServiceTests
# ✓ CreateBookingAsync_ShouldFormatBookingCodeCorrectly
# ✓ SaveChangesAsync_ShouldBeCalledExpectedNumberOfTimes
```

**Công nghệ:** xUnit + Moq (Mocking IUnitOfWork, không cần kết nối DB thật).

---

## 🚀 Hướng Dẫn Cài Đặt

### Yêu cầu
- **.NET 8 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server LocalDB** — Đi kèm Visual Studio 2022

### Chạy dự án

```powershell
# 1. Clone dự án
git clone https://github.com/Lamm123435469898/quanlyresort.git
cd quanlyresort

# 2. Cài đặt dependencies
dotnet restore

# 3. Tạo Database (EF Core Migrations)
dotnet tool install --global dotnet-ef
dotnet ef database update --project QuanLyResort

# 4. Khởi chạy
dotnet run --project QuanLyResort
```

> 💡 **Lưu ý:** Tạo file `.env` hoặc đặt các biến môi trường theo mẫu trong `appsettings.json` trước khi chạy.

### Seed dữ liệu mẫu

```
POST https://localhost:7000/api/admin/seed
Authorization: Bearer <admin_token>
```

---

## 🔗 URLs Nhanh

| Trang | URL |
|-------|-----|
| 📄 **API Documentation (Swagger)** | `https://localhost:7000/swagger` |
| 🛎️ **Customer Booking Portal** | `https://localhost:7000/customer/index.html` |
| 🖥️ **Admin Dashboard** | `https://localhost:7000/admin/index.html` |

### Tài khoản thử nghiệm (Sau khi Seed)

| Vai trò | Email | Mật khẩu |
|---------|-------|----------|
| **Admin** | `admin@resort.test` | `P@ssw0rd123` |
| **Lễ tân** | `frontdesk@resort.test` | `P@ssw0rd123` |
| **Thu ngân** | `cashier@resort.test` | `P@ssw0rd123` |
| **Khách hàng** | `customer1@guest.test` | `Guest@123` |

---

## 🗺️ Luồng Nghiệp Vụ

```
[Customer]  →  Tìm phòng  →  Đặt phòng  →  Thanh toán cọc
                                                    ↓
[FrontDesk] →  Duyệt đặt phòng  →  Phân phòng  →  Check-in
                                                    ↓
            →  Gắn dịch vụ (Nhà hàng, Minibar, Spa)
                                                    ↓
[Cashier]   →  Tổng hợp hóa đơn  →  Thanh toán  →  Check-out
                                                    ↓
[Manager]   →  Xem báo cáo  →  Xuất Audit Log  →  Quyết toán
```

---

## 🗺️ Roadmap

- [ ] **AutoMapper DTOs** — Tách Entity khỏi Response model, tăng bảo mật.
- [ ] **Redis Cache** — Caching Dashboard reports (TTL: 5 phút) tránh query lặp lại.
- [ ] **Hangfire** — Background Jobs cho Email/SMS, không block luồng HTTP.
- [ ] **Soft-Delete** — Bổ sung `IsDeleted` flag thay thế Hard-Delete cho dữ liệu kế toán.
- [ ] **Docker Compose** — Đóng gói toàn bộ stack cho môi trường Production.

---

## 👥 Nhóm Phát triển

| Thành viên | Vai trò |
|------------|---------|
| Nhựt | Backend Architecture, Security |
| Nguyên | Business Logic, API Design |
| Lam | Frontend Admin & Customer |
| Ninh | Database Design, Reporting |

---

<div align="center">

**🎉 Built with ❤️ — Enterprise standards, from day one.**

*Last updated: August 2026 — Post refactoring & security hardening*

</div>
