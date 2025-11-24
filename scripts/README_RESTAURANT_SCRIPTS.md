# Scripts Đặt và Thanh Toán Món Ăn

## Mô tả

Các script này giúp tạo đơn đặt món và thanh toán để test hoặc tạo dữ liệu mẫu.

## Yêu cầu

- Python 3.7+
- Thư viện `requests`: `pip install requests`

## Các Script

### 1. `create_restaurant_order.py`

Script tạo một đơn đặt món và thanh toán.

**Cách sử dụng:**
```bash
python create_restaurant_order.py
```

**Tính năng:**
- Đăng nhập với email/password
- Lấy danh sách món ăn
- Tạo đơn hàng với 2-3 món
- Thanh toán đơn hàng

**Cấu hình:**
- Sửa `BASE_URL` trong file nếu cần
- Thay đổi email/password mặc định nếu cần

### 2. `create_restaurant_order_batch.py`

Script tạo nhiều đơn đặt món và thanh toán (batch).

**Cách sử dụng:**
```bash
# Tạo 5 đơn (mặc định)
python create_restaurant_order_batch.py

# Tạo 10 đơn
python create_restaurant_order_batch.py 10
```

**Tính năng:**
- Tạo nhiều đơn hàng cùng lúc
- Mỗi đơn có 1-3 món ngẫu nhiên
- Tự động thanh toán sau khi tạo

## Ví dụ sử dụng

### Tạo một đơn hàng:
```bash
cd scripts
python create_restaurant_order.py
```

### Tạo 20 đơn hàng:
```bash
cd scripts
python create_restaurant_order_batch.py 20
```

## Lưu ý

1. **URL API**: Mặc định dùng production URL. Để test local, sửa `BASE_URL` trong script:
   ```python
   BASE_URL = "http://localhost:5000"
   ```

2. **Thông tin đăng nhập**: Script sẽ hỏi email/password. Có thể để trống để dùng mặc định (cần sửa trong code).

3. **Token**: Script tự động lấy token sau khi đăng nhập.

4. **Thanh toán**: Script sẽ thanh toán ngay sau khi tạo đơn. Nếu thanh toán thất bại, đơn vẫn được tạo nhưng chưa thanh toán.

## Cấu trúc API

### Tạo đơn hàng:
```
POST /api/restaurant-orders
Body: {
  "customerId": int,
  "items": [
    {
      "serviceId": int,
      "quantity": int,
      "specialNote": string (optional)
    }
  ],
  "deliveryAddress": string,
  "paymentMethod": "QR" | "Card" | "Cash" | "RoomCharge" | "BankTransfer"
}
```

### Thanh toán:
```
POST /api/restaurant-orders/{orderId}/pay
Body: {
  "paymentMethod": string,
  "transactionId": string (optional)
}
```

## Troubleshooting

1. **Lỗi đăng nhập**: Kiểm tra email/password và URL API
2. **Không có món ăn**: Đảm bảo đã có services trong database
3. **Lỗi thanh toán**: Kiểm tra quyền của user (Customer chỉ thanh toán được đơn của mình)

