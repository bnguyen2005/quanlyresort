-- Fix Room Prices in Database
-- Giá phòng hiện tại trong database đang bị nhân 100
-- Script này sẽ sửa lại giá đúng

-- Standard Room: 500,000 VND -> 5,000 VND (hoặc 50,000 VND nếu muốn giá cao hơn)
UPDATE RoomTypes 
SET BasePrice = 5000  -- Thay đổi thành giá mong muốn (5000 hoặc 50000)
WHERE TypeCode = 'STD';

-- Deluxe Room: 800,000 VND -> 8,000 VND (hoặc 80,000 VND)
UPDATE RoomTypes 
SET BasePrice = 8000  -- Thay đổi thành giá mong muốn (8000 hoặc 80000)
WHERE TypeCode = 'DLX';

-- Suite Room: 1,500,000 VND -> 15,000 VND (hoặc 150,000 VND)
UPDATE RoomTypes 
SET BasePrice = 15000  -- Thay đổi thành giá mong muốn (15000 hoặc 150000)
WHERE TypeCode = 'SUT';

-- Villa: 3,000,000 VND -> 30,000 VND (hoặc 300,000 VND)
UPDATE RoomTypes 
SET BasePrice = 30000  -- Thay đổi thành giá mong muốn (30000 hoặc 300000)
WHERE TypeCode = 'VIL';

-- Fix Rooms table
UPDATE Rooms 
SET PricePerNight = 5000  -- Thay đổi thành giá mong muốn
WHERE RoomType = 'Standard';

UPDATE Rooms 
SET PricePerNight = 8000  -- Thay đổi thành giá mong muốn
WHERE RoomType = 'Deluxe';

UPDATE Rooms 
SET PricePerNight = 15000  -- Thay đổi thành giá mong muốn
WHERE RoomType = 'Suite';

UPDATE Rooms 
SET PricePerNight = 30000  -- Thay đổi thành giá mong muốn
WHERE RoomType = 'Villa';

-- Fix existing bookings (optional - chỉ chạy nếu muốn sửa bookings cũ)
-- UPDATE Bookings 
-- SET EstimatedTotalAmount = EstimatedTotalAmount / 100
-- WHERE EstimatedTotalAmount >= 1000000;

