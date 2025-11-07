# 🧪 HƯỚNG DẪN TEST JWT AUTHORIZATION MIDDLEWARE

## ✅ TEST 1: Admin truy cập admin pages (Nên thành công)

### Bước 1: Đăng nhập với Admin
1. Mở: http://localhost:5130/customer/login.html
2. Đăng nhập với:
   - Email: `admin@resort.test`
   - Password: `Admin@123456`

### Bước 2: Truy cập các trang Admin
- http://localhost:5130/admin/html/index.html (Dashboard)
- http://localhost:5130/admin/html/users.html (Quản lý Users)
- http://localhost:5130/admin/html/employees.html (Quản lý Nhân viên)

### Kết quả mong đợi:
✅ **Tất cả các trang đều load thành công**
✅ **Dữ liệu hiển thị bình thường**
✅ **Server logs hiển thị**: `[Authorization] User: admin (ID: ..., Role: Admin) accessing: /api/...`

---

## ❌ TEST 2: Customer cố truy cập Admin API (Nên bị chặn - 403)

### Bước 1: Tạo tài khoản Customer
1. Logout khỏi admin (nếu đang đăng nhập)
2. Mở: http://localhost:5130/customer/register.html
3. Đăng ký tài khoản mới:
   - Username: `testcustomer`
   - Email: `test@customer.com`
   - Password: `Test@123456`
   - Full Name: `Test Customer`

### Bước 2: Thử truy cập Admin API
1. Sau khi đăng ký thành công, mở **Console** (F12)
2. Chạy lệnh sau trong Console:

```javascript
// Thử gọi API admin với token customer
fetch('http://localhost:5130/api/usermanagement', {
  headers: {
    'Authorization': 'Bearer ' + localStorage.getItem('token')
  }
})
.then(response => {
  console.log('Status:', response.status);
  return response.text();
})
.then(data => {
  console.log('Response:', data);
});
```

### Kết quả mong đợi:
❌ **Status: 403 Forbidden**
❌ **Response: "Forbidden: Insufficient permissions."**
❌ **Server logs hiển thị**: `[Authorization] FORBIDDEN - Customer role attempted to access /api/usermanagement`

---

## 🔒 TEST 3: Không có token (Nên bị chặn - 401)

### Bước 1: Xóa token
1. Mở **Console** (F12)
2. Xóa token:
```javascript
localStorage.removeItem('token');
```

### Bước 2: Thử gọi API
```javascript
// Thử gọi API không có token
fetch('http://localhost:5130/api/usermanagement')
.then(response => {
  console.log('Status:', response.status);
  return response.text();
})
.then(data => {
  console.log('Response:', data);
});
```

### Kết quả mong đợi:
🔒 **Status: 401 Unauthorized**
🔒 **Response: "Unauthorized: No token provided."**
🔒 **Server logs hiển thị**: `[Authorization] No token provided for API path: /api/usermanagement`

---

## 🔑 TEST 4: Token không hợp lệ (Nên bị chặn - 401)

### Bước 1: Đặt token giả
1. Mở **Console** (F12)
2. Đặt token giả:
```javascript
localStorage.setItem('token', 'fake-invalid-token-12345');
```

### Bước 2: Thử gọi API
```javascript
fetch('http://localhost:5130/api/usermanagement', {
  headers: {
    'Authorization': 'Bearer ' + localStorage.getItem('token')
  }
})
.then(response => {
  console.log('Status:', response.status);
  return response.text();
})
.then(data => {
  console.log('Response:', data);
});
```

### Kết quả mong đợi:
🔒 **Status: 401 Unauthorized**
🔒 **Response: "Unauthorized: Invalid token."**
🔒 **Server logs hiển thị**: `[Authorization] Token validation failed.`

---

## 📊 TEST 5: Kiểm tra Logs trong Terminal

Mở terminal đang chạy server và tìm các dòng log:

### Log thành công (Admin):
```
info: QuanLyResort.Middleware.JwtAuthorizationMiddleware[0]
      [Authorization] User: admin (ID: 1, Role: Admin) accessing: /api/usermanagement
```

### Log bị chặn (Customer):
```
warn: QuanLyResort.Middleware.JwtAuthorizationMiddleware[0]
      [Authorization] FORBIDDEN - Customer role attempted to access /api/usermanagement
```

### Log không có token:
```
warn: QuanLyResort.Middleware.JwtAuthorizationMiddleware[0]
      [Authorization] No token provided for API path: /api/usermanagement
```

---

## 🎯 TEST 6: Test với các Role khác nhau

### Manager (Nên có quyền truy cập Admin)
1. Đăng nhập: `manager@resort.test` / `Manager@123456`
2. Truy cập admin pages
3. **Kết quả**: ✅ Thành công

### FrontDesk (Nên có quyền truy cập Admin)
1. Đăng nhập: `frontdesk@resort.test` / `FrontDesk@123456`
2. Truy cập admin pages
3. **Kết quả**: ✅ Thành công

### Customer (KHÔNG có quyền)
1. Đăng nhập với tài khoản customer bất kỳ
2. Thử truy cập admin pages
3. **Kết quả**: ❌ 403 Forbidden

---

## ✅ Checklist Test Hoàn Chỉnh

- [ ] Admin có thể truy cập tất cả trang admin
- [ ] Manager có thể truy cập tất cả trang admin
- [ ] FrontDesk có thể truy cập trang admin
- [ ] Customer KHÔNG thể truy cập admin pages (403)
- [ ] Không có token -> 401 Unauthorized
- [ ] Token không hợp lệ -> 401 Unauthorized
- [ ] Server logs hiển thị đúng thông tin user và action
- [ ] Tất cả API calls đều được middleware kiểm tra

---

## 🔧 Debug Tips

Nếu test không như mong đợi:

1. **Kiểm tra Console (F12)**: Xem error messages
2. **Kiểm tra Network Tab**: Xem response status và headers
3. **Kiểm tra Terminal**: Xem middleware logs
4. **Kiểm tra Token**: 
   ```javascript
   console.log('Token:', localStorage.getItem('token'));
   console.log('User:', JSON.parse(localStorage.getItem('user')));
   ```
5. **Clear Cache**: Ctrl + Shift + R để refresh trang

---

## 📝 Ghi chú

- Middleware chỉ áp dụng cho `/api/*` endpoints (trừ `/api/auth/*`)
- Static files và public pages không bị middleware chặn
- Middleware log mỗi request để dễ debug
- Token được lưu trong `localStorage` với key `'token'`

