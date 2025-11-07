# 🔐 FLOW ĐĂNG NHẬP & REDIRECT

## 📋 **TỔNG QUAN:**

Khi user đăng nhập, hệ thống sẽ **tự động redirect** dựa trên **role** của user.

---

## 🎯 **LOGIC REDIRECT:**

### **1. Admin & Staff Roles → Dashboard**

**Các role sau sẽ redirect đến `/admin/html/index.html`:**

| Role | Mô tả | Redirect URL |
|------|-------|--------------|
| `Admin` | Quản trị viên | `/admin/html/index.html` |
| `Manager` | Quản lý | `/admin/html/index.html` |
| `Business` | Kinh doanh | `/admin/html/index.html` |
| `FrontDesk` | Lễ tân | `/admin/html/index.html` |
| `Cashier` | Thu ngân | `/admin/html/index.html` |
| `Accounting` | Kế toán | `/admin/html/index.html` |
| `Inventory` | Kho | `/admin/html/index.html` |

### **2. Customer Role → Customer Portal**

**Role Customer sẽ redirect đến `/customer/index.html`:**

| Role | Mô tả | Redirect URL |
|------|-------|--------------|
| `Customer` | Khách hàng | `/customer/index.html` |

---

## 💻 **CODE IMPLEMENTATION:**

### **File:** `wwwroot/customer/login.html`

```javascript
// Determine redirect URL based on role
const role = result.user?.role;
let redirectUrl;

if (role === 'Admin' || role === 'Manager' || role === 'Business' || 
    role === 'FrontDesk' || role === 'Cashier' || role === 'Accounting' || 
    role === 'Inventory') {
  redirectUrl = '/admin/html/index.html';  // ← Admin Dashboard
} else {
  redirectUrl = '/customer/index.html';     // ← Customer Portal
}

console.log('🎯 User role:', role);
console.log('🔄 Redirecting to:', redirectUrl);

// Redirect after 1 second
setTimeout(() => {
  console.log('🚀 Executing redirect...');
  window.location.href = redirectUrl;
}, 1000);
```

---

## 📊 **FLOW DIAGRAM:**

```
┌─────────────────────────────────────────────────────────────┐
│                    ĐĂNG NHẬP                                 │
│         http://localhost:5130/customer/login.html            │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │  Nhập email & password  │
              └─────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │   Gọi API /auth/login   │
              └─────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │   Kiểm tra role         │
              └─────────────────────────┘
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
    ┌──────────────────┐        ┌──────────────────┐
    │  Admin/Staff     │        │    Customer      │
    │  Roles           │        │    Role          │
    └──────────────────┘        └──────────────────┘
              │                           │
              ▼                           ▼
    ┌──────────────────┐        ┌──────────────────┐
    │  /admin/html/    │        │  /customer/      │
    │  index.html      │        │  index.html      │
    │  (Dashboard)     │        │  (Customer Home) │
    └──────────────────┘        └──────────────────┘
```

---

## 🧪 **TEST FLOW:**

### **Test 1: Đăng nhập Admin**

**Bước 1:** Vào login page
```
http://localhost:5130/customer/login.html
```

**Bước 2:** Nhập credentials
```
Email: admin@resort.test
Password: P@ssw0rd123
```

**Bước 3:** Click "Đăng nhập"

**Bước 4:** Mở Console (F12) → Xem logs
```
🎯 User role: Admin
🔄 Redirecting to: /admin/html/index.html
🚀 Executing redirect...
```

**Bước 5:** Sau 1 giây → Auto redirect đến
```
✅ http://localhost:5130/admin/html/index.html
```

**Kết quả:**
- ✅ Hiển thị Admin Dashboard
- ✅ Sidebar đầy đủ menu
- ✅ Navbar hiển thị "Nguyễn Văn Admin - Quản trị viên"

---

### **Test 2: Đăng nhập Customer**

**Bước 1:** Vào login page
```
http://localhost:5130/customer/login.html
```

**Bước 2:** Nhập credentials
```
Email: customer@resort.test
Password: P@ssw0rd123
```

**Bước 3:** Click "Đăng nhập"

**Bước 4:** Console logs
```
🎯 User role: Customer
🔄 Redirecting to: /customer/index.html
🚀 Executing redirect...
```

**Bước 5:** Auto redirect đến
```
✅ http://localhost:5130/customer/index.html
```

**Kết quả:**
- ✅ Hiển thị Customer Portal
- ✅ Navbar customer

---

## 🔍 **TROUBLESHOOTING:**

### **Vấn đề 1: Redirect sai trang**

**Triệu chứng:**
- Admin login nhưng vẫn đến customer page

**Nguyên nhân:**
- Role không đúng trong database
- Logic redirect bị sửa

**Giải pháp:**
```sql
-- Kiểm tra role của user trong database
SELECT UserId, Username, Email, Role, IsActive 
FROM Users 
WHERE Email = 'admin@resort.test';

-- Phải thấy: Role = 'Admin'
```

### **Vấn đề 2: Không redirect**

**Triệu chứng:**
- Đăng nhập thành công nhưng không chuyển trang

**Nguyên nhân:**
- JavaScript error
- Redirect bị block

**Giải pháp:**
```javascript
// Mở Console → Copy & paste để test manual redirect
const role = 'Admin';
const redirectUrl = role === 'Admin' ? '/admin/html/index.html' : '/customer/index.html';
console.log('Test redirect to:', redirectUrl);
window.location.href = redirectUrl;
```

### **Vấn đề 3: Redirect đến trang 404**

**Triệu chứng:**
- Redirect nhưng trang không tồn tại

**Nguyên nhân:**
- File `index.html` không tồn tại trong `/admin/html/`

**Giải pháp:**
```bash
# Kiểm tra file tồn tại
ls wwwroot/admin/html/index.html

# Phải thấy file
```

---

## 📱 **REDIRECT URLS SUMMARY:**

### **Admin Portal:**
```
🏠 Dashboard:        /admin/html/index.html
👥 Users:            /admin/html/users.html
🧑‍💼 Employees:       /admin/html/employees.html
👨‍👩‍👧‍👦 Customers:      /admin/html/customers.html
🏠 Rooms:            /admin/rooms.html
📅 Bookings:         /admin/bookings.html
📜 Audit Logs:       /admin/html/audit-logs.html
📊 Reports:          /admin/reports.html
```

### **Customer Portal:**
```
🏠 Home:             /customer/index.html
🏨 Rooms:            /customer/rooms.html
📅 My Bookings:      /customer/my-bookings.html
👤 Profile:          /customer/profile.html
```

---

## ✨ **DEFAULT LANDING PAGES:**

| User Type | Default Landing Page | Description |
|-----------|---------------------|-------------|
| Admin | `/admin/html/index.html` | Dashboard với stats cards, quick actions |
| Manager | `/admin/html/index.html` | Dashboard (same as Admin) |
| Staff | `/admin/html/index.html` | Dashboard (same as Admin) |
| Customer | `/customer/index.html` | Customer home với room search |

---

## 🎯 **KẾT LUẬN:**

### **✅ Đã implement:**
- ✅ Role-based redirect
- ✅ Admin → Dashboard (`/admin/html/index.html`)
- ✅ Customer → Customer Portal
- ✅ Console logging cho debug
- ✅ 1 second delay cho smooth transition
- ✅ Success message trước khi redirect

### **✅ Trải nghiệm người dùng:**
1. User login
2. Thấy "Đăng nhập thành công! Đang chuyển hướng..."
3. Sau 1 giây auto redirect
4. Đến đúng trang dựa vào role

### **✅ Bảo mật:**
- Role được check từ API response
- Token được lưu trong localStorage
- User info được lưu trong localStorage
- Dashboard pages có auth check

---

## 🧪 **TEST CHECKLIST:**

Khi test login, verify:

- [ ] Admin login → redirect đến `/admin/html/index.html`
- [ ] Dashboard hiển thị đầy đủ
- [ ] Sidebar có menu Users, Employees
- [ ] Navbar hiển thị đúng user info
- [ ] Customer login → redirect đến `/customer/index.html`
- [ ] Console logs đúng role và redirect URL
- [ ] Success message hiển thị trước redirect
- [ ] Redirect mất ~1 giây (smooth transition)

---

## 📚 **TÀI LIỆU LIÊN QUAN:**

- `DONG-NHAT-100-PHAN-TRAM.md` - Menu thống nhất
- `THONG-NHAT-HOAN-THANH-FINAL.md` - Tổng kết hoàn thành
- `FIX-SIDEBAR-NOT-SHOWING.md` - Fix sidebar issues
- `THONG-TIN-DANG-NHAP.txt` - Login credentials

---

*Updated: 21/10/2025*
*Status: ✅ WORKING - Role-based redirect implemented*
*Default Admin Landing: `/admin/html/index.html`*

