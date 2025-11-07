# 🔄 HƯỚNG DẪN REFRESH DANH SÁCH USERS

## ❓ **VẤN ĐỀ:**

Đăng ký tài khoản `phamthahlam@gmail.com` nhưng không thấy trong trang User Management.

---

## ✅ **GIẢI PHÁP:**

### **Trang Users ĐÃ CÓ nút refresh!**

```
http://localhost:5130/admin/html/users.html

→ Ở phần Filter
→ Có 3 dropdown: Role, Active Status, Search
→ Bên phải có nút "🔍 Tìm kiếm" ← CLICK ĐÂY!
```

**Nút "Tìm kiếm" sẽ:**
1. Call API `/api/user-management/users`
2. Lấy data mới nhất
3. Rebuild table

---

## 🧪 **TEST NGAY:**

### **Bước 1: Vào trang Users**

```
http://localhost:5130/admin/html/users.html
```

### **Bước 2: Click "Tìm kiếm"**

**Ở phần Filter, góc phải, nút màu xanh primary**

### **Bước 3: Check table**

**Scroll xuống xem danh sách users**

### **Bước 4: Dùng Search**

**DataTables có search box ở góc phải trên table**

```
Gõ: phamthahlam
```

**→ Phải thấy user!**

---

## 🔍 **DEBUG NẾU KHÔNG THẤY:**

### **Step 1: Check Console**

```
F12 → Console tab

Click nút "Tìm kiếm"

Xem có error không?
```

### **Step 2: Check Network**

```
F12 → Network tab

Click "Tìm kiếm"

Tìm request: user-management/users
→ Status: 200?
→ Preview: Có user phamthahlam không?
```

### **Step 3: Check Filters**

**Xóa tất cả filters:**

```
Role: (Tất cả)
Active Status: (Tất cả)
Search: (empty)

→ Click "Tìm kiếm"
```

### **Step 4: Check API trực tiếp**

```
http://localhost:5130/swagger

→ UserManagement → GET /api/user-management/users
→ Try it out
→ Execute

Xem response có email phamthahlam không?
```

### **Step 5: Check Database**

**Mở Server Explorer trong Visual Studio:**

```sql
SELECT * FROM Users 
WHERE Email = 'phamthahlam@gmail.com'
```

**Hoặc:**

```sql
SELECT TOP 10 
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

**Xem user mới nhất**

---

## 📊 **EXPECTED DATA:**

### **Trong Database:**

```
Users table:
UserId | Username        | Email                  | Role     | CustomerId | IsActive
-------|-----------------|------------------------|----------|------------|----------
XX     | phamthahlam     | phamthahlam@gmail.com | Customer | YY         | 1

Customers table:
CustomerId | FullName        | Email                  | CustomerType
-----------|-----------------|------------------------|-------------
YY         | Pham Thai Lam   | phamthahlam@gmail.com | Regular
```

### **Trong Admin UI:**

```
Danh sách Users:
ID | Username      | Email                  | Họ tên         | Role     | Trạng thái
---|---------------|------------------------|----------------|----------|------------
XX | phamthahlam   | phamthahlam@gmail.com | Pham Thai Lam  | Customer | Hoạt động
```

---

## 💡 **GHI CHÚ:**

### **Khi đăng ký Customer:**

1. **Tạo Customer entity** trong bảng `Customers`
2. **Tạo User entity** trong bảng `Users` với:
   - `Role = "Customer"`
   - `CustomerId` link đến Customer
   - `IsActive = true`

### **Trang User Management:**

- Hiển thị **TẤT CẢ users** bao gồm:
  - Admin
  - Manager
  - Staff (FrontDesk, Cashier, etc.)
  - **Customers** ← Bao gồm user đăng ký

### **DataTable Features:**

- **Search:** Tìm theo bất kỳ field nào
- **Sort:** Click header để sắp xếp
- **Pagination:** 10 items/page mặc định
- **Filters:** Role, Active Status

---

## 🎯 **QUICK CHECKLIST:**

- [ ] Vào trang Users
- [ ] Clear tất cả filters (chọn "Tất cả")
- [ ] Click nút "🔍 Tìm kiếm"
- [ ] Mở Console (F12) xem có error?
- [ ] Check Network xem API response
- [ ] Dùng DataTables Search box gõ "phamthahlam"
- [ ] Nếu vẫn không thấy → Check database SQL

---

## 📞 **NẾU CẦN HỖ TRỢ:**

**Gửi cho tôi:**

1. **Screenshot** trang Users
2. **Console logs** (F12 → Console)
3. **Network response** (F12 → Network → user-management/users)
4. **SQL query result** từ database

**→ Tôi sẽ debug tiếp!**

---

*Created: 21/10/2025*
*Issue: User không hiển thị*
*Solution: Click nút "Tìm kiếm" để refresh data*

