# 📖 HƯỚNG DẪN SỬ DỤNG MODULE QUẢN LÝ HÓA ĐƠN

## 🎯 TỔNG QUAN

Module Quản lý Hóa đơn cho phép bạn:
- Xem danh sách tất cả hóa đơn
- Thống kê hóa đơn (tổng, đã thanh toán, chưa thanh toán, doanh thu)
- Xem chi tiết hóa đơn
- Thanh toán hóa đơn
- Hủy hóa đơn (nếu chưa thanh toán)
- Xuất PDF danh sách hóa đơn và chi tiết hóa đơn

---

## 🚀 TRUY CẬP MODULE

### Cách 1: Từ Menu
1. Đăng nhập vào hệ thống
2. Click menu bên trái → **Hóa đơn** (icon 📋)
3. Hoặc truy cập trực tiếp: `http://localhost:5130/admin/html/invoices.html`

### Cách 2: Từ Dashboard
1. Vào **Dashboard**
2. Click vào thẻ "Hóa đơn" hoặc link "Xem chi tiết"

---

## 📊 THỐNG KÊ

Trang hóa đơn hiển thị 4 thẻ thống kê ở đầu:

1. **Tổng hóa đơn** - Tổng số hóa đơn hiện có
2. **Đã thanh toán** - Số hóa đơn đã thanh toán hoàn toàn
3. **Chưa thanh toán** - Số hóa đơn chưa thanh toán hoặc còn nợ
4. **Tổng doanh thu** - Tổng doanh thu từ các hóa đơn

> 💡 Các thống kê tự động cập nhật khi bạn thêm, sửa, xóa, hoặc thanh toán hóa đơn

---

## 🔍 LỌC & TÌM KIẾM

### Tìm kiếm
- Nhập từ khóa vào ô **Tìm kiếm**
- Hệ thống tìm kiếm theo:
  - Số hóa đơn
  - Tên khách hàng
- Nhấn **Enter** hoặc click **Lọc**

### Lọc theo Trạng thái
Chọn trạng thái từ dropdown:
- **Tất cả** - Hiển thị tất cả hóa đơn
- **Issued** - Đã phát hành (chưa thanh toán)
- **PartiallyPaid** - Đã thanh toán một phần
- **Paid** - Đã thanh toán hoàn toàn
- **Cancelled** - Đã hủy

### Lọc theo Ngày
- **Từ ngày**: Chọn ngày bắt đầu
- **Đến ngày**: Chọn ngày kết thúc
- Click **Lọc** để áp dụng

> 💡 Bạn có thể kết hợp nhiều bộ lọc cùng lúc

---

## 📋 DANH SÁCH HÓA ĐƠN

### Cấu trúc cột
1. **Số HĐ** - Mã hóa đơn duy nhất
2. **Ngày phát hành** - Ngày tạo hóa đơn
3. **Khách hàng** - Tên khách hàng
4. **Tổng tiền** - Tổng giá trị hóa đơn
5. **Đã trả** - Số tiền đã thanh toán
6. **Còn lại** - Số tiền còn nợ
7. **Trạng thái** - Badge màu theo trạng thái
8. **Thao tác** - Các nút hành động

### Thao tác trong danh sách
- **🔍 Xem chi tiết** - Mở modal xem chi tiết
- **💳 Thanh toán** - Thanh toán hóa đơn (chỉ hiện khi còn nợ)
- **❌ Hủy** - Hủy hóa đơn (chỉ hiện khi chưa thanh toán)

> 💡 DataTable hỗ trợ search, sort, pagination

---

## 👁️ XEM CHI TIẾT HÓA ĐƠN

### Cách mở
- Click **Xem chi tiết** trên một hóa đơn trong danh sách

### Thông tin hiển thị
Modal hiển thị:
- **Số hóa đơn**
- **Ngày phát hành**
- **Khách hàng** (tên, email, SĐT)
- **Booking ID**
- **Chi tiết thanh toán**:
  - Tổng tiền
  - Thuế
  - Giảm giá
  - Thành tiền
  - Đã thanh toán
  - Còn lại
- **Trạng thái** - Badge màu
- **Ngày thanh toán** (nếu có)
- **Phương thức thanh toán** (nếu có)
- **Số tham chiếu** (nếu có)

### Xuất PDF từ modal
- Click **Xuất PDF** ở footer modal
- File PDF được tải về tên `hoa-don-{SO-HOA-DON}.pdf`

---

## 💳 THANH TOÁN HÓA ĐƠN

### Điều kiện
- Hóa đơn còn nợ (status: Issued hoặc PartiallyPaid)
- Nút **Thanh toán** chỉ hiện khi điều kiện đủ

### Các bước
1. Click **Thanh toán** trên hóa đơn
2. Modal mở với:
   - Số hóa đơn
   - Số tiền còn lại
3. Điền form:
   - **Số tiền**: Nhập số tiền thanh toán (mặc định = còn lại)
   - **Phương thức**: Chọn 1 trong 5:
     - Tiền mặt (Cash)
     - Thẻ tín dụng (CreditCard)
     - Chuyển khoản (BankTransfer)
     - MoMo
     - ZaloPay
   - **Số tham chiếu**: (Optional) Nhập số tham chiếu
4. Click **Xác nhận thanh toán**
5. Hệ thống:
   - Cập nhật số tiền đã trả
   - Cập nhật trạng thái (Issued → PartiallyPaid → Paid)
   - Ghi nhận phương thức thanh toán
   - Tạo audit log

### Trạng thái sau thanh toán
- **Issued**: Chưa thanh toán → **PartiallyPaid** nếu trả một phần, **Paid** nếu trả đủ
- **PartiallyPaid**: Đã trả một phần → **Paid** khi trả đủ, **PartiallyPaid** nếu còn nợ

---

## ❌ HỦY HÓA ĐƠN

### Điều kiện
- Chưa thanh toán hoàn toàn
- Chỉ người có quyền mới hủy được

### Các bước
1. Click **Hủy** trên hóa đơn
2. Xác nhận trong dialog
3. Hệ thống:
   - Chuyển trạng thái thành **Cancelled**
   - Tạo audit log
   - Không thể thanh toán sau khi hủy

> ⚠️ **Lưu ý**: Không thể hủy hóa đơn đã thanh toán hoàn toàn

---

## 📄 XUẤT PDF

### Xuất báo cáo danh sách
- Click nút **Xuất báo cáo** ở header trang
- File `bao-cao-hoa-don.pdf` được tải về
- Bao gồm tất cả hóa đơn đang hiển thị

### Xuất chi tiết một hóa đơn
1. Mở chi tiết hóa đơn (Xem chi tiết)
2. Click **Xuất PDF** trong modal
3. File `hoa-don-{SO-HOA-DON}.pdf` được tải về

### Nội dung PDF
**Báo cáo danh sách**:
- Tiêu đề "BÁO CÁO HÓA ĐƠN"
- Danh sách tất cả hóa đơn:
  - Số HĐ
  - Khách hàng
  - Ngày
  - Tổng tiền
  - Trạng thái

**Chi tiết hóa đơn**:
- Tiêu đề "HÓA ĐƠN"
- Số HĐ
- Ngày
- Khách hàng
- Chi tiết:
  - Tổng tiền
  - Thuế
  - Giảm giá
  - Tổng cộng
  - Đã thanh toán
  - Còn lại

---

## 🔐 PHÂN QUYỀN

### Ai có quyền truy cập?
- **Admin**: Toàn quyền
- **Manager**: Toàn quyền
- **Accounting**: Toàn quyền
- **Cashier**: Có thể xem và thanh toán hóa đơn
- **FrontDesk**: Chỉ xem

### Hành động theo quyền
| Hành động | Admin | Manager | Accounting | Cashier | FrontDesk |
|-----------|-------|---------|------------|---------|-----------|
| Xem danh sách | ✅ | ✅ | ✅ | ✅ | ✅ |
| Xem chi tiết | ✅ | ✅ | ✅ | ✅ | ✅ |
| Thanh toán | ✅ | ✅ | ✅ | ✅ | ❌ |
| Hủy hóa đơn | ✅ | ✅ | ✅ | ❌ | ❌ |
| Xuất PDF | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 🔧 XỬ LÝ LỖI THƯỜNG GẶP

### 1. Không hiển thị danh sách hóa đơn
**Nguyên nhân**:
- Chưa đăng nhập
- Token hết hạn
- API lỗi

**Giải pháp**:
1. Refresh trang (F5)
2. Đăng xuất và đăng nhập lại
3. Kiểm tra Console để xem lỗi API

### 2. Không thể thanh toán
**Nguyên nhân**:
- Hóa đơn đã thanh toán hoàn toàn
- Không có quyền thanh toán
- Số tiền nhập lớn hơn số còn lại

**Giải pháp**:
- Xem lại số tiền còn lại
- Liên hệ admin để được phân quyền

### 3. Không xuất được PDF
**Nguyên nhân**:
- Browser không hỗ trợ
- Chặn popup/download
- Lỗi jsPDF

**Giải pháp**:
1. Cho phép download trong browser
2. Thử lại với Chrome hoặc Edge
3. Check Console xem có lỗi không

### 4. DataTable không load
**Nguyên nhân**:
- Network lag
- API lỗi

**Giải pháp**:
1. Refresh trang
2. Check API response
3. Clear cache và reload

---

## 💡 CÁC TÍNH NĂNG NÂNG CAO

### 1. Export Excel (sẽ thêm)
- Xuất danh sách hóa đơn ra Excel
- Hỗ trợ filter và sorting

### 2. In hóa đơn trực tiếp
- In từ máy in
- Hỗ trợ máy in nhiệt, máy in thường

### 3. Email hóa đơn
- Gửi hóa đơn qua email cho khách hàng
- Tự động khi thanh toán hoàn tất

### 4. Biểu đồ thống kê
- Biểu đồ doanh thu theo ngày/tuần/tháng
- Biểu đồ tỷ lệ thanh toán
- Xu hướng thanh toán

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Xem log trong Console (F12)
2. Kiểm tra API response
3. Liên hệ Admin

**Chúc bạn sử dụng hiệu quả! 🎉**

