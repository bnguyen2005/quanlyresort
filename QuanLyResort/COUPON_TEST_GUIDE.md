# 🎟️ Hướng dẫn Test Mã giảm giá

## ✅ Mã giảm giá test đã được tạo

Đã tạo 4 mã test trong database:

| Mã | Loại | Giá trị | Giảm tối đa | Mô tả |
|---|---|---|---|---|
| `SUMMER2024` | Phần trăm | 10% | 50,000₫ | Giảm 10%, tối đa 50k |
| `WEEK3` | Phần trăm | 20% | Không giới hạn | Giảm 20% |
| `FIXED50K` | Số tiền | 50,000₫ | - | Giảm cố định 50k |
| `VIP15` | Phần trăm | 15% | 100,000₫ | Giảm 15%, tối đa 100k |

---

## 📋 Các bước test

### **Bước 1: Test trang Admin - Tạo/Quản lý mã giảm giá**

1. **Đăng nhập với tài khoản Admin**
   ```
   URL: http://localhost:5130/admin/html/coupons.html
   ```

2. **Kiểm tra danh sách mã**
   - Trang sẽ hiển thị 4 mã test đã tạo
   - Kiểm tra các thông tin: Code, Loại, Giá trị, Ngày hết hạn...

3. **Tạo mã mới (Test)**
   - Nhấn nút "Tạo Mã giảm giá"
   - Điền thông tin:
     - Code: `TEST2024`
     - Loại: Phần trăm (%)
     - Giá trị: `25`
     - Ngày bắt đầu: Hôm nay
     - Ngày kết thúc: +30 ngày
   - Nhấn "Lưu"
   - ✅ Kiểm tra: Mã xuất hiện trong danh sách

4. **Sửa mã**
   - Nhấn nút "..." → "Sửa"
   - Thay đổi mô tả
   - Lưu và kiểm tra

5. **Bật/Tắt mã**
   - Nhấn "..." → "Tắt" mã `SUMMER2024`
   - Kiểm tra: Badge chuyển thành "Đã tắt"
   - Bật lại và kiểm tra

---

### **Bước 2: Test trang Customer - Áp dụng mã giảm giá**

#### **2.1. Test Validate Coupon (Có thể bị 403 - bình thường)**

1. **Mở trang chi tiết phòng**
   ```
   URL: http://localhost:5130/customer/room-detail.html?id=1
   ```

2. **Nhập mã giảm giá**
   - Trong sidebar "Đặt phòng ngay"
   - Tìm ô "🎟️ Mã giảm giá"
   - Nhập mã: `SUMMER2024`
   - Nhấn "Áp dụng"

3. **Kết quả mong đợi:**
   - **Nếu có quyền validate:**
     - ✅ Thông báo: "Áp dụng mã thành công"
     - ✅ Dòng "Giảm giá: -X ₫" xuất hiện
     - ✅ Tổng cộng được giảm
   
   - **Nếu không có quyền (403):**
     - ⚠️ Thông báo: "Mã sẽ được kiểm tra khi xác nhận đặt phòng. Tổng tiền hiện tại chưa áp dụng giảm."
     - ✅ Mã được lưu tạm (sẽ gửi khi đặt phòng)

#### **2.2. Test Tính toán Giảm giá**

**Test với mã phần trăm:**
- Chọn phòng giá 500,000₫/đêm
- Chọn 2 đêm → Tổng: 1,000,000₫
- Áp dụng mã `SUMMER2024` (10%, max 50k)
  - Giảm: 100,000 * 10% = 10,000₫ (chưa đạt max)
  - **Tổng sau giảm: 990,000₫**

**Test với mã phần trăm đạt max:**
- Chọn phòng giá 1,000,000₫/đêm  
- Chọn 1 đêm → Tổng: 1,000,000₫
- Áp dụng mã `SUMMER2024` (10%, max 50k)
  - Giảm tính: 1,000,000 * 10% = 100,000₫
  - **Nhưng max là 50k → Giảm: 50,000₫**
  - **Tổng sau giảm: 950,000₫**

**Test với mã số tiền cố định:**
- Chọn phòng giá 500,000₫/đêm
- Chọn 2 đêm → Tổng: 1,000,000₫
- Áp dụng mã `FIXED50K` (50,000₫)
  - **Giảm: 50,000₫**
  - **Tổng sau giảm: 950,000₫**

#### **2.3. Test Modal xác nhận đặt phòng**

1. Chọn ngày check-in/check-out
2. Chọn số khách
3. Áp dụng mã giảm giá (ví dụ: `WEEK3`)
4. Nhấn "📅 Đặt phòng ngay"

**Kiểm tra trong Modal:**
- ✅ Hiển thị: Ngày nhận/trả, số đêm, số khách
- ✅ Hiển thị: "🎟️ Mã: WEEK3 · Giảm: -X ₫"
- ✅ Hiển thị: "💰 Tổng cộng: X ₫" (đã giảm)

#### **2.4. Test Dialog xác nhận cuối**

1. Điền đầy đủ thông tin khách hàng trong Modal
2. Nhấn "✅ Xác nhận đặt phòng"

**Kiểm tra trong Dialog:**
- ✅ Hiển thị đầy đủ thông tin
- ✅ Hiển thị: "🎟️ Mã giảm giá: WEEK3"
- ✅ Hiển thị: "💸 Giảm: -X ₫"
- ✅ Hiển thị: "💰 Tổng tiền: X ₫" (đã giảm)

---

### **Bước 3: Test API trực tiếp (Advanced)**

#### **3.1. Test Validate Coupon API**

```bash
# Test validate (không cần auth)
curl "http://localhost:5130/api/coupons/validate?code=SUMMER2024"

# Kết quả mong đợi:
{
  "code": "SUMMER2024",
  "type": "percent",
  "value": 10,
  "maxDiscount": 50000,
  "description": "Giảm giá mùa hè 10%"
}
```

#### **3.2. Test Get Coupons (Admin)**

```bash
# Cần token Admin
curl -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  "http://localhost:5130/api/coupons"
```

#### **3.3. Test Create Coupon (Admin)**

```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -d '{
    "code": "NEWCODE",
    "description": "Mã test mới",
    "type": "percent",
    "value": 15,
    "maxDiscount": 75000,
    "maxUses": 100,
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-12-31T23:59:59Z",
    "isActive": true
  }' \
  "http://localhost:5130/api/coupons"
```

---

### **Bước 4: Test các trường hợp Edge Cases**

#### **4.1. Mã không tồn tại**
- Nhập: `INVALID123`
- ✅ Thông báo: "Mã giảm giá không tồn tại"

#### **4.2. Mã đã hết hạn**
- Tạo mã với ngày kết thúc là quá khứ
- Nhập mã đó
- ✅ Thông báo: "Mã giảm giá đã hết hạn"

#### **4.3. Mã đã hết lượt sử dụng**
- Cập nhật `UsesCount = MaxUses` trong database
- Nhập mã đó
- ✅ Thông báo: "Mã giảm giá đã hết lượt sử dụng"

#### **4.4. Mã bị tắt**
- Tắt mã `VIP15` trong admin
- Nhập mã đó
- ✅ Thông báo: "Mã giảm giá đã bị tắt"

#### **4.5. Mã chưa có hiệu lực**
- Tạo mã với ngày bắt đầu là tương lai
- Nhập mã đó
- ✅ Thông báo: "Mã giảm giá chưa có hiệu lực"

---

### **Bước 5: Test với Booking Flow hoàn chỉnh**

1. **Chọn phòng** → `room-detail.html?id=1`
2. **Chọn ngày** → Check-in: Hôm nay + 1, Check-out: Hôm nay + 3
3. **Áp dụng mã** → `WEEK3`
4. **Nhấn "Đặt phòng ngay"**
5. **Điền thông tin** trong Modal
6. **Xác nhận đặt phòng**
7. **Kiểm tra Booking được tạo** với `couponCode` trong database

**Query kiểm tra:**
```sql
SELECT BookingId, BookingCode, EstimatedTotalAmount, SpecialRequests 
FROM Bookings 
ORDER BY CreatedAt DESC 
LIMIT 1;
```

**Kiểm tra trong SpecialRequests hoặc tạo field riêng cho CouponCode trong Booking model**

---

## 🔍 Kiểm tra Database

```sql
-- Xem tất cả mã giảm giá
SELECT Code, Type, Value, MaxDiscount, UsesCount, IsActive, StartDate, EndDate 
FROM Coupons;

-- Xem mã đã được dùng bao nhiêu lần
SELECT Code, UsesCount, MaxUses 
FROM Coupons 
WHERE UsesCount > 0;

-- Tìm mã theo code
SELECT * FROM Coupons WHERE Code = 'SUMMER2024';
```

---

## ✅ Checklist Test

- [ ] Admin: Tạo mã mới thành công
- [ ] Admin: Sửa mã thành công  
- [ ] Admin: Bật/Tắt mã thành công
- [ ] Admin: Xóa mã thành công
- [ ] Customer: Nhập mã hợp lệ → Hiển thị giảm giá
- [ ] Customer: Nhập mã không tồn tại → Thông báo lỗi
- [ ] Customer: Nhập mã hết hạn → Thông báo lỗi
- [ ] Customer: Tính toán giảm giá đúng (% và số tiền)
- [ ] Customer: Modal hiển thị mã giảm giá
- [ ] Customer: Dialog xác nhận hiển thị mã giảm giá
- [ ] Booking: CouponCode được gửi lên server khi đặt phòng
- [ ] API: GET /api/coupons/validate hoạt động
- [ ] API: CRUD coupons hoạt động (Admin)

---

## 🐛 Troubleshooting

**Lỗi: "Mã sẽ được kiểm tra khi xác nhận đặt phòng"**
- ✅ **Bình thường** nếu customer không có quyền validate
- Mã sẽ được gửi kèm khi submit booking
- Backend sẽ validate khi nhận booking request

**Lỗi: "API mã giảm giá chưa được triển khai"**
- Kiểm tra server đã restart chưa
- Kiểm tra CouponsController có trong project
- Kiểm tra database có bảng Coupons chưa

**Lỗi: "Cannot set properties of undefined" (DataTable)**
- ✅ Đã fix - không còn lỗi này

---

## 📝 Notes

- Mã giảm giá được lưu trong `localStorage` khi customer áp dụng
- Nếu validate fail (403), mã vẫn được lưu để gửi lên khi đặt phòng
- Backend sẽ validate lại khi nhận booking với `couponCode`

