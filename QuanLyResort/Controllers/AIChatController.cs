using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

/// <summary>
/// Controller để xử lý AI Chat requests
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
    /// Gửi message đến AI và nhận response
    /// Public endpoint - không cần authentication, nhưng có rate limit 20 req/phút mỗi IP
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
                return BadRequest(new { error = "Message không được để trống" });
            }

            _logger.LogInformation("[AI Chat Controller] 📨 Received chat request");
            _logger.LogInformation("[AI Chat Controller] 📨 Message length: {Length}", request.Message?.Length ?? 0);
            _logger.LogInformation("[AI Chat Controller] 📨 Message preview: {Message}", request.Message?.Substring(0, Math.Min(50, request.Message?.Length ?? 0)) ?? "");
            _logger.LogInformation("[AI Chat Controller] 📨 Has context: {HasContext}", !string.IsNullOrEmpty(request.Context));

            // Get customer ID from JWT token if available
            int? customerId = null;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
            {
                customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            if (!string.IsNullOrEmpty(customerIdClaim) && int.TryParse(customerIdClaim, out var id))
            {
                customerId = id;
                _logger.LogInformation("[AI Chat Controller] 📨 Customer ID from token: {CustomerId}", customerId);
            }

            var response = await _aiChatService.SendMessageAsync(request.Message, request.Context, customerId);
            
            _logger.LogInformation("[AI Chat Controller] ✅ Got response from service");
            _logger.LogInformation("[AI Chat Controller] ✅ Response length: {Length}", response?.Length ?? 0);

            return Ok(new
            {
                success = true,
                message = response,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ❌ Error processing chat message: {Message}", ex.Message);
            
            var errorMessage = ex.Message.Contains("Unauthorized") || ex.Message.Contains("401")
                ? "API key không hợp lệ hoặc đã hết hạn"
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
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Debug endpoint - xem cấu hình AI và test gọi Groq trực tiếp
    /// XÓA endpoint này sau khi debug xong
    /// </summary>
    [HttpGet("debug")]
    [AllowAnonymous]
    public async Task<IActionResult> Debug([FromServices] IConfiguration config)
    {
        var aiConfig = config.GetSection("AIChat");
        var apiKey = aiConfig["ApiKey"] ?? "";
        var provider = aiConfig["Provider"] ?? "unknown";
        var model = aiConfig["Model"] ?? "unknown";
        var apiUrl = aiConfig["ApiUrl"] ?? "unknown";

        // Test gọi Groq trực tiếp
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
