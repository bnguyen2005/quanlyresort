# Hệ Thống Quản Lý Resort - Admin Dashboard

## Tổng Quan

Hệ thống quản lý resort đã được tích hợp hoàn chỉnh với giao diện admin sử dụng template **Sneat Bootstrap 5** (phiên bản miễn phí).

## Các Trang Đã Được Tạo

### 1. **Dashboard (index.html)** ✅
- Tổng quan hệ thống
- Thống kê doanh thu, đặt phòng
- Trạng thái phòng (Trống, Đang sử dụng, Bảo trì, Không khả dụng)
- Danh sách đặt phòng gần đây
- Cảnh báo hệ thống
- Kho sắp hết

### 2. **Quản Lý Phòng (rooms.html)** ✅
- Xem danh sách tất cả phòng
- Lọc theo trạng thái và loại phòng
- Tìm kiếm phòng
- Thêm phòng mới
- Chỉnh sửa thông tin phòng
- Xóa phòng

### 3. **Quản Lý Đặt Phòng (bookings.html)** ✅
- Xem danh sách đặt phòng
- Lọc theo trạng thái và ngày
- Tìm kiếm đặt phòng
- Xem chi tiết đặt phòng
- Xác nhận đặt phòng (phân phòng)
- Check-in
- Check-out
- Hủy đặt phòng

### 4. **Đăng Nhập Admin (html/auth-login-basic.html)** ✅
- Form đăng nhập với validation
- Toggle hiển thị mật khẩu
- Thông báo lỗi
- Loading state
- Remember me checkbox

## Các File JavaScript

### 1. **js/api.js** - API Helper
Chứa tất cả các hàm gọi API:
- Auth API (adminLogin, logout)
- Rooms API (CRUD operations)
- Bookings API (CRUD + confirm, check-in, check-out, cancel)
- Invoices API
- Inventory API
- Reports API
- Alerts API
- Audit API
- Utility functions (formatCurrency, formatDate, getStatusBadgeClass, etc.)

### 2. **js/dashboard.js**
Xử lý logic cho trang dashboard:
- Load dashboard stats
- Load room status
- Load recent bookings
- Load alerts
- Load low stock items

### 3. **js/rooms.js**
Xử lý logic cho trang quản lý phòng:
- Load và hiển thị danh sách phòng
- Filter và search
- CRUD operations
- Form validation

### 4. **js/bookings.js**
Xử lý logic cho trang quản lý đặt phòng:
- Load và hiển thị danh sách đặt phòng
- Filter và search
- View booking details
- Confirm booking (assign room)
- Check-in/Check-out
- Cancel booking

### 5. **js/auth-login.js**
Xử lý logic đăng nhập:
- Form validation
- Login API call
- Token storage
- Redirect after login
- Password toggle
- Error handling

## Cấu Trúc Menu

```
📊 Tổng Quan (Dashboard)

📁 Quản Lý
  - 🚪 Quản Lý Phòng
  - 📅 Đặt Phòng
  - 🧾 Hóa Đơn
  - 📦 Quản Lý Kho

📈 Báo Cáo
  - 📊 Báo Cáo
  - 🔔 Cảnh Báo
  - 📝 Nhật Ký

⚙️ Hệ Thống
  - 👤 Tài Khoản
```

## Tính Năng Chính

### ✅ Đã Hoàn Thành
1. **Dashboard tổng quan** với real-time statistics
2. **Quản lý phòng** đầy đủ CRUD
3. **Quản lý đặt phòng** với workflow hoàn chỉnh
4. **Đăng nhập Admin** với authentication
5. **API Integration** hoàn chỉnh
6. **Responsive Design** - tương thích mobile
7. **Clean UI/UX** - giao diện chuyên nghiệp
8. **Vietnamese Language** - 100% tiếng Việt

### 🎨 UI/UX Features
- Modern Bootstrap 5 design
- Boxicons icon library
- Perfect Scrollbar
- Modal dialogs
- Dropdown menus
- Loading states
- Toast notifications
- Badge status indicators
- Responsive tables
- Search và filter

## Cấu Hình API

Trong file `js/api.js`, cấu hình API base URL:

```javascript
const API_BASE_URL = 'https://localhost:5001/api';
```

Thay đổi URL này nếu backend chạy ở port khác.

## Cách Sử Dụng

### 1. Đăng Nhập
- Truy cập: `/admin/html/auth-login-basic.html`
- Nhập username và password
- Hệ thống sẽ lưu token vào localStorage
- Tự động redirect về dashboard

### 2. Dashboard
- Xem tổng quan hệ thống
- Thống kê realtime
- Truy cập nhanh các chức năng

### 3. Quản Lý Phòng
- Thêm phòng mới bằng nút "Thêm Phòng Mới"
- Click vào menu 3 chấm để Xem/Sửa/Xóa
- Sử dụng filter để lọc phòng
- Tìm kiếm theo số phòng

### 4. Quản Lý Đặt Phòng
- Xem danh sách đặt phòng
- Xác nhận đặt phòng (Pending → Confirmed)
- Check-in (Confirmed → CheckedIn)
- Check-out (CheckedIn → CheckedOut)
- Hủy đặt phòng (Pending/Confirmed → Cancelled)

## Authentication Flow

1. User nhập username/password
2. Call API `/api/auth/admin-login`
3. Nhận token và user info
4. Lưu vào localStorage:
   - `authToken`: JWT token
   - `user`: User information
5. Mọi API call sau đó đều attach token vào header
6. Nếu token hết hạn (401), tự động redirect về login

## Status Mapping

### Booking Status
- **Pending** (Chờ xác nhận) - Vàng
- **Confirmed** (Đã xác nhận) - Xanh dương
- **CheckedIn** (Đã nhận phòng) - Xanh lá
- **CheckedOut** (Đã trả phòng) - Xám
- **Cancelled** (Đã hủy) - Đỏ

### Room Status
- **Available** (Trống) - Xanh lá
- **Occupied** (Đang sử dụng) - Xanh dương
- **Maintenance** (Bảo trì) - Vàng
- **Unavailable** (Không khả dụng) - Đỏ

## Browser Support

- ✅ Chrome (Latest)
- ✅ Firefox (Latest)
- ✅ Safari (Latest)
- ✅ Edge (Latest)

## Dependencies

### CSS
- Bootstrap 5
- Boxicons
- Perfect Scrollbar
- Custom theme styles

### JavaScript
- jQuery 3.x
- Bootstrap 5 JS
- Perfect Scrollbar JS
- ApexCharts (for dashboard)
- Custom API helper
- Page-specific scripts

## File Structure

```
admin/
├── index.html              # Dashboard
├── rooms.html             # Quản lý phòng
├── bookings.html          # Quản lý đặt phòng
├── html/
│   └── auth-login-basic.html  # Đăng nhập
├── js/
│   ├── api.js             # API Helper
│   ├── dashboard.js       # Dashboard logic
│   ├── rooms.js           # Rooms logic
│   ├── bookings.js        # Bookings logic
│   └── auth-login.js      # Login logic
├── assets/
│   ├── vendor/
│   │   ├── css/           # Core CSS
│   │   ├── js/            # Core JS
│   │   ├── fonts/         # Boxicons
│   │   └── libs/          # Libraries
│   ├── css/
│   │   └── demo.css
│   ├── js/
│   │   ├── config.js
│   │   └── main.js
│   └── img/               # Images & icons
└── README.md              # This file
```

## Troubleshooting

### 1. API Connection Failed
- Kiểm tra backend đã chạy chưa
- Kiểm tra API_BASE_URL trong api.js
- Kiểm tra CORS settings

### 2. Không Thể Đăng Nhập
- Kiểm tra username/password
- Kiểm tra API endpoint /api/auth/admin-login
- Kiểm tra console log để xem error

### 3. Token Expired
- Hệ thống sẽ tự động redirect về login
- Đăng nhập lại để lấy token mới

### 4. Data Không Hiển Thị
- Mở Console (F12) để xem error
- Kiểm tra API response
- Kiểm tra token còn valid không

## Next Steps (Optional Enhancements)

- [ ] Trang quản lý hóa đơn (invoices.html)
- [ ] Trang quản lý kho (inventory.html)
- [ ] Trang báo cáo (reports.html)
- [ ] Trang cảnh báo (alerts.html)
- [ ] Trang nhật ký (audit.html)
- [ ] Export Excel/PDF
- [ ] Advanced filtering
- [ ] Pagination
- [ ] Real-time notifications
- [ ] Chart visualizations

## Support

Nếu có vấn đề, vui lòng kiểm tra:
1. Console log (F12)
2. Network tab để xem API calls
3. localStorage để kiểm tra token

---

**© 2025 Hệ Thống Quản Lý Resort**
Phiên bản: 1.0.0
Template: Sneat Bootstrap 5 (Free Version)
