SET NOCOUNT ON;

-- 1. Create a Customer if not exists
IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerId = 1)
BEGIN
    SET IDENTITY_INSERT Customers ON;
    INSERT INTO Customers (CustomerId, FullName, Email, PhoneNumber, PasswordHash, CreatedAt, IsActive)
    VALUES (1, 'Load Test User', 'test@example.com', '0123456789', 'hash', GETUTCDATE(), 1);
    SET IDENTITY_INSERT Customers OFF;
END

-- 2. Create some Rooms if not exists
IF NOT EXISTS (SELECT 1 FROM Rooms)
BEGIN
    INSERT INTO Rooms (RoomNumber, RoomType, Status, PricePerNight, Capacity, CreatedAt, Description)
    VALUES 
    ('101', 'Standard', 'Available', 100, 2, GETUTCDATE(), 'Standard Room 1'),
    ('102', 'Deluxe', 'Available', 200, 2, GETUTCDATE(), 'Deluxe Room 2'),
    ('103', 'Suite', 'Available', 500, 4, GETUTCDATE(), 'Suite Room 3'),
    ('104', 'Villa', 'Available', 1000, 8, GETUTCDATE(), 'Villa Room 4');
END

-- Ensure we have variables
DECLARE @CustomerId INT = (SELECT TOP 1 CustomerId FROM Customers);
DECLARE @RoomId INT = (SELECT TOP 1 RoomId FROM Rooms);

-- 3. Generate 1,000,000 Bookings using a Tally Table (CTE)
PRINT 'Starting bulk insert of 1,000,000 Bookings...';

WITH 
  E1(N) AS (SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1), -- 10
  E2(N) AS (SELECT 1 FROM E1 a CROSS JOIN E1 b), -- 100
  E4(N) AS (SELECT 1 FROM E2 a CROSS JOIN E2 b), -- 10,000
  E6(N) AS (SELECT 1 FROM E4 a CROSS JOIN E2 b), -- 1,000,000
  Tally(N) AS (SELECT TOP 1000000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM E6)
INSERT INTO Bookings (BookingCode, CustomerId, RoomId, RequestedRoomType, CheckInDate, CheckOutDate, NumberOfGuests, Status, EstimatedTotalAmount, BookedPrice, CreatedAt, Source)
SELECT 
    'BKG-' + CAST(NEWID() AS VARCHAR(36)),
    @CustomerId,
    @RoomId,
    CASE WHEN N % 4 = 0 THEN 'Standard' WHEN N % 4 = 1 THEN 'Deluxe' WHEN N % 4 = 2 THEN 'Suite' ELSE 'Villa' END,
    DATEADD(DAY, (N % 365), GETUTCDATE()),
    DATEADD(DAY, (N % 365) + 2, GETUTCDATE()),
    1,
    CASE WHEN N % 4 = 0 THEN 'Pending' WHEN N % 4 = 1 THEN 'Confirmed' WHEN N % 4 = 2 THEN 'CheckedIn' ELSE 'CheckedOut' END,
    200,
    100,
    GETUTCDATE(),
    'Direct'
FROM Tally;

PRINT 'Bulk insert completed!';
