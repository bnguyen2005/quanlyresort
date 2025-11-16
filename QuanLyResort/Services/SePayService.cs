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
    private readonly string? _merchantId;
    
    // SePay Static QR Code configuration
    private readonly string? _bankAccountNumber; // Số tài khoản ngân hàng

    public SePayService(
        ILogger<SePayService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Load configuration from appsettings.json hoặc environment variables
        // SePay API có thể dùng:
        // - Production: https://pgapi.sepay.vn
        // - User API: https://my.sepay.vn/userapi
        
        // Hỗ trợ cả format cũ (SePay:*) và format mới (SEPAY_*)
        _apiBaseUrl = _configuration["SePay:ApiBaseUrl"] 
                   ?? _configuration["SEPAY_API_BASE_URL"] 
                   ?? "https://pgapi.sepay.vn";
        
        // API_KEY: Khóa bí mật để call API (format cũ: SePay:ApiToken, format mới: SEPAY_API_KEY)
        _apiToken = _configuration["SePay:ApiToken"] 
                 ?? _configuration["SEPAY_API_KEY"];
        
        // CLIENT_ID: Mã định danh ứng dụng (format cũ: SePay:AccountId, format mới: SEPAY_CLIENT_ID)
        _accountId = _configuration["SePay:AccountId"] 
                  ?? _configuration["SePay:ClientId"]
                  ?? _configuration["SEPAY_CLIENT_ID"];
        
        _bankCode = _configuration["SePay:BankCode"] ?? "MB"; // Default to MB
        
        // MERCHANT ID (có thể khác Account ID)
        _merchantId = _configuration["SePay:MerchantId"];
        if (!string.IsNullOrEmpty(_merchantId))
        {
            _logger.LogInformation("[SEPAY] 🔍 Merchant ID configured: {MerchantId}", _merchantId);
        }
        
        // Bank Account Number (cho static QR code)
        _bankAccountNumber = _configuration["SePay:BankAccountNumber"];
        if (string.IsNullOrEmpty(_bankAccountNumber))
        {
            _logger.LogWarning("[SEPAY] ⚠️ SePay Bank Account Number chưa được cấu hình. Static QR code sẽ không hoạt động.");
        }

        if (string.IsNullOrEmpty(_apiToken))
        {
            _logger.LogWarning("[SEPAY] ⚠️ SePay API Key chưa được cấu hình. Vui lòng thêm 'SePay:ApiToken' hoặc 'SEPAY_API_KEY' vào environment variables.");
        }

        if (string.IsNullOrEmpty(_accountId))
        {
            _logger.LogWarning("[SEPAY] ⚠️ SePay Client ID chưa được cấu hình. Vui lòng thêm 'SePay:AccountId' hoặc 'SEPAY_CLIENT_ID' vào environment variables.");
        }
    }

    /// <summary>
    /// Tạo đơn hàng và QR code động cho booking
    /// </summary>
    public async Task<SePayOrderResponse?> CreateBookingOrderAsync(int bookingId, decimal amount, int durationSeconds = 300)
    {
        try
        {
            var orderCode = $"BOOKING{bookingId}";
            var description = $"BOOKING{bookingId}"; // Format ngắn gọn cho QR code

            // Thử gọi API trước
            if (!string.IsNullOrEmpty(_apiToken) && !string.IsNullOrEmpty(_accountId))
            {
                var result = await CreateOrderAsync(orderCode, amount, description, durationSeconds);
                if (result != null)
                {
                    return result;
                }
            }

            // Fallback: Tạo QR code tĩnh nếu API không hoạt động hoặc chưa cấu hình
            _logger.LogInformation("[SEPAY] 🔄 Fallback sang static QR code cho booking {BookingId}", bookingId);
            return CreateStaticQRCodeResponse(orderCode, amount, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi tạo đơn hàng booking {BookingId}", bookingId);
            
            // Fallback: Tạo QR code tĩnh
            var orderCode = $"BOOKING{bookingId}";
            var description = $"BOOKING{bookingId}";
            return CreateStaticQRCodeResponse(orderCode, amount, description);
        }
    }

    /// <summary>
    /// Tạo đơn hàng và QR code động cho restaurant order
    /// </summary>
    public async Task<SePayOrderResponse?> CreateRestaurantOrderAsync(int orderId, decimal amount, int durationSeconds = 300)
    {
        try
        {
            var orderCode = $"ORDER{orderId}";
            var description = $"ORDER{orderId}"; // Format ngắn gọn cho QR code

            // Thử gọi API trước
            if (!string.IsNullOrEmpty(_apiToken) && !string.IsNullOrEmpty(_accountId))
            {
                var result = await CreateOrderAsync(orderCode, amount, description, durationSeconds);
                if (result != null)
                {
                    return result;
                }
            }

            // Fallback: Tạo QR code tĩnh nếu API không hoạt động hoặc chưa cấu hình
            _logger.LogInformation("[SEPAY] 🔄 Fallback sang static QR code cho restaurant order {OrderId}", orderId);
            return CreateStaticQRCodeResponse(orderCode, amount, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi tạo đơn hàng restaurant {OrderId}", orderId);
            
            // Fallback: Tạo QR code tĩnh
            var orderCode = $"ORDER{orderId}";
            var description = $"ORDER{orderId}";
            return CreateStaticQRCodeResponse(orderCode, amount, description);
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

            // SePay API request body - có thể cần format khác tùy endpoint
            object requestBody;
            
            if (_apiBaseUrl.Contains("pgapi.sepay.vn"))
            {
                // Production API format - có thể cần merchant_id, description, etc.
                var prodBody = new Dictionary<string, object>
                {
                    { "amount", (long)(amount) },
                    { "order_code", orderCode },
                    { "description", description },
                    { "duration", durationSeconds },
                    { "with_qrcode", true }
                };
                
                // Thêm merchant_id nếu có
                if (!string.IsNullOrEmpty(_merchantId))
                {
                    prodBody["merchant_id"] = _merchantId;
                }
                
                requestBody = prodBody;
            }
            else
            {
                // User API format
                requestBody = new
                {
                    amount = (long)(amount), // SePay expects amount in VND (long)
                    order_code = orderCode,
                    duration = durationSeconds, // Thời gian hiệu lực (giây)
                    with_qrcode = true // Yêu cầu tạo QR code
                };
            }

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
                
                // Fallback: Tạo QR code tĩnh nếu API không hoạt động
                _logger.LogWarning("[SEPAY] ⚠️ SePay API không hoạt động, fallback sang static QR code");
                return CreateStaticQRCodeResponse(orderCode, amount, description);
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
                
                // Fallback: Tạo QR code tĩnh nếu API trả về lỗi
                _logger.LogWarning("[SEPAY] ⚠️ SePay API trả về lỗi, fallback sang static QR code");
                return CreateStaticQRCodeResponse(orderCode, amount, description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi gọi SePay API");
            
            // Fallback: Tạo QR code tĩnh nếu có lỗi
            _logger.LogWarning("[SEPAY] ⚠️ SePay API lỗi, fallback sang static QR code");
            return CreateStaticQRCodeResponse(orderCode, amount, description);
        }
    }

    /// <summary>
    /// Tạo QR code tĩnh từ SePay URL (fallback khi API không hoạt động)
    /// Format: https://qr.sepay.vn/img?acc=SO_TAI_KHOAN&bank=NGAN_HANG&amount=SO_TIEN&des=NOI_DUNG
    /// QR code này vẫn ĐỘNG về số tiền vì amount thay đổi theo booking/order
    /// </summary>
    private SePayOrderResponse? CreateStaticQRCodeResponse(string orderCode, decimal amount, string description)
    {
        try
        {
            if (string.IsNullOrEmpty(_bankAccountNumber))
            {
                _logger.LogError("[SEPAY] ❌ Bank Account Number chưa được cấu hình. Không thể tạo static QR code.");
                return null;
            }

            // URL encode các tham số
            var encodedDescription = Uri.EscapeDataString(description);
            var bankCodeForUrl = _bankCode ?? "MB";
            
            // Tạo QR code URL tĩnh (nhưng số tiền vẫn động)
            // Format: https://qr.sepay.vn/img?acc=SO_TAI_KHOAN&bank=NGAN_HANG&amount=SO_TIEN&des=NOI_DUNG
            var qrCodeUrl = $"https://qr.sepay.vn/img?acc={_bankAccountNumber}&bank={bankCodeForUrl}&amount={(long)amount}&des={encodedDescription}";
            
            _logger.LogInformation("[SEPAY] 📸 Tạo static QR code URL (amount động): {Url}", qrCodeUrl);

            // Tạo response tương tự API response
            return new SePayOrderResponse
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderCode = orderCode,
                VaNumber = orderCode,
                VaHolderName = "Resort Deluxe",
                Amount = (long)amount,
                Status = "pending",
                BankName = bankCodeForUrl,
                AccountHolderName = "Resort Deluxe",
                AccountNumber = _bankAccountNumber,
                ExpiredAt = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-dd HH:mm:ss"),
                QrCode = null, // Static QR code không có base64
                QrCodeUrl = qrCodeUrl // URL để hiển thị QR code (số tiền động)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi tạo static QR code");
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

