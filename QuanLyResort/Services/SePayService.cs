using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyResort.Services;

/// <summary>
/// Service để tương tác với SePay API - tạo QR code động
/// </summary>
public class SePayService
{
    private readonly ILogger<SePayService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    // SePay API configuration
    private readonly string? _apiBaseUrl;
    private readonly string? _apiToken;
    private readonly string? _accountId;
    private readonly string? _bankCode;

    public SePayService(
        ILogger<SePayService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Load configuration from appsettings.json
        // SePay API có thể dùng:
        // - Production: https://pgapi.sepay.vn
        // - User API: https://my.sepay.vn/userapi
        _apiBaseUrl = _configuration["SePay:ApiBaseUrl"] ?? "https://pgapi.sepay.vn";
        _apiToken = _configuration["SePay:ApiToken"];
        _accountId = _configuration["SePay:AccountId"];
        _bankCode = _configuration["SePay:BankCode"] ?? "MB"; // Default to MB
        
        // MERCHANT ID (có thể khác Account ID)
        var merchantId = _configuration["SePay:MerchantId"];
        if (!string.IsNullOrEmpty(merchantId))
        {
            _logger.LogInformation("[SEPAY] 🔍 Merchant ID configured: {MerchantId}", merchantId);
        }

        if (string.IsNullOrEmpty(_apiToken))
        {
            _logger.LogWarning("[SEPAY] ⚠️ SePay API Token chưa được cấu hình. Vui lòng thêm 'SePay:ApiToken' vào appsettings.json hoặc environment variables.");
        }

        if (string.IsNullOrEmpty(_accountId))
        {
            _logger.LogWarning("[SEPAY] ⚠️ SePay Account ID chưa được cấu hình. Vui lòng thêm 'SePay:AccountId' vào appsettings.json hoặc environment variables.");
        }
    }

    /// <summary>
    /// Tạo đơn hàng và QR code động cho booking
    /// </summary>
    public async Task<SePayOrderResponse?> CreateBookingOrderAsync(int bookingId, decimal amount, int durationSeconds = 300)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_accountId))
            {
                _logger.LogError("[SEPAY] ❌ API Token hoặc Account ID chưa được cấu hình");
                return null;
            }

            var orderCode = $"BOOKING{bookingId}";
            var description = $"Thanh toán đặt phòng {bookingId}";

            return await CreateOrderAsync(orderCode, amount, description, durationSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi tạo đơn hàng booking {BookingId}", bookingId);
            return null;
        }
    }

    /// <summary>
    /// Tạo đơn hàng và QR code động cho restaurant order
    /// </summary>
    public async Task<SePayOrderResponse?> CreateRestaurantOrderAsync(int orderId, decimal amount, int durationSeconds = 300)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_accountId))
            {
                _logger.LogError("[SEPAY] ❌ API Token hoặc Account ID chưa được cấu hình");
                return null;
            }

            var orderCode = $"ORDER{orderId}";
            var description = $"Thanh toán đơn hàng nhà hàng {orderId}";

            return await CreateOrderAsync(orderCode, amount, description, durationSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi tạo đơn hàng restaurant {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>
    /// Tạo đơn hàng và QR code động qua SePay API
    /// </summary>
    private async Task<SePayOrderResponse?> CreateOrderAsync(string orderCode, decimal amount, string description, int durationSeconds)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiToken) || string.IsNullOrEmpty(_accountId))
            {
                return null;
            }

            // SePay API endpoint: Có thể có nhiều format
            // Option 1: POST /api/v1/orders (pgapi.sepay.vn - Production API)
            // Option 2: POST /userapi/{bankCode}/{accountId}/orders (my.sepay.vn - User API)
            // Option 3: POST /userapi/{merchantId}/orders (không có bankCode)
            
            string url;
            if (_apiBaseUrl.Contains("pgapi.sepay.vn"))
            {
                // Production API: https://pgapi.sepay.vn/api/v1/orders
                url = $"{_apiBaseUrl}/api/v1/orders";
            }
            else if (_apiBaseUrl.Contains("my.sepay.vn"))
            {
                // User API: https://my.sepay.vn/userapi/{bankCode}/{accountId}/orders
                url = $"{_apiBaseUrl}/{_bankCode}/{_accountId}/orders";
            }
            else
            {
                // Fallback: thử format userapi
                url = $"{_apiBaseUrl}/{_bankCode}/{_accountId}/orders";
            }
            
            _logger.LogInformation("[SEPAY] 🔍 API URL: {Url}, AccountId: {AccountId}, BankCode: {BankCode}, ApiBaseUrl: {ApiBaseUrl}", 
                url, _accountId, _bankCode, _apiBaseUrl);
            
            // Log request body để debug
            var requestBodyJson = JsonSerializer.Serialize(new
            {
                amount = (long)(amount),
                order_code = orderCode,
                duration = durationSeconds,
                with_qrcode = true
            });
            _logger.LogInformation("[SEPAY] 🔍 Request body: {Body}", requestBodyJson);

            var requestBody = new
            {
                amount = (long)(amount), // SePay expects amount in VND (long)
                order_code = orderCode,
                duration = durationSeconds, // Thời gian hiệu lực (giây)
                with_qrcode = true // Yêu cầu tạo QR code
            };

            _logger.LogInformation("[SEPAY] 🔄 Tạo đơn hàng SePay: OrderCode={OrderCode}, Amount={Amount}, Duration={Duration}s", 
                orderCode, amount, durationSeconds);

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            
            // SePay có thể dùng Bearer token hoặc Basic Auth
            // Thử Bearer token trước (format: spsk_live_...)
            if (_apiToken.StartsWith("spsk_"))
            {
                // Bearer token format
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
            }
            else
            {
                // Fallback: Bearer token
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
            }
            
            _logger.LogInformation("[SEPAY] 🔍 Authorization header: Bearer {TokenPrefix}...", 
                _apiToken?.Substring(0, Math.Min(20, _apiToken?.Length ?? 0)) ?? "NULL");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("[SEPAY] ❌ SePay API error: Status={Status}, Response={Response}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var sepayResponse = JsonSerializer.Deserialize<SePayApiResponse>(responseContent, options);

            if (sepayResponse?.Status == "success" && sepayResponse.Data != null)
            {
                _logger.LogInformation("[SEPAY] ✅ Đơn hàng tạo thành công: OrderId={OrderId}, OrderCode={OrderCode}, VA={VaNumber}", 
                    sepayResponse.Data.OrderId, sepayResponse.Data.OrderCode, sepayResponse.Data.VaNumber);

                return sepayResponse.Data;
            }
            else
            {
                _logger.LogError("[SEPAY] ❌ SePay API trả về lỗi: Status={Status}, Message={Message}", 
                    sepayResponse?.Status, sepayResponse?.Message);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi gọi SePay API");
            return null;
        }
    }
}

/// <summary>
/// Response từ SePay API
/// </summary>
public class SePayApiResponse
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public SePayOrderResponse? Data { get; set; }
}

/// <summary>
/// Thông tin đơn hàng từ SePay
/// </summary>
public class SePayOrderResponse
{
    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }

    [JsonPropertyName("order_code")]
    public string? OrderCode { get; set; }

    [JsonPropertyName("va_number")]
    public string? VaNumber { get; set; }

    [JsonPropertyName("va_holder_name")]
    public string? VaHolderName { get; set; }

    public long Amount { get; set; }

    public string? Status { get; set; }

    [JsonPropertyName("bank_name")]
    public string? BankName { get; set; }

    [JsonPropertyName("account_holder_name")]
    public string? AccountHolderName { get; set; }

    [JsonPropertyName("account_number")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("expired_at")]
    public string? ExpiredAt { get; set; }

    [JsonPropertyName("qr_code")]
    public string? QrCode { get; set; } // Base64 image

    [JsonPropertyName("qr_code_url")]
    public string? QrCodeUrl { get; set; } // URL to QR code image
}

