# 🧪 HƯỚNG DẪN TEST - QUẢN LÝ NGƯỜI DÙNG & PHÂN QUYỀN

## ✅ ĐÃ HOÀN THÀNH

### **Backend APIs (40+ endpoints)**
- ✅ UserManagementController
- ✅ EmployeeManagementController
- ✅ CustomerManagementController
- ✅ AuditController (updated)

### **Frontend UI Pages**
- ✅ `/admin/html/users.html` - Quản lý Users
- ✅ `/admin/html/employees.html` - Quản lý Nhân viên

### **Database**
- ✅ Customer model updated (TotalSpent, LoyaltyPoints, Notes)
- ✅ Migration created & applied

---

## 🚀 BẮT ĐẦU TEST

### **1. Kiểm tra Server đã chạy**
Mở trình duyệt, vào:
```
http://localhost:5130/swagger
```

✅ **Bạn sẽ thấy Swagger UI** với các endpoints mới:
- `/api/UserManagement`
- `/api/EmployeeManagement`
- `/api/CustomerManagement`
- `/api/Audit`

---

## 🧪 TEST APIs QUA SWAGGER

### **A. Test UserManagement APIs**

#### **1. Lấy danh sách Users**
- Mở `GET /api/UserManagement`
- Click **Try it out** → **Execute**
- Filter: `isActive=true`
- Nhập Bearer token (login trước để lấy token)

**Kết quả mong đợi:**
```json
[
  {
    "userId": 1,
    "username": "admin",
    "email": "admin@resort.test",
    "role": "Admin",
    "fullName": "Nguyễn Văn Admin",
    "isActive": true,
    ...
  }
]
```

#### **2. Tạo User mới**
- Mở `POST /api/UserManagement`
- Click **Try it out**
- Request body:
```json
{
  "username": "testuser",
  "email": "test@resort.test",
  "password": "Test@123456",
  "role": "FrontDesk",
  "fullName": "Test User",
  "phoneNumber": "0987654321",
  "isActive": true
}
```
- Click **Execute**

**Kết quả:** User mới được tạo, trả về userId

#### **3. Đổi mật khẩu**
- Mở `POST /api/UserManagement/{id}/change-password`
- Nhập ID của user vừa tạo
- Request body:
```json
{
  "newPassword": "NewPass@123"
}
```

#### **4. Đổi Role**
- Mở `POST /api/UserManagement/{id}/change-role`
- Request body:
```json
{
  "newRole": "Manager"
}
```

#### **5. Khóa/Mở khóa User**
- Mở `POST /api/UserManagement/{id}/toggle-active`
- Click **Execute**

---

### **B. Test EmployeeManagement APIs**

#### **1. Lấy thống kê nhân viên**
- Mở `GET /api/EmployeeManagement/statistics`
- Click **Execute**

**Kết quả mong đợi:**
```json
{
  "totalEmployees": 10,
  "activeEmployees": 9,
  "inactiveEmployees": 1,
  "byDepartment": [
    {"department": "FrontDesk", "count": 3},
    {"department": "Housekeeping", "count": 4},
    ...
  ]
}
```

#### **2. Tạo nhân viên mới**
- Mở `POST /api/EmployeeManagement`
- Request body:
```json
{
  "fullName": "Nguyễn Văn Test",
  "email": "test.employee@resort.test",
  "phoneNumber": "0912345678",
  "position": "Receptionist",
  "department": "FrontDesk",
  "salary": 10000000,
  "hireDate": "2025-10-21"
}
```

#### **3. Chấm dứt hợp đồng**
- Mở `POST /api/EmployeeManagement/{id}/terminate`
- Request body:
```json
{
  "terminationDate": "2025-10-21",
  "reason": "Test chấm dứt hợp đồng"
}
```

---

### **C. Test CustomerManagement APIs**

#### **1. Lấy thống kê khách hàng**
- Mở `GET /api/CustomerManagement/statistics`
- Click **Execute**

#### **2. Tạo khách hàng mới**
- Mở `POST /api/CustomerManagement`
- Request body:
```json
{
  "fullName": "Test Customer",
  "email": "testcustomer@example.com",
  "phoneNumber": "0901234567",
  "nationality": "Vietnam",
  "customerType": "Regular"
}
```

#### **3. Thêm Loyalty Points**
- Mở `POST /api/CustomerManagement/{id}/add-points`
- Request body:
```json
{
  "points": 100,
  "reason": "Test thêm điểm"
}
```

---

### **D. Test Audit APIs**

#### **1. Xem Audit Logs**
- Mở `GET /api/Audit/logs`
- Parameters:
  - `page`: 1
  - `pageSize`: 10
  - `entityName`: User (optional)
- Click **Execute**

**Kết quả:** Danh sách logs với pagination

#### **2. Xem User Activity**
- Mở `GET /api/Audit/user-activity`
- Click **Execute**

**Kết quả:** Thống kê hoạt động theo user

---

## 🎨 TEST UI PAGES

### **1. Test trang Quản lý Users**

**Bước 1: Đăng nhập**
```
http://localhost:5130/customer/login.html
```
- Email: `admin@resort.test`
- Password: `P@ssw0rd123`

**Bước 2: Vào trang Users**
```
http://localhost:5130/admin/html/users.html
```

**Các chức năng test:**

✅ **Xem danh sách Users**
- Kiểm tra DataTable hiển thị đúng
- Test search, sort, pagination

✅ **Filter Users**
- Filter theo Role (chọn "FrontDesk")
- Filter theo Status (chọn "Đang hoạt động")
- Click "Tìm kiếm"

✅ **Tạo User mới**
- Click "Tạo User"
- Nhập thông tin:
  - Username: `testui`
  - Email: `testui@resort.test`
  - Password: `Test@123`
  - Role: FrontDesk
  - Họ tên: Test UI User
- Click "Lưu"
- **Kết quả:** User mới xuất hiện trong danh sách

✅ **Sửa User**
- Click menu dropdown (3 chấm) ở user vừa tạo
- Click "Sửa"
- Đổi Họ tên → "Updated Test User"
- Click "Lưu"
- **Kết quả:** Tên đã được cập nhật

✅ **Đổi mật khẩu**
- Click menu dropdown
- Click "Đổi mật khẩu"
- Nhập mật khẩu mới: `NewTest@123`
- Xác nhận mật khẩu: `NewTest@123`
- Click "Đổi mật khẩu"
- **Kết quả:** Thông báo thành công

✅ **Khóa User**
- Click menu dropdown
- Click "Khóa"
- Confirm
- **Kết quả:** Badge chuyển sang "Đã khóa" (đỏ)

✅ **Mở khóa User**
- Click menu dropdown
- Click "Mở khóa"
- Confirm
- **Kết quả:** Badge chuyển về "Hoạt động" (xanh)

✅ **Xóa User**
- Click menu dropdown
- Click "Xóa"
- Confirm
- **Kết quả:** User biến mất khỏi danh sách

---

### **2. Test trang Quản lý Employees**

**URL:**
```
http://localhost:5130/admin/html/employees.html
```

**Các chức năng test:**

✅ **Xem thống kê**
- Kiểm tra 4 cards thống kê hiển thị:
  - Tổng NV
  - Đang làm
  - Đã nghỉ
  - Phòng ban

✅ **Filter Employees**
- Filter theo Phòng ban (chọn "Lễ Tân")
- Filter theo Chức vụ
- Filter theo Trạng thái
- Click "Tìm kiếm"

✅ **Thêm nhân viên mới**
- Click "Thêm NV"
- Nhập thông tin:
  - Họ tên: Nguyễn Văn Test
  - Email: nvtest@resort.test
  - Điện thoại: 0901234567
  - Phòng ban: Lễ Tân
  - Chức vụ: Receptionist
  - Lương: 10,000,000
  - Số CMND: 123456789
  - Ngày vào làm: Chọn ngày hôm nay
- Click "Lưu"
- **Kết quả:** Nhân viên mới xuất hiện, thống kê tăng

✅ **Sửa nhân viên**
- Click menu dropdown
- Click "Sửa"
- Đổi thông tin
- Click "Lưu"

✅ **Chấm dứt hợp đồng**
- Click menu dropdown
- Click "Chấm dứt HĐ"
- Nhập lý do: "Test chấm dứt"
- **Kết quả:** 
  - Badge chuyển sang "Đã nghỉ"
  - Thống kê cập nhật
  - Menu đổi thành "Kích hoạt lại"

✅ **Kích hoạt lại**
- Click menu dropdown
- Click "Kích hoạt lại"
- Confirm
- **Kết quả:** Nhân viên active lại

✅ **Xóa nhân viên**
- Click menu dropdown
- Click "Xóa"
- Confirm
- **Kết quả:** Nhân viên bị xóa

---

## 🔍 KIỂM TRA AUDIT LOGS

Sau khi thực hiện các thao tác trên, kiểm tra Audit Logs:

**URL:**
```
http://localhost:5130/swagger
```

Mở `GET /api/Audit/logs` và Execute

**Kết quả mong đợi:** Tất cả thao tác đã được ghi log:
- Create User
- Update User
- ChangePassword
- ChangeRole
- Activate/Deactivate
- Delete
- Create Employee
- Update Employee
- Terminate
- Reactivate
- Delete Employee

Mỗi log có:
- EntityName (User/Employee/Customer)
- EntityId
- Action
- PerformedBy (username của admin)
- OldValues & NewValues (JSON)
- Timestamp
- Description

---

## 🎯 CHECKLIST HOÀN CHỈNH

### **Backend APIs**
- [ ] UserManagement - 10 endpoints hoạt động
- [ ] EmployeeManagement - 11 endpoints hoạt động
- [ ] CustomerManagement - 11 endpoints hoạt động
- [ ] Audit - 8 endpoints hoạt động

### **Frontend UI**
- [ ] Users page - CRUD hoạt động
- [ ] Users page - Change password hoạt động
- [ ] Users page - Toggle active hoạt động
- [ ] Users page - Filters hoạt động
- [ ] Employees page - CRUD hoạt động
- [ ] Employees page - Terminate/Reactivate hoạt động
- [ ] Employees page - Statistics hiển thị đúng

### **Security**
- [ ] JWT authentication hoạt động
- [ ] Role-based access control (Admin only)
- [ ] Unauthorized users bị redirect

### **Audit Trail**
- [ ] Tất cả thao tác được ghi log
- [ ] Old/New values được lưu
- [ ] User activity tracking hoạt động

---

## 🐛 NẾU CÓ LỖI

### **Lỗi 401 Unauthorized**
- Kiểm tra token trong localStorage
- Đăng nhập lại
- Check Bearer token trong Swagger

### **Lỗi 403 Forbidden**
- User không phải Admin/Manager
- Đăng nhập bằng tài khoản admin

### **Lỗi CORS**
- Check CORS settings trong `Program.cs`
- Restart server

### **UI không load data**
- F12 → Console → Xem lỗi
- Check API_BASE URL = `http://localhost:5130/api`
- Check token expired

---

## 📊 KẾT QUẢ MONG ĐỢI

Sau khi test xong, bạn đã verify được:

✅ **40+ APIs hoạt động hoàn hảo**
✅ **2 UI pages admin (Users + Employees) hoạt động mượt**
✅ **Authentication & Authorization chính xác**
✅ **Audit logging ghi đầy đủ mọi thao tác**
✅ **CRUD operations hoạt động**
✅ **Filters & Search hoạt động**
✅ **Statistics & Reporting chính xác**

---

## 🚀 BƯỚC TIẾP THEO

Sau khi test OK, ta có thể:

1. **Tạo 2 trang còn lại:**
   - `customers.html` - Quản lý Khách hàng
   - `audit-logs.html` - Xem Audit Logs

2. **Hoặc deploy lên production** nếu đã OK!

---

**Chúc bạn test thành công!** 🎉

*Cập nhật: 21/10/2025*

