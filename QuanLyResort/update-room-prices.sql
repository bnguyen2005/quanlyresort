-- Update Room Prices in Existing Database
-- Chạy script này để sửa giá phòng trong database hiện có

-- Update RoomTypes
UPDATE RoomTypes 
SET BasePrice = 5000,
    ExtraPersonCharge = 2000
WHERE TypeCode = 'STD';

UPDATE RoomTypes 
SET BasePrice = 8000,
    ExtraPersonCharge = 2000
WHERE TypeCode = 'DLX';

UPDATE RoomTypes 
SET BasePrice = 15000,
    ExtraPersonCharge = 3000
WHERE TypeCode = 'SUT';

UPDATE RoomTypes 
SET BasePrice = 30000,
    ExtraPersonCharge = 5000
WHERE TypeCode = 'VIL';

-- Update Rooms
UPDATE Rooms 
SET PricePerNight = 5000
WHERE RoomType = 'Standard';

UPDATE Rooms 
SET PricePerNight = 8000
WHERE RoomType = 'Deluxe';

UPDATE Rooms 
SET PricePerNight = 15000
WHERE RoomType = 'Suite';

UPDATE Rooms 
SET PricePerNight = 30000
WHERE RoomType = 'Villa';

-- Update existing bookings (chia 100)
UPDATE Bookings 
SET EstimatedTotalAmount = EstimatedTotalAmount / 100
WHERE EstimatedTotalAmount >= 100000;

-- Update existing charges (chia 100)
UPDATE Charges
SET Amount = Amount / 100,
    TotalAmount = TotalAmount / 100
WHERE Amount >= 100000;

