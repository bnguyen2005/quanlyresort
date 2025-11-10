using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;

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

    public AIChatController(
        AIChatService aiChatService,
        ILogger<AIChatController> logger)
    {
        _aiChatService = aiChatService;
        _logger = logger;
    }

    /// <summary>
    /// Gửi message đến AI và nhận response
    /// Public endpoint - không cần authentication
    /// </summary>
    [HttpPost("send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message không được để trống" });
            }

            _logger.LogInformation("[AI Chat] 📨 Received message: {Message}", request.Message.Substring(0, Math.Min(50, request.Message.Length)));

            var response = await _aiChatService.SendMessageAsync(request.Message, request.Context);

            return Ok(new
            {
                success = true,
                message = response,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ❌ Error processing chat message");
            return StatusCode(500, new { error = "Đã xảy ra lỗi khi xử lý tin nhắn" });
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
}

/// <summary>
/// Request model cho chat message
/// </summary>
public class ChatMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
}

