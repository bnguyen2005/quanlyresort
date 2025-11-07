# 🔐 CHI TIẾT HỆ THỐNG PHÂN QUYỀN

## 📋 MỤC LỤC
1. [Tổng quan hệ thống](#tổng-quan-hệ-thống)
2. [Các Roles và Quyền hạn](#các-roles-và-quyền-hạn)
3. [Public Endpoints](#public-endpoints)
4. [Protected Endpoints theo Role](#protected-endpoints-theo-role)
5. [Middleware Logic](#middleware-logic)
6. [Controller Authorization](#controller-authorization)
7. [Flow phân quyền](#flow-phân-quyền)

---

## 🎯 TỔNG QUAN HỆ THỐNG

Hệ thống sử dụng **JWT Authentication** và **Role-Based Access Control (RBAC)** với 2 lớp bảo vệ:

1. **Middleware Layer** (`JwtAuthorizationMiddleware`): Kiểm tra token và role trước khi đến controller
2. **Controller Layer**: Kiểm tra quyền chi tiết với `[Authorize]` attributes

---

## 👥 CÁC ROLES VÀ QUYỀN HẠN

### **1. Admin** 👑
**Quyền hạn cao nhất - Full Access**

#### ✅ **Có thể làm:**
- ✅ Truy cập **TẤT CẢ** endpoints
- ✅ Tạo, sửa, xóa bất kỳ resource nào
- ✅ Quản lý users và employees
- ✅ Quản lý rooms, bookings, invoices
- ✅ Quản lý coupons, services, inventory
- ✅ Xem và tạo reports
- ✅ Xóa reviews, bookings, invoices
- ✅ Check-in/check-out bookings
- ✅ Process payments
- ✅ Upload images cho rooms/services

#### ❌ **Không có giới hạn**

---

### **2. Manager** 📊
**Quyền quản lý - Gần như full access**

#### ✅ **Có thể làm:**
- ✅ Truy cập hầu hết endpoints (trừ một số endpoints nhạy cảm)
- ✅ Quản lý bookings, rooms, customers
- ✅ Quản lý coupons (CRUD)
- ✅ Xem và tạo reports
- ✅ Check-in/check-out bookings
- ✅ Assign rooms
- ✅ Respond to reviews
- ✅ Upload images cho rooms/services

#### ❌ **KHÔNG thể:**
- ❌ Xóa users (`/api/usermanagement/{id}/delete`)
- ❌ Xóa employees (`/api/employeemanagement/{id}/delete`)
- ❌ Một số endpoints nhạy cảm khác (nếu có)

---

### **3. Business** 💼
**Nhân viên kinh doanh**

#### ✅ **Có thể làm:**
- ✅ Xem bookings (tất cả)
- ✅ Xem rooms và room statistics
- ✅ Xem customers
- ✅ Xem reports
- ✅ Quản lý customer information

#### ❌ **KHÔNG thể:**
- ❌ Tạo/sửa/xóa bookings
- ❌ Quản lý rooms (chỉ xem)
- ❌ Quản lý invoices
- ❌ Quản lý users/employees

---

### **4. FrontDesk** 🏨
**Lễ tân - Quản lý check-in/check-out**

#### ✅ **Có thể làm:**
- ✅ Xem bookings (tất cả)
- ✅ Tạo và quản lý bookings
- ✅ Assign rooms cho bookings
- ✅ Check-in bookings
- ✅ Check-out bookings (có thể thêm charges)
- ✅ Xem rooms và room statistics
- ✅ Xem customers
- ✅ Quản lý restaurant orders
- ✅ Upload images cho rooms

#### ❌ **KHÔNG thể:**
- ❌ Xóa bookings, rooms, customers
- ❌ Quản lý users/employees
- ❌ Xóa restaurant orders
- ❌ Process payments (trừ checkout)

---

### **5. Cashier** 💵
**Thu ngân - Xử lý thanh toán**

#### ✅ **Có thể làm:**
- ✅ Xem invoices (tất cả)
- ✅ Process payments cho invoices
- ✅ Xem bookings (để liên kết với invoices)
- ✅ Check-out bookings (có thể process payment)
- ✅ Xem charges

#### ❌ **KHÔNG thể:**
- ❌ Tạo/sửa invoices
- ❌ Xóa invoices
- ❌ Quản lý rooms, bookings (chỉ xem)
- ❌ Quản lý customers

---

### **6. Accounting** 📈
**Kế toán - Quản lý tài chính**

#### ✅ **Có thể làm:**
- ✅ Xem invoices (tất cả)
- ✅ Xem reports (tài chính)
- ✅ Quản lý inventory
- ✅ Process payments

#### ❌ **KHÔNG thể:**
- ❌ Tạo/sửa bookings
- ❌ Quản lý rooms, customers
- ❌ Quản lý users/employees

---

### **7. Inventory** 📦
**Quản lý kho**

#### ✅ **Có thể làm:**
- ✅ Truy cập **CHỈ** inventory endpoints
- ✅ Quản lý inventory items

#### ❌ **KHÔNG thể:**
- ❌ Truy cập bất kỳ endpoint nào khác

---

### **8. Customer** 👤
**Khách hàng - Quyền hạn giới hạn**

#### ✅ **Có thể làm:**
- ✅ Xem rooms (public)
- ✅ Xem services (public)
- ✅ Xem reviews (public)
- ✅ Tạo bookings **cho chính mình**
- ✅ Xem bookings **của chính mình**
- ✅ Cancel bookings **của chính mình**
- ✅ Transfer bookings **của chính mình** to FrontDesk
- ✅ Xem/cập nhật thông tin cá nhân qua `/api/customermanagement`
- ✅ Tạo restaurant orders (walk-in hoặc có account)
- ✅ Xem restaurant orders **của chính mình**
- ✅ Pay restaurant orders **của chính mình**
- ✅ Tạo reviews (sau khi đã stay)
- ✅ Xem reviews (public)
- ✅ Validate và apply coupon codes

#### ❌ **KHÔNG thể:**
- ❌ Xem bookings của khách khác
- ❌ Xem invoices của khách khác
- ❌ Xem customers khác
- ❌ Quản lý rooms, services (chỉ xem)
- ❌ Xóa reviews (chỉ admin)
- ❌ Quản lý bất kỳ resource nào khác

---

## 🌐 PUBLIC ENDPOINTS

### **Không cần authentication - Ai cũng có thể truy cập**

#### **1. Authentication Endpoints**
```
POST /api/auth/login
POST /api/auth/customer-login
POST /api/auth/register
POST /api/auth/staff-login
```

#### **2. Rooms & Room Types**
```
GET /api/rooms                    # Xem danh sách phòng
GET /api/rooms/{id}               # Xem chi tiết phòng
GET /api/rooms/floors             # Xem danh sách tầng
GET /api/room-types               # Xem loại phòng
GET /api/room-types/{id}          # Xem chi tiết loại phòng
```

#### **3. Reviews**
```
GET /api/reviews                  # Xem tất cả reviews
GET /api/reviews/{id}             # Xem chi tiết review
GET /api/reviews?roomId={id}      # Xem reviews theo phòng
```

#### **4. Coupons**
```
GET /api/coupons/validate?code={code}    # Validate coupon code
GET /api/coupons/active                   # Xem danh sách mã giảm giá active
```

#### **5. Services**
```
GET /api/services/restaurant/menu        # Xem menu nhà hàng
GET /api/services/types                  # Xem loại dịch vụ
```

#### **6. Restaurant Orders (Một phần)**
```
POST /api/restaurant-orders              # Đặt món (walk-in, không cần login)
GET /api/restaurant-orders/{id}          # Xem order details (nếu biết ID)
```

---

## 🔒 PROTECTED ENDPOINTS THEO ROLE

### **📋 BOOKINGS**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/bookings` | POST | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ (chỉ mình) |
| `/api/bookings/my` | GET | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ |
| `/api/bookings` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| `/api/bookings/{id}/transfer-to-frontdesk` | POST | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ (chỉ mình) |
| `/api/bookings/{id}/assign-room` | POST | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ |
| `/api/bookings/{id}/checkin` | POST | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `/api/bookings/{id}/add-charge` | POST | ✅ | ❌ | ❌ | ✅ | ✅ | ❌ |
| `/api/bookings/{id}/checkout` | POST | ✅ | ❌ | ❌ | ✅ | ✅ | ❌ |
| `/api/bookings/{id}/cancel` | POST | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ (chỉ mình) |
| `/api/bookings/{id}/pay-online` | POST | ✅ | ❌ | ❌ | ✅ | ✅ | ✅ (chỉ mình) |

### **🏨 ROOMS**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/rooms` | GET | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| `/api/rooms/{id}` | GET | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| `/api/rooms/statistics` | GET | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `/api/rooms` | POST | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/rooms/{id}` | PUT | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/rooms/{id}` | DELETE | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `/api/rooms/{id}/upload-image` | POST | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ |
| `/api/rooms/{id}/status` | PATCH | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ |

### **🎟️ COUPONS**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/coupons/validate` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/coupons/active` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/coupons` | GET | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/coupons/{id}` | GET | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/coupons` | POST | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/coupons/{id}` | PUT | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/coupons/{id}` | PATCH | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/coupons/{id}` | DELETE | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

### **💬 REVIEWS**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/reviews` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/reviews/{id}` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/reviews` | POST | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| `/api/reviews/{id}/response` | PUT | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/reviews/{id}` | DELETE | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `/api/reviews/can-review/{roomId}` | GET | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| `/api/reviews/reviewable-rooms` | GET | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

### **🍽️ RESTAURANT ORDERS**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/restaurant-orders` | POST | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ |
| `/api/restaurant-orders/my` | GET | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ |
| `/api/restaurant-orders` | GET | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ |
| `/api/restaurant-orders/{id}` | GET | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ (chỉ mình) |
| `/api/restaurant-orders/{id}/status` | PATCH | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ |
| `/api/restaurant-orders/{id}/pay` | POST | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ (chỉ mình) |

### **📄 INVOICES**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/invoices` | GET | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/invoices/statistics` | GET | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| `/api/invoices/{id}/pay` | POST | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ |
| `/api/invoices/{id}` | DELETE | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

### **👥 CUSTOMER MANAGEMENT**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/customermanagement` | GET | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ (chỉ mình) |
| `/api/customermanagement/{id}` | GET | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ (chỉ mình) |
| `/api/customermanagement/{id}` | PUT | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ (chỉ mình) |

### **📊 REPORTS**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Customer |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:--------:|
| `/api/reports/*` | GET | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

### **📦 INVENTORY**

| Endpoint | Method | Admin | Manager | Business | FrontDesk | Cashier | Accounting | Inventory |
|----------|--------|:-----:|:-------:|:--------:|:---------:|:-------:|:----------:|:---------:|
| `/api/inventory/*` | All | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ |

### **👨‍💼 USER & EMPLOYEE MANAGEMENT**

| Endpoint | Method | Admin | Manager | Others |
|----------|--------|:-----:|:-------:|:------:|
| `/api/usermanagement/*` | GET/POST/PUT | ✅ | ✅* | ❌ |
| `/api/usermanagement/{id}/delete` | DELETE | ✅ | ❌ | ❌ |
| `/api/employeemanagement/*` | GET/POST/PUT | ✅ | ✅* | ❌ |
| `/api/employeemanagement/{id}/delete` | DELETE | ✅ | ❌ | ❌ |

*Manager có thể xem/sửa nhưng không thể xóa

---

## ⚙️ MIDDLEWARE LOGIC

### **Flow xử lý request:**

```
1. Request đến → JwtAuthorizationMiddleware
   ↓
2. Check Public Endpoints (TRƯỚC TIÊN)
   - Nếu là public → Bypass authentication → Cho phép
   ↓
3. Check Authentication
   - Nếu không có token → 401 Unauthorized
   ↓
4. Check Role Validity
   - Nếu role không hợp lệ → 403 Forbidden
   ↓
5. Check Role Permissions (HasPermissionToAccess)
   - Nếu role không có quyền → 403 Forbidden
   ↓
6. Cho phép truy cập → Controller
```

### **Public Endpoints Check (Priority 1)**
```csharp
// Check TRƯỚC khi kiểm tra token
- /api/reviews (GET)
- /api/coupons/validate (GET)
- /api/coupons/active (GET)
- /api/room-types (GET)
- /api/rooms (GET)
- /api/rooms/{id} (GET)
- /api/rooms/floors (GET)
- /api/services/restaurant/menu (GET)
- /api/services/types (GET)
- /api/restaurant-orders (POST)
- /api/restaurant-orders/{id} (GET)
```

### **Role Permission Check (HasPermissionToAccess)**

Logic kiểm tra quyền dựa trên path và role:

```csharp
Admin → return true (tất cả)
Manager → return true (trừ xóa users/employees)
Business → /bookings, /rooms, /customers, /reports
FrontDesk → /bookings, /rooms, /customers, /restaurant-orders (không xóa)
Cashier → /invoices, /bookings, /charges
Accounting → /invoices, /reports, /inventory
Inventory → /inventory (chỉ)
Customer → /rooms, /services, /bookings, /customermanagement, /restaurant-orders, /reviews
```

---

## 🎯 CONTROLLER AUTHORIZATION

### **1. Controller-Level Authorization**

```csharp
// Ví dụ: Yêu cầu authentication cho tất cả methods
[Authorize]
public class BookingsController : ControllerBase { }

// Ví dụ: Yêu cầu role cụ thể
[Authorize(Roles = "Admin,Manager")]
public class ServicesController : ControllerBase { }
```

### **2. Method-Level Authorization**

```csharp
// Override controller-level authorization
[AllowAnonymous]  // Cho phép không cần auth
public async Task<IActionResult> GetActiveCoupons() { }

// Yêu cầu role cụ thể
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteCoupon(int id) { }
```

### **3. Priority Order:**
1. **Method-level** override **Class-level**
2. **Middleware** check trước **Controller attributes**

---

## 🔄 FLOW PHÂN QUYỀN

### **Ví dụ 1: Customer xem danh sách phòng**

```
Request: GET /api/rooms
↓
Middleware: Check public endpoints → ✅ Match
↓
Bypass authentication → Controller
↓
Controller: [AllowAnonymous] → ✅ OK
↓
Response: 200 OK với danh sách phòng
```

### **Ví dụ 2: Customer tạo booking**

```
Request: POST /api/bookings
↓
Middleware: Không phải public endpoint
↓
Check token → ✅ Có token
↓
Check role → ✅ Role = "Customer"
↓
Check permission → ✅ path.Contains("/bookings") → OK
↓
Controller: [Authorize] → ✅ OK
↓
Controller logic: Kiểm tra CustomerId trong request = CustomerId trong token
↓
Response: 201 Created hoặc 403 Forbidden (nếu không phải booking của mình)
```

### **Ví dụ 3: FrontDesk check-in booking**

```
Request: POST /api/bookings/{id}/checkin
↓
Middleware: Không phải public endpoint
↓
Check token → ✅ Có token
↓
Check role → ✅ Role = "FrontDesk"
↓
Check permission → ✅ path.Contains("/bookings") → OK
↓
Controller: [Authorize(Roles = "Admin,FrontDesk")] → ✅ OK
↓
Response: 200 OK
```

### **Ví dụ 4: Manager xóa user (KHÔNG được)**

```
Request: DELETE /api/usermanagement/{id}
↓
Middleware: Không phải public endpoint
↓
Check token → ✅ Có token
↓
Check role → ✅ Role = "Manager"
↓
Check permission → ❌ path.Contains("/usermanagement") && path.Contains("/delete") → FALSE
↓
Response: 403 Forbidden
```

---

## 🔍 CHI TIẾT THEO TỪNG CONTROLLER

### **CouponsController**

| Endpoint | Auth | Role | Mô tả |
|----------|------|------|-------|
| `GET /api/coupons/validate` | ❌ | - | Validate coupon (public) |
| `GET /api/coupons/active` | ❌ | - | Xem coupons active (public) |
| `GET /api/coupons` | ✅ | Admin, Manager | Xem tất cả coupons |
| `GET /api/coupons/{id}` | ✅ | Admin, Manager | Xem chi tiết coupon |
| `POST /api/coupons` | ✅ | Admin, Manager | Tạo coupon mới |
| `PUT /api/coupons/{id}` | ✅ | Admin, Manager | Sửa coupon |
| `PATCH /api/coupons/{id}` | ✅ | Admin, Manager | Update status coupon |
| `DELETE /api/coupons/{id}` | ✅ | Admin | Xóa coupon |

### **ReviewsController**

| Endpoint | Auth | Role | Mô tả |
|----------|------|------|-------|
| `GET /api/reviews` | ❌ | - | Xem reviews (public) |
| `GET /api/reviews/{id}` | ❌ | - | Xem chi tiết review (public) |
| `POST /api/reviews` | ✅ | Customer | Tạo review |
| `PUT /api/reviews/{id}/response` | ✅ | Admin, Manager | Trả lời review |
| `DELETE /api/reviews/{id}` | ✅ | Admin | Xóa review |
| `GET /api/reviews/can-review/{roomId}` | ✅ | Customer | Kiểm tra có thể review không |
| `GET /api/reviews/reviewable-rooms` | ✅ | Customer | Xem phòng có thể review |

### **RoomsController**

| Endpoint | Auth | Role | Mô tả |
|----------|------|------|-------|
| `GET /api/rooms` | ❌ | - | Xem danh sách phòng (public) |
| `GET /api/rooms/{id}` | ❌ | - | Xem chi tiết phòng (public) |
| `GET /api/rooms/floors` | ❌ | - | Xem danh sách tầng (public) |
| `GET /api/rooms/statistics` | ✅ | Admin, Manager, Business, FrontDesk | Xem thống kê phòng |
| `POST /api/rooms` | ✅ | Admin, Manager | Tạo phòng mới |
| `PUT /api/rooms/{id}` | ✅ | Admin, Manager | Sửa phòng |
| `PATCH /api/rooms/{id}/status` | ✅ | Admin, Manager, FrontDesk | Cập nhật trạng thái phòng |
| `DELETE /api/rooms/{id}` | ✅ | Admin | Xóa phòng |
| `POST /api/rooms/{id}/upload-image` | ✅ | Admin, Manager, FrontDesk | Upload hình ảnh |

### **BookingsController**

| Endpoint | Auth | Role | Mô tả |
|----------|------|------|-------|
| `POST /api/bookings` | ✅ | All authenticated | Tạo booking (controller kiểm tra CustomerId) |
| `GET /api/bookings/my` | ✅ | Customer, Admin, FrontDesk, Manager | Xem bookings của mình |
| `GET /api/bookings` | ✅ | Admin, FrontDesk, Manager, Cashier | Xem tất cả bookings |
| `POST /api/bookings/{id}/transfer-to-frontdesk` | ✅ | Customer, Admin, FrontDesk | Transfer booking |
| `POST /api/bookings/{id}/assign-room` | ✅ | Admin, FrontDesk, Manager | Assign phòng |
| `POST /api/bookings/{id}/checkin` | ✅ | Admin, FrontDesk | Check-in |
| `POST /api/bookings/{id}/add-charge` | ✅ | Admin, FrontDesk, Cashier | Thêm phụ phí |
| `POST /api/bookings/{id}/checkout` | ✅ | Admin, FrontDesk, Cashier | Check-out |
| `POST /api/bookings/{id}/cancel` | ✅ | All authenticated | Cancel booking (controller kiểm tra ownership) |
| `POST /api/bookings/{id}/pay-online` | ✅ | Customer, Admin, FrontDesk, Cashier | Thanh toán online |

### **RestaurantOrdersController**

| Endpoint | Auth | Role | Mô tả |
|----------|------|------|-------|
| `POST /api/restaurant-orders` | ❌ | - | Tạo order (public, walk-in) |
| `GET /api/restaurant-orders/{id}` | ❌ | - | Xem order (public, nếu biết ID) |
| `GET /api/restaurant-orders/my` | ✅ | Customer, Admin, FrontDesk, Manager | Xem orders của mình |
| `GET /api/restaurant-orders` | ✅ | Admin, Manager, FrontDesk | Xem tất cả orders |
| `PATCH /api/restaurant-orders/{id}/status` | ✅ | Admin, Manager, FrontDesk | Cập nhật trạng thái |
| `POST /api/restaurant-orders/{id}/pay` | ✅ | All authenticated | Thanh toán order |

---

## 🛡️ BẢO MẬT BỔ SUNG

### **1. Controller-Level Validation**

Một số controllers có validation bổ sung:

- **BookingsController**: Kiểm tra `CustomerId` trong request phải match với `CustomerId` trong token
- **ReviewsController**: Kiểm tra customer đã stay ở phòng đó chưa
- **CustomerManagementController**: Kiểm tra customer chỉ có thể xem/sửa thông tin của chính mình

### **2. Business Logic Protection**

- Không cho phép double-booking
- Không cho phép check-in phòng đã occupied
- Không cho phép cancel booking đã check-in
- Không cho phép review phòng chưa stay

---

## 📝 LƯU Ý QUAN TRỌNG

1. **Middleware chạy TRƯỚC Controller**: Nếu middleware chặn → Không đến controller
2. **Method-level override Class-level**: `[AllowAnonymous]` ở method sẽ override `[Authorize]` ở class
3. **Path matching**: Sử dụng `Contains()` nên cần cẩn thận với path conflicts
4. **Customer permissions**: Nhiều endpoints cho phép Customer nhưng controller sẽ kiểm tra ownership
5. **Public endpoints**: Cần được list trong middleware để bypass authentication

---

## ✅ KẾT LUẬN

Hệ thống phân quyền đã được thiết kế **đầy đủ và chặt chẽ** với:

- ✅ 8 roles với quyền hạn rõ ràng
- ✅ Public endpoints cho customer-facing features
- ✅ Role-based access control cho protected endpoints
- ✅ Controller-level validation bổ sung
- ✅ Business logic protection

**Không cần update thêm gì quan trọng!** 🎉

