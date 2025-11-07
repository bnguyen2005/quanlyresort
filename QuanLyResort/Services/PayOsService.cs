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
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        var payOsConfig = _configuration.GetSection("BankWebhook:PayOs");
        _clientId = payOsConfig["ClientId"] ?? throw new ArgumentNullException(nameof(payOsConfig), "PayOs ClientId is not configured");
        _apiKey = payOsConfig["ApiKey"] ?? throw new ArgumentNullException(nameof(payOsConfig), "PayOs ApiKey is not configured");
        _checksumKey = payOsConfig["ChecksumKey"] ?? payOsConfig["SecretKey"] ?? throw new ArgumentNullException(nameof(payOsConfig), "PayOs ChecksumKey/SecretKey is not configured");
        
        _logger.LogInformation("✅ [PayOs] Service initialized with ClientId: {ClientId}", _clientId.Substring(0, Math.Min(8, _clientId.Length)));
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
            
            _logger.LogInformation("🔐 [PayOs] Signature string: {SignatureString}", signatureString);
            
            // Create signature using HMAC_SHA256
            var signature = ComputeHmacSha256(signatureString, _checksumKey);
            
            _logger.LogInformation("🔐 [PayOs] Computed signature: {Signature}", signature.Substring(0, Math.Min(16, signature.Length)) + "...");

            // Prepare request body
            // expiredAt must be Int32 Unix Timestamp (not double)
            var expiredAtUnix = 0;
            if (expiredAt.HasValue)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                expiredAtUnix = (int)(expiredAt.Value.ToUniversalTime().Subtract(epoch).TotalSeconds);
            }
            
            var requestBody = new
            {
                orderCode = orderCode,
                amount = amountInt,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                expiredAt = expiredAtUnix,
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

            PayOsPaymentLinkResponse? result = null;
            try
            {
                result = JsonSerializer.Deserialize<PayOsPaymentLinkResponse>(responseContent);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "❌ [PayOs] Failed to deserialize response: {ResponseBody}", responseContent);
                return null;
            }
            
            if (result?.Code == "00" && result.Data != null)
            {
                _logger.LogInformation("✅ [PayOs] Payment link created: PaymentLinkId={PaymentLinkId}", 
                    result.Data.PaymentLinkId);
                
                // Log QR code details
                var hasQrCode = !string.IsNullOrEmpty(result.Data.QrCode);
                _logger.LogInformation("🔍 [PayOs] QR Code available: {HasQR}, Length: {Length}", 
                    hasQrCode, result.Data.QrCode?.Length ?? 0);
                
                if (hasQrCode)
                {
                    _logger.LogInformation("🔍 [PayOs] QR Code preview (first 50 chars): {Preview}", 
                        result.Data.QrCode.Substring(0, Math.Min(50, result.Data.QrCode.Length)));
                }
                else
                {
                    _logger.LogWarning("⚠️ [PayOs] QR Code is NULL or empty. Available fields: PaymentLinkId={PaymentLinkId}, CheckoutUrl={CheckoutUrl}", 
                        result.Data.PaymentLinkId, result.Data.CheckoutUrl);
                }
                
                return result;
            }
            else
            {
                _logger.LogWarning("⚠️ [PayOs] Payment link creation failed: Code={Code}, Desc={Desc}, Response={Response}", 
                    result?.Code ?? "NULL", result?.Desc ?? "NULL", responseContent);
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

