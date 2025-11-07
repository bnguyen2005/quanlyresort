# 🏨 HƯỚNG DẪN SỬ DỤNG: QUẢN LÝ PHÒNG

## 📋 Tổng quan

Trang **Quản lý Phòng** (`rooms.html`) cung cấp đầy đủ tính năng để quản lý các phòng trong hệ thống resort, bao gồm:

- ✅ **Xem danh sách phòng** với thông tin chi tiết
- ✅ **Thống kê phòng** theo trạng thái
- ✅ **Tạo/sửa/xóa phòng** với validation đầy đủ
- ✅ **Cập nhật trạng thái phòng** và housekeeping
- ✅ **Lọc và tìm kiếm** phòng theo nhiều tiêu chí
- ✅ **Tích hợp với RoomTypes** để quản lý loại phòng

---

## 🚀 Cách truy cập

### **1. Khởi động server:**
```bash
cd "D:\Lam\QuanLyResort-main (1)\QuanLyResort-main\QuanLyResort"
dotnet run --urls "http://localhost:5130"
```

### **2. Đăng nhập:**
```
URL: http://localhost:5130/customer/login.html
Email: admin@resort.test
Password: P@ssw0rd123
```

### **3. Truy cập trang quản lý phòng:**
```
URL: http://localhost:5130/admin/html/rooms.html
```

---

## 🎯 Tính năng chính

### **📊 Thống kê phòng**
- **Tổng phòng**: Tổng số phòng trong hệ thống
- **Sẵn sàng**: Phòng có thể đặt ngay
- **Đang dùng**: Phòng đang có khách
- **Bảo trì**: Phòng đang được bảo trì

### **🔍 Lọc và tìm kiếm**
- **Lọc theo loại phòng**: Standard, Deluxe, Suite, Villa
- **Lọc theo tầng**: 1, 2, 3, Garden
- **Lọc theo trạng thái**: Sẵn sàng, Đang dùng
- **Tìm kiếm**: Theo số phòng, mô tả

### **➕ Thêm phòng mới**
1. Click nút **"Thêm Phòng mới"**
2. Điền thông tin:
   - **Số phòng** (bắt buộc): VD: 101, 102, 201...
   - **Loại phòng** (bắt buộc): Chọn từ dropdown
   - **Tầng**: VD: 1, 2, 3, Garden
   - **Giá mỗi đêm** (bắt buộc): VNĐ
   - **Sức chứa tối đa** (bắt buộc): Số người
   - **Mô tả**: Mô tả chi tiết phòng
   - **Tiện nghi**: Phân cách bởi dấu phẩy
   - **Trạng thái**: Sẵn sàng/Đang dùng
   - **Housekeeping Status**: Clean/Dirty/InProgress/Ready/Maintenance
   - **Ghi chú**: Ghi chú thêm
3. Click **"Lưu"**

### **✏️ Sửa phòng**
1. Click nút **"⋮"** trong hàng phòng cần sửa
2. Chọn **"Sửa"**
3. Cập nhật thông tin cần thiết
4. Click **"Lưu"**

### **🔄 Cập nhật trạng thái phòng**
1. Click nút **"⋮"** trong hàng phòng
2. Chọn **"Cập nhật trạng thái"**
3. Thay đổi:
   - **Trạng thái**: Sẵn sàng/Đang dùng
   - **Housekeeping Status**: Clean/Dirty/InProgress/Ready/Maintenance
   - **Ghi chú**: Ghi chú về thay đổi
4. Click **"Cập nhật"**

### **🗑️ Xóa phòng**
1. Click nút **"⋮"** trong hàng phòng
2. Chọn **"Xóa"**
3. Xác nhận xóa

**⚠️ Lưu ý**: Không thể xóa phòng đang có booking active!

---

## 🎨 Giao diện

### **Thiết kế Apple-style**
- **Màu sắc**: Trắng, xám nhẹ, xanh dương accent
- **Typography**: Font hệ thống, dễ đọc
- **Layout**: Card-based, responsive
- **Icons**: Boxicons, đơn giản và rõ ràng

### **Responsive Design**
- **Desktop**: Table view với đầy đủ thông tin
- **Mobile**: Card view, tối ưu cho touch
- **Tablet**: Hybrid layout

---

## 🔧 Cấu hình kỹ thuật

### **API Endpoints sử dụng:**
```
GET    /api/rooms                    - Lấy danh sách phòng
GET    /api/rooms/{id}               - Chi tiết phòng
POST   /api/rooms                    - Tạo phòng mới
PUT    /api/rooms/{id}               - Cập nhật phòng
PATCH  /api/rooms/{id}/status        - Cập nhật trạng thái
DELETE /api/rooms/{id}               - Xóa phòng
GET    /api/rooms/statistics         - Thống kê phòng
GET    /api/room-types               - Danh sách loại phòng
```

### **Authentication:**
- **JWT Token**: Lưu trong localStorage
- **Role-based Access**: Admin, Manager mới có quyền chỉnh sửa
- **Auto-redirect**: Chuyển về login nếu hết hạn

### **Validation:**
- **Frontend**: HTML5 validation + custom validation
- **Backend**: Model validation + business rules
- **Error Handling**: User-friendly error messages

---

## 📱 Tính năng nâng cao

### **Real-time Updates**
- **Auto-refresh**: Tự động cập nhật khi có thay đổi
- **Live Statistics**: Thống kê cập nhật real-time
- **Status Sync**: Đồng bộ trạng thái với backend

### **Data Export**
- **CSV Export**: Xuất danh sách phòng
- **PDF Report**: Báo cáo thống kê phòng
- **Print View**: Chế độ in thân thiện

### **Bulk Operations**
- **Multi-select**: Chọn nhiều phòng cùng lúc
- **Bulk Status Update**: Cập nhật trạng thái hàng loạt
- **Bulk Delete**: Xóa nhiều phòng (có điều kiện)

---

## 🐛 Troubleshooting

### **Lỗi thường gặp:**

#### **1. "Không thể tải danh sách phòng"**
- ✅ Kiểm tra server có chạy không
- ✅ Kiểm tra kết nối internet
- ✅ Kiểm tra console browser (F12)

#### **2. "Phiên đăng nhập đã hết hạn"**
- ✅ Đăng nhập lại
- ✅ Kiểm tra token trong localStorage

#### **3. "Bạn không có quyền truy cập"**
- ✅ Đăng nhập với tài khoản Admin/Manager
- ✅ Kiểm tra role trong localStorage

#### **4. "Không thể xóa phòng"**
- ✅ Kiểm tra phòng có booking active không
- ✅ Hủy hoặc hoàn thành booking trước

### **Debug Mode:**
```javascript
// Mở console (F12) và chạy:
console.log('Current user:', JSON.parse(localStorage.getItem('user')));
console.log('Token:', localStorage.getItem('token'));
```

---

## 📈 Performance

### **Optimization:**
- **Lazy Loading**: Tải dữ liệu theo trang
- **Caching**: Cache API responses
- **Debouncing**: Giảm số lần gọi API khi search
- **Virtual Scrolling**: Hiển thị nhiều dữ liệu mượt mà

### **Monitoring:**
- **API Response Time**: Theo dõi thời gian phản hồi
- **Error Rate**: Tỷ lệ lỗi API
- **User Actions**: Tracking hành động người dùng

---

## 🔮 Roadmap

### **Tính năng sắp tới:**
- 🎯 **Room Calendar**: Lịch đặt phòng trực quan
- 🎯 **Room Photos**: Upload và quản lý hình ảnh phòng
- 🎯 **Room Pricing**: Quản lý giá theo mùa/ngày
- 🎯 **Room Maintenance**: Lịch bảo trì phòng
- 🎯 **Room Analytics**: Phân tích hiệu suất phòng

### **Integration:**
- 🔗 **Booking System**: Tích hợp với hệ thống đặt phòng
- 🔗 **Housekeeping App**: App cho nhân viên dọn phòng
- 🔗 **Guest Services**: Dịch vụ khách hàng
- 🔗 **Revenue Management**: Quản lý doanh thu

---

## 📞 Hỗ trợ

### **Liên hệ:**
- **Email**: support@resort.test
- **Phone**: 1900-xxxx
- **Documentation**: `/docs/api/rooms`

### **Training:**
- **Video Tutorial**: `/training/rooms-management`
- **User Manual**: `/docs/user-manual.pdf`
- **FAQ**: `/docs/faq.html`

---

**🎉 Chúc bạn sử dụng hệ thống quản lý phòng hiệu quả!**

*Generated: 21/10/2025*  
*Version: 1.0.0*  
*Status: ✅ READY FOR PRODUCTION*
