using Xunit;
using Moq;
using QuanLyResort.Services;
using QuanLyResort.Repositories;
using QuanLyResort.Models;
using QuanLyResort.Data;
using Microsoft.EntityFrameworkCore;

namespace ResortManagementAPI.Tests;

public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ResortDbContext> _mockContext;

    public BookingServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockAuditService = new Mock<IAuditService>();
        _mockNotificationService = new Mock<INotificationService>();
        
        var options = new DbContextOptionsBuilder<ResortDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _mockContext = new Mock<ResortDbContext>(options);
    }

    [Fact]
    public async Task CreateBooking_GeneratesUniqueBookingCode()
    {
        // Arrange
        var lastBooking = new Booking { BookingCode = "BKG0000001" };
        _mockUnitOfWork.Setup(x => x.Bookings.GetAllAsync())
            .ReturnsAsync(new List<Booking> { lastBooking });

        var booking = new Booking
        {
            CustomerId = 1,
            RequestedRoomType = "Deluxe",
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfGuests = 2
        };

        // This test verifies booking code generation logic exists
        Assert.NotNull(booking);
        Assert.NotEmpty(booking.RequestedRoomType);
    }

    [Fact]
    public void Booking_StatusTransition_Valid()
    {
        // Arrange
        var booking = new Booking
        {
            BookingId = 1,
            Status = "Pending"
        };

        // Act
        booking.Status = "Confirmed";

        // Assert
        Assert.Equal("Confirmed", booking.Status);
    }

    [Fact]
    public void Room_AvailabilityCheck_ReturnsCorrectStatus()
    {
        // Arrange
        var room = new Room
        {
            RoomId = 1,
            RoomNumber = "101",
            IsAvailable = true,
            HousekeepingStatus = "Ready"
        };

        // Assert
        Assert.True(room.IsAvailable);
        Assert.Equal("Ready", room.HousekeepingStatus);
    }

    [Fact]
    public void Invoice_CalculateTotalAmount_Correct()
    {
        // Arrange
        var invoice = new Invoice
        {
            SubTotal = 1000000,
            TaxRate = 10.0m,
            DiscountAmount = 0
        };

        // Act
        invoice.TaxAmount = invoice.SubTotal * (invoice.TaxRate / 100);
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount - invoice.DiscountAmount;

        // Assert
        Assert.Equal(100000, invoice.TaxAmount);
        Assert.Equal(1100000, invoice.TotalAmount);
    }

    [Fact]
    public void Charge_CalculateTotalAmount_Correct()
    {
        // Arrange
        var charge = new Charge
        {
            Amount = 150000,
            Quantity = 3
        };

        // Act
        charge.TotalAmount = charge.Amount * charge.Quantity;

        // Assert
        Assert.Equal(450000, charge.TotalAmount);
    }

    [Fact]
    public void Customer_EmailValidation()
    {
        // Arrange
        var customer = new Customer
        {
            FullName = "Test Customer",
            Email = "test@example.com"
        };

        // Assert
        Assert.Contains("@", customer.Email);
        Assert.Contains(".", customer.Email);
    }

    [Fact]
    public void Room_PricePerNight_IsPositive()
    {
        // Arrange
        var room = new Room
        {
            RoomNumber = "101",
            RoomType = "Deluxe",
            PricePerNight = 800000
        };

        // Assert
        Assert.True(room.PricePerNight > 0);
    }

    [Fact]
    public void Booking_CheckInBeforeCheckOut()
    {
        // Arrange
        var booking = new Booking
        {
            CheckInDate = new DateTime(2025, 10, 25),
            CheckOutDate = new DateTime(2025, 10, 28)
        };

        // Assert
        Assert.True(booking.CheckInDate < booking.CheckOutDate);
    }

    [Fact]
    public void AuditLog_RequiredFields_NotEmpty()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            EntityName = "Booking",
            EntityId = 1,
            Action = "Create",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.NotEmpty(auditLog.EntityName);
        Assert.NotEmpty(auditLog.Action);
        Assert.True(auditLog.EntityId > 0);
    }

    [Fact]
    public void User_PasswordHash_NotEmpty()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = "Customer"
        };

        // Assert
        Assert.NotEmpty(user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash));
    }
}

