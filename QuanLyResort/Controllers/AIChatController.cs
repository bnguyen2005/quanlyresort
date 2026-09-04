using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Repositories;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

/// <summary>
/// Controller xử lý AI Chat requests và cung cấp thông tin thời gian thực từ hệ thống Resort
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AIChatController : ControllerBase
{
    private readonly AIChatService _aiChatService;
    private readonly ILogger<AIChatController> _logger;
    private readonly IMemoryCache _cache;

    // Rate limit: 20 requests per minute per IP
    private const int RateLimitPerMinute = 20;

    public AIChatController(
        AIChatService aiChatService,
        ILogger<AIChatController> logger,
        IMemoryCache cache)
    {
        _aiChatService = aiChatService;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Gửi câu hỏi đến AI và nhận phản hồi được làm giàu dữ liệu từ Database
    /// Public endpoint - không bắt buộc đăng nhập, có rate limit 20 req/phút mỗi IP
    /// </summary>
    [HttpPost("send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
    {
        // --- Rate Limiting ---
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cacheKey = $"aichat_rate_{ip}";

        if (!_cache.TryGetValue(cacheKey, out int requestCount))
        {
            requestCount = 0;
        }

        if (requestCount >= RateLimitPerMinute)
        {
            _logger.LogWarning("[AI Chat] ⚠️ Rate limit exceeded for IP: {IP} ({Count} req/min)", ip, requestCount);
            return StatusCode(429, new { 
                success = false,
                error = "Bạn đã gửi quá nhiều tin nhắn. Vui lòng thử lại sau 1 phút.",
                retryAfterSeconds = 60
            });
        }

        _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(1));
        // --- End Rate Limiting ---

        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Tin nhắn không được để trống" });
            }

            _logger.LogInformation("[AI Chat Controller] 📨 Nhận câu hỏi: '{Message}'", request.Message);

            // Lấy CustomerId từ JWT token nếu khách đã đăng nhập
            int? customerId = null;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
            {
                customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            if (!string.IsNullOrEmpty(customerIdClaim) && int.TryParse(customerIdClaim, out var id))
            {
                customerId = id;
                _logger.LogInformation("[AI Chat Controller] 👤 Khách hàng xác thực: CustomerId={CustomerId}", customerId);
            }

            var response = await _aiChatService.SendMessageAsync(request.Message, request.Context, customerId);

            _logger.LogInformation("[AI Chat Controller] ✅ Đã phản hồi thành công (Độ dài: {Length} ký tự)", response?.Length ?? 0);

            return Ok(new
            {
                success = true,
                message = response,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ❌ Lỗi xử lý tin nhắn chat: {Message}", ex.Message);
            
            var errorMessage = ex.Message.Contains("Unauthorized") || ex.Message.Contains("401")
                ? "API Key AI hiện chưa hợp lệ hoặc đã hết hạn"
                : "Đã xảy ra lỗi khi xử lý tin nhắn";
                
            return StatusCode(500, new { 
                success = false,
                error = errorMessage,
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Health check cho AI Chat service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "active",
            service = "AI Chat",
            hasDb = _aiChatService.HasDbConnection,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Debug endpoint - kiểm tra cấu hình AI, kiểm tra kết nối Database và khả năng trích xuất dữ liệu thực tế
    /// </summary>
    [HttpGet("debug")]
    [AllowAnonymous]
    public async Task<IActionResult> Debug(
        [FromServices] IConfiguration config,
        [FromServices] IUnitOfWork? unitOfWork = null)
    {
        var aiConfig = config.GetSection("AIChat");
        var apiKey = aiConfig["ApiKey"] ?? "";
        var provider = aiConfig["Provider"] ?? "unknown";
        var model = aiConfig["Model"] ?? "unknown";
        var apiUrl = aiConfig["ApiUrl"] ?? "unknown";

        // 1. Kiểm tra truy xuất Database thực tế
        bool dbConnected = false;
        int roomTypesCount = 0;
        int roomsCount = 0;
        int servicesCount = 0;
        int faqsCount = 0;
        int reviewsCount = 0;
        string dbSampleDataPreview = "";

        try
        {
            if (unitOfWork?.Context != null)
            {
                dbConnected = await unitOfWork.Context.Database.CanConnectAsync();
                roomTypesCount = await unitOfWork.Context.RoomTypes.CountAsync();
                roomsCount = await unitOfWork.Context.Rooms.CountAsync();
                servicesCount = await unitOfWork.Context.Services.CountAsync();
                faqsCount = await unitOfWork.Context.FAQs.CountAsync();
                reviewsCount = await unitOfWork.Context.Reviews.CountAsync();

                // Test gọi trích xuất dữ liệu thật
                dbSampleDataPreview = await _aiChatService.FetchRealDataAsync("giá phòng và thực đơn nhà hàng");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat Debug] Lỗi khi kiểm tra Database");
        }

        // 2. Test gọi Groq trực tiếp nếu cấu hình
        string groqTestResult = "not tested";
        string groqStatusCode = "";
        string groqResponse = "";

        if (!string.IsNullOrEmpty(apiKey) && provider == "groq")
        {
            try
            {
                using var http = new System.Net.Http.HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var testBody = System.Text.Json.JsonSerializer.Serialize(new
                {
                    model = model,
                    messages = new[] { new { role = "user", content = "Say hello in 5 words" } },
                    max_tokens = 20
                });

                var resp = await http.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new System.Net.Http.StringContent(testBody, System.Text.Encoding.UTF8, "application/json")
                );

                groqStatusCode = ((int)resp.StatusCode).ToString();
                groqResponse = await resp.Content.ReadAsStringAsync();
                groqTestResult = resp.IsSuccessStatusCode ? "SUCCESS" : "FAILED";
            }
            catch (Exception ex)
            {
                groqTestResult = "EXCEPTION";
                groqResponse = ex.Message;
            }
        }

        return Ok(new
        {
            config = new
            {
                provider,
                model,
                apiUrl,
                apiKeySet = !string.IsNullOrEmpty(apiKey),
                apiKeyPrefix = apiKey.Length > 10 ? apiKey.Substring(0, 10) + "..." : "(empty)"
            },
            database = new
            {
                connected = dbConnected,
                roomTypesCount,
                roomsCount,
                servicesCount,
                faqsCount,
                reviewsCount,
                realDataFetchedSuccessfully = !string.IsNullOrEmpty(dbSampleDataPreview),
                realDataPreviewLength = dbSampleDataPreview.Length,
                realDataPreview = dbSampleDataPreview.Length > 500 ? dbSampleDataPreview.Substring(0, 500) + "..." : dbSampleDataPreview
            },
            groqTest = new
            {
                result = groqTestResult,
                statusCode = groqStatusCode,
                responsePreview = groqResponse.Length > 300 ? groqResponse.Substring(0, 300) : groqResponse
            }
        });
    }
}

/// <summary>
/// Request model cho chat message
/// </summary>
public class ChatMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
}
