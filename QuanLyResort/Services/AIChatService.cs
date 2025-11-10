using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyResort.Services;

/// <summary>
/// Service để tương tác với AI Chat API
/// Hỗ trợ OpenAI hoặc các AI service khác
/// </summary>
public class AIChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIChatService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _apiUrl;
    private readonly string _model;
    private readonly string _provider; // "openai", "groq", "huggingface", "cohere", "sample"

    public AIChatService(
        IConfiguration configuration,
        ILogger<AIChatService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        var aiConfig = _configuration.GetSection("AIChat");
        _apiKey = aiConfig["ApiKey"];
        _provider = aiConfig["Provider"] ?? "sample"; // Default to sample if no provider specified
        _model = aiConfig["Model"] ?? "gpt-3.5-turbo";
        
        // Set API URL based on provider
        if (string.IsNullOrEmpty(_apiKey) || _provider == "sample")
        {
            _apiUrl = "";
            _logger.LogInformation("[AI Chat] Using sample responses (no API key or provider=sample)");
        }
        else if (_provider == "groq")
        {
            _apiUrl = aiConfig["ApiUrl"] ?? "https://api.groq.com/openai/v1/chat/completions";
            _model = aiConfig["Model"] ?? "llama-3.1-8b-instant"; // Groq free model
        }
        else if (_provider == "huggingface")
        {
            _apiUrl = aiConfig["ApiUrl"] ?? $"https://api-inference.huggingface.co/models/{_model}";
        }
        else if (_provider == "cohere")
        {
            _apiUrl = aiConfig["ApiUrl"] ?? "https://api.cohere.ai/v1/chat";
        }
        else // Default to OpenAI
        {
            _apiUrl = aiConfig["ApiUrl"] ?? "https://api.openai.com/v1/chat/completions";
        }

            if (!string.IsNullOrEmpty(_apiKey) && _provider != "sample")
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "ResortDeluxe-AIChat/1.0");
                
                // Hugging Face cần header đặc biệt
                if (_provider == "huggingface")
                {
                    _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
                }
                
                _logger.LogInformation("[AI Chat] ✅ API Key configured (length: {Length}, provider: {Provider})", _apiKey.Length, _provider);
            }
            else
            {
                _logger.LogInformation("[AI Chat] 📝 Using sample responses (no API key or provider=sample)");
            }
        
        // Set timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _logger.LogInformation("[AI Chat] ✅ Service initialized - Provider: {Provider}, Model: {Model}, API URL: {ApiUrl}", _provider, _model, _apiUrl);
    }

    /// <summary>
    /// Gửi message đến AI và nhận response
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, string? conversationContext = null)
    {
        try
        {
            // Nếu không có API key, trả về response mẫu
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("[AI Chat] ⚠️ No API key configured, returning sample response");
                return GetSampleResponse(userMessage);
            }

            // Tạo system prompt cho resort context
            var systemPrompt = @"Bạn là trợ lý AI thân thiện của Resort Deluxe. 
Bạn giúp khách hàng với các câu hỏi về:
- Đặt phòng và booking
- Dịch vụ resort (nhà hàng, spa, hồ bơi, v.v.)
- Thanh toán và hóa đơn
- Chính sách hủy và đổi
- Thông tin về phòng và tiện nghi
- Hướng dẫn sử dụng website

Hãy trả lời ngắn gọn, thân thiện và hữu ích bằng tiếng Việt.";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Thêm context nếu có
            if (!string.IsNullOrEmpty(conversationContext))
            {
                messages.Add(new { role = "assistant", content = conversationContext });
            }

            // Thêm user message
            messages.Add(new { role = "user", content = userMessage });

            // Tạo request body tùy theo provider
            object requestBody;
            if (_provider == "cohere")
            {
                // Cohere có format khác
                requestBody = new
                {
                    message = userMessage,
                    model = _model,
                    temperature = 0.7,
                    max_tokens = 500
                };
            }
            else if (_provider == "huggingface")
            {
                // Hugging Face có format khác
                requestBody = new
                {
                    inputs = userMessage,
                    parameters = new
                    {
                        max_new_tokens = 500,
                        temperature = 0.7
                    }
                };
            }
            else
            {
                // OpenAI/Groq format (standard)
                requestBody = new
                {
                    model = _model,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 500
                };
            }

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("[AI Chat] 📤 Sending message to AI");
            _logger.LogInformation("[AI Chat] 📤 Message preview: {Message}", userMessage.Substring(0, Math.Min(50, userMessage.Length)));
            _logger.LogInformation("[AI Chat] 📤 API URL: {ApiUrl}", _apiUrl);
            _logger.LogInformation("[AI Chat] 📤 Model: {Model}", _model);
            _logger.LogInformation("[AI Chat] 📤 Has API Key: {HasKey}", !string.IsNullOrEmpty(_apiKey));
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogInformation("[AI Chat] 📤 API Key prefix: {Prefix}", _apiKey.Substring(0, Math.Min(10, _apiKey.Length)));
            }

            _logger.LogInformation("[AI Chat] 📤 Request body: {Body}", json);

            var response = await _httpClient.PostAsync(_apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("[AI Chat] 📥 Response status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("[AI Chat] 📥 Response headers: {Headers}", string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[AI Chat] ❌ API Error - Status: {StatusCode}", response.StatusCode);
                _logger.LogError("[AI Chat] ❌ API Error - Response: {Response}", responseContent);
                _logger.LogError("[AI Chat] ❌ API Error - Request URL: {Url}", _apiUrl);
                _logger.LogError("[AI Chat] ❌ API Error - API Key configured: {HasKey}", !string.IsNullOrEmpty(_apiKey));
                
                // Xử lý các lỗi cụ thể
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("[AI Chat] ❌ Unauthorized (401) - API Key có thể không hợp lệ hoặc đã hết hạn");
                    _logger.LogError("[AI Chat] ❌ Check API Key in configuration");
                    return "Xin lỗi, API key không hợp lệ. Vui lòng liên hệ quản trị viên để cập nhật cấu hình.";
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError("[AI Chat] ❌ Rate limit exceeded (429)");
                    return "Xin lỗi, hệ thống đang quá tải. Vui lòng thử lại sau vài phút.";
                }
                
                _logger.LogError("[AI Chat] ❌ Other error: {StatusCode}", response.StatusCode);
                return "Xin lỗi, tôi gặp sự cố khi xử lý câu hỏi của bạn. Vui lòng thử lại sau hoặc liên hệ bộ phận hỗ trợ.";
            }

            _logger.LogInformation("[AI Chat] 📥 Response content length: {Length}", responseContent.Length);
            _logger.LogInformation("[AI Chat] 📥 Response preview: {Preview}", responseContent.Substring(0, Math.Min(200, responseContent.Length)));

            // Parse response tùy theo provider
            string? aiResponse = null;
            
            if (_provider == "cohere")
            {
                var responseJson = JsonDocument.Parse(responseContent);
                aiResponse = responseJson.RootElement
                    .GetProperty("text")
                    .GetString();
            }
            else if (_provider == "huggingface")
            {
                var responseJson = JsonDocument.Parse(responseContent);
                // Hugging Face trả về array
                if (responseJson.RootElement.ValueKind == JsonValueKind.Array && responseJson.RootElement.GetArrayLength() > 0)
                {
                    aiResponse = responseJson.RootElement[0]
                        .GetProperty("generated_text")
                        .GetString();
                }
            }
            else
            {
                // OpenAI/Groq format (standard)
                var responseJson = JsonDocument.Parse(responseContent);
                aiResponse = responseJson.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }

            _logger.LogInformation("[AI Chat] ✅ Successfully parsed AI response");
            _logger.LogInformation("[AI Chat] ✅ Response length: {Length}", aiResponse?.Length ?? 0);

            return aiResponse ?? "Xin lỗi, tôi không thể tạo phản hồi. Vui lòng thử lại.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ❌ Exception occurred");
            _logger.LogError("[AI Chat] ❌ Exception type: {Type}", ex.GetType().Name);
            _logger.LogError("[AI Chat] ❌ Exception message: {Message}", ex.Message);
            _logger.LogError("[AI Chat] ❌ Stack trace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("[AI Chat] ❌ Inner exception: {Inner}", ex.InnerException.Message);
            }
            return "Xin lỗi, đã xảy ra lỗi khi xử lý câu hỏi của bạn. Vui lòng thử lại sau.";
        }
    }

    /// <summary>
    /// Trả về response mẫu khi không có API key
    /// </summary>
    private string GetSampleResponse(string userMessage)
    {
        var lowerMessage = userMessage.ToLower();

        if (lowerMessage.Contains("đặt phòng") || lowerMessage.Contains("booking"))
        {
            return "Để đặt phòng, bạn có thể:\n" +
                   "1. Chọn phòng trên trang 'Phòng' của website\n" +
                   "2. Chọn ngày check-in và check-out\n" +
                   "3. Điền thông tin và xác nhận đặt phòng\n" +
                   "4. Thanh toán qua PayOs hoặc chuyển khoản\n\n" +
                   "Nếu cần hỗ trợ, vui lòng liên hệ hotline: 1900-xxxx";
        }

        if (lowerMessage.Contains("giá") || lowerMessage.Contains("phí"))
        {
            return "Giá phòng tại Resort Deluxe dao động từ 500.000₫ - 2.000.000₫/đêm tùy loại phòng.\n" +
                   "Bạn có thể xem chi tiết giá trên trang 'Phòng' hoặc liên hệ để được tư vấn cụ thể.";
        }

        if (lowerMessage.Contains("dịch vụ") || lowerMessage.Contains("nhà hàng") || lowerMessage.Contains("spa"))
        {
            return "Resort Deluxe cung cấp nhiều dịch vụ:\n" +
                   "🍽️ Nhà hàng với menu đa dạng\n" +
                   "💆 Spa và massage\n" +
                   "🏊 Hồ bơi ngoài trời\n" +
                   "🏋️ Phòng gym\n" +
                   "🎮 Khu vui chơi\n\n" +
                   "Bạn có thể đặt dịch vụ qua website hoặc liên hệ lễ tân.";
        }

        if (lowerMessage.Contains("hủy") || lowerMessage.Contains("đổi"))
        {
            return "Chính sách hủy/đổi:\n" +
                   "• Hủy trước 24h: Miễn phí\n" +
                   "• Hủy trong 24h: Phí 50%\n" +
                   "• Không đến: Phí 100%\n\n" +
                   "Để hủy/đổi booking, vui lòng vào trang 'Đặt phòng của tôi' hoặc liên hệ hotline.";
        }

        return "Xin chào! Tôi là trợ lý AI của Resort Deluxe. Tôi có thể giúp bạn:\n" +
               "• Tư vấn đặt phòng\n" +
               "• Thông tin về dịch vụ\n" +
               "• Hướng dẫn thanh toán\n" +
               "• Chính sách hủy/đổi\n\n" +
               "Bạn có câu hỏi gì không?";
    }
}

