using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyResort.Services;

/// <summary>
/// Service để tương tác trực tiếp với MB Bank API
/// Sử dụng OAuth2 để authenticate
/// </summary>
public class MBBankApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MBBankApiService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiresAt;

    public MBBankApiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MBBankApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Lấy OAuth2 Access Token từ MB Bank
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        // Kiểm tra token còn hiệu lực không
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt)
        {
            return _accessToken;
        }

        try
        {
            var clientId = _configuration["BankWebhook:MBBank:ClientId"];
            var clientSecret = _configuration["BankWebhook:MBBank:ClientSecret"];
            var tokenUrl = _configuration["BankWebhook:MBBank:OAuth2TokenUrl"] 
                ?? "https://api-sandbox.mbbank.com.vn/oauth2/v1/token";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("MB Bank Client ID or Client Secret not configured");
                return null;
            }

            var client = _httpClientFactory.CreateClient();
            
            // Tạo Basic Auth header
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Tạo request body
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.PostAsync(tokenUrl, requestBody);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("MB Bank OAuth2 token request failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<MBOAuth2TokenResponse>(jsonResponse);

            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogError("MB Bank OAuth2 token response is invalid");
                return null;
            }

            _accessToken = tokenResponse.AccessToken;
            // Token thường hết hạn sau 1 giờ, set expiresAt trước 5 phút để refresh sớm
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn ?? 3300);

            _logger.LogInformation("MB Bank OAuth2 token obtained successfully");
            return _accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MB Bank OAuth2 token");
            return null;
        }
    }

    /// <summary>
    /// Query transaction từ MB Bank API
    /// </summary>
    public async Task<MBTransactionResponse?> QueryTransactionAsync(string transactionId)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot query transaction: No access token");
                return null;
            }

            var apiBaseUrl = _configuration["BankWebhook:MBBank:ApiBaseUrl"] 
                ?? "https://api-sandbox.mbbank.com.vn";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Tạo clientMessageId (UUID)
            var clientMessageId = Guid.NewGuid().ToString();

            // Query transaction API (cần tham khảo MB Bank documentation cho endpoint chính xác)
            var queryUrl = $"{apiBaseUrl}/api/v1/transactions/{transactionId}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Add("clientMessageId", clientMessageId);
            request.Headers.Add("transactionId", transactionId);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("MB Bank query transaction failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var transaction = JsonSerializer.Deserialize<MBTransactionResponse>(jsonResponse);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying MB Bank transaction");
            return null;
        }
    }
}

/// <summary>
/// OAuth2 Token Response từ MB Bank
/// </summary>
public class MBOAuth2TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

/// <summary>
/// Transaction Response từ MB Bank API
/// </summary>
public class MBTransactionResponse
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }

    [JsonPropertyName("transactionDate")]
    public DateTime? TransactionDate { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

