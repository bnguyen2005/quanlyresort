# 🏖️ Hướng Dẫn Sử Dụng Resort Management System

## 🎯 **BẮT ĐẦU NHANH**

### 1️⃣ **Truy cập Portal (KHUYẾN NGHỊ)**
```
http://localhost:5130/portal.html
```
Đây là điểm bắt đầu TỐT NHẤT - giao diện đẹp để chọn giữa Admin và Customer portal.

### 2️⃣ **Chọn Portal của bạn**
- **👨‍💼 Admin Portal**: Dành cho quản trị viên và nhân viên
- **👤 Customer Portal**: Dành cho khách hàng

### 3️⃣ **Đăng nhập**
Nhập email/username và mật khẩu:

**Admin:**
- Email: `admin@resort.test` hoặc Username: `admin`
- Password: `P@ssw0rd123`

**Customer:**
- Email: `customer1@guest.test` hoặc Username: `customer1`
- Password: `Guest@123`

---

## ✨ **TÍNH NĂNG MỚI**

### 🚫 **KHÔNG CẦN Ctrl+Shift+R nữa!**

Chúng tôi đã giải quyết vấn đề cache bằng cách:

✅ **Cache Control Headers** - Trang login tự động không lưu cache  
✅ **Portal Page** - UI đẹp, phân loại rõ ràng  
✅ **Query Parameters** - Tự động detect portal type  
✅ **Dynamic Styling** - Info box thay đổi màu theo portal  

---

## 📱 **CÁC TRANG CHÍNH**

| Trang | URL | Mô tả |
|-------|-----|-------|
| **Portal** ⭐ | `/portal.html` | Chọn Admin/Customer portal |
| Welcome | `/welcome.html` | Trang hướng dẫn tổng quan |
| Login | `/customer/login.html` | Trang đăng nhập chính |
| Customer Home | `/customer/index.html` | Trang chủ khách hàng |
| Admin Dashboard | `/admin/html/index.html` | Trang quản trị |
| API Docs | `/swagger` | Swagger API documentation |

---

## 🔐 **THÔNG TIN ĐĂNG NHẬP ĐẦY ĐỦ**

### 👨‍💼 **Admin & Staff**

| Role | Email | Username | Password |
|------|-------|----------|----------|
| Admin | admin@resort.test | admin | P@ssw0rd123 |
| Business | business@resort.test | business | P@ssw0rd123 |
| FrontDesk | frontdesk@resort.test | frontdesk | P@ssw0rd123 |
| Cashier | cashier@resort.test | cashier | P@ssw0rd123 |
| Accounting | accounting@resort.test | accounting | P@ssw0rd123 |
| Inventory | inventory@resort.test | inventory | P@ssw0rd123 |
| Manager | manager@resort.test | manager | P@ssw0rd123 |

### 👤 **Customer**

| Email | Username | Password |
|-------|----------|----------|
| customer1@guest.test | customer1 | Guest@123 |

---

## 🎨 **ƯU ĐIỂM PORTAL PAGE**

### **Trước đây:**
- ❌ Một trang login cho cả admin và customer
- ❌ Không rõ ràng ai đăng nhập gì
- ❌ Phải Ctrl+Shift+R mỗi lần quay lại
- ❌ Cache gây vấn đề

### **Bây giờ:**
- ✅ Portal page đẹp mắt, chuyên nghiệp
- ✅ Phân loại rõ ràng: Admin vs Customer
- ✅ Tự động detect portal từ URL
- ✅ KHÔNG cần Ctrl+Shift+R
- ✅ Cache control tự động
- ✅ Info box màu khác nhau theo portal
- ✅ UX/UI mượt mà

---

## 🔄 **LUỒNG ĐĂNG NHẬP**

```
1. http://localhost:5130 
   └─> Tự động redirect đến /portal.html

2. /portal.html 
   ├─> Click "Admin Portal" 
   │   └─> /customer/login.html?portal=admin (màu xanh dương)
   │       └─> Đăng nhập admin → /admin/html/index.html
   │
   └─> Click "Customer Portal"
       └─> /customer/login.html?portal=customer (màu hồng)
           └─> Đăng nhập customer → /customer/index.html
```

---

## 🛠️ **KỸ THUẬT ĐÃ ÁP DỤNG**

### **1. Cache Control Headers**
```html
<meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate">
<meta http-equiv="Pragma" content="no-cache">
<meta http-equiv="Expires" content="0">
```

### **2. URL Query Parameters**
```javascript
const urlParams = new URLSearchParams(window.location.search);
const portal = urlParams.get('portal'); // 'admin' or 'customer'
```

### **3. Dynamic UI Customization**
```javascript
if (portal === 'admin') {
  // Màu xanh dương, thông tin admin
} else if (portal === 'customer') {
  // Màu hồng, thông tin customer
}
```

### **4. Universal Login Logic**
```javascript
// Thử customer login trước
try { await customerLogin(); } catch {}
// Nếu thất bại, thử admin login
try { await adminLogin(); } catch {}
```

---

## 🚀 **QUICK START**

1. **Mở trình duyệt**
2. **Truy cập**: `http://localhost:5130`
3. **Tự động redirect** đến Portal page
4. **Chọn portal** (Admin hoặc Customer)
5. **Đăng nhập** với thông tin bên trên
6. **Tự động redirect** đến dashboard tương ứng

---

## 💡 **TIPS & TRICKS**

✅ Sử dụng `/portal.html` làm bookmark để truy cập nhanh  
✅ Không cần xóa cache hoặc hard reload  
✅ Có thể đăng nhập bằng email HOẶC username  
✅ Token JWT có thời hạn 24 giờ  
✅ Xem console logs (F12) để debug nếu cần  

---

## 📞 **HỖ TRỢ**

Nếu gặp vấn đề:
1. Mở Console (F12) và copy toàn bộ logs
2. Kiểm tra server có đang chạy: `netstat -ano | findstr :5130`
3. Seed data nếu thiếu: POST request đến `/api/admin/seed`
4. Thử Incognito mode nếu vẫn có vấn đề cache

---

## 🎉 **HOÀN TẤT!**

Hệ thống đã sẵn sàng với:
- ✅ Portal page đẹp mắt
- ✅ Login không bị cache
- ✅ UX/UI mượt mà
- ✅ Phân loại rõ ràng
- ✅ Code clean, dễ maintain

**Chúc bạn sử dụng vui vẻ!** 🏖️

---

*Cập nhật: 21/10/2025*

