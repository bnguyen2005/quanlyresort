# Hướng Dẫn Sửa Giá Phòng Trong Database

## ✅ Đã Sửa DataSeeder.cs

Giá phòng đã được sửa trong `DataSeeder.cs`:
- Standard Room: `500,000` → `5,000` VND/đêm
- Deluxe Room: `800,000` → `8,000` VND/đêm
- Suite Room: `1,500,000` → `15,000` VND/đêm
- Villa: `3,000,000` → `30,000` VND/đêm

## 🔄 Cách Cập Nhật Database Hiện Tại

### Option 1: Drop và Re-create Database (Khuyến Nghị - Nếu Dữ Liệu Test)

```bash
cd QuanLyResort

# Drop database
dotnet ef database drop --force

# Re-create với data mới
dotnet ef database update
```

Sau đó chạy lại ứng dụng để seed data mới.

### Option 2: Update Database Hiện Tại (Giữ Dữ Liệu)

Chạy SQL script:

```bash
# Nếu dùng SQL Server
sqlcmd -S (localdb)\mssqllocaldb -d ResortManagementDb -i update-room-prices.sql

# Hoặc mở SQL Server Management Studio và chạy file update-room-prices.sql
```

Hoặc chạy SQL trực tiếp:

```sql
-- Update RoomTypes
UPDATE RoomTypes SET BasePrice = 5000, ExtraPersonCharge = 2000 WHERE TypeCode = 'STD';
UPDATE RoomTypes SET BasePrice = 8000, ExtraPersonCharge = 2000 WHERE TypeCode = 'DLX';
UPDATE RoomTypes SET BasePrice = 15000, ExtraPersonCharge = 3000 WHERE TypeCode = 'SUT';
UPDATE RoomTypes SET BasePrice = 30000, ExtraPersonCharge = 5000 WHERE TypeCode = 'VIL';

-- Update Rooms
UPDATE Rooms SET PricePerNight = 5000 WHERE RoomType = 'Standard';
UPDATE Rooms SET PricePerNight = 8000 WHERE RoomType = 'Deluxe';
UPDATE Rooms SET PricePerNight = 15000 WHERE RoomType = 'Suite';
UPDATE Rooms SET PricePerNight = 30000 WHERE RoomType = 'Villa';

-- Update existing bookings (optional - chỉ nếu muốn sửa bookings cũ)
UPDATE Bookings 
SET EstimatedTotalAmount = EstimatedTotalAmount / 100
WHERE EstimatedTotalAmount >= 100000;

-- Update existing charges (optional)
UPDATE Charges
SET Amount = Amount / 100,
    TotalAmount = TotalAmount / 100
WHERE Amount >= 100000;
```

## ✅ Sau Khi Sửa

1. **Restart backend** để load data mới
2. **Refresh frontend** (Ctrl+F5)
3. **Kiểm tra:**
   - Giá phòng hiển thị đúng (5,000 VND thay vì 500,000 VND)
   - Không còn cảnh báo về amount correction
   - QR code có số tiền đúng

## 📋 Giá Mới

- **Standard Room:** 5,000 VND/đêm
- **Deluxe Room:** 8,000 VND/đêm  
- **Suite Room:** 15,000 VND/đêm
- **Villa:** 30,000 VND/đêm

## ⚠️ Lưu Ý

- Nếu có bookings thật (không phải test), cần cân nhắc trước khi update
- Script update sẽ chia tất cả `EstimatedTotalAmount >= 100,000` cho 100
- Nên backup database trước khi update

