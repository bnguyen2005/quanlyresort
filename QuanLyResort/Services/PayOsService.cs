using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyResort.Services;

/// <summary>
/// Service để tạo PayOs payment link
/// </summary>
public class PayOsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayOsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly string _checksumKey;
    private readonly string _baseUrl = "https://api-merchant.payos.vn";

    public PayOsService(
        IConfiguration configuration,
        ILogger<PayOsService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        
        var payOsConfig = _configuration.GetSection("BankWebhook:PayOs");
        _clientId = payOsConfig["ClientId"] ?? "c704495b-5984-4ad3-aa23-b2794a02aa83";
        _apiKey = payOsConfig["ApiKey"] ?? "f6ea421b-a8b7-46b8-92be-209eb1a9b2fb";
        _checksumKey = payOsConfig["ChecksumKey"] ?? payOsConfig["SecretKey"] ?? "429890033cc6f1ca9696c91bb4abf499de9ac6132c64e335e46f4c87e6d95313";
    }

    /// <summary>
    /// Tạo PayOs payment link
    /// </summary>
    public async Task<PayOsPaymentLinkResponse?> CreatePaymentLinkAsync(
        int orderCode,
        decimal amount,
        string description,
        string returnUrl,
        string cancelUrl,
        DateTime? expiredAt = null)
    {
        try
        {
            _logger.LogInformation("🔄 [PayOs] Creating payment link: OrderCode={OrderCode}, Amount={Amount:N0} VND", orderCode, amount);

            // Convert amount to integer (PayOs expects integer)
            var amountInt = (int)Math.Round(amount);

            // Prepare data for signature
            var dataForSignature = new Dictionary<string, string>
            {
                { "amount", amountInt.ToString() },
                { "cancelUrl", cancelUrl },
                { "description", description },
                { "orderCode", orderCode.ToString() },
                { "returnUrl", returnUrl }
            };

            // Sort alphabetically and create signature string
            var sortedKeys = dataForSignature.Keys.OrderBy(k => k).ToList();
            var signatureString = string.Join("&", sortedKeys.Select(k => $"{k}={dataForSignature[k]}"));
            
            // Create signature using HMAC_SHA256
            var signature = ComputeHmacSha256(signatureString, _checksumKey);

            // Prepare request body
            var requestBody = new
            {
                orderCode = orderCode,
                amount = amountInt,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                expiredAt = expiredAt?.Subtract(new DateTime(1970, 1, 1)).TotalSeconds ?? 0,
                signature = signature
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation("📤 [PayOs] Request body: {Body}", jsonBody);

            // Create HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/payment-requests")
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);

            // Send request
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 [PayOs] Response status: {Status}", response.StatusCode);
            _logger.LogInformation("📥 [PayOs] Response body: {Body}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ [PayOs] Failed to create payment link: {Status} - {Content}", 
                    response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<PayOsPaymentLinkResponse>(responseContent);
            
            if (result?.Code == "00")
            {
                _logger.LogInformation("✅ [PayOs] Payment link created: PaymentLinkId={PaymentLinkId}, QRCode={HasQR}", 
                    result.Data?.PaymentLinkId, !string.IsNullOrEmpty(result.Data?.QrCode));
                return result;
            }
            else
            {
                _logger.LogWarning("⚠️ [PayOs] Payment link creation failed: Code={Code}, Desc={Desc}", 
                    result?.Code, result?.Desc);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [PayOs] Error creating payment link");
            return null;
        }
    }

    /// <summary>
    /// Compute HMAC SHA256
    /// </summary>
    private string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

/// <summary>
/// Response từ PayOs khi tạo payment link
/// </summary>
public class PayOsPaymentLinkResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public PayOsPaymentLinkData? Data { get; set; }
    
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>
/// Data trong PayOs payment link response
/// </summary>
public class PayOsPaymentLinkData
{
    [JsonPropertyName("bin")]
    public string? Bin { get; set; }
    
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }
    
    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    
    [JsonPropertyName("paymentLinkId")]
    public string? PaymentLinkId { get; set; }
    
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("orderCode")]
    public int OrderCode { get; set; }
    
    [JsonPropertyName("expiredAt")]
    public long ExpiredAt { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("checkoutUrl")]
    public string? CheckoutUrl { get; set; }
    
    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; } // Base64 QR code image
}

