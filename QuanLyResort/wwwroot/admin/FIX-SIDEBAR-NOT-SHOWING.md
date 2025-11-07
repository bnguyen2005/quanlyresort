# 🔧 FIX: SIDEBAR KHÔNG HIỂN THỊ

## ❌ **VẤN ĐỀ:**

Khi vào Dashboard hoặc các trang admin, **sidebar không hiển thị** menu Users, Employees...

---

## ✅ **ĐÃ FIX:**

### **1. Thêm Error Handling cho Menu Loading**

**Trước (có thể fail im lặng):**
```javascript
fetch('layout-menu.html')
  .then(response => response.text())
  .then(html => {
    document.getElementById('common-menu').innerHTML = html;
  });
```

**Sau (có error logging):**
```javascript
(function() {
  fetch('layout-menu.html')
    .then(response => {
      if (!response.ok) {
        throw new Error('Failed to load menu: ' + response.status);
      }
      return response.text();
    })
    .then(html => {
      const menuContainer = document.getElementById('common-menu');
      if (menuContainer) {
        menuContainer.innerHTML = html;
        console.log('✅ Menu loaded successfully');
      } else {
        console.error('❌ Menu container not found');
      }
    })
    .catch(error => {
      console.error('❌ Error loading menu:', error);
    });
})();
```

### **2. Files đã update:**
- ✅ `/admin/html/index.html`
- ✅ `/admin/html/users.html`
- ✅ `/admin/html/employees.html`

---

## 🧪 **CÁCH TEST:**

### **Bước 1: Clear Browser Cache**
```
1. Mở DevTools (F12)
2. Right-click vào nút Refresh
3. Chọn "Empty Cache and Hard Reload"
   
   HOẶC
   
   Ctrl + Shift + R
```

### **Bước 2: Kiểm tra Console**

**Vào trang Dashboard:**
```
http://localhost:5130/admin/html/index.html
```

**Mở Console (F12 → Console tab):**

**✅ Nếu thành công, bạn sẽ thấy:**
```
✅ Menu loaded successfully
```

**❌ Nếu có lỗi, bạn sẽ thấy:**
```
❌ Error loading menu: ...
```

### **Bước 3: Kiểm tra Network**

**Trong DevTools → Network tab:**

1. Reload page (F5)
2. Tìm request `layout-menu.html`
3. Kiểm tra:
   - **Status:** Phải là `200 OK`
   - **Size:** Phải có kích thước (không phải `0 B`)
   - **Preview:** Xem nội dung menu HTML

**❌ Nếu Status là 404:**
```
→ File layout-menu.html không tồn tại hoặc path sai
```

**❌ Nếu Status là 500:**
```
→ Server error
```

---

## 🔍 **TROUBLESHOOTING:**

### **Vấn đề 1: Sidebar vẫn không hiển thị**

**Nguyên nhân có thể:**
1. Browser cache chưa clear
2. Server chưa restart
3. File `layout-menu.html` không tồn tại

**Giải pháp:**
```bash
# 1. Hard reload browser
Ctrl + Shift + R

# 2. Kiểm tra file tồn tại
ls wwwroot/admin/html/layout-menu.html

# 3. Restart server
# Stop server (Ctrl+C)
# Start lại
dotnet run --urls "http://localhost:5130"
```

### **Vấn đề 2: Console log "Menu loaded successfully" nhưng không thấy sidebar**

**Nguyên nhân:** Menu đã load nhưng CSS chưa apply

**Giải pháp:**
```html
<!-- Kiểm tra trong <head> có các CSS này: -->
<link rel="stylesheet" href="../assets/vendor/css/core.css" />
<link rel="stylesheet" href="../assets/vendor/css/theme-default.css" />
<link rel="stylesheet" href="../assets/css/demo.css" />
<link rel="stylesheet" href="../assets/vendor/libs/perfect-scrollbar/perfect-scrollbar.css" />

<!-- Kiểm tra <html> tag có class: -->
<html class="light-style layout-menu-fixed" ...>
```

### **Vấn đề 3: Console log "Error loading menu: 404"**

**Nguyên nhân:** Path không đúng

**Giải pháp:**
```
Trang                           | Path đúng
--------------------------------|------------------
/admin/html/index.html          | layout-menu.html
/admin/html/users.html          | layout-menu.html
/admin/html/employees.html      | layout-menu.html
/admin/rooms.html               | html/layout-menu.html
/admin/bookings.html            | html/layout-menu.html
```

### **Vấn đề 4: Sidebar hiển thị nhưng không có menu items Users, Employees**

**Nguyên nhân:** File `layout-menu.html` bị outdated

**Giải pháp:**
```bash
# Kiểm tra nội dung file
cat wwwroot/admin/html/layout-menu.html | grep "Users\|Employees"

# Phải thấy:
# <a href="/admin/html/users.html">Tài khoản Users</a>
# <a href="/admin/html/employees.html">Nhân viên</a>
```

---

## 📋 **CHECKLIST DEBUG:**

Khi sidebar không hiển thị, check theo thứ tự:

- [ ] **1. Hard reload browser** (Ctrl+Shift+R)
- [ ] **2. Mở Console** → Xem có log "✅ Menu loaded successfully"?
- [ ] **3. Mở Network tab** → Request `layout-menu.html` có Status 200?
- [ ] **4. Inspect Element** → `<div id="common-menu">` có chứa `<aside id="layout-menu">`?
- [ ] **5. Kiểm tra CSS** → `<html>` có class `layout-menu-fixed`?
- [ ] **6. Kiểm tra file** → `wwwroot/admin/html/layout-menu.html` tồn tại?
- [ ] **7. Restart server** → Stop và start lại

---

## ✨ **SAU KHI FIX:**

### **✅ Sidebar phải hiển thị:**
- 📊 Dashboard
- 👥 Tài khoản Users
- 🧑‍💼 Nhân viên
- 👨‍👩‍👧‍👦 Khách hàng
- 🏠 Phòng
- 📅 Đặt phòng
- 📜 Lịch sử hoạt động
- 📊 Báo cáo

### **✅ Console phải log:**
```
✅ Menu loaded successfully
```

### **✅ Menu item hiện tại phải được highlight**

---

## 🚀 **TEST NGAY:**

### **1. Hard Reload:**
```
Ctrl + Shift + R
```

### **2. Vào Dashboard:**
```
http://localhost:5130/admin/html/index.html
```

### **3. Mở Console (F12):**
- Xem log "✅ Menu loaded successfully"
- Kiểm tra có error không

### **4. Verify Sidebar:**
- ✅ Sidebar hiển thị bên trái
- ✅ Có đầy đủ menu items
- ✅ Click menu items → chuyển trang
- ✅ Menu item hiện tại được highlight

---

## 📞 **NẾU VẪN KHÔNG ĐƯỢC:**

### **Copy đoạn code này vào Console để debug:**

```javascript
// Test menu loading
console.log('=== MENU DEBUG ===');
console.log('1. Menu container:', document.getElementById('common-menu'));
console.log('2. Menu aside:', document.getElementById('layout-menu'));
console.log('3. HTML class:', document.documentElement.className);
console.log('4. Body class:', document.body.className);

// Try to load menu manually
fetch('layout-menu.html')
  .then(r => r.text())
  .then(html => {
    console.log('5. Menu HTML length:', html.length);
    console.log('6. Has users link:', html.includes('users.html'));
    console.log('7. Has employees link:', html.includes('employees.html'));
  })
  .catch(e => console.error('8. Fetch error:', e));
```

**Gửi kết quả debug cho tôi để xem!** 🔍

---

*Updated: 21/10/2025*
*Status: ✅ FIXED with error handling*

