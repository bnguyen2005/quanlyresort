using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Net.Http;

namespace QuanLyResort.Services;

public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly string? _smsProvider;
    private readonly string? _smsApiKey;
    private readonly string? _smsApiSecret;
    private readonly string? _smsApiUrl;
    private readonly string? _smsSenderId;
    private readonly string? _smsAccountSid; // For Twilio
    private readonly bool _smsEnabled;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _smsProvider = _configuration["SmsSettings:Provider"] ?? "generic";
        _smsApiKey = _configuration["SmsSettings:ApiKey"];
        _smsApiSecret = _configuration["SmsSettings:ApiSecret"];
        _smsApiUrl = _configuration["SmsSettings:ApiUrl"];
        _smsSenderId = _configuration["SmsSettings:SenderId"] ?? "RESORT";
        _smsAccountSid = _configuration["SmsSettings:AccountSid"]; // For Twilio
        _smsEnabled = bool.Parse(_configuration["SmsSettings:Enabled"] ?? "false");
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            if (!_smsEnabled)
            {
                _logger.LogInformation("[SmsService] 📱 SMS disabled. Would send to {PhoneNumber}: {Message}", phoneNumber, message);
                return false;
            }

            if (string.IsNullOrEmpty(_smsApiKey))
            {
                _logger.LogWarning("[SmsService] ⚠️ SMS API key not configured. SMS will not be sent.");
                return false;
            }

            // Format phone number (remove spaces, add country code if needed)
            var formattedPhone = FormatPhoneNumber(phoneNumber);

            // Route to appropriate provider
            return _smsProvider?.ToLower() switch
            {
                "twilio" => await SendViaTwilioAsync(formattedPhone, message),
                "aws" => await SendViaAwsSnsAsync(formattedPhone, message),
                "generic" => await SendViaGenericApiAsync(formattedPhone, message),
                _ => await SendViaGenericApiAsync(formattedPhone, message)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SmsService] ❌ Error sending SMS to {PhoneNumber}: {Message}", phoneNumber, ex.Message);
            return false;
        }
    }

    public async Task<bool> SendBookingConfirmationSmsAsync(string phoneNumber, string bookingCode, DateTime checkInDate, DateTime checkOutDate)
    {
        var message = $"🎉 Đặt phòng thành công!\n" +
                     $"Mã đặt phòng: {bookingCode}\n" +
                     $"Ngày nhận phòng: {checkInDate:dd/MM/yyyy}\n" +
                     $"Ngày trả phòng: {checkOutDate:dd/MM/yyyy}\n" +
                     $"Cảm ơn bạn đã chọn Resort Deluxe!";
        
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendPaymentConfirmationSmsAsync(string phoneNumber, string invoiceNumber, decimal amount)
    {
        var formattedAmount = amount.ToString("N0") + " ₫";
        var message = $"✅ Thanh toán thành công!\n" +
                     $"Mã hóa đơn: {invoiceNumber}\n" +
                     $"Số tiền: {formattedAmount}\n" +
                     $"Cảm ơn bạn!";
        
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendOrderConfirmationSmsAsync(string phoneNumber, string orderNumber, decimal amount)
    {
        var formattedAmount = amount.ToString("N0") + " ₫";
        var message = $"🍽️ Đặt món thành công!\n" +
                     $"Mã đơn: {orderNumber}\n" +
                     $"Tổng tiền: {formattedAmount}\n" +
                     $"Cảm ơn bạn!";
        
        return await SendSmsAsync(phoneNumber, message);
    }

    private string FormatPhoneNumber(string phoneNumber)
    {
        // Remove spaces and special characters
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        // Add country code if not present (Vietnam: +84)
        if (!cleaned.StartsWith("+84") && !cleaned.StartsWith("84"))
        {
            if (cleaned.StartsWith("0"))
            {
                cleaned = "+84" + cleaned.Substring(1);
            }
            else
            {
                cleaned = "+84" + cleaned;
            }
        }
        else if (cleaned.StartsWith("84") && !cleaned.StartsWith("+84"))
        {
            cleaned = "+" + cleaned;
        }
        
        return cleaned;
    }

    // Twilio SMS Provider
    private async Task<bool> SendViaTwilioAsync(string phoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrEmpty(_smsAccountSid) || string.IsNullOrEmpty(_smsApiSecret))
            {
                _logger.LogWarning("[SmsService] ⚠️ Twilio AccountSid or AuthToken not configured");
                return false;
            }

            using var httpClient = new HttpClient();
            var authBytes = System.Text.Encoding.ASCII.GetBytes($"{_smsAccountSid}:{_smsApiSecret}");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var twilioUrl = $"https://api.twilio.com/2010-04-01/Accounts/{_smsAccountSid}/Messages.json";
            var formData = new List<KeyValuePair<string, string>>
            {
                new("From", _smsSenderId ?? "+1234567890"), // Twilio phone number
                new("To", phoneNumber),
                new("Body", message)
            };

            var response = await httpClient.PostAsync(twilioUrl, new FormUrlEncodedContent(formData));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[SmsService] ✅ Twilio SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            else
            {
                _logger.LogError("[SmsService] ❌ Twilio SMS failed: {StatusCode} - {Error}", response.StatusCode, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SmsService] ❌ Twilio SMS error: {Message}", ex.Message);
            return false;
        }
    }

    // AWS SNS SMS Provider
    private async Task<bool> SendViaAwsSnsAsync(string phoneNumber, string message)
    {
        try
        {
            // AWS SNS requires AWS SDK or custom implementation with AWS signature
            // This is a simplified version - in production, use AWS SDK for .NET
            _logger.LogWarning("[SmsService] ⚠️ AWS SNS requires AWS SDK. Using generic API fallback.");
            return await SendViaGenericApiAsync(phoneNumber, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SmsService] ❌ AWS SNS error: {Message}", ex.Message);
            return false;
        }
    }

    // Generic HTTP API (for most SMS gateways)
    private async Task<bool> SendViaGenericApiAsync(string phoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrEmpty(_smsApiUrl))
            {
                _logger.LogWarning("[SmsService] ⚠️ SMS API URL not configured");
                return false;
            }

            using var httpClient = new HttpClient();
            
            // Add authorization header if API key is provided
            if (!string.IsNullOrEmpty(_smsApiKey))
            {
                if (_smsApiKey.StartsWith("Bearer ") || _smsApiKey.StartsWith("Basic "))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", _smsApiKey);
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_smsApiKey}");
                }
            }

            // Generic payload format (adjust based on your SMS provider's API)
            var payload = new
            {
                to = phoneNumber,
                message = message,
                sender = _smsSenderId,
                // Add other fields your provider might need
                api_key = _smsApiKey
            };

            var response = await httpClient.PostAsJsonAsync(_smsApiUrl, payload);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[SmsService] ✅ Generic SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            else
            {
                _logger.LogError("[SmsService] ❌ Generic SMS failed: {StatusCode} - {Error}", response.StatusCode, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SmsService] ❌ Generic SMS error: {Message}", ex.Message);
            return false;
        }
    }
}

