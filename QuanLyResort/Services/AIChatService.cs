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
/// Service d? tuong tác v?i AI Chat API
/// H? tr? OpenAI ho?c các AI service khác
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

        // Clear any existing BaseAddress d? tránh conflict v?i absolute URLs
        if (_httpClient.BaseAddress != null)
        {
            _logger.LogWarning("[AI Chat] ?? HttpClient has BaseAddress: {BaseAddress}, clearing it", _httpClient.BaseAddress);
            _httpClient.BaseAddress = null;
        }
        
        // Clear default headers d? tránh conflict
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
            // Groq s? d?ng format gi?ng OpenAI, nhung URL ph?i chính xác
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
    /// G?i message d?n AI và nh?n response
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, string? conversationContext = null, int? customerId = null)
    {
        try
        {
            // Fetch real data t? database d?a trên user message
            var realData = await FetchRealDataAsync(userMessage, customerId);
            
            // N?u không có API key ho?c provider là "sample", tr? v? response m?u v?i d? li?u th?t
            if (string.IsNullOrEmpty(_apiKey) || _provider == "sample")
            {
                _logger.LogInformation("[AI Chat] ?? Using sample response mode with real data");
                return GetSampleResponseWithRealData(userMessage, realData);
            }

            // T?o system prompt cho resort context v?i d? li?u th?t
            var systemPrompt = $@"B?n là tr? lý AI thân thi?n c?a Resort Deluxe. 
B?n giúp khách hàng v?i các câu h?i v?:
- Ð?t phòng và booking
- D?ch v? resort (nhà hàng, spa, h? boi, v.v.)
- Thanh toán và hóa don
- Chính sách h?y và d?i
- Thông tin v? phòng và ti?n nghi
- Hu?ng d?n s? d?ng website

D? li?u th?t t? website:
{realData}

Hãy tr? l?i ng?n g?n, thân thi?n và h?u ích b?ng ti?ng Vi?t, d?a trên d? li?u th?t ? trên.";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Thêm context n?u có
            if (!string.IsNullOrEmpty(conversationContext))
            {
                messages.Add(new { role = "assistant", content = conversationContext });
            }

            // Thêm user message
            messages.Add(new { role = "user", content = userMessage });

            // T?o request body tùy theo provider
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

            // Ð?m b?o không có BaseAddress conflict - s? d?ng absolute URI
            Uri requestUri;
            if (Uri.TryCreate(_apiUrl, UriKind.Absolute, out requestUri))
            {
                // URL dã là absolute, s? d?ng tr?c ti?p
            }
            else
            {
                // N?u URL không absolute, t?o absolute URI
                requestUri = new Uri(_apiUrl, UriKind.Absolute);
            }

            // T?o HttpRequestMessage v?i POST method rõ ràng
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
                
                // X? lý các l?i c? th?
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("[AI Chat] ? Unauthorized (401) - API Key có th? không h?p l? ho?c dã h?t h?n");
                    _logger.LogError("[AI Chat] ? Check API Key in configuration");
                    return "Xin l?i, API key không h?p l?. Vui lòng liên h? qu?n tr? viên d? c?p nh?t c?u hình.";
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError("[AI Chat] ? Rate limit exceeded (429)");
                    return "Xin l?i, h? th?ng dang quá t?i. Vui lòng th? l?i sau vài phút.";
                }
                
                _logger.LogError("[AI Chat] ? Other error: {StatusCode}", response.StatusCode);
                return "Xin l?i, tôi g?p s? c? khi x? lý câu h?i c?a b?n. Vui lòng th? l?i sau ho?c liên h? b? ph?n h? tr?.";
            }

            _logger.LogInformation("[AI Chat] ?? Response content length: {Length}", responseContent.Length);
            _logger.LogInformation("[AI Chat] ?? Response preview: {Preview}", responseContent.Substring(0, Math.Min(200, responseContent.Length)));

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

            return aiResponse ?? "Xin l?i, tôi không th? t?o ph?n h?i. Vui lòng th? l?i.";
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
            return "Xin l?i, dã x?y ra l?i khi x? lý câu h?i c?a b?n. Vui lòng th? l?i sau.";
        }
    }

    /// <summary>
    /// Tr? v? response m?u khi không có API key
    /// </summary>
    private string GetSampleResponse(string userMessage)
    {
        var lowerMessage = userMessage.ToLower();

        if (lowerMessage.Contains("d?t phòng") || lowerMessage.Contains("booking"))
        {
            return "Ð? d?t phòng, b?n có th?:\n" +
                   "1. Ch?n phòng trên trang 'Phòng' c?a website\n" +
                   "2. Ch?n ngày check-in và check-out\n" +
                   "3. Ði?n thông tin và xác nh?n d?t phòng\n" +
                   "4. Thanh toán qua PayOs ho?c chuy?n kho?n\n\n" +
                   "N?u c?n h? tr?, vui lòng liên h? hotline: 1900-xxxx";
        }

        if (lowerMessage.Contains("giá") || lowerMessage.Contains("phí"))
        {
            return "Giá phòng t?i Resort Deluxe dao d?ng t? 500.000? - 2.000.000?/dêm tùy lo?i phòng.\n" +
                   "B?n có th? xem chi ti?t giá trên trang 'Phòng' ho?c liên h? d? du?c tu v?n c? th?.";
        }

        if (lowerMessage.Contains("d?ch v?") || lowerMessage.Contains("nhà hàng") || lowerMessage.Contains("spa"))
        {
            return "Resort Deluxe cung c?p nhi?u d?ch v?:\n" +
                   "??? Nhà hàng v?i menu da d?ng\n" +
                   "?? Spa và massage\n" +
                   "?? H? boi ngoài tr?i\n" +
                   "??? Phòng gym\n" +
                   "?? Khu vui choi\n\n" +
                   "B?n có th? d?t d?ch v? qua website ho?c liên h? l? tân.";
        }

        if (lowerMessage.Contains("h?y") || lowerMessage.Contains("d?i"))
        {
            return "Chính sách h?y/d?i:\n" +
                   "• H?y tru?c 24h: Mi?n phí\n" +
                   "• H?y trong 24h: Phí 50%\n" +
                   "• Không d?n: Phí 100%\n\n" +
                   "Ð? h?y/d?i booking, vui lòng vào trang 'Ð?t phòng c?a tôi' ho?c liên h? hotline.";
        }

        return "Xin chào! Tôi là tr? lý AI c?a Resort Deluxe. Tôi có th? giúp b?n:\n" +
               "• Tu v?n d?t phòng\n" +
               "• Thông tin v? d?ch v?\n" +
               "• Hu?ng d?n thanh toán\n" +
               "• Chính sách h?y/d?i\n\n" +
               "B?n có câu h?i gì không?";
    }

    /// <summary>
    /// L?y d? li?u th?t t? database d?a trên user message
    /// </summary>
    private async Task<string> FetchRealDataAsync(string userMessage, int? customerId = null)
    {
        var dataContext = new StringBuilder();
        var lowerMessage = userMessage.ToLower();

        try
        {
            // Detect intent: H?i v? phòng
            if (lowerMessage.Contains("phòng") || lowerMessage.Contains("room") || 
                lowerMessage.Contains("giá") || lowerMessage.Contains("price") ||
                lowerMessage.Contains("còn tr?ng") || lowerMessage.Contains("available"))
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
                            dataContext.AppendLine($"\n?? Phòng còn tr?ng: {rooms.Count()} phòng");
                            foreach (var room in rooms.Take(10))
                            {
                                var price = room.PricePerNight > 0 
                                    ? $"{room.PricePerNight:N0} VND/dêm" 
                                    : "Liên h?";
                                dataContext.AppendLine($"  • Phòng {room.RoomNumber} ({room.RoomType}): {price}");
                            }
                            if (rooms.Count() > 10)
                            {
                                dataContext.AppendLine($"  ... và {rooms.Count() - 10} phòng khác");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ?? Error fetching available rooms");
                    }
                }

                // L?y room types và prices
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
                            dataContext.AppendLine($"\n?? Lo?i phòng và giá:");
                            foreach (var rt in roomTypes)
                            {
                                dataContext.AppendLine($"  • {rt.TypeName}: {rt.BasePrice:N0} VND/dêm");
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
            if ((lowerMessage.Contains("booking") || lowerMessage.Contains("d?t phòng") || 
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
                            var status = booking.Status ?? "Chua xác d?nh";
                            var amount = booking.EstimatedTotalAmount > 0 
                                ? $"{booking.EstimatedTotalAmount:N0} VND" 
                                : "Chua tính";
                            var checkIn = booking.CheckInDate.ToString("dd/MM/yyyy");
                            var checkOut = booking.CheckOutDate.ToString("dd/MM/yyyy");
                            dataContext.AppendLine($"  • {booking.BookingCode}: {status}, {checkIn} - {checkOut}, {amount}");
                        }
                        if (bookings.Count() > 5)
                        {
                            dataContext.AppendLine($"  ... và {bookings.Count() - 5} booking khác");
                        }
                    }
                    else
                    {
                        dataContext.AppendLine($"\n?? B?n chua có booking nào");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] ?? Error fetching bookings");
                }
            }

            // Detect intent: H?i v? nhà hàng / menu
            if (lowerMessage.Contains("nhà hàng") || lowerMessage.Contains("restaurant") || 
                lowerMessage.Contains("menu") || lowerMessage.Contains("món an") ||
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
                            dataContext.AppendLine($"\n??? Menu nhà hàng: {menuItems.Count} món");
                            foreach (var item in menuItems)
                            {
                                var price = item.Price > 0 
                                    ? $"{item.Price:N0} VND" 
                                    : "Liên h?";
                                var unit = !string.IsNullOrEmpty(item.Unit) ? $" / {item.Unit}" : "";
                                dataContext.AppendLine($"  • {item.ServiceName}: {price}{unit}");
                                if (!string.IsNullOrEmpty(item.Description) && item.Description.Length <= 80)
                                {
                                    dataContext.AppendLine($"    ({item.Description})");
                                }
                            }
                            if (menuItems.Count == 20)
                            {
                                dataContext.AppendLine($"  ... và nhi?u món khác");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ?? Error fetching restaurant menu");
                    }
                }
            }

            // Detect intent: H?i v? dánh giá / reviews
            if (lowerMessage.Contains("dánh giá") || lowerMessage.Contains("review") || 
                lowerMessage.Contains("nh?n xét") || lowerMessage.Contains("comment") ||
                lowerMessage.Contains("sao") || lowerMessage.Contains("rating"))
            {
                _logger.LogInformation("[AI Chat] ?? Detected review-related query, fetching reviews data...");

                if (_context != null)
                {
                    try
                    {
                        // L?y reviews m?i nh?t và có rating cao
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
                                CustomerName = r.Customer != null ? (r.Customer.FullName ?? "Khách hàng") : "Khách hàng",
                                RoomNumber = r.Room != null ? r.Room.RoomNumber : null
                            })
                            .ToListAsync();

                        // Tính toán th?ng kê
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
                            dataContext.AppendLine($"\n? Ðánh giá c?a khách hàng:");
                            dataContext.AppendLine($"  • T?ng s? dánh giá: {totalReviews}");
                            dataContext.AppendLine($"  • Ði?m trung bình: {avgRating:F1}/5.0 sao");
                            
                            // Th?ng kê theo sao
                            foreach (var stat in stats.OrderByDescending(s => s.Rating))
                            {
                                var stars = new string('?', stat.Rating);
                                dataContext.AppendLine($"  • {stars} ({stat.Rating} sao): {stat.Count} dánh giá");
                            }

                            // M?t s? reviews m?i nh?t
                            if (recentReviews.Any())
                            {
                                dataContext.AppendLine($"\n  ?? M?t s? dánh giá g?n dây:");
                                foreach (var review in recentReviews.Take(5))
                                {
                                    var stars = new string('?', review.Rating);
                                    var roomInfo = !string.IsNullOrEmpty(review.RoomNumber) 
                                        ? $" (Phòng {review.RoomNumber})" 
                                        : "";
                                    var comment = !string.IsNullOrEmpty(review.Comment) && review.Comment.Length > 60
                                        ? review.Comment.Substring(0, 60) + "..."
                                        : review.Comment ?? "";
                                    dataContext.AppendLine($"    • {stars} {review.CustomerName}{roomInfo}: {comment}");
                                }
                            }
                        }
                        else
                        {
                            dataContext.AppendLine($"\n? Chua có dánh giá nào");
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

        if (lowerMessage.Contains("phòng") || lowerMessage.Contains("room") || 
            lowerMessage.Contains("giá") || lowerMessage.Contains("price") ||
            lowerMessage.Contains("còn tr?ng") || lowerMessage.Contains("available"))
        {
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine("Thông tin phòng t? h? th?ng:");
                response.AppendLine(realData);
                response.AppendLine("\nB?n có th? xem chi ti?t và d?t phòng trên trang 'Phòng' c?a website.");
            }
            else
            {
                response.AppendLine("Hi?n t?i tôi không th? l?y thông tin phòng t? h? th?ng.");
                response.AppendLine("Vui lòng xem trên trang 'Phòng' c?a website ho?c liên h? hotline: 1900-xxxx");
            }
            return response.ToString();
        }

        if ((lowerMessage.Contains("booking") || lowerMessage.Contains("d?t phòng") || 
             lowerMessage.Contains("don d?t")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Thông tin booking c?a b?n:");
            response.AppendLine(realData);
            response.AppendLine("\nB?n có th? xem chi ti?t trên trang 'Ð?t phòng c?a tôi'.");
            return response.ToString();
        }

        if ((lowerMessage.Contains("nhà hàng") || lowerMessage.Contains("restaurant") || 
             lowerMessage.Contains("menu") || lowerMessage.Contains("món an")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Thông tin menu nhà hàng:");
            response.AppendLine(realData);
            response.AppendLine("\nB?n có th? xem chi ti?t và d?t món trên trang 'Nhà hàng' c?a website.");
            return response.ToString();
        }

        if ((lowerMessage.Contains("dánh giá") || lowerMessage.Contains("review") || 
             lowerMessage.Contains("nh?n xét") || lowerMessage.Contains("sao")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Thông tin dánh giá:");
            response.AppendLine(realData);
            response.AppendLine("\nB?n có th? xem t?t c? dánh giá trên trang 'Ðánh giá' c?a website.");
            return response.ToString();
        }

        // Fallback to normal sample response
        return GetSampleResponse(userMessage);
    }
}


