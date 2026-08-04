using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IEmailService emailService, ILogger<ContactController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Gửi email liên hệ từ form trên website
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SendContact([FromBody] ContactRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { success = false, message = "Họ và tên là bắt buộc" });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            {
                return BadRequest(new { success = false, message = "Email không hợp lệ" });
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return BadRequest(new { success = false, message = "Chủ đề là bắt buộc" });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Nội dung là bắt buộc" });
            }

            _logger.LogInformation("[Contact] 📧 Received contact form submission from {Name} ({Email})", 
                request.FullName, request.Email);

            var success = await _emailService.SendContactEmailAsync(
                request.Email,
                request.FullName,
                request.Subject,
                request.Message
            );

            if (success)
            {
                _logger.LogInformation("[Contact] ✅ Contact email sent successfully");
                return Ok(new 
                { 
                    success = true, 
                    message = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể." 
                });
            }
            else
            {
                _logger.LogWarning("[Contact] ⚠️ Failed to send contact email (SMTP not configured)");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Không thể gửi email. Vui lòng thử lại sau hoặc liên hệ trực tiếp qua số điện thoại." 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Contact] ❌ Error processing contact request: {Message}", ex.Message);
            return StatusCode(500, new 
            { 
                success = false, 
                message = "Đã xảy ra lỗi khi xử lý yêu cầu. Vui lòng thử lại sau." 
            });
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class ContactRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

