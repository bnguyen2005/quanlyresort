# 🧪 TEST MENU MỚI - THỐNG NHẤT

## 🚀 BẮT ĐẦU TEST

### **Bước 1: Đăng nhập**
```
http://localhost:5130/customer/login.html
```
- Email: `admin@resort.test`
- Password: `P@ssw0rd123`

---

### **Bước 2: Test trang Users**
```
http://localhost:5130/admin/html/users.html
```

**Kiểm tra:**
- [ ] ✅ Sidebar hiển thị đầy đủ menu items
- [ ] ✅ "Tài khoản Users" được highlight (active)
- [ ] ✅ Logo "Resort Admin" hiển thị
- [ ] ✅ Navbar hiển thị "👥 Quản lý Users"
- [ ] ✅ User dropdown hiển thị tên + role
- [ ] ✅ Table data load được

**Click các menu items:**
- [ ] Dashboard → chuyển trang
- [ ] Nhân viên → chuyển sang employees.html
- [ ] Khách hàng → (chưa có trang)
- [ ] Phòng → chuyển sang rooms.html
- [ ] Đặt phòng → chuyển sang bookings.html

---

### **Bước 3: Test trang Employees**
```
http://localhost:5130/admin/html/employees.html
```

**Kiểm tra:**
- [ ] ✅ Sidebar **GIỐNG HỆT** trang Users
- [ ] ✅ "Nhân viên" được highlight (active)
- [ ] ✅ Navbar hiển thị "🧑‍💼 Quản lý Nhân viên"
- [ ] ✅ User dropdown hiển thị tên + role  
- [ ] ✅ Statistics cards hiển thị
- [ ] ✅ Table data load được

**Click menu:**
- [ ] Tài khoản Users → chuyển sang users.html
- [ ] Các menu khác hoạt động

---

### **Bước 4: So sánh với trang Rooms (cũ)**
```
http://localhost:5130/admin/html/../rooms.html
```

**So sánh:**
- ⚠️ Sidebar **KHÁC** với Users/Employees
- ⚠️ Menu items khác hoàn toàn
- ⚠️ Style có thể khác

**→ ĐÂY LÀ VẤN ĐỀ CẦN FIX!**

---

## ✅ KẾT QUẢ MONG ĐỢI

### **Trang Users:**
```
✅ Sidebar thống nhất
✅ Menu highlight đúng (Users active)
✅ Navbar consistent
✅ Data load được
```

### **Trang Employees:**
```
✅ Sidebar GIỐNG trang Users
✅ Menu highlight đúng (Employees active)
✅ Navbar consistent
✅ Data load được
```

### **Navigation:**
```
✅ Click Users → chuyển Users (menu vẫn giống)
✅ Click Employees → chuyển Employees (menu vẫn giống)
✅ Click Rooms → chuyển Rooms (⚠️ menu khác - chưa fix)
```

---

## 🐛 NẾU CÓ VẤN ĐỀ

### **Sidebar không hiển thị:**
- Check console: `fetch('layout-menu.html')` error?
- Đảm bảo file `layout-menu.html` tồn tại
- Check path đúng (`html/layout-menu.html`)

### **Menu không highlight:**
- Check `data-page` attribute
- Check script auto-highlight đã chạy

### **Data không load:**
- Check API endpoint
- Check token trong localStorage
- Check console errors

---

## 📊 CHECKLIST HOÀN CHỈNH

### **Sidebar:**
- [ ] Logo hiển thị
- [ ] Menu items đầy đủ (8 items)
- [ ] Active page được highlight
- [ ] Icons hiển thị đúng
- [ ] Text rõ ràng, dễ đọc

### **Navbar:**
- [ ] Page title đúng
- [ ] User avatar hiển thị
- [ ] Dropdown mở được
- [ ] Tên user hiển thị
- [ ] Role hiển thị (tiếng Việt)
- [ ] Logout hoạt động

### **Navigation:**
- [ ] Click menu → chuyển trang
- [ ] URL đúng
- [ ] Menu vẫn giống nhau
- [ ] Active highlight thay đổi

### **Consistency:**
- [ ] Users & Employees menu giống 100%
- [ ] Layout giống nhau
- [ ] Style thống nhất
- [ ] UX mượt mà

---

## 🎯 TIÊU CHÍ THÀNH CÔNG

✅ **Menu thống nhất** trên users.html & employees.html  
✅ **Navigation mượt mà** giữa các trang  
✅ **Active highlight** chính xác  
✅ **User info** hiển thị đầy đủ  
✅ **No console errors**  
✅ **No broken links**

---

## 🚧 NEXT STEPS

Sau khi test OK menu mới:

1. **Apply cho các trang còn lại:**
   - rooms.html
   - bookings.html  
   - index.html

2. **Tạo trang mới với menu thống nhất:**
   - customers.html
   - audit-logs.html

3. **Cleanup:**
   - Remove old menu code
   - Optimize performance

---

**Test ngay để verify menu đã thống nhất!** 🎉

*Hướng dẫn: 21/10/2025*

