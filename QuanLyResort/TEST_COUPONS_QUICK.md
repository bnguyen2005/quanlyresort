# ⚡ Quick Test Guide - Mã giảm giá

## 🎯 Test nhanh nhất (3 phút)

### 1. Test Admin (1 phút)
```
1. Vào: http://localhost:5130/admin/html/coupons.html
2. Đăng nhập Admin
3. Xem danh sách mã (SUMMER2024, WEEK3, FIXED50K, VIP15)
4. Nhấn "Tạo Mã giảm giá" → Tạo mã TEST2024 → Lưu
```

### 2. Test Customer (2 phút)
```
1. Vào: http://localhost:5130/customer/room-detail.html?id=1
2. Chọn ngày (check-in: hôm nay +1, check-out: hôm nay +3)
3. Nhập mã: WEEK3
4. Nhấn "Áp dụng"
   → Nếu 403: Thấy "Mã sẽ được kiểm tra khi xác nhận đặt phòng"
   → Nếu OK: Thấy "Áp dụng mã thành công" + Tổng tiền giảm
5. Nhấn "Đặt phòng ngay"
6. Kiểm tra Modal → Thấy mã giảm giá và tổng tiền đã giảm
```

## 🎟️ Mã test sẵn có

- `SUMMER2024` - 10% (max 50k)
- `WEEK3` - 20% (không max)  
- `FIXED50K` - 50,000₫ cố định
- `VIP15` - 15% (max 100k)

## ✅ Kết quả mong đợi

**Thành công:**
- ✅ Mã được validate → Hiển thị giảm giá ngay
- ✅ Modal/Dialog hiển thị mã và tổng tiền đã giảm
- ✅ Booking được tạo với couponCode

**403 Forbidden (vẫn OK):**
- ⚠️ "Mã sẽ được kiểm tra khi xác nhận đặt phòng"
- ✅ Mã được lưu để gửi lên server khi đặt phòng
- ✅ Backend sẽ validate khi nhận booking

---

Xem chi tiết: `COUPON_TEST_GUIDE.md`

