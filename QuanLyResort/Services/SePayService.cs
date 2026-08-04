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
    
    // Rate limiting: SePay giới hạn 2 requests/second
    private static readonly SemaphoreSlim _rateLimiter = new SemaphoreSlim(2, 2);
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private static readonly TimeSpan _minRequestInterval = TimeSpan.FromMilliseconds(500); // 500ms = 2 requests/second

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
        // Hỗ trợ cả format đúng (SePay__MerchantId) và format sai (SePayMerchantId) để tương thích
        _merchantId = _configuration["SePay:MerchantId"]
                   ?? _configuration["SePayMerchantId"]; // Fallback cho format sai (không có __)
        if (!string.IsNullOrEmpty(_merchantId))
        {
            _logger.LogInformation("[SEPAY] 🔍 Merchant ID configured: {MerchantId}", _merchantId);
        }
        else
        {
            _logger.LogWarning("[SEPAY] ⚠️ Merchant ID chưa được cấu hình. Vui lòng thêm 'SePay__MerchantId' (với 2 dấu gạch dưới) vào Railway variables.");
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

            // Rate limiting: Đảm bảo không vượt quá 2 requests/second
            await EnforceRateLimitAsync();

            // Thử các endpoint khác nhau nếu endpoint đầu tiên không hoạt động
            var endpoints = GetApiEndpoints();
            
            foreach (var endpoint in endpoints)
            {
                var result = await TryCreateOrderAsync(endpoint, orderCode, amount, description, durationSeconds);
                if (result != null)
                {
                    return result;
                }
            }
            
            // Nếu tất cả endpoints đều thất bại, return null để fallback sang static QR
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi tạo đơn hàng: OrderCode={OrderCode}", orderCode);
            return null;
        }
    }
    
    /// <summary>
    /// Lấy danh sách các API endpoints để thử (theo thứ tự ưu tiên)
    /// </summary>
    private List<(string Url, string Type)> GetApiEndpoints()
    {
        var endpoints = new List<(string Url, string Type)>();
        
        if (_apiBaseUrl.Contains("pgapi.sepay.vn"))
        {
            // Production API endpoints (thử nhiều format)
            
            // Option 1: Standard endpoint
            endpoints.Add(($"{_apiBaseUrl}/api/v1/orders", "Production Standard"));
            
            // Option 2: Với merchant_id trong path (nếu có)
            if (!string.IsNullOrEmpty(_merchantId))
            {
                endpoints.Add(($"{_apiBaseUrl}/api/v1/merchants/{_merchantId}/orders", "Production Merchant"));
            }
            
            // Option 3: Với account_id trong path
            if (!string.IsNullOrEmpty(_accountId))
            {
                endpoints.Add(($"{_apiBaseUrl}/api/v1/accounts/{_accountId}/orders", "Production Account"));
            }
            
            // Option 4: Thử User API nếu Production API không hoạt động
            // User API có thể hoạt động ngay cả khi dùng Production base URL
            if (!string.IsNullOrEmpty(_bankCode) && !string.IsNullOrEmpty(_accountId))
            {
                endpoints.Add(($"https://my.sepay.vn/userapi/{_bankCode}/{_accountId}/orders", "User API Bank+Account (Fallback)"));
            }
            
            if (!string.IsNullOrEmpty(_merchantId))
            {
                endpoints.Add(($"https://my.sepay.vn/userapi/{_merchantId}/orders", "User API Merchant (Fallback)"));
            }
            
            if (!string.IsNullOrEmpty(_accountId))
            {
                endpoints.Add(($"https://my.sepay.vn/userapi/{_accountId}/orders", "User API Account (Fallback)"));
            }
        }
        else if (_apiBaseUrl.Contains("my.sepay.vn"))
        {
            // User API endpoints
            
            // Option 1: Với bankCode và accountId
            if (!string.IsNullOrEmpty(_bankCode) && !string.IsNullOrEmpty(_accountId))
            {
                endpoints.Add(($"{_apiBaseUrl}/userapi/{_bankCode}/{_accountId}/orders", "User API Bank+Account"));
            }
            
            // Option 2: Với merchant_id (nếu có)
            if (!string.IsNullOrEmpty(_merchantId))
            {
                endpoints.Add(($"{_apiBaseUrl}/userapi/{_merchantId}/orders", "User API Merchant"));
            }
            
            // Option 3: Chỉ với accountId
            if (!string.IsNullOrEmpty(_accountId))
            {
                endpoints.Add(($"{_apiBaseUrl}/userapi/{_accountId}/orders", "User API Account"));
            }
        }
        else
        {
            // Fallback: thử format userapi
            if (!string.IsNullOrEmpty(_bankCode) && !string.IsNullOrEmpty(_accountId))
            {
                endpoints.Add(($"{_apiBaseUrl}/userapi/{_bankCode}/{_accountId}/orders", "Fallback UserAPI"));
            }
        }
        
        return endpoints;
    }
    
    /// <summary>
    /// Thử tạo order với một endpoint cụ thể
    /// </summary>
    private async Task<SePayOrderResponse?> TryCreateOrderAsync((string Url, string Type) endpoint, string orderCode, decimal amount, string description, int durationSeconds)
    {
        try
        {
            _logger.LogInformation("[SEPAY] 🔄 Thử endpoint: {Type} - {Url}", endpoint.Type, endpoint.Url);
            
            // Tạo request body
            var requestBody = CreateRequestBody(orderCode, amount, description, durationSeconds, endpoint.Type);
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
            
            _logger.LogInformation("[SEPAY] 🔍 Request body: {Body}", json);

            var response = await _httpClient.PostAsync(endpoint.Url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var sepayResponse = JsonSerializer.Deserialize<SePayApiResponse>(responseContent, options);

                if (sepayResponse?.Status == "success" && sepayResponse.Data != null)
                {
                    _logger.LogInformation("[SEPAY] ✅ Đơn hàng tạo thành công với endpoint {Type}: OrderId={OrderId}, OrderCode={OrderCode}", 
                        endpoint.Type, sepayResponse.Data.OrderId, sepayResponse.Data.OrderCode);
                    return sepayResponse.Data;
                }
                else
                {
                    _logger.LogWarning("[SEPAY] ⚠️ Endpoint {Type} trả về nhưng status không phải success: {Status}, Message={Message}", 
                        endpoint.Type, sepayResponse?.Status, sepayResponse?.Message);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Nếu là 404, thử endpoint tiếp theo
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("[SEPAY] ⚠️ Endpoint {Type} trả về 404, thử endpoint tiếp theo", endpoint.Type);
                    return null; // Thử endpoint tiếp theo
                }
                
                // Nếu là 429 (Rate Limit), đợi và retry
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("[SEPAY] ⚠️ Rate limit (429) từ endpoint {Type}, đợi 1 giây và retry...", endpoint.Type);
                    await Task.Delay(1000);
                    return null; // Retry với endpoint này
                }
                
                _logger.LogError("[SEPAY] ❌ Endpoint {Type} error: Status={Status}, Response={Response}", 
                    endpoint.Type, response.StatusCode, errorContent);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEPAY] ❌ Lỗi khi thử endpoint {Type}: {Url}", endpoint.Type, endpoint.Url);
            return null;
        }
    }
    
    /// <summary>
    /// Tạo request body tùy theo endpoint type
    /// </summary>
    private object CreateRequestBody(string orderCode, decimal amount, string description, int durationSeconds, string endpointType)
    {
        if (endpointType.Contains("Production"))
        {
            // Production API format
            var prodBody = new Dictionary<string, object>
            {
                { "amount", (long)(amount) },
                { "order_code", orderCode },
                { "description", description },
                { "duration", durationSeconds },
                { "with_qrcode", true }
            };
            
            // Thêm merchant_id nếu có (QUAN TRỌNG cho Production API!)
            if (!string.IsNullOrEmpty(_merchantId))
            {
                prodBody["merchant_id"] = _merchantId;
                _logger.LogInformation("[SEPAY] 🔍 Added merchant_id to request: {MerchantId}", _merchantId);
            }
            else
            {
                _logger.LogWarning("[SEPAY] ⚠️ merchant_id chưa được cấu hình. Production API có thể yêu cầu merchant_id!");
            }
            
            return prodBody;
        }
        else if (endpointType.Contains("User API"))
        {
            // User API format - không cần description và merchant_id
            return new
            {
                amount = (long)(amount),
                order_code = orderCode,
                duration = durationSeconds,
                with_qrcode = true
            };
        }
        else
        {
            // Fallback format
            return new
            {
                amount = (long)(amount),
                order_code = orderCode,
                duration = durationSeconds,
                with_qrcode = true
            };
        }
    }
    
    /// <summary>
    /// Enforce rate limiting: Đảm bảo không vượt quá 2 requests/second
    /// </summary>
    private async Task EnforceRateLimitAsync()
    {
        await _rateLimiter.WaitAsync();
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            if (timeSinceLastRequest < _minRequestInterval)
            {
                var delay = _minRequestInterval - timeSinceLastRequest;
                _logger.LogDebug("[SEPAY] ⏱️ Rate limiting: Đợi {Delay}ms để đảm bảo không vượt quá 2 requests/second", delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
            _lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            _rateLimiter.Release();
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

