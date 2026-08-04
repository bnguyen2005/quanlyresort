using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyResort.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string? _smtpHost;
    private readonly int _smtpPort;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;
    private readonly string? _smtpFromEmail;
    private readonly string? _smtpFromName;
    private readonly bool _smtpEnableSsl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Read SMTP settings from configuration
        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        _smtpFromEmail = _configuration["EmailSettings:FromEmail"] ?? _smtpUsername;
        _smtpFromName = _configuration["EmailSettings:FromName"] ?? "Resort Deluxe";
        _smtpEnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogWarning("[EmailService] ⚠️ SMTP credentials not configured. Email will not be sent.");
                return false;
            }

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _smtpEnableSsl,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpFromEmail!, _smtpFromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("[EmailService] ✅ Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EmailService] ❌ Failed to send email to {To}: {Message}", to, ex.Message);
            return false;
        }
    }

    public async Task<bool> SendContactEmailAsync(string fromEmail, string fromName, string subject, string message)
    {
        try
        {
            // Get recipient email from configuration (default to phamthahlam@gmail.com)
            var recipientEmail = _configuration["EmailSettings:ContactRecipient"] ?? "phamthahlam@gmail.com";
            
            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #c8a97e 0%, #b89968 100%); color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }}
        .info-row {{ margin: 10px 0; padding: 10px; background: white; border-radius: 4px; }}
        .label {{ font-weight: bold; color: #c8a97e; }}
        .message-box {{ background: white; padding: 15px; border-left: 4px solid #c8a97e; margin-top: 15px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2 style=""margin: 0;"">📧 Liên hệ mới từ Resort Deluxe</h2>
        </div>
        <div class=""content"">
            <div class=""info-row"">
                <span class=""label"">👤 Tên người gửi:</span> {fromName}
            </div>
            <div class=""info-row"">
                <span class=""label"">📧 Email:</span> {fromEmail}
            </div>
            <div class=""info-row"">
                <span class=""label"">📝 Chủ đề:</span> {subject}
            </div>
            <div class=""message-box"">
                <div class=""label"">💬 Nội dung:</div>
                <div style=""margin-top: 10px; white-space: pre-wrap;"">{message}</div>
            </div>
            <div style=""margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666;"">
                <p>Email này được gửi tự động từ form liên hệ trên website Resort Deluxe.</p>
                <p>Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
            </div>
        </div>
    </div>
</body>
</html>";

            var emailSubject = $"[Resort Deluxe] Liên hệ: {subject}";
            
            return await SendEmailAsync(recipientEmail, emailSubject, emailBody, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EmailService] ❌ Failed to send contact email: {Message}", ex.Message);
            return false;
        }
    }
}

