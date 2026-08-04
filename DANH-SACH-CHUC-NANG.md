# 📋 Danh Sách Chức Năng - Resort Management System

## 👤 CHỨC NĂNG CUSTOMER (Khách hàng)

### 🔐 Xác thực & Tài khoản
- ✅ **Đăng ký tài khoản** (`register.html`)
  - Đăng ký thông tin cá nhân
  - Xác thực email
  - Tạo tài khoản customer

- ✅ **Đăng nhập** (`login.html`)
  - Đăng nhập bằng email/password
  - Lưu session token
  - Tự động redirect sau khi đăng nhập

- ✅ **Quản lý tài khoản** (`account.html`)
  - Xem thông tin cá nhân
  - Cập nhật thông tin (tên, email, số điện thoại, địa chỉ)
  - Đổi mật khẩu
  - Xem điểm thưởng (Loyalty Points)
  - Xem lịch sử giao dịch

### 🏨 Đặt phòng & Quản lý Booking
- ✅ **Xem danh sách phòng** (`rooms.html`, `rooms-single.html`)
  - Xem tất cả phòng available
  - Lọc theo loại phòng
  - Xem chi tiết từng phòng (ảnh, tiện ích, giá)
  - Tìm kiếm phòng

- ✅ **Chi tiết phòng** (`room-detail.html`)
  - Xem thông tin chi tiết phòng
  - Xem ảnh phòng
  - Xem đánh giá của khách hàng khác
  - Đặt phòng trực tiếp

- ✅ **Đặt phòng** (`room-detail.html`, `booking-success.html`)
  - Chọn ngày check-in/check-out
  - Chọn số lượng khách
  - Thêm yêu cầu đặc biệt
  - Chọn phương thức thanh toán (QR, Tiền mặt)
  - Tạo booking request
  - Xác nhận đặt phòng thành công

- ✅ **Quản lý đặt phòng của tôi** (`my-bookings.html`)
  - Xem danh sách tất cả bookings
  - Xem trạng thái booking (Pending, Confirmed, Paid, Cancelled)
  - Xem chi tiết booking
  - Hủy booking (nếu chưa thanh toán)
  - Thanh toán booking (QR code, Tiền mặt)
  - Yêu cầu thanh toán tiền mặt tại khách sạn

- ✅ **Chi tiết đặt phòng** (`booking-details.html`)
  - Xem thông tin đầy đủ booking
  - Xem thông tin phòng được assign
  - Xem hóa đơn
  - Thanh toán online (QR code)
  - Yêu cầu thanh toán tiền mặt
  - Xem lịch sử thanh toán

### 🍽️ Nhà hàng & Đặt món
- ✅ **Xem menu nhà hàng** (`restaurant.html`)
  - Xem danh sách món ăn
  - Lọc theo loại món
  - Tìm kiếm món ăn
  - Xem chi tiết món (ảnh, giá, mô tả)

- ✅ **Đặt món** (`restaurant.html`)
  - Thêm món vào giỏ hàng
  - Xem giỏ hàng
  - Chỉnh sửa số lượng
  - Xóa món khỏi giỏ hàng
  - Đặt món (tạo order)
  - Chọn phương thức thanh toán (QR, Tiền mặt)

- ✅ **Quản lý đơn đặt món** (`my-restaurant-orders.html`)
  - Xem danh sách đơn đặt món
  - Xem trạng thái đơn (Pending, Confirmed, Preparing, Ready, Completed)
  - Xem chi tiết đơn
  - Thanh toán đơn (QR code, Tiền mặt)
  - Yêu cầu thanh toán tiền mặt tại nhà hàng

- ✅ **Chi tiết đơn hàng** (`order-details.html`)
  - Xem thông tin đầy đủ đơn hàng
  - Xem danh sách món đã đặt
  - Xem tổng tiền
  - Thanh toán online (QR code)
  - Yêu cầu thanh toán tiền mặt

- ✅ **Xác nhận đặt món thành công** (`order-success.html`)
  - Hiển thị mã đơn hàng
  - Hiển thị thông tin thanh toán
  - Link đến chi tiết đơn hàng

### 💳 Thanh toán
- ✅ **Thanh toán QR Code** (`simple-payment.js`, `restaurant-payment.js`)
  - Tạo QR code thanh toán (PayOs/VietQR)
  - Quét QR code để thanh toán
  - Tự động kiểm tra trạng thái thanh toán
  - Thông báo khi thanh toán thành công

- ✅ **Thanh toán tiền mặt**
  - Yêu cầu thanh toán tiền mặt tại khách sạn/nhà hàng
  - Chờ admin xác nhận thanh toán
  - Nhận thông báo khi admin xác nhận

- ✅ **Lịch sử thanh toán**
  - Xem lịch sử tất cả giao dịch
  - Xem chi tiết từng giao dịch
  - Tải hóa đơn PDF

### ⭐ Đánh giá & Phản hồi
- ✅ **Xem đánh giá** (`reviews.html`)
  - Xem tất cả đánh giá của khách hàng
  - Xem đánh giá theo phòng
  - Xem rating và comment

- ✅ **Tạo đánh giá** (`reviews.html`)
  - Đánh giá phòng sau khi ở
  - Chọn rating (1-5 sao)
  - Viết comment
  - Upload ảnh (nếu có)

### 🎫 Mã giảm giá
- ✅ **Sử dụng mã giảm giá** (`coupons.js`)
  - Nhập mã giảm giá
  - Validate mã
  - Áp dụng mã khi đặt phòng/đặt món
  - Xem danh sách mã giảm giá active

### 🎧 Hỗ trợ khách hàng
- ✅ **Tạo ticket hỗ trợ** (`support.html`, `my-tickets.html`)
  - Tạo ticket mới
  - Chọn loại vấn đề
  - Mô tả vấn đề
  - Upload file đính kèm (nếu có)
  - Xem trạng thái ticket

- ✅ **Quản lý tickets** (`my-tickets.html`)
  - Xem danh sách tickets của mình
  - Xem chi tiết ticket
  - Xem phản hồi từ admin
  - Cập nhật ticket

### 🤖 Chat AI
- ✅ **Trợ lý AI** (`ai-chat.js`)
  - Chat với AI về thông tin resort
  - Hỏi về phòng, dịch vụ
  - Hỏi về chính sách
  - Hỗ trợ đặt phòng

### 📄 Trang thông tin
- ✅ **Trang chủ** (`index.html`)
  - Giới thiệu resort
  - Video giới thiệu
  - Phòng nổi bật
  - Dịch vụ
  - Đánh giá khách hàng

- ✅ **Giới thiệu** (`about.html`)
  - Thông tin về resort
  - Lịch sử
  - Tiện ích

- ✅ **Liên hệ** (`contact.html`)
  - Form liên hệ
  - Thông tin liên hệ
  - Bản đồ

- ✅ **FAQ** (`faq.html`)
  - Câu hỏi thường gặp
  - Tìm kiếm câu hỏi

- ✅ **Blog** (`blog.html`, `blog-single.html`)
  - Xem danh sách bài viết
  - Xem chi tiết bài viết

---

## 👨‍💼 CHỨC NĂNG ADMIN (Quản trị viên)

### 📊 Dashboard & Tổng quan
- ✅ **Dashboard chính** (`index.html`)
  - Tổng quan doanh thu hôm nay
  - Tỷ lệ lấp đầy phòng
  - Đặt phòng đang hoạt động
  - Tăng trưởng tháng này
  - Biểu đồ xu hướng doanh thu (30 ngày)
  - Biểu đồ tỷ lệ lấp đầy
  - Biểu đồ doanh thu dịch vụ
  - Top 10 khách hàng chi tiêu nhiều nhất
  - Hoạt động gần đây
  - Auto refresh mỗi 5 phút

### 👥 Quản lý Người dùng
- ✅ **Quản lý Users** (`users.html`)
  - Xem danh sách tất cả users
  - Tạo user mới
  - Sửa thông tin user
  - Xóa user
  - Phân quyền (Role: Admin, Manager, FrontDesk, Cashier, etc.)
  - Tìm kiếm user
  - Lọc theo role

- ✅ **Quản lý Nhân viên** (`employees.html`)
  - Xem danh sách nhân viên
  - Tạo nhân viên mới
  - Sửa thông tin nhân viên
  - Xóa nhân viên
  - Gán role cho nhân viên
  - Tìm kiếm nhân viên
  - Lọc theo department/role

- ✅ **Quản lý Khách hàng** (`customers.html`)
  - Xem danh sách tất cả khách hàng
  - Xem chi tiết khách hàng (trang riêng)
  - Xem lịch sử đặt phòng của khách hàng
  - Xem lịch sử đặt món
  - Xem tổng chi tiêu
  - Xem điểm thưởng
  - Tìm kiếm khách hàng
  - Lọc theo loại khách hàng
  - Cập nhật thông tin khách hàng

### 🏨 Quản lý Phòng
- ✅ **Quản lý Loại phòng** (`room-types.html`)
  - Xem danh sách loại phòng
  - Tạo loại phòng mới
  - Sửa thông tin loại phòng
  - Xóa loại phòng
  - Upload ảnh loại phòng
  - Xem thống kê (số phòng, tỷ lệ lấp đầy)

- ✅ **Quản lý Phòng** (`rooms.html`)
  - Xem danh sách tất cả phòng
  - Tạo phòng mới
  - Sửa thông tin phòng
  - Xóa phòng
  - Assign phòng cho booking
  - Cập nhật trạng thái phòng (Available, Occupied, Maintenance)
  - Upload ảnh phòng
  - Tìm kiếm phòng
  - Lọc theo loại phòng, tầng, trạng thái

### 📋 Quản lý Đặt phòng & Hóa đơn
- ✅ **Quản lý Hóa đơn** (`invoices.html`)
  - Xem danh sách tất cả invoices
  - Tạo invoice mới
  - Xem chi tiết invoice
  - Cập nhật trạng thái invoice (Issued, Paid, Cancelled)
  - Xử lý thanh toán
  - Tìm kiếm invoice
  - Lọc theo trạng thái, ngày
  - Xuất invoice PDF

- ✅ **Quản lý Bookings** (tích hợp trong invoices)
  - Xem bookings từ invoices
  - Check-in booking
  - Check-out booking
  - Assign phòng
  - Thêm charges (dịch vụ)
  - Hủy booking

### 🍽️ Quản lý Nhà hàng
- ✅ **Quản lý Đơn đặt món** (`restaurant-orders.html`)
  - Xem danh sách tất cả đơn đặt món
  - Xem chi tiết đơn
  - Cập nhật trạng thái đơn (Pending, Confirmed, Preparing, Ready, Completed)
  - Cập nhật trạng thái thanh toán (Unpaid, AwaitingConfirmation, Paid)
  - Xác nhận thanh toán tiền mặt
  - Tìm kiếm đơn
  - Lọc theo trạng thái, ngày
  - Thống kê: Tổng đơn, Đã thanh toán, Chờ xác nhận, Tổng doanh thu

- ✅ **Quản lý Menu** (`menu-items.html`)
  - Xem danh sách món ăn
  - Tạo món mới
  - Sửa thông tin món
  - Xóa món
  - Upload ảnh món
  - Cập nhật giá
  - Cập nhật trạng thái (Active, Inactive)
  - Tìm kiếm món
  - Lọc theo loại món
  - Thống kê: Tổng món, Active, Inactive, Giá trung bình

### 🎫 Quản lý Mã giảm giá
- ✅ **Quản lý Coupons** (`coupons.html`)
  - Xem danh sách mã giảm giá
  - Tạo mã mới
  - Sửa thông tin mã
  - Xóa mã
  - Cập nhật trạng thái (Active, Inactive)
  - Xem số lần sử dụng
  - Tìm kiếm mã
  - Lọc theo trạng thái
  - Thống kê: Tổng mã, Active, Inactive, Tổng lượt sử dụng

### 📦 Quản lý Kho
- ✅ **Quản lý Kho hàng** (`inventory.html`)
  - Xem danh sách items trong kho
  - Tạo item mới
  - Sửa thông tin item
  - Xóa item
  - Cập nhật số lượng tồn kho
  - Nhập kho
  - Xuất kho
  - Tìm kiếm item
  - Lọc theo loại, trạng thái
  - Thống kê: Tổng items, Tổng giá trị, Items sắp hết

- ✅ **Quản lý Nhà cung cấp** (`suppliers.html`)
  - Xem danh sách nhà cung cấp
  - Tạo nhà cung cấp mới
  - Sửa thông tin nhà cung cấp
  - Xóa nhà cung cấp
  - Xem lịch sử giao dịch với nhà cung cấp
  - Tìm kiếm nhà cung cấp

### 📊 Báo cáo & Thống kê
- ✅ **Báo cáo tổng hợp** (`reports.html`)
  - Tổng quan hôm nay (Doanh thu, Tỷ lệ lấp đầy, Đặt phòng đang hoạt động, Tăng trưởng)
  - Báo cáo doanh thu:
    - Biểu đồ doanh thu theo ngày
    - Doanh thu theo loại (Đặt phòng, Nhà hàng)
  - Báo cáo tỷ lệ lấp đầy:
    - Biểu đồ tỷ lệ lấp đầy theo ngày
  - Phân tích khách hàng:
    - Top 10 khách hàng chi tiêu nhiều nhất
    - Phân loại khách hàng (biểu đồ)
  - Sử dụng dịch vụ:
    - Doanh thu theo dịch vụ
  - Xuất báo cáo:
    - Xuất PDF (thiết kế đẹp, có màu sắc, boxes)
    - Xuất Excel (multi-sheet)
  - Lọc theo khoảng thời gian
  - Tự động refresh dữ liệu

### 🎫 Quản lý Tickets
- ✅ **Quản lý Support Tickets** (`support-tickets.html`)
  - Xem danh sách tất cả tickets
  - Xem chi tiết ticket
  - Phản hồi ticket
  - Cập nhật trạng thái (Open, In Progress, Resolved, Closed)
  - Gán ticket cho nhân viên
  - Tìm kiếm ticket
  - Lọc theo trạng thái, priority
  - Thống kê: Tổng tickets, Open, Resolved, Closed

### ⭐ Quản lý Đánh giá
- ✅ **Lịch sử Đánh giá** (`reviews-history.html`)
  - Xem tất cả đánh giá
  - Xem chi tiết đánh giá
  - Xóa đánh giá không phù hợp
  - Phản hồi đánh giá
  - Tìm kiếm đánh giá
  - Lọc theo rating, phòng

### 💰 Lịch sử Thanh toán
- ✅ **Lịch sử Thanh toán** (`payment-history.html`)
  - Xem tất cả giao dịch thanh toán
  - Xem chi tiết từng giao dịch
  - Tìm kiếm giao dịch
  - Lọc theo phương thức thanh toán, ngày
  - Xuất báo cáo thanh toán

### 📝 Audit Logs
- ✅ **Lịch sử Hoạt động** (`audit-logs.html`)
  - Xem tất cả hoạt động trong hệ thống
  - Xem ai đã làm gì, khi nào
  - Tìm kiếm hoạt động
  - Lọc theo user, action, entity
  - Reconciliation

### ⚙️ Cài đặt & Hồ sơ
- ✅ **Hồ sơ của tôi** (`profile.html`)
  - Xem thông tin cá nhân
  - Cập nhật thông tin
  - Đổi mật khẩu
  - Upload avatar

- ✅ **Cài đặt** (`settings.html`)
  - Cài đặt hệ thống
  - Cài đặt thông báo
  - Cài đặt giao diện

---

## 🚀 CHỨC NĂNG NÂNG CAO

### 🔐 Bảo mật & Phân quyền
- ✅ **Role-Based Access Control (RBAC)**
  - Phân quyền chi tiết theo role (Admin, Manager, FrontDesk, Cashier, Accounting, Inventory, Business, Customer)
  - Mỗi role có quyền truy cập khác nhau
  - JWT token authentication
  - Session management

- ✅ **Audit Logging**
  - Tự động ghi log tất cả hoạt động
  - Track ai đã làm gì, khi nào
  - Reconciliation reports

### 💳 Thanh toán
- ✅ **Tích hợp PayOs/VietQR**
  - Tạo QR code thanh toán
  - Webhook nhận thông báo thanh toán
  - Tự động cập nhật trạng thái sau khi thanh toán
  - Polling để kiểm tra trạng thái

- ✅ **Thanh toán tiền mặt**
  - Yêu cầu thanh toán tiền mặt
  - Admin xác nhận thanh toán
  - Thông báo real-time

- ✅ **Nhiều phương thức thanh toán**
  - QR Code (PayOs/VietQR)
  - Tiền mặt tại khách sạn/nhà hàng
  - Thanh toán online

### 📊 Analytics & Reporting
- ✅ **Dashboard Real-time**
  - Dữ liệu real-time từ API
  - Auto refresh mỗi 5 phút
  - Biểu đồ tương tác (Chart.js)
  - Top customers, recent activities

- ✅ **Báo cáo nâng cao**
  - Báo cáo doanh thu chi tiết
  - Báo cáo tỷ lệ lấp đầy
  - Phân tích khách hàng
  - Sử dụng dịch vụ
  - Xuất PDF/Excel chuyên nghiệp

### 🤖 AI Chat
- ✅ **Trợ lý AI**
  - Chat với AI về thông tin resort
  - Hỗ trợ khách hàng 24/7
  - Tích hợp vào tất cả trang customer

### 🔔 Thông báo
- ✅ **Real-time Notifications**
  - Toast notifications
  - Thông báo khi có sự kiện mới
  - Thông báo thanh toán thành công

### 📱 Responsive Design
- ✅ **Mobile-First**
  - Responsive cho tất cả màn hình
  - Touch-friendly
  - Mobile menu

### 🎨 UI/UX
- ✅ **Nalika-inspired Design**
  - Giao diện hiện đại, đẹp mắt
  - Stats cards với gradient
  - Hover effects
  - Animations
  - Glassmorphism effects

### 🔍 Tìm kiếm & Lọc
- ✅ **Advanced Filtering**
  - Tìm kiếm real-time
  - Lọc theo nhiều tiêu chí
  - Debouncing cho performance

### 📄 Export
- ✅ **Xuất dữ liệu**
  - Xuất PDF (thiết kế đẹp, có màu sắc)
  - Xuất Excel (multi-sheet)
  - Print reports

### 🔄 Auto Refresh
- ✅ **Tự động làm mới**
  - Dashboard auto refresh
  - Reports auto refresh
  - Real-time data updates

---

## 📈 CHỨC NĂNG CÓ THỂ PHÁT TRIỂN THÊM

### 🔮 Tính năng đề xuất
- 📧 **Email Notifications**
  - Gửi email xác nhận đặt phòng
  - Gửi email hóa đơn
  - Gửi email thông báo

- 📱 **SMS Notifications**
  - Gửi SMS xác nhận
  - Gửi SMS nhắc nhở

- 🔔 **Push Notifications**
  - Push notifications cho mobile
  - Browser notifications

- 📊 **Advanced Analytics**
  - Predictive analytics
  - Machine learning recommendations
  - Customer segmentation

- 💰 **Loyalty Program**
  - Tích điểm thưởng
  - Đổi điểm lấy voucher
  - Chương trình khách hàng thân thiết

- 🎁 **Promotions & Campaigns**
  - Tạo chương trình khuyến mãi
  - Flash sales
  - Seasonal promotions

- 📅 **Calendar Integration**
  - Lịch đặt phòng
  - Lịch nhân viên
  - Google Calendar sync

- 📸 **Image Gallery**
  - Gallery ảnh phòng
  - 360° view
  - Virtual tour

- 🌐 **Multi-language**
  - Hỗ trợ nhiều ngôn ngữ
  - i18n

- 🔐 **2FA (Two-Factor Authentication)**
  - Bảo mật 2 lớp
  - OTP verification

- 📊 **Business Intelligence**
  - Advanced dashboards
  - Custom reports
  - Data visualization

- 🤝 **CRM Integration**
  - Tích hợp CRM
  - Customer journey tracking
  - Lead management

---

## 📝 Ghi chú

- Tất cả chức năng đã được implement và test
- Hệ thống sử dụng JWT authentication
- Dữ liệu được lấy từ API real-time
- UI/UX được thiết kế theo phong cách Nalika
- Responsive cho mọi thiết bị
- Export PDF/Excel chuyên nghiệp

