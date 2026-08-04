-- Fix Room Prices in Database
-- Giá phòng hiện tại trong database đang bị nhân 100
-- Script này sẽ sửa lại giá đúng: 5,000 VND/đêm cho Standard Room

-- 1. Fix RoomTypes table
UPDATE RoomTypes 
SET BasePrice = 5000  -- Standard Room: 5,000 VND/đêm
WHERE TypeCode = 'STD';

UPDATE RoomTypes 
SET BasePrice = 8000,  -- Deluxe Room: 8,000 VND/đêm
    ExtraPersonCharge = 2000  -- 2,000 VND
WHERE TypeCode = 'DLX';

UPDATE RoomTypes 
SET BasePrice = 15000,  -- Suite Room: 15,000 VND/đêm
    ExtraPersonCharge = 3000  -- 3,000 VND
WHERE TypeCode = 'SUT';

UPDATE RoomTypes 
SET BasePrice = 30000,  -- Villa: 30,000 VND/đêm
    ExtraPersonCharge = 5000  -- 5,000 VND
WHERE TypeCode = 'VIL';

-- 2. Fix Rooms table
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

-- 3. Fix existing bookings (chia 100)
UPDATE Bookings 
SET EstimatedTotalAmount = EstimatedTotalAmount / 100
WHERE EstimatedTotalAmount >= 1000000;

-- 4. Fix Charges (chia 100)
UPDATE Charges 
SET Amount = Amount / 100,
    TotalAmount = TotalAmount / 100
WHERE Amount >= 100000;

-- 5. Fix Invoices (chia 100)
UPDATE Invoices 
SET SubTotal = SubTotal / 100,
    TaxAmount = TaxAmount / 100,
    TotalAmount = TotalAmount / 100,
    PaidAmount = PaidAmount / 100,
    BalanceDue = BalanceDue / 100
WHERE TotalAmount >= 1000000;

-- Verify
SELECT 
    TypeCode, 
    TypeName, 
    BasePrice,
    ExtraPersonCharge
FROM RoomTypes;

SELECT 
    RoomNumber,
    RoomType,
    PricePerNight
FROM Rooms;

