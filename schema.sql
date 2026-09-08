CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PhoneNumber NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Rooms (
    RoomId INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber NVARCHAR(50) NOT NULL,
    RoomType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    PricePerNight DECIMAL(18,2) NOT NULL,
    Capacity INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    Description NVARCHAR(MAX) NULL
);

CREATE TABLE Bookings (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    BookingCode NVARCHAR(50) NOT NULL,
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    RoomId INT NULL FOREIGN KEY REFERENCES Rooms(RoomId),
    RequestedRoomType NVARCHAR(50) NOT NULL,
    CheckInDate DATETIME2 NOT NULL,
    CheckOutDate DATETIME2 NOT NULL,
    NumberOfGuests INT NOT NULL DEFAULT 1,
    Status NVARCHAR(30) NOT NULL,
    EstimatedTotalAmount DECIMAL(18,2) NULL,
    BookedPrice DECIMAL(18,2) NOT NULL,
    SpecialRequests NVARCHAR(1000) NULL,
    Source NVARCHAR(50) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    ActualCheckInTime DATETIME2 NULL,
    ActualCheckOutTime DATETIME2 NULL,
    CancellationReason NVARCHAR(500) NULL
);
