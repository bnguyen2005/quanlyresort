# 🏨 Resort Management System — QuanLyResort

Hệ thống quản lý resort toàn diện: đặt phòng, thanh toán trực tuyến thật, chat hỗ trợ bằng AI, dashboard quản trị, real-time cập nhật trạng thái.

---

## 📋 Tổng quan

**QuanLyResort** là hệ thống quản lý resort full-stack, gồm:

- **Backend:** ASP.NET Core Web API (.NET 8) + Entity Framework Core + SQL Server LocalDB
- **Frontend Customer:** Deluxe theme (responsive, PWA)
- **Frontend Admin:** Sneat Admin Dashboard
- **Authentication:** JWT (phân quyền Admin / Staff / Customer)
- **Thanh toán trực tuyến thật:** PayOS (chính), VietQR, MB Bank (webhook dự phòng)
- **Chat hỗ trợ AI:** đa nhà cung cấp (OpenAI / Groq / HuggingFace / Cohere) + fallback trả lời bằng dữ liệu thật từ database
- **Real-time:** SignalR — cập nhật trạng thái thanh toán/booking tức thì, không cần polling
- **PWA:** cài đặt như native app, hỗ trợ offline qua Service Worker
- **Responsive:** tự động chuyển giao diện mobile/desktop
- **Audit logs, Reports, Notifications**
- **Deploy:** Render.com (production), hỗ trợ Docker

---

## 🏗️ Cấu trúc Project

```
QuanLyResort/
├── Models/                     # Entities (Room, Booking, Customer, Invoice, RestaurantOrder, ...)
├── Data/                       # DbContext, DataSeeder
├── Repositories/                # Repository pattern + Unit of Work
├── Services/                    # Business logic
│   ├── BookingService.cs
│   ├── RoomService.cs
│   ├── PayOsService.cs          # Tạo payment link qua PayOS
│   ├── PayOsWebhookService.cs   # Xử lý webhook PayOS (có verify signature)
│   ├── SePayService.cs / VietQRService.cs
│   └── AIChatService.cs         # Chat AI đa nhà cung cấp + fallback
├── Controllers/
│   ├── PaymentController.cs         # Session-based payment, các webhook phụ (VietQR, MB Bank)
│   ├── SimplePaymentController.cs   # Webhook chính đang dùng thật trên production
│   └── AIChatController.cs
├── Hubs/
│   └── PaymentHub.cs             # SignalR — broadcast trạng thái thanh toán real-time
├── wwwroot/
│   ├── customer/                # Deluxe theme - Customer frontend
│   ├── admin/                   # Sneat theme - Admin dashboard
│   ├── js/                      # API helpers, auth, booking integration
│   ├── manifest.json            # PWA manifest
│   └── service-worker.js        # PWA service worker
├── config-payos-after-deploy.sh # Script đăng ký webhook PayOS sau khi deploy
├── test-webhook-ngrok.sh        # Script test webhook qua ngrok khi dev local
└── postman_resort_frontend.json # Postman collection
```

---

## 💳 Kiến trúc thanh toán

Hệ thống hỗ trợ **thanh toán thật** qua nhiều kênh:

| Kênh | Vai trò | Endpoint webhook |
|---|---|---|
| **PayOS** | Cổng chính (MB Bank Payment Gateway) | `POST /api/simplepayment/webhook` |
| VietQR | Dự phòng | `POST /api/payment/vietqr-webhook` |
| MB Bank | Dự phòng | `POST /api/payment/mbbank-webhook` |
| Generic Bank | Webhook chung, có verify chữ ký | `POST /api/payment/bank-webhook` |

**Luồng thanh toán:**
1. Khách tạo booking → gọi `POST /api/simplepayment/create-link` để tạo QR PayOS đúng số tiền
2. Khách quét QR, chuyển khoản
3. PayOS phát hiện giao dịch → gọi webhook về server
4. Server xác nhận, cập nhật booking → broadcast qua SignalR (`PaymentHub`) → frontend tự động ẩn QR, hiển thị "Đã thanh toán"

**Payment Session:** mỗi lần tạo QR sẽ sinh một session tạm có thời hạn (`ExpiryMinutes`), tách biệt khỏi trạng thái booking chính — cho phép hủy/hết hạn linh hoạt mà không ảnh hưởng dữ liệu booking gốc.

> ⚠️ **Lưu ý bảo mật (đang xử lý):** Endpoint webhook chính (`/api/simplepayment/webhook`) hiện là `[AllowAnonymous]` và **chưa verify chữ ký** từ PayOS/SePay. Đây là hạng mục ưu tiên cần vá trước khi đưa vào môi trường có giao dịch thật quy mô lớn — xem mục [Bảo mật](#-bảo-mật--known-issues) bên dưới.

---

## 🤖 Chat hỗ trợ AI

- Endpoint: `POST /api/aichat/send` (public, không cần đăng nhập)
- Hỗ trợ nhiều nhà cung cấp AI, cấu hình qua `appsettings.json` (`AIChat:Provider`): `openai`, `groq`, `huggingface`, `cohere`, hoặc `sample` (không cần API key)
- **RAG đơn giản:** trước khi gửi câu hỏi cho AI, hệ thống tự động nhận diện ý định (phòng, booking, nhà hàng, đánh giá...) và truy vấn dữ liệu thật từ database, đưa vào system prompt — giúp AI trả lời chính xác theo dữ liệu thật thay vì bịa
- Nếu khách đã đăng nhập, `customerId` được lấy từ JWT token (không tin dữ liệu client tự gửi) → chatbot có thể tra cứu đúng booking của khách đang chat
- Chế độ **fallback không cần AI thật:** nếu chưa cấu hình API key, hệ thống vẫn trả lời bằng logic keyword-matching + dữ liệu DB thật

---

## 🚀 Cài đặt (Local Development)

### Yêu cầu hệ thống

| Phần mềm | Version | Ghi chú |
|---|---|---|
| .NET 8 SDK | 8.0+ | ⚠️ Bắt buộc |
| SQL Server LocalDB | 2019+ | ⚠️ Bắt buộc (đi kèm Visual Studio) |
| Visual Studio 2022 | Community+ | Khuyên dùng |
| ngrok | Latest | Dùng để test webhook thanh toán khi chạy local |
| Postman | Latest | Optional — test API |

### Kiểm tra hệ thống

```bash
dotnet --version        # Expected: 8.0.x+
sqllocaldb info          # Expected: danh sách instances
dotnet ef --version      # Expected: 8.0.x+
```

### Các bước cài đặt

```bash
# 1. Clone project
git clone <repository-url>
cd QuanLyResort/QuanLyResort

# 2. Restore dependencies
dotnet restore

# 3. Cấu hình appsettings.json (KHÔNG commit key thật — xem mục Bảo mật)
#    - ConnectionStrings:DefaultConnection
#    - AIChat:ApiKey (nếu dùng AI thật)
#    - BankWebhook:PayOs:ChecksumKey / SecretKey

# 4. Tạo database
dotnet ef database update

# 5. Chạy project
dotnet run
# → https://localhost:7000/swagger
```

### Seed dữ liệu mẫu

```
POST /api/auth/login          # Đăng nhập admin lấy token
POST /api/admin/seed          # Seed data mẫu (rooms, customers, bookings...)
```

**Tài khoản mặc định (sau khi seed):**

| Email | Password | Role |
|---|---|---|
| admin@resort.test | P@ssw0rd123 | Admin |
| frontdesk@resort.test | P@ssw0rd123 | FrontDesk |
| cashier@resort.test | P@ssw0rd123 | Cashier |
| manager@resort.test | P@ssw0rd123 | Manager |
| customer1@guest.test | Guest@123 | Customer |

> Đổi các mật khẩu mẫu này trước khi dùng trên môi trường thật.

### Test webhook thanh toán khi dev local

```bash
# Terminal 1: chạy backend
dotnet run

# Terminal 2: chạy ngrok để lộ localhost ra internet
ngrok http 5130

# Terminal 3: test webhook giả lập
./test-webhook-ngrok.sh https://<your-ngrok-url>.ngrok.io 6 5000
```

---

## 🌐 Deployment (Render.com)

```bash
# Sau khi deploy, đăng ký webhook URL với PayOS
./config-payos-after-deploy.sh https://<your-app>.onrender.com
```

> ⚠️ Script này đọc `ClientId`/`ApiKey` từ `appsettings.json`. **Không để giá trị mặc định/fallback là key thật trong source code** — dùng biến môi trường hoặc secret manager của Render.

**Production URLs:**

| Mô tả | URL |
|---|---|
| API Swagger | `/swagger` |
| Customer Frontend | `/customer/index.html` |
| Admin Dashboard | `/admin/index.html` |
| Webhook thanh toán (đăng ký với PayOS) | `/api/simplepayment/webhook` |

---

## 📚 API chính

| Nhóm | Endpoints | Mô tả |
|---|---|---|
| Authentication | `/api/auth/*` | Login, Register |
| Rooms | `/api/rooms/*` | Quản lý phòng, availability |
| Bookings | `/api/bookings/*` | Đặt phòng, check-in/out |
| Payment | `/api/simplepayment/*`, `/api/payment/*` | Tạo QR, webhook, session |
| Invoices | `/api/invoices/*` | Hóa đơn |
| Reports | `/api/reports/*` | Doanh thu, công suất |
| Audit | `/api/audit/*` | Audit logs |
| AI Chat | `/api/aichat/*` | Chat hỗ trợ AI |
| Admin | `/api/admin/*` | Seed data, stats |

Chi tiết đầy đủ: xem `CLIENT_API_MAP.md`.

---

## 🔒 Bảo mật / Known Issues (Điểm yếu của dự án)

- [ ] **Webhook thanh toán chính không xác thực chữ ký** — `/api/simplepayment/webhook` là `[AllowAnonymous]`, không verify signature từ PayOS/SePay. Bất kỳ ai biết booking ID (số nguyên tăng dần, dễ đoán) đều có thể tự POST request giả để đánh dấu booking "đã thanh toán" mà không cần trả tiền thật.
- [ ] **API Key/ClientId PayOS thật bị hardcode và lộ trong repo public** — file `config-payos-after-deploy.sh` có giá trị fallback là credentials thật, cần **revoke và tạo key mới ngay**, đồng thời xóa khỏi Git history.
- [ ] **Endpoint test có thể giả lập thanh toán thành công** — `PaymentController.TestPayment()` cho phép tự đánh dấu booking của mình là "Paid"; nếu còn tồn tại trên production đây là backdoor.
- [ ] **Signature verification (ở `PayOsWebhookService.cs`) dùng công thức tự chế, không đúng chuẩn PayOS chính thức** — có nguy cơ luôn reject webhook thật, dẫn đến việc phải tắt xác thực để "chạy được" (mất luôn lớp bảo vệ đó).

- [ ] **Chưa đối soát ngược lại với PayOS** — hệ thống tin `amount` do webhook payload tự khai báo, chưa gọi API PayOS xác nhận lại giao dịch thật trước khi cập nhật booking.
- [ ] **Chat AI endpoint public không rate-limit** — `/api/aichat/send` không giới hạn số lần gọi, có thể bị lợi dụng để tốn tiền/quota API AI trả phí.
- [ ] **Lộ chi tiết lỗi nội bộ ra client** — `AIChatController` trả `ex.Message` thẳng cho người dùng ở response lỗi, có thể hé lộ thông tin cấu trúc hệ thống.
- [ ] **Log quá chi tiết, có thể lộ dữ liệu nhạy cảm** — log in ra prefix API key, toàn bộ request/response body (bao gồm dữ liệu booking khách hàng); rủi ro nếu log bị truy cập trái phép.

- [ ] **Tài liệu không đồng bộ với code thật** — README/CLIENT_API_MAP.md từng ghi "chưa có payment gateway" trong khi thực tế đã tích hợp PayOS/VietQR/MB Bank hoàn chỉnh (đã cập nhật trong bản này).
- [ ] **File rác bị commit vào git** — `.DS_Store`, `.vs/` không nằm trong `.gitignore`.
- [ ] **Chưa có CI/CD** — dù GitHub Actions có sẵn trong repo, chưa thấy pipeline tự động build/test.
- [ ] **Nhiều route `/test/*` còn tồn tại song song với code production** — nên tách riêng môi trường Dev/Staging hoặc dùng feature flag để ẩn hoàn toàn khỏi production.

> **Tóm gọn:** phần kiến trúc, phạm vi tính năng và tư duy hệ thống (session pattern, real-time SignalR, RAG-lite cho chatbot) đều ở mức tốt — điểm yếu gần như tập trung hết vào **lớp bảo mật xác thực của webhook thanh toán**, đây cũng là hạng mục duy nhất mang tính "phải sửa" thay vì "nên sửa".

---

## 📋 TODO & Future Enhancements

- [ ] Đối soát giao dịch tự động (reconciliation) — xác nhận lại với API PayOS trước khi tin webhook
- [ ] Email/SMS notifications
- [ ] Multi-language support
- [ ] Advanced reporting (export Excel)
- [ ] Rate limiting toàn hệ thống

---

## 🧪 Testing

- Project test riêng: `ResortManagementAPI.Tests`
- Postman collection: `postman_resort_frontend.json`
- Test webhook local: `test-webhook-ngrok.sh` (xem mục cài đặt)

---

## 👥 Team & Contact

- **Developers:** Nhựt, Nguyên, Lam, Ninh
- **Email:** phamthahlam@gmail.com
- **Docs:** `CLIENT_API_MAP.md`

---
