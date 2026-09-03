using QuanLyResort.Repositories;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;

namespace QuanLyResort.Services;

/// <summary>
/// Service d? tuong t�c v?i AI Chat API
/// H? tr? OpenAI ho?c c�c AI service kh�c
/// </summary>
public class AIChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIChatService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IBookingService? _bookingService;
    private readonly IRoomService? _roomService;
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;
    private readonly string? _apiKey;
    private readonly string _apiUrl;
    private readonly string _model;
    private readonly string _provider; // "openai", "groq", "huggingface", "cohere", "sample"

    public AIChatService(
        IConfiguration configuration,
        ILogger<AIChatService> logger,
        HttpClient httpClient,
        IBookingService? bookingService = null,
        IRoomService? roomService = null,
        IUnitOfWork unitOfWork = null)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _bookingService = bookingService;
        _roomService = roomService;
        _unitOfWork = unitOfWork;

        // Clear any existing BaseAddress d? tr�nh conflict v?i absolute URLs
        if (_httpClient.BaseAddress != null)
        {
            _logger.LogWarning("[AI Chat] ?? HttpClient has BaseAddress: {BaseAddress}, clearing it", _httpClient.BaseAddress);
            _httpClient.BaseAddress = null;
        }
        
        // Clear default headers d? tr�nh conflict
        _httpClient.DefaultRequestHeaders.Clear();

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
            // Groq s? d?ng format gi?ng OpenAI, nhung URL ph?i ch�nh x�c
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
            
            // Hugging Face c?n header d?c bi?t
            if (_provider == "huggingface")
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
            
            _logger.LogInformation("[AI Chat] ? API Key configured (length: {Length}, provider: {Provider})", _apiKey.Length, _provider);
        }
        else
        {
            _logger.LogInformation("[AI Chat] ?? Using sample responses (no API key or provider=sample)");
        }
        
        // Set timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Log final HttpClient state
        _logger.LogInformation("[AI Chat] ?? HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress?.ToString() ?? "null");

        _logger.LogInformation("[AI Chat] ? Service initialized - Provider: {Provider}, Model: {Model}, API URL: {ApiUrl}", _provider, _model, _apiUrl);
    }

    /// <summary>
    /// G?i message d?n AI v� nh?n response
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, string? conversationContext = null, int? customerId = null)
    {
        try
        {
            // Fetch real data t? database d?a tr�n user message
            var realData = await FetchRealDataAsync(userMessage, customerId);
            
            // N?u kh�ng c� API key ho?c provider l� "sample", tr? v? response m?u v?i d? li?u th?t
            if (string.IsNullOrEmpty(_apiKey) || _provider == "sample")
            {
                _logger.LogInformation("[AI Chat] ?? Using sample response mode with real data");
                return GetSampleResponseWithRealData(userMessage, realData);
            }

            // T?o system prompt cho resort context v?i d? li?u th?t
            var systemPrompt = $@"B?n l� tr? l� AI th�n thi?n c?a Resort Deluxe. 
B?n gi�p kh�ch h�ng v?i c�c c�u h?i v?:
- �?t ph�ng v� booking
- D?ch v? resort (nh� h�ng, spa, h? boi, v.v.)
- Thanh to�n v� h�a don
- Ch�nh s�ch h?y v� d?i
- Th�ng tin v? ph�ng v� ti?n nghi
- Hu?ng d?n s? d?ng website

D? li?u th?t t? website:
{realData}

H�y tr? l?i ng?n g?n, th�n thi?n v� h?u �ch b?ng ti?ng Vi?t, d?a tr�n d? li?u th?t ? tr�n.";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Th�m context n?u c�
            if (!string.IsNullOrEmpty(conversationContext))
            {
                messages.Add(new { role = "assistant", content = conversationContext });
            }

            // Th�m user message
            messages.Add(new { role = "user", content = userMessage });

            // T?o request body t�y theo provider
            object requestBody;
            if (_provider == "cohere")
            {
                // Cohere c� format kh�c
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
                // Hugging Face c� format kh�c
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

            _logger.LogInformation("[AI Chat] ?? Sending message to AI");
            _logger.LogInformation("[AI Chat] ?? Message preview: {Message}", userMessage.Substring(0, Math.Min(50, userMessage.Length)));
            _logger.LogInformation("[AI Chat] ?? API URL: {ApiUrl}", _apiUrl);
            _logger.LogInformation("[AI Chat] ?? Model: {Model}", _model);
            _logger.LogInformation("[AI Chat] ?? Has API Key: {HasKey}", !string.IsNullOrEmpty(_apiKey));
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogInformation("[AI Chat] ?? API Key prefix: {Prefix}", _apiKey.Substring(0, Math.Min(10, _apiKey.Length)));
            }

            _logger.LogInformation("[AI Chat] ?? Request body: {Body}", json);
            _logger.LogInformation("[AI Chat] ?? Request method: POST");
            _logger.LogInformation("[AI Chat] ?? Full URL: {Url}", _apiUrl);

            // �?m b?o kh�ng c� BaseAddress conflict - s? d?ng absolute URI
            Uri requestUri;
            if (Uri.TryCreate(_apiUrl, UriKind.Absolute, out requestUri))
            {
                // URL d� l� absolute, s? d?ng tr?c ti?p
            }
            else
            {
                // N?u URL kh�ng absolute, t?o absolute URI
                requestUri = new Uri(_apiUrl, UriKind.Absolute);
            }

            // T?o HttpRequestMessage v?i POST method r� r�ng
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };
            
            _logger.LogInformation("[AI Chat] ?? Final request URI: {Uri}", request.RequestUri);
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("[AI Chat] ?? Response status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("[AI Chat] ?? Response headers: {Headers}", string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[AI Chat] ? API Error - Status: {StatusCode}", response.StatusCode);
                _logger.LogError("[AI Chat] ? API Error - Response: {Response}", responseContent);
                _logger.LogError("[AI Chat] ? API Error - Request URL: {Url}", _apiUrl);
                _logger.LogError("[AI Chat] ? API Error - API Key configured: {HasKey}", !string.IsNullOrEmpty(_apiKey));
                
                // X? l� c�c l?i c? th?
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("[AI Chat] ? Unauthorized (401) - API Key c� th? kh�ng h?p l? ho?c d� h?t h?n");
                    _logger.LogError("[AI Chat] ? Check API Key in configuration");
                    return "Xin l?i, API key kh�ng h?p l?. Vui l�ng li�n h? qu?n tr? vi�n d? c?p nh?t c?u h�nh.";
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError("[AI Chat] ? Rate limit exceeded (429)");
                    return "Xin l?i, h? th?ng dang qu� t?i. Vui l�ng th? l?i sau v�i ph�t.";
                }
                
                _logger.LogError("[AI Chat] ? Other error: {StatusCode}", response.StatusCode);
                return "Xin l?i, t�i g?p s? c? khi x? l� c�u h?i c?a b?n. Vui l�ng th? l?i sau ho?c li�n h? b? ph?n h? tr?.";
            }

            _logger.LogInformation("[AI Chat] ?? Response content length: {Length}", responseContent.Length);
            _logger.LogInformation("[AI Chat] ?? Response preview: {Preview}", responseContent.Substring(0, Math.Min(200, responseContent.Length)));

            // Parse response t�y theo provider
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
                // Hugging Face tr? v? array
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

            _logger.LogInformation("[AI Chat] ? Successfully parsed AI response");
            _logger.LogInformation("[AI Chat] ? Response length: {Length}", aiResponse?.Length ?? 0);

            return aiResponse ?? "Xin l?i, t�i kh�ng th? t?o ph?n h?i. Vui l�ng th? l?i.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ? Exception occurred");
            _logger.LogError("[AI Chat] ? Exception type: {Type}", ex.GetType().Name);
            _logger.LogError("[AI Chat] ? Exception message: {Message}", ex.Message);
            _logger.LogError("[AI Chat] ? Stack trace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("[AI Chat] ? Inner exception: {Inner}", ex.InnerException.Message);
            }
            return "Xin l?i, d� x?y ra l?i khi x? l� c�u h?i c?a b?n. Vui l�ng th? l?i sau.";
        }
    }

    /// <summary>
    /// Tr? v? response m?u khi kh�ng c� API key
    /// </summary>
    private string GetSampleResponse(string userMessage)
    {
        var lowerMessage = userMessage.ToLower();

        if (lowerMessage.Contains("d?t ph�ng") || lowerMessage.Contains("booking"))
        {
            return "�? d?t ph�ng, b?n c� th?:\n" +
                   "1. Ch?n ph�ng tr�n trang 'Ph�ng' c?a website\n" +
                   "2. Ch?n ng�y check-in v� check-out\n" +
                   "3. �i?n th�ng tin v� x�c nh?n d?t ph�ng\n" +
                   "4. Thanh to�n qua PayOs ho?c chuy?n kho?n\n\n" +
                   "N?u c?n h? tr?, vui l�ng li�n h? hotline: 1900-xxxx";
        }

        if (lowerMessage.Contains("gi�") || lowerMessage.Contains("ph�"))
        {
            return "Gi� ph�ng t?i Resort Deluxe dao d?ng t? 500.000? - 2.000.000?/d�m t�y lo?i ph�ng.\n" +
                   "B?n c� th? xem chi ti?t gi� tr�n trang 'Ph�ng' ho?c li�n h? d? du?c tu v?n c? th?.";
        }

        if (lowerMessage.Contains("d?ch v?") || lowerMessage.Contains("nh� h�ng") || lowerMessage.Contains("spa"))
        {
            return "Resort Deluxe cung c?p nhi?u d?ch v?:\n" +
                   "??? Nh� h�ng v?i menu da d?ng\n" +
                   "?? Spa v� massage\n" +
                   "?? H? boi ngo�i tr?i\n" +
                   "??? Ph�ng gym\n" +
                   "?? Khu vui choi\n\n" +
                   "B?n c� th? d?t d?ch v? qua website ho?c li�n h? l? t�n.";
        }

        if (lowerMessage.Contains("h?y") || lowerMessage.Contains("d?i"))
        {
            return "Ch�nh s�ch h?y/d?i:\n" +
                   "� H?y tru?c 24h: Mi?n ph�\n" +
                   "� H?y trong 24h: Ph� 50%\n" +
                   "� Kh�ng d?n: Ph� 100%\n\n" +
                   "�? h?y/d?i booking, vui l�ng v�o trang '�?t ph�ng c?a t�i' ho?c li�n h? hotline.";
        }

        return "Xin ch�o! T�i l� tr? l� AI c?a Resort Deluxe. T�i c� th? gi�p b?n:\n" +
               "� Tu v?n d?t ph�ng\n" +
               "� Th�ng tin v? d?ch v?\n" +
               "� Hu?ng d?n thanh to�n\n" +
               "� Ch�nh s�ch h?y/d?i\n\n" +
               "B?n c� c�u h?i g� kh�ng?";
    }

    /// <summary>
    /// L?y d? li?u th?t t? database d?a tr�n user message
    /// </summary>
    private async Task<string> FetchRealDataAsync(string userMessage, int? customerId = null)
    {
        var dataContext = new StringBuilder();
        var lowerMessage = userMessage.ToLower();

        try
        {
            // Detect intent: H?i v? ph�ng
            if (lowerMessage.Contains("ph�ng") || lowerMessage.Contains("room") || 
                lowerMessage.Contains("gi�") || lowerMessage.Contains("price") ||
                lowerMessage.Contains("c�n tr?ng") || lowerMessage.Contains("available"))
            {
                _logger.LogInformation("[AI Chat] ?? Detected room-related query, fetching room data...");

                // L?y available rooms
                if (_roomService != null)
                {
                    try
                    {
                        var rooms = await _roomService.GetAvailableRoomsAsync();
                        if (rooms != null && rooms.Any())
                        {
                            dataContext.AppendLine($"\n?? Ph�ng c�n tr?ng: {rooms.Count()} ph�ng");
                            foreach (var room in rooms.Take(10))
                            {
                                var price = room.PricePerNight > 0 
                                    ? $"{room.PricePerNight:N0} VND/d�m" 
                                    : "Li�n h?";
                                dataContext.AppendLine($"  � Ph�ng {room.RoomNumber} ({room.RoomType}): {price}");
                            }
                            if (rooms.Count() > 10)
                            {
                                dataContext.AppendLine($"  ... v� {rooms.Count() - 10} ph�ng kh�c");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ?? Error fetching available rooms");
                    }
                }

                // L?y room types v� prices
                if (_context != null)
                {
                    try
                    {
                        var roomTypes = await _context.RoomTypes
                            .Where(rt => rt.IsActive)
                            .OrderBy(rt => rt.BasePrice)
                            .ToListAsync();
                        
                        if (roomTypes.Any())
                        {
                            dataContext.AppendLine($"\n?? Lo?i ph�ng v� gi�:");
                            foreach (var rt in roomTypes)
                            {
                                dataContext.AppendLine($"  � {rt.TypeName}: {rt.BasePrice:N0} VND/d�m");
                                if (!string.IsNullOrEmpty(rt.Description))
                                {
                                    var shortDesc = rt.Description.Length > 100 
                                        ? rt.Description.Substring(0, 100) + "..." 
                                        : rt.Description;
                                    dataContext.AppendLine($"    ({shortDesc})");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ?? Error fetching room types");
                    }
                }
            }

            // Detect intent: H?i v? booking
            if ((lowerMessage.Contains("booking") || lowerMessage.Contains("d?t ph�ng") || 
                 lowerMessage.Contains("don d?t") || lowerMessage.Contains("reservation")) &&
                customerId.HasValue && _bookingService != null)
            {
                _logger.LogInformation("[AI Chat] ?? Detected booking-related query, fetching booking data for customer {CustomerId}...", customerId);
                
                try
                {
                    var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId.Value);
                    if (bookings != null && bookings.Any())
                    {
                        dataContext.AppendLine($"\n?? Booking c?a b?n: {bookings.Count()} booking");
                        foreach (var booking in bookings.Take(5).OrderByDescending(b => b.CreatedAt))
                        {
                            var status = booking.Status ?? "Chua x�c d?nh";
                            var amount = booking.EstimatedTotalAmount > 0 
                                ? $"{booking.EstimatedTotalAmount:N0} VND" 
                                : "Chua t�nh";
                            var checkIn = booking.CheckInDate.ToString("dd/MM/yyyy");
                            var checkOut = booking.CheckOutDate.ToString("dd/MM/yyyy");
                            dataContext.AppendLine($"  � {booking.BookingCode}: {status}, {checkIn} - {checkOut}, {amount}");
                        }
                        if (bookings.Count() > 5)
                        {
                            dataContext.AppendLine($"  ... v� {bookings.Count() - 5} booking kh�c");
                        }
                    }
                    else
                    {
                        dataContext.AppendLine($"\n?? B?n chua c� booking n�o");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] ?? Error fetching bookings");
                }
            }

            // Detect intent: H?i v? nh� h�ng / menu
            if (lowerMessage.Contains("nh� h�ng") || lowerMessage.Contains("restaurant") || 
                lowerMessage.Contains("menu") || lowerMessage.Contains("m�n an") ||
                lowerMessage.Contains("d? an") || lowerMessage.Contains("th?c an"))
            {
                _logger.LogInformation("[AI Chat] ?? Detected restaurant-related query, fetching menu data...");

                if (_context != null)
                {
                    try
                    {
                        var menuItems = await _context.Services
                            .Where(s => s.ServiceType == "Restaurant" && s.IsActive)
                            .OrderBy(s => s.ServiceName)
                            .Take(20)
                            .ToListAsync();

                        if (menuItems.Any())
                        {
                            dataContext.AppendLine($"\n??? Menu nh� h�ng: {menuItems.Count} m�n");
                            foreach (var item in menuItems)
                            {
                                var price = item.Price > 0 
                                    ? $"{item.Price:N0} VND" 
                                    : "Li�n h?";
                                var unit = !string.IsNullOrEmpty(item.Unit) ? $" / {item.Unit}" : "";
                                dataContext.AppendLine($"  � {item.ServiceName}: {price}{unit}");
                                if (!string.IsNullOrEmpty(item.Description) && item.Description.Length <= 80)
                                {
                                    dataContext.AppendLine($"    ({item.Description})");
                                }
                            }
                            if (menuItems.Count == 20)
                            {
                                dataContext.AppendLine($"  ... v� nhi?u m�n kh�c");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ?? Error fetching restaurant menu");
                    }
                }
            }

            // Detect intent: H?i v? d�nh gi� / reviews
            if (lowerMessage.Contains("d�nh gi�") || lowerMessage.Contains("review") || 
                lowerMessage.Contains("nh?n x�t") || lowerMessage.Contains("comment") ||
                lowerMessage.Contains("sao") || lowerMessage.Contains("rating"))
            {
                _logger.LogInformation("[AI Chat] ?? Detected review-related query, fetching reviews data...");

                if (_context != null)
                {
                    try
                    {
                        // L?y reviews m?i nh?t v� c� rating cao
                        var recentReviews = await _context.Reviews
                            .Include(r => r.Customer)
                            .Include(r => r.Room)
                            .Where(r => r.IsVisible && r.IsApproved)
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(10)
                            .Select(r => new
                            {
                                r.Rating,
                                r.Comment,
                                CustomerName = r.Customer != null ? (r.Customer.FullName ?? "Kh�ch h�ng") : "Kh�ch h�ng",
                                RoomNumber = r.Room != null ? r.Room.RoomNumber : null
                            })
                            .ToListAsync();

                        // T�nh to�n th?ng k�
                        var stats = await _context.Reviews
                            .Where(r => r.IsVisible && r.IsApproved)
                            .GroupBy(r => r.Rating)
                            .Select(g => new
                            {
                                Rating = g.Key,
                                Count = g.Count()
                            })
                            .ToListAsync();

                        var totalReviews = stats.Sum(s => s.Count);
                        var avgRating = totalReviews > 0 
                            ? stats.Sum(s => s.Rating * s.Count) / (double)totalReviews 
                            : 0;

                        if (totalReviews > 0)
                        {
                            dataContext.AppendLine($"\n? ��nh gi� c?a kh�ch h�ng:");
                            dataContext.AppendLine($"  � T?ng s? d�nh gi�: {totalReviews}");
                            dataContext.AppendLine($"  � �i?m trung b�nh: {avgRating:F1}/5.0 sao");
                            
                            // Th?ng k� theo sao
                            foreach (var stat in stats.OrderByDescending(s => s.Rating))
                            {
                                var stars = new string('?', stat.Rating);
                                dataContext.AppendLine($"  � {stars} ({stat.Rating} sao): {stat.Count} d�nh gi�");
                            }

                            // M?t s? reviews m?i nh?t
                            if (recentReviews.Any())
                            {
                                dataContext.AppendLine($"\n  ?? M?t s? d�nh gi� g?n d�y:");
                                foreach (var review in recentReviews.Take(5))
                                {
                                    var stars = new string('?', review.Rating);
                                    var roomInfo = !string.IsNullOrEmpty(review.RoomNumber) 
                                        ? $" (Ph�ng {review.RoomNumber})" 
                                        : "";
                                    var comment = !string.IsNullOrEmpty(review.Comment) && review.Comment.Length > 60
                                        ? review.Comment.Substring(0, 60) + "..."
                                        : review.Comment ?? "";
                                    dataContext.AppendLine($"    � {stars} {review.CustomerName}{roomInfo}: {comment}");
                                }
                            }
                        }
                        else
                        {
                            dataContext.AppendLine($"\n? Chua c� d�nh gi� n�o");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ?? Error fetching reviews");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ? Error in FetchRealDataAsync");
        }

        var result = dataContext.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            _logger.LogInformation("[AI Chat] ? Fetched real data: {Length} characters", result.Length);
        }
        
        return result;
    }

    /// <summary>
    /// Tr? v? response m?u v?i d? li?u th?t
    /// </summary>
    private string GetSampleResponseWithRealData(string userMessage, string realData)
    {
        var lowerMessage = userMessage.ToLower();
        var response = new StringBuilder();

        if (lowerMessage.Contains("ph�ng") || lowerMessage.Contains("room") || 
            lowerMessage.Contains("gi�") || lowerMessage.Contains("price") ||
            lowerMessage.Contains("c�n tr?ng") || lowerMessage.Contains("available"))
        {
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine("Th�ng tin ph�ng t? h? th?ng:");
                response.AppendLine(realData);
                response.AppendLine("\nB?n c� th? xem chi ti?t v� d?t ph�ng tr�n trang 'Ph�ng' c?a website.");
            }
            else
            {
                response.AppendLine("Hi?n t?i t�i kh�ng th? l?y th�ng tin ph�ng t? h? th?ng.");
                response.AppendLine("Vui l�ng xem tr�n trang 'Ph�ng' c?a website ho?c li�n h? hotline: 1900-xxxx");
            }
            return response.ToString();
        }

        if ((lowerMessage.Contains("booking") || lowerMessage.Contains("d?t ph�ng") || 
             lowerMessage.Contains("don d?t")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Th�ng tin booking c?a b?n:");
            response.AppendLine(realData);
            response.AppendLine("\nB?n c� th? xem chi ti?t tr�n trang '�?t ph�ng c?a t�i'.");
            return response.ToString();
        }

        if ((lowerMessage.Contains("nh� h�ng") || lowerMessage.Contains("restaurant") || 
             lowerMessage.Contains("menu") || lowerMessage.Contains("m�n an")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Th�ng tin menu nh� h�ng:");
            response.AppendLine(realData);
            response.AppendLine("\nB?n c� th? xem chi ti?t v� d?t m�n tr�n trang 'Nh� h�ng' c?a website.");
            return response.ToString();
        }

        if ((lowerMessage.Contains("d�nh gi�") || lowerMessage.Contains("review") || 
             lowerMessage.Contains("nh?n x�t") || lowerMessage.Contains("sao")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Th�ng tin d�nh gi�:");
            response.AppendLine(realData);
            response.AppendLine("\nB?n c� th? xem t?t c? d�nh gi� tr�n trang '��nh gi�' c?a website.");
            return response.ToString();
        }

        // Fallback to normal sample response
        return GetSampleResponse(userMessage);
    }
}


