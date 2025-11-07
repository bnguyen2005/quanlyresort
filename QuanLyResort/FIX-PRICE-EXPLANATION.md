# Giải Thích Cảnh Báo Về Giá Phòng

## ⚠️ Cảnh Báo Hiện Tại

Console đang hiển thị nhiều cảnh báo:
```
⚠️ [renderBookings] Booking 31: Amount too large for 1 night, corrected by dividing by 100: 5000
⚠️ [renderBookings] Booking 29: Backend amount >= 1M, corrected by dividing by 100: 20000
```

## 🔍 Nguyên Nhân

**Database đang lưu giá phòng bị nhân 100:**
- Standard Room: `BasePrice = 500000` (nên là 5,000 VND hoặc 50,000 VND)
- Deluxe Room: `BasePrice = 800000` (nên là 8,000 VND hoặc 80,000 VND)
- Suite Room: `BasePrice = 1500000` (nên là 15,000 VND hoặc 150,000 VND)
- Villa: `BasePrice = 3000000` (nên là 30,000 VND hoặc 300,000 VND)

## ✅ Giải Pháp

### Option 1: Sửa Database (Khuyến Nghị)

**Nếu giá đúng nên là 5,000 VND/đêm cho Standard Room:**

1. Chạy SQL script:
   ```bash
   # Sử dụng SQL Server Management Studio hoặc SQLite Browser
   # Hoặc dùng dotnet ef migrations
   ```

2. Hoặc sửa trực tiếp trong `DataSeeder.cs`:
   ```csharp
   BasePrice = 5000,  // Thay vì 500000
   ```

3. Re-seed database:
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```

### Option 2: Giữ Nguyên Database, Xóa Cảnh Báo

Nếu giá 500,000 VND là đúng (500k VND/đêm), thì xóa logic chia 100 trong frontend.

**Sửa trong `my-bookings.html` và `simple-payment.js`:**
- Xóa hoặc comment các đoạn code chia 100
- Xóa warnings về amount correction

## 💡 Khuyến Nghị

**Giá phòng hợp lý cho resort:**
- Standard Room: **50,000 - 500,000 VND/đêm** (tùy vào resort)
- Deluxe Room: **80,000 - 800,000 VND/đêm**
- Suite Room: **150,000 - 1,500,000 VND/đêm**
- Villa: **300,000 - 3,000,000 VND/đêm**

**Nếu giá hiện tại (500,000 VND) là đúng:**
- Xóa logic correction trong frontend
- Giữ nguyên database

**Nếu giá nên là 5,000 VND:**
- Sửa database: `BasePrice = 5000`
- Xóa logic correction trong frontend

## 🛠️ Cách Sửa Nhanh

### Sửa Database (Nếu giá nên là 5,000 VND):

```sql
UPDATE RoomTypes SET BasePrice = 5000 WHERE TypeCode = 'STD';
UPDATE RoomTypes SET BasePrice = 8000 WHERE TypeCode = 'DLX';
UPDATE RoomTypes SET BasePrice = 15000 WHERE TypeCode = 'SUT';
UPDATE RoomTypes SET BasePrice = 30000 WHERE TypeCode = 'VIL';
```

### Hoặc Sửa DataSeeder.cs:

```csharp
BasePrice = 5000,  // Standard Room
BasePrice = 8000,  // Deluxe Room
BasePrice = 15000, // Suite Room
BasePrice = 30000, // Villa
```

Sau đó re-seed database.

