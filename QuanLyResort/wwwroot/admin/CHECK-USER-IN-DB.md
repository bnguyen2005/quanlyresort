# 🔍 KIỂM TRA USER TRONG DATABASE

## ❓ **VẤN ĐỀ:**

Đăng ký tài khoản `phamthahlam@gmail.com` nhưng không thấy trong trang User Management.

---

## ✅ **CÁCH HOẠT ĐỘNG:**

### **Khi đăng ký Customer:**

**1. Endpoint:** `/api/auth/register-customer`

**2. Tạo 2 entities:**

```csharp
// AuthService.RegisterCustomerAsync()

// Bước 1: Tạo Customer
var customer = new Customer {
    FullName = request.FullName,
    Email = request.Email,
    PhoneNumber = request.PhoneNumber,
    CustomerType = "Regular"
};
await _unitOfWork.Customers.AddAsync(customer);
await _unitOfWork.SaveChangesAsync();

// Bước 2: Tạo User với Role = "Customer"
var user = new User {
    Username = username,
    Email = customer.Email,
    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
    Role = "Customer",  ← Quan trọng!
    FullName = customer.FullName,
    PhoneNumber = customer.PhoneNumber,
    CustomerId = customer.CustomerId,
    IsActive = true
};
await _unitOfWork.Users.AddAsync(user);
await _unitOfWork.SaveChangesAsync();
```

**→ User ĐƯỢC TẠO trong database!**

---

## 🔍 **KIỂM TRA:**

### **Cách 1: Vào trang Users**

```
http://localhost:5130/admin/html/users.html

Click nút "Refresh" ← MỚI THÊM!
```

### **Cách 2: Gọi API trực tiếp**

```
http://localhost:5130/swagger

→ UserManagement
→ GET /api/user-management/users
→ Try it out → Execute
```

**Xem response có user `phamthahlam@gmail.com` không?**

### **Cách 3: Check trong Database**

**Mở SQL Server Object Explorer:**

```sql
SELECT * FROM Users 
WHERE Email LIKE '%phamthahlam%'
```

**Phải thấy:**
```
UserId | Username | Email                  | Role     | IsActive
-------|----------|------------------------|----------|----------
XX     | ...      | phamthahlam@gmail.com | Customer | True
```

---

## 🔧 **FIX:**

### **Đã thêm nút Refresh:**

```html
<button type="button" class="btn btn-outline-secondary me-2" onclick="loadUsers()">
  <i class="bx bx-refresh me-1"></i> Refresh
</button>
```

**→ Click để reload data!**

---

## 🧪 **TEST:**

### **Bước 1: Đăng ký mới**

```
http://localhost:5130/customer/register.html

Email: test@example.com
Password: Test@123
Full Name: Test User
...
```

### **Bước 2: Vào trang Users**

```
http://localhost:5130/admin/html/users.html
```

### **Bước 3: Click "Refresh"**

**✅ Phải thấy user mới trong danh sách!**

---

## ❓ **NẾU VẪN KHÔNG THẤY:**

### **Debug Steps:**

**1. Mở Console (F12):**
```
Click "Refresh"
→ Xem có error không?
→ Xem request /api/user-management/users
→ Xem response data
```

**2. Check Network:**
```
DevTools → Network tab
Click "Refresh"
→ Tìm request: user-management/users
→ Status: 200?
→ Response Preview: Có user không?
```

**3. Check API trực tiếp:**
```
http://localhost:5130/swagger

GET /api/user-management/users
→ Execute
→ Xem response body
```

**4. Check Database:**
```sql
SELECT 
    UserId, 
    Username, 
    Email, 
    Role, 
    FullName,
    CustomerId,
    IsActive,
    CreatedAt
FROM Users
ORDER BY CreatedAt DESC
```

**Xem user mới nhất có không?**

---

## 💡 **POSSIBLE ISSUES:**

### **Issue 1: DataTable Cache**

**Solution:**
```javascript
// Click nút Refresh sẽ:
1. Destroy DataTable
2. Fetch fresh data
3. Rebuild table
```

### **Issue 2: Registration Failed**

**Check:**
```
1. Registration có hiển thị "success" message?
2. Check Console có error?
3. Check Network request status?
```

**Nếu failed:**
- Email already exists?
- Validation error?
- Server error?

### **Issue 3: Role Filter**

**Check:**
```javascript
// Frontend có filter role không?
// Xem code loadUsers() function
```

**Không có filter** → Hiển thị TẤT CẢ users bao gồm Customers

---

## 📊 **EXPECTED BEHAVIOR:**

### **✅ Sau khi đăng ký:**

1. **Customer table:**
   ```
   CustomerId | FullName | Email | CustomerType
   -----------|----------|-------|-------------
   XX         | ...      | phamthahlam@... | Regular
   ```

2. **Users table:**
   ```
   UserId | Email | Role | CustomerId | IsActive
   -------|-------|------|------------|----------
   XX     | phamthahlam@... | Customer | XX | True
   ```

3. **Admin Users page:**
   ```
   ✅ Hiển thị trong danh sách
   Role: Customer (badge primary)
   Status: Hoạt động (badge success)
   ```

---

## 🎯 **QUICK FIX:**

### **Vào ngay trang Users:**

```
http://localhost:5130/admin/html/users.html
```

### **Click nút "Refresh"** (góc phải)

### **Scroll xuống table**

### **Search "phamthahlam"** trong search box

**→ Phải thấy user!**

---

## 📞 **NẾU VẪN KHÔNG THẤY:**

**Gửi cho tôi:**

1. **Console logs** khi click Refresh
2. **Network request** response
3. **SQL query** result từ database

**Để debug tiếp!**

---

*Created: 21/10/2025*
*Issue: User không hiển thị sau register*
*Fix: Thêm nút Refresh + Check database*

