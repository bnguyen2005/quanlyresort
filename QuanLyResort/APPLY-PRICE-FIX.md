# Hướng Dẫn Sửa Giá Phòng Trong Database

## ✅ Đã Sửa DataSeeder.cs

Giá phòng đã được sửa trong `DataSeeder.cs`:
- Standard Room: `500,000` → `5,000 VND/đêm`
- Deluxe Room: `800,000` → `8,000 VND/đêm`
- Suite Room: `1,500,000` → `15,000 VND/đêm`
- Villa: `3,000,000` → `30,000 VND/đêm`

## 🔄 Cách Apply Fix Vào Database

### Option 1: Re-seed Database (Khuyến Nghị - SQLite)

**Nếu dùng SQLite (development):**

```bash
# 1. Stop backend (Ctrl+C)

# 2. Xóa database cũ
cd QuanLyResort
rm -f bin/Debug/net8.0/ResortManagementDb.db
# Hoặc tìm file .db trong thư mục bin/Debug/net8.0/

# 3. Restart backend
dotnet run

# Database sẽ tự động được tạo lại với giá đúng (5,000 VND)
```

### Option 2: Update Database Trực Tiếp (SQL Server)

**Nếu dùng SQL Server:**

1. **Mở SQL Server Management Studio**

2. **Connect đến database:** `ResortManagementDb`

3. **Chạy script:**
   ```sql
   -- Copy nội dung từ file: fix-prices-database.sql
   -- Hoặc chạy từng dòng:
   
   UPDATE RoomTypes SET BasePrice = 5000 WHERE TypeCode = 'STD';
   UPDATE RoomTypes SET BasePrice = 8000, ExtraPersonCharge = 2000 WHERE TypeCode = 'DLX';
   UPDATE RoomTypes SET BasePrice = 15000, ExtraPersonCharge = 3000 WHERE TypeCode = 'SUT';
   UPDATE RoomTypes SET BasePrice = 30000, ExtraPersonCharge = 5000 WHERE TypeCode = 'VIL';
   
   UPDATE Rooms SET PricePerNight = 5000 WHERE RoomType = 'Standard';
   UPDATE Rooms SET PricePerNight = 8000 WHERE RoomType = 'Deluxe';
   UPDATE Rooms SET PricePerNight = 15000 WHERE RoomType = 'Suite';
   UPDATE Rooms SET PricePerNight = 30000 WHERE RoomType = 'Villa';
   
   -- Fix existing bookings
   UPDATE Bookings SET EstimatedTotalAmount = EstimatedTotalAmount / 100 WHERE EstimatedTotalAmount >= 1000000;
   ```

### Option 3: Dùng Entity Framework Migrations

```bash
cd QuanLyResort

# Tạo migration mới
dotnet ef migrations add FixRoomPrices

# Apply migration
dotnet ef database update
```

## ✅ Sau Khi Sửa

1. **Restart backend** (nếu đang chạy)
2. **Refresh browser** (Ctrl+F5)
3. **Kiểm tra:**
   - Không còn cảnh báo về amount correction
   - Giá phòng hiển thị đúng (5,000 VND thay vì 500,000 VND)
   - QR code có số tiền đúng

## 🔍 Verify

Sau khi sửa, kiểm tra:

```sql
-- Kiểm tra RoomTypes
SELECT TypeCode, TypeName, BasePrice FROM RoomTypes;

-- Expected:
-- STD | Standard Room | 5000
-- DLX | Deluxe Room   | 8000
-- SUT | Suite Room    | 15000
-- VIL | Villa         | 30000

-- Kiểm tra Rooms
SELECT RoomNumber, RoomType, PricePerNight FROM Rooms;

-- Kiểm tra Bookings (nếu đã fix)
SELECT BookingId, BookingCode, EstimatedTotalAmount FROM Bookings;
```

## ⚠️ Lưu Ý

- **Existing bookings** sẽ có `EstimatedTotalAmount` được chia 100
- **New bookings** sẽ tự động dùng giá mới (đúng)
- **Frontend** sẽ không còn cảnh báo về amount correction

