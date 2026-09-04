using Moq;
using Xunit;
using QuanLyResort.Services;
using QuanLyResort.Models;
using QuanLyResort.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using QuanLyResort.Data;
using Microsoft.EntityFrameworkCore;

namespace QuanLyResort.Tests.Services;

public class AIChatServiceTests
{
    private readonly ResortDbContext _context;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<AIChatService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly AIChatService _aiChatService;

    public AIChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<ResortDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_AIChat_Db_" + Guid.NewGuid().ToString())
            .Options;
        _context = new ResortDbContext(options);

        // Seed sample database data
        _context.RoomTypes.AddRange(
            new RoomType { RoomTypeId = 1, TypeName = "Standard Deluxe", TypeCode = "STD", BasePrice = 1200000, StandardOccupancy = 2, MaxOccupancy = 3, IsActive = true },
            new RoomType { RoomTypeId = 2, TypeName = "Ocean Villa", TypeCode = "VIL", BasePrice = 3500000, StandardOccupancy = 4, MaxOccupancy = 6, IsActive = true }
        );

        _context.Rooms.AddRange(
            new Room { RoomId = 1, RoomNumber = "101", RoomType = "Standard Deluxe", PricePerNight = 1200000, IsAvailable = true },
            new Room { RoomId = 2, RoomNumber = "201", RoomType = "Ocean Villa", PricePerNight = 3500000, IsAvailable = true }
        );

        _context.Services.AddRange(
            new Service { ServiceId = 1, ServiceName = "Bò Bít Tết Wagyu", ServiceType = "Restaurant", Price = 450000, Unit = "phần", IsActive = true },
            new Service { ServiceId = 2, ServiceName = "Cocktail Mojito", ServiceType = "Restaurant", Price = 120000, Unit = "ly", IsActive = true },
            new Service { ServiceId = 3, ServiceName = "Massage Trị Liệu Toàn Thân", ServiceType = "Spa", Price = 600000, Unit = "suất 60 phút", IsActive = true }
        );

        _context.SaveChanges();

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Context).Returns(_context);

        _mockLogger = new Mock<ILogger<AIChatService>>();

        var configValues = new Dictionary<string, string?>
        {
            {"AIChat:Provider", "sample"},
            {"AIChat:ApiKey", ""}
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        _httpClient = new HttpClient();

        _aiChatService = new AIChatService(
            _configuration,
            _mockLogger.Object,
            _httpClient,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task FetchRealDataAsync_WhenAskingAboutRooms_ReturnsDatabaseRoomTypesAndPrices()
    {
        // Act - query in Vietnamese with accents
        var result = await _aiChatService.FetchRealDataAsync("Cho tôi hỏi giá phòng của resort");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Standard Deluxe", result);
        Assert.Contains("Ocean Villa", result);
        Assert.Contains("1,200,000 VND", result);
        Assert.Contains("3,500,000 VND", result);
    }

    [Fact]
    public async Task FetchRealDataAsync_WhenAskingWithoutAccents_StillMatchesAndReturnsDatabaseData()
    {
        // Act - query in unaccented Vietnamese
        var result = await _aiChatService.FetchRealDataAsync("gia phong va phong trong");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Standard Deluxe", result);
        Assert.Contains("Phòng 101", result);
    }

    [Fact]
    public async Task FetchRealDataAsync_WhenAskingAboutRestaurant_ReturnsDatabaseMenuItems()
    {
        // Act
        var result = await _aiChatService.FetchRealDataAsync("Nhà hàng có món gì ngon?");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Bò Bít Tết Wagyu", result);
        Assert.Contains("450,000 VND", result);
        Assert.Contains("Cocktail Mojito", result);
    }

    [Fact]
    public async Task SendMessageAsync_InSampleMode_ReturnsResponseContainingRealDatabasePrices()
    {
        // Act
        var response = await _aiChatService.SendMessageAsync("Tư vấn phòng giúp tôi");

        // Assert
        Assert.NotNull(response);
        Assert.Contains("THÔNG TIN PHÒNG VÀ GIÁ", response);
        Assert.Contains("Standard Deluxe", response);
        Assert.Contains("1,200,000 VND", response);
    }
}
