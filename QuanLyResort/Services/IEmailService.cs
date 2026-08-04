namespace QuanLyResort.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendContactEmailAsync(string fromEmail, string fromName, string subject, string message);
}

