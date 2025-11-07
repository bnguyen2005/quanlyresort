# 🔧 Hướng Dẫn Fix Cache Issue

## ❓ Vấn Đề
- Trang login bị cache
- Cần Ctrl+Shift+R mỗi lần quay lại
- Thay đổi code không thấy update

## ✅ Giải Pháp Đã Áp Dụng

### 1. **Cache Control Headers**
Thêm vào `<head>` của login.html:
```html
<meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate, max-age=0">
<meta http-equiv="Pragma" content="no-cache">
<meta http-equiv="Expires" content="0">
```

### 2. **Dynamic Cache Busting**
JavaScript files được load với timestamp:
```javascript
const timestamp = new Date().getTime();
script.src = `js/navbar-auth.js?v=${timestamp}`;
```

### 3. **Back/Forward Detection**
Tự động reload khi dùng nút Back:
```javascript
if (performance.navigation.type === performance.navigation.TYPE_BACK_FORWARD) {
  window.location.reload(true);
}
```

### 4. **Service Worker Update**
- KHÔNG cache `login.html`
- KHÔNG cache các JS files quan trọng
- Luôn fetch fresh cho trang login

### 5. **Manual Cache Clear**
Link "Xóa cache & reload" ở dưới form login

---

## 🚀 Sử Dụng

### **Cách 1: Normal (Khuyến nghị)**
```
http://localhost:5130/customer/login.html
```
Nhập email/password → Tự động detect role → Redirect

### **Cách 2: Qua Portal**
```
http://localhost:5130/portal.html
```
Chọn Admin hoặc Customer → Login → Redirect

### **Cách 3: Test Page**
```
http://localhost:5130/test-cache.html
```
Kiểm tra cache status & test login

---

## 🔍 Auto-Detect Role

Login page TỰ ĐỘNG phát hiện role dựa vào:

1. **Portal hint** từ URL: `?portal=admin` hoặc `?portal=customer`
2. **Email pattern**:
   - Chứa `admin`, `manager`, `business`, etc. → Thử admin first
   - Khác → Thử customer first
3. **Fallback**: Thử cả 2 nếu cái đầu fail

### Ví dụ:

**Admin login:**
```
Email: admin@resort.test
Password: P@ssw0rd123
→ Tự động detect là Admin → /admin/html/index.html
```

**Customer login:**
```
Email: customer1@guest.test
Password: Guest@123
→ Tự động detect là Customer → /customer/index.html
```

---

## 🧹 Xóa Cache Thủ Công

### **Option 1: Trong trang login**
Click link "Vấn đề cache? Xóa cache & reload" ở dưới form

### **Option 2: Trong test page**
```
http://localhost:5130/test-cache.html
```
Click "Clear All Cache"

### **Option 3: Console**
Mở Console (F12) và chạy:
```javascript
clearAllCache();
```

### **Option 4: Browser DevTools**
1. F12 → Application tab
2. Clear storage → Clear site data

---

## 📊 Kiểm Tra Cache

Mở Console (F12) khi load trang login, bạn sẽ thấy:
```
🚀 ===== LOGIN PAGE LOADED (NEW SIMPLE LOGIC) =====
📍 URL: http://localhost:5130/customer/login.html
⏰ Time: 1:23:45 AM
🔄 Cache timestamp: 1729468425789
✅ navbar-auth.js loaded with cache buster: 1729468425789
```

Nếu thấy logs này → Code mới đã load → Không bị cache ✅

---

## ❌ Nếu Vẫn Có Vấn Đề

### 1. **Clear cache hoàn toàn:**
```
http://localhost:5130/test-cache.html
→ Click "Clear All Cache"
```

### 2. **Dùng Incognito/Private mode:**
- Chrome: Ctrl + Shift + N
- Edge: Ctrl + Shift + P
- Firefox: Ctrl + Shift + P

### 3. **Disable Service Worker:**
- F12 → Application → Service Workers
- Click "Unregister"

### 4. **Hard Reload:**
- Ctrl + Shift + R (Windows)
- Cmd + Shift + R (Mac)

### 5. **Clear browser cache hoàn toàn:**
- Chrome: Ctrl + Shift + Delete
- Chọn "Cached images and files"
- Time range: "All time"
- Clear data

---

## 📝 Console Logs Quan Trọng

Khi login, bạn nên thấy:
```
🎯 ===== LOGIN BUTTON CLICKED =====
📧 Email: admin@resort.test
🔐 ===== UNIVERSAL LOGIN STARTED =====
🔵 Strategy: Try ADMIN first (based on hint)
👨‍💼 Trying admin/staff login...
📡 API Call: http://localhost:5130/api/auth/login
📨 Response status: 200
✅ Admin/staff login successful!
🎉 Login result: {...}
💾 Saved to localStorage
🎯 User role: Admin
🔄 Redirecting to: /admin/html/index.html
🚀 Executing redirect...
```

---

## 🎯 Tóm Tắt

| Vấn đề | Giải pháp |
|--------|-----------|
| ❌ Phải Ctrl+Shift+R | ✅ Cache headers + dynamic busting |
| ❌ Code cũ load | ✅ Timestamp trong script URLs |
| ❌ Service Worker cache | ✅ NEVER_CACHE_URLS list |
| ❌ Back button cache | ✅ Auto reload on back/forward |
| ❌ Không clear được | ✅ clearAllCache() function |

---

## ✨ Kết Quả

- ✅ KHÔNG cần Ctrl+Shift+R
- ✅ Tự động detect Admin/Customer
- ✅ Luôn load code mới nhất
- ✅ Service Worker không cache login
- ✅ Manual clear cache nếu cần
- ✅ Test page để verify

---

*Cập nhật: 21/10/2025*

