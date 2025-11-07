# 🔍 HƯỚNG DẪN DEBUG: KHẮC PHỤC LỖI KHÔNG CÓ DỮ LIỆU

## 🚨 Vấn đề: Trang rooms.html không hiển thị dữ liệu

### ✅ **Đã kiểm tra và khắc phục:**

1. **✅ CORS Configuration** - Đã cấu hình đúng trong Program.cs
2. **✅ API Endpoints** - Server đang chạy và API hoạt động bình thường
3. **✅ Server Status** - Server đang chạy trên cổng 5130
4. **✅ Network Issues** - Không có vấn đề mạng
5. **✅ Error Handling** - Đã thêm logging chi tiết

---

## 🛠️ **Các bước debug đã thực hiện:**

### **1. Kiểm tra Server Status**
```bash
# Server đang chạy trên cổng 5130
netstat -an | findstr :5130
# Kết quả: TCP 127.0.0.1:5130 LISTENING
```

### **2. Kiểm tra API Response**
```bash
# API trả về dữ liệu bình thường
Invoke-WebRequest -Uri "http://localhost:5130/api/rooms" -Method GET
# Kết quả: StatusCode 200, có dữ liệu JSON
```

### **3. Thêm Debug Logging**
- ✅ Thêm console.log vào tất cả hàm API calls
- ✅ Log chi tiết request/response
- ✅ Log authentication status
- ✅ Log error messages

---

## 🔧 **Các file đã cập nhật:**

### **1. rooms.html** - Enhanced với debug logging
- ✅ Thêm logging cho `loadRooms()`
- ✅ Thêm logging cho `loadRoomTypes()`
- ✅ Thêm logging cho `loadStatistics()`
- ✅ Thêm logging cho `initRoomPage()`
- ✅ Thêm logging cho `DOMContentLoaded`

### **2. debug-rooms-connection.html** - Tool debug toàn diện
- ✅ Kiểm tra server status
- ✅ Test API endpoints
- ✅ Test CORS configuration
- ✅ Test authentication
- ✅ Test rooms data
- ✅ Network issues detection
- ✅ Quick fixes suggestions

---

## 🚀 **Cách sử dụng debug tools:**

### **1. Truy cập trang debug:**
```
URL: http://localhost:5130/debug-rooms-connection.html
```

### **2. Mở Console trong Browser:**
```
F12 -> Console tab
```

### **3. Truy cập trang rooms với debug:**
```
URL: http://localhost:5130/admin/html/rooms.html
```

---

## 🔍 **Các lỗi có thể gặp và cách khắc phục:**

### **1. Lỗi Authentication (401 Unauthorized)**
**Triệu chứng:**
- Console log: "❌ [loadStatistics] Response error: 401"
- Statistics cards hiển thị "0"

**Khắc phục:**
```javascript
// Kiểm tra token trong localStorage
console.log('Token:', localStorage.getItem('token'));
console.log('User:', localStorage.getItem('user'));

// Nếu không có token, đăng nhập lại
window.location.href = '/customer/login.html';
```

### **2. Lỗi CORS (Cross-Origin)**
**Triệu chứng:**
- Console log: "CORS error" hoặc "blocked by CORS policy"
- Network tab hiển thị CORS error

**Khắc phục:**
- Kiểm tra CORS configuration trong Program.cs
- Đảm bảo frontend và backend cùng origin

### **3. Lỗi Network (Connection refused)**
**Triệu chứng:**
- Console log: "Failed to fetch" hoặc "Connection refused"
- Network tab hiển thị red requests

**Khắc phục:**
```bash
# Khởi động lại server
dotnet run --urls "http://localhost:5130"
```

### **4. Lỗi Data Format**
**Triệu chứng:**
- Console log: "Unexpected token" hoặc JSON parse error
- API trả về HTML thay vì JSON

**Khắc phục:**
- Kiểm tra API endpoint có đúng không
- Kiểm tra Content-Type header

---

## 📋 **Checklist Debug:**

### **Backend:**
- [ ] Server đang chạy trên cổng 5130
- [ ] API `/api/rooms` trả về StatusCode 200
- [ ] API `/api/room-types` trả về StatusCode 200
- [ ] API `/api/rooms/statistics` trả về StatusCode 200 (cần auth)
- [ ] CORS được cấu hình đúng
- [ ] Database có dữ liệu mẫu

### **Frontend:**
- [ ] Trang load không có JavaScript errors
- [ ] Console hiển thị debug logs
- [ ] localStorage có token và user data
- [ ] API calls được thực hiện đúng URL
- [ ] Response được parse thành công

### **Authentication:**
- [ ] User đã đăng nhập với role Admin/Manager
- [ ] Token được lưu trong localStorage
- [ ] Token được gửi trong Authorization header
- [ ] Token chưa hết hạn

---

## 🎯 **Quick Fixes:**

### **1. Nếu không có dữ liệu:**
```javascript
// Mở console và chạy:
localStorage.clear();
window.location.href = '/customer/login.html';
// Đăng nhập lại với admin@resort.test / P@ssw0rd123
```

### **2. Nếu API không hoạt động:**
```bash
# Khởi động lại server
cd "D:\Lam\QuanLyResort-main (1)\QuanLyResort-main\QuanLyResort"
dotnet run --urls "http://localhost:5130"
```

### **3. Nếu có lỗi CORS:**
```csharp
// Trong Program.cs, đảm bảo có:
app.UseCors("LocalDevAllow");
```

### **4. Nếu không có dữ liệu mẫu:**
```csharp
// Trong Program.cs, đảm bảo DataSeeder chạy:
var seeder = new DataSeeder(context);
await seeder.SeedAsync();
```

---

## 📊 **Expected Results:**

### **Console Logs (Success):**
```
🚀 [DOMContentLoaded] Starting room page initialization...
🔵 [initRoomPage] Starting...
✅ [initRoomPage] User authenticated: Admin
🔵 [loadRoomTypes] Starting...
✅ [loadRoomTypes] Room types loaded: 4 types
🔵 [loadRooms] Starting...
✅ [loadRooms] Rooms loaded: 5 rooms
🔵 [loadStatistics] Starting...
✅ [loadStatistics] Statistics loaded: {totalRooms: 5, ...}
```

### **Page Display (Success):**
- Statistics cards hiển thị số liệu thực
- DataTable hiển thị 5 phòng
- Filter dropdowns có dữ liệu
- Không có error messages

---

## 🆘 **Nếu vẫn không hoạt động:**

### **1. Kiểm tra Browser Console:**
- Mở F12 -> Console
- Tìm các error messages
- Copy error messages để debug

### **2. Kiểm tra Network Tab:**
- Mở F12 -> Network
- Reload trang
- Kiểm tra các API calls
- Xem response của từng request

### **3. Kiểm tra Application Tab:**
- Mở F12 -> Application
- Local Storage -> localhost:5130
- Kiểm tra token và user data

### **4. Sử dụng Debug Tool:**
- Truy cập `/debug-rooms-connection.html`
- Chạy tất cả tests
- Xem kết quả chi tiết

---

## 📞 **Support:**

Nếu vẫn gặp vấn đề, hãy cung cấp:
1. **Console logs** từ browser
2. **Network requests** từ Network tab
3. **Server logs** từ terminal
4. **Screenshot** của trang lỗi

---

**🎉 Sau khi debug thành công, bạn sẽ thấy trang rooms.html hiển thị đầy đủ dữ liệu!**

*Generated: 26/10/2025*  
*Status: ✅ DEBUG TOOLS READY*
