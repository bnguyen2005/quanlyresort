# Hướng Dẫn Test Dashboard và Reports

## Mục đích

Test xem trang Dashboard và Reports có hiển thị đúng số liệu từ khách hàng sau khi đặt phòng và đặt món ăn không.

## Yêu cầu

1. Python 3.7+
2. Thư viện `requests`: `pip install requests`
3. Tài khoản khách hàng hợp lệ trong hệ thống
4. Tài khoản admin để xem dashboard (tùy chọn)

## Các Script Test

### 1. Test Tổng Hợp (Khuyến nghị)

**Script**: `test_dashboard_reports.py`

Script này sẽ:
- Đăng nhập với tài khoản khách hàng
- Tạo và thanh toán 1 đặt phòng
- Tạo và thanh toán 1 đơn đặt món
- Kiểm tra dashboard stats (nếu có admin token)

**Cách chạy:**
```bash
cd scripts
python3 test_dashboard_reports.py
```

**Input cần:**
- Email khách hàng
- Password khách hàng
- Email admin (tùy chọn, để xem dashboard stats)
- Password admin (tùy chọn)

### 2. Test Đặt Phòng Riêng

**Script**: `create_booking_and_pay.py`

**Cách chạy:**
```bash
cd scripts
python3 create_booking_and_pay.py
```

### 3. Test Đặt Món Ăn Riêng

**Script**: `create_restaurant_order.py`

**Cách chạy:**
```bash
cd scripts
python3 create_restaurant_order.py
```

### 4. Test Nhiều Đơn Món Ăn

**Script**: `create_restaurant_order_batch.py`

**Cách chạy:**
```bash
cd scripts
python3 create_restaurant_order_batch.py 10  # Tạo 10 đơn
```

## Quy Trình Test

### Bước 1: Chuẩn bị

1. Đảm bảo có tài khoản khách hàng trong hệ thống
2. Đảm bảo có món ăn (services) trong database
3. Đảm bảo có loại phòng (room types) trong database

### Bước 2: Chạy Test

```bash
cd scripts
python3 test_dashboard_reports.py
```

Nhập thông tin khi được hỏi:
- Email khách hàng
- Password khách hàng
- Email admin (nếu muốn xem dashboard stats ngay)
- Password admin (nếu muốn xem dashboard stats ngay)

### Bước 3: Kiểm Tra Kết Quả

Sau khi chạy script, kiểm tra:

1. **Trang Dashboard** (`/admin/html/index.html`):
   - ✅ Doanh thu hôm nay có tăng không?
   - ✅ Tỷ lệ lấp đầy có thay đổi không?
   - ✅ Đặt phòng đang hoạt động có tăng không?
   - ✅ Top khách hàng có hiển thị không?
   - ✅ Hoạt động gần đây có hiển thị không?

2. **Trang Reports** (`/admin/html/reports.html`):
   - ✅ Doanh thu hôm nay có hiển thị không?
   - ✅ Biểu đồ doanh thu theo ngày có dữ liệu không?
   - ✅ Doanh thu theo loại có hiển thị "Đặt phòng" và "Nhà hàng" không?
   - ✅ Top khách hàng có hiển thị không?
   - ✅ Doanh thu theo dịch vụ có dữ liệu không?

## Lưu Ý

1. **URL API**: Mặc định dùng production URL. Để test local:
   ```python
   BASE_URL = "http://localhost:5000"
   ```

2. **Thời gian**: Sau khi tạo đơn/đặt phòng, đợi vài giây để data được cập nhật trước khi kiểm tra dashboard.

3. **Token**: Script tự động lấy token sau khi đăng nhập.

4. **Thanh toán**: Script sẽ tự động thanh toán sau khi tạo đơn/đặt phòng.

## Troubleshooting

### Lỗi đăng nhập
- Kiểm tra email/password có đúng không
- Kiểm tra tài khoản có tồn tại trong database không
- Kiểm tra URL API có đúng không

### Không có món ăn
- Đảm bảo có services với `ServiceType = "Restaurant"` trong database
- Kiểm tra services có `IsActive = true` không

### Không có loại phòng
- Đảm bảo có room types trong database
- Kiểm tra API `/api/room-types` có trả về data không

### Dashboard không hiển thị dữ liệu
- Kiểm tra token admin có hợp lệ không
- Kiểm tra API `/api/reports/dashboard-stats` có trả về data không
- Kiểm tra console browser có lỗi không
- Đảm bảo đã thanh toán thành công (PaymentStatus = "Paid")

## Kết Quả Mong Đợi

Sau khi chạy test thành công:

1. **Dashboard** sẽ hiển thị:
   - Doanh thu hôm nay > 0
   - Có đặt phòng hoặc đơn đặt món mới
   - Top khách hàng có tên khách hàng vừa đặt
   - Hoạt động gần đây có ghi nhận

2. **Reports** sẽ hiển thị:
   - Doanh thu hôm nay > 0
   - Biểu đồ có điểm dữ liệu mới
   - Doanh thu theo loại có "Đặt phòng" và/hoặc "Nhà hàng"
   - Top khách hàng có tên khách hàng vừa đặt

