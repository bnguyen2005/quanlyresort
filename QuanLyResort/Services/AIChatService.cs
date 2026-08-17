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
/// Service để tương tác với AI Chat API
/// Hỗ trợ OpenAI hoặc các AI service khác
/// </summary>
public class AIChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIChatService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IBookingService? _bookingService;
    private readonly IRoomService? _roomService;
    private readonly IUnitOfWork _unitOfWork;
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

        // Clear any existing BaseAddress để tránh conflict với absolute URLs
        if (_httpClient.BaseAddress != null)
        {
            _logger.LogWarning("[AI Chat] ⚠️ HttpClient has BaseAddress: {BaseAddress}, clearing it", _httpClient.BaseAddress);
            _httpClient.BaseAddress = null;
        }
        
        // Clear default headers để tránh conflict
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
            // Groq sử dụng format giống OpenAI, nhưng URL phải chính xác
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
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
            
            _logger.LogInformation("[AI Chat] ✅ API Key configured (length: {Length}, provider: {Provider})", _apiKey.Length, _provider);
        }
        else
        {
            _logger.LogInformation("[AI Chat] 📝 Using sample responses (no API key or provider=sample)");
        }
        
        // Set timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Log final HttpClient state
        _logger.LogInformation("[AI Chat] 📋 HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress?.ToString() ?? "null");

        _logger.LogInformation("[AI Chat] ✅ Service initialized - Provider: {Provider}, Model: {Model}, API URL: {ApiUrl}", _provider, _model, _apiUrl);
    }

    /// <summary>
    /// Gửi message đến AI và nhận response
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, string? conversationContext = null, int? customerId = null)
    {
        try
        {
            // Fetch real data từ database dựa trên user message
            var realData = await FetchRealDataAsync(userMessage, customerId);
            
            // Nếu không có API key hoặc provider là "sample", trả về response mẫu với dữ liệu thật
            if (string.IsNullOrEmpty(_apiKey) || _provider == "sample")
            {
                _logger.LogInformation("[AI Chat] 📝 Using sample response mode with real data");
                return GetSampleResponseWithRealData(userMessage, realData);
            }

            // Tạo system prompt cho resort context với dữ liệu thật
            var systemPrompt = $@"Bạn là trợ lý AI thân thiện của Resort Deluxe. 
Bạn giúp khách hàng với các câu hỏi về:
- Đặt phòng và booking
- Dịch vụ resort (nhà hàng, spa, hồ bơi, v.v.)
- Thanh toán và hóa đơn
- Chính sách hủy và đổi
- Thông tin về phòng và tiện nghi
- Hướng dẫn sử dụng website

Dữ liệu thật từ website:
{realData}

Hãy trả lời ngắn gọn, thân thiện và hữu ích bằng tiếng Việt, dựa trên dữ liệu thật ở trên.";

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
            _logger.LogInformation("[AI Chat] 📤 Request method: POST");
            _logger.LogInformation("[AI Chat] 📤 Full URL: {Url}", _apiUrl);

            // Đảm bảo không có BaseAddress conflict - sử dụng absolute URI
            Uri requestUri;
            if (Uri.TryCreate(_apiUrl, UriKind.Absolute, out requestUri))
            {
                // URL đã là absolute, sử dụng trực tiếp
            }
            else
            {
                // Nếu URL không absolute, tạo absolute URI
                requestUri = new Uri(_apiUrl, UriKind.Absolute);
            }

            // Tạo HttpRequestMessage với POST method rõ ràng
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };
            
            _logger.LogInformation("[AI Chat] 📤 Final request URI: {Uri}", request.RequestUri);
            
            var response = await _httpClient.SendAsync(request);
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

    /// <summary>
    /// Lấy dữ liệu thật từ database dựa trên user message
    /// </summary>
    private async Task<string> FetchRealDataAsync(string userMessage, int? customerId = null)
    {
        var dataContext = new StringBuilder();
        var lowerMessage = userMessage.ToLower();

        try
        {
            // Detect intent: Hỏi về phòng
            if (lowerMessage.Contains("phòng") || lowerMessage.Contains("room") || 
                lowerMessage.Contains("giá") || lowerMessage.Contains("price") ||
                lowerMessage.Contains("còn trống") || lowerMessage.Contains("available"))
            {
                _logger.LogInformation("[AI Chat] 🔍 Detected room-related query, fetching room data...");

                // Lấy available rooms
                if (_roomService != null)
                {
                    try
                    {
                        var rooms = await _roomService.GetAvailableRoomsAsync();
                        if (rooms != null && rooms.Any())
                        {
                            dataContext.AppendLine($"\n📋 Phòng còn trống: {rooms.Count()} phòng");
                            foreach (var room in rooms.Take(10))
                            {
                                var price = room.PricePerNight > 0 
                                    ? $"{room.PricePerNight:N0} VND/đêm" 
                                    : "Liên hệ";
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
                        _logger.LogWarning(ex, "[AI Chat] ⚠️ Error fetching available rooms");
                    }
                }

                // Lấy room types và prices
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
                            dataContext.AppendLine($"\n💰 Loại phòng và giá:");
                            foreach (var rt in roomTypes)
                            {
                                dataContext.AppendLine($"  • {rt.TypeName}: {rt.BasePrice:N0} VND/đêm");
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
                        _logger.LogWarning(ex, "[AI Chat] ⚠️ Error fetching room types");
                    }
                }
            }

            // Detect intent: Hỏi về booking
            if ((lowerMessage.Contains("booking") || lowerMessage.Contains("đặt phòng") || 
                 lowerMessage.Contains("đơn đặt") || lowerMessage.Contains("reservation")) &&
                customerId.HasValue && _bookingService != null)
            {
                _logger.LogInformation("[AI Chat] 🔍 Detected booking-related query, fetching booking data for customer {CustomerId}...", customerId);
                
                try
                {
                    var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId.Value);
                    if (bookings != null && bookings.Any())
                    {
                        dataContext.AppendLine($"\n📅 Booking của bạn: {bookings.Count()} booking");
                        foreach (var booking in bookings.Take(5).OrderByDescending(b => b.CreatedAt))
                        {
                            var status = booking.Status ?? "Chưa xác định";
                            var amount = booking.EstimatedTotalAmount > 0 
                                ? $"{booking.EstimatedTotalAmount:N0} VND" 
                                : "Chưa tính";
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
                        dataContext.AppendLine($"\n📅 Bạn chưa có booking nào");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] ⚠️ Error fetching bookings");
                }
            }

            // Detect intent: Hỏi về nhà hàng / menu
            if (lowerMessage.Contains("nhà hàng") || lowerMessage.Contains("restaurant") || 
                lowerMessage.Contains("menu") || lowerMessage.Contains("món ăn") ||
                lowerMessage.Contains("đồ ăn") || lowerMessage.Contains("thức ăn"))
            {
                _logger.LogInformation("[AI Chat] 🔍 Detected restaurant-related query, fetching menu data...");

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
                            dataContext.AppendLine($"\n🍽️ Menu nhà hàng: {menuItems.Count} món");
                            foreach (var item in menuItems)
                            {
                                var price = item.Price > 0 
                                    ? $"{item.Price:N0} VND" 
                                    : "Liên hệ";
                                var unit = !string.IsNullOrEmpty(item.Unit) ? $" / {item.Unit}" : "";
                                dataContext.AppendLine($"  • {item.ServiceName}: {price}{unit}");
                                if (!string.IsNullOrEmpty(item.Description) && item.Description.Length <= 80)
                                {
                                    dataContext.AppendLine($"    ({item.Description})");
                                }
                            }
                            if (menuItems.Count == 20)
                            {
                                dataContext.AppendLine($"  ... và nhiều món khác");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ⚠️ Error fetching restaurant menu");
                    }
                }
            }

            // Detect intent: Hỏi về đánh giá / reviews
            if (lowerMessage.Contains("đánh giá") || lowerMessage.Contains("review") || 
                lowerMessage.Contains("nhận xét") || lowerMessage.Contains("comment") ||
                lowerMessage.Contains("sao") || lowerMessage.Contains("rating"))
            {
                _logger.LogInformation("[AI Chat] 🔍 Detected review-related query, fetching reviews data...");

                if (_context != null)
                {
                    try
                    {
                        // Lấy reviews mới nhất và có rating cao
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

                        // Tính toán thống kê
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
                            dataContext.AppendLine($"\n⭐ Đánh giá của khách hàng:");
                            dataContext.AppendLine($"  • Tổng số đánh giá: {totalReviews}");
                            dataContext.AppendLine($"  • Điểm trung bình: {avgRating:F1}/5.0 sao");
                            
                            // Thống kê theo sao
                            foreach (var stat in stats.OrderByDescending(s => s.Rating))
                            {
                                var stars = new string('⭐', stat.Rating);
                                dataContext.AppendLine($"  • {stars} ({stat.Rating} sao): {stat.Count} đánh giá");
                            }

                            // Một số reviews mới nhất
                            if (recentReviews.Any())
                            {
                                dataContext.AppendLine($"\n  📝 Một số đánh giá gần đây:");
                                foreach (var review in recentReviews.Take(5))
                                {
                                    var stars = new string('⭐', review.Rating);
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
                            dataContext.AppendLine($"\n⭐ Chưa có đánh giá nào");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AI Chat] ⚠️ Error fetching reviews");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] ❌ Error in FetchRealDataAsync");
        }

        var result = dataContext.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            _logger.LogInformation("[AI Chat] ✅ Fetched real data: {Length} characters", result.Length);
        }
        
        return result;
    }

    /// <summary>
    /// Trả về response mẫu với dữ liệu thật
    /// </summary>
    private string GetSampleResponseWithRealData(string userMessage, string realData)
    {
        var lowerMessage = userMessage.ToLower();
        var response = new StringBuilder();

        if (lowerMessage.Contains("phòng") || lowerMessage.Contains("room") || 
            lowerMessage.Contains("giá") || lowerMessage.Contains("price") ||
            lowerMessage.Contains("còn trống") || lowerMessage.Contains("available"))
        {
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine("Thông tin phòng từ hệ thống:");
                response.AppendLine(realData);
                response.AppendLine("\nBạn có thể xem chi tiết và đặt phòng trên trang 'Phòng' của website.");
            }
            else
            {
                response.AppendLine("Hiện tại tôi không thể lấy thông tin phòng từ hệ thống.");
                response.AppendLine("Vui lòng xem trên trang 'Phòng' của website hoặc liên hệ hotline: 1900-xxxx");
            }
            return response.ToString();
        }

        if ((lowerMessage.Contains("booking") || lowerMessage.Contains("đặt phòng") || 
             lowerMessage.Contains("đơn đặt")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Thông tin booking của bạn:");
            response.AppendLine(realData);
            response.AppendLine("\nBạn có thể xem chi tiết trên trang 'Đặt phòng của tôi'.");
            return response.ToString();
        }

        if ((lowerMessage.Contains("nhà hàng") || lowerMessage.Contains("restaurant") || 
             lowerMessage.Contains("menu") || lowerMessage.Contains("món ăn")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Thông tin menu nhà hàng:");
            response.AppendLine(realData);
            response.AppendLine("\nBạn có thể xem chi tiết và đặt món trên trang 'Nhà hàng' của website.");
            return response.ToString();
        }

        if ((lowerMessage.Contains("đánh giá") || lowerMessage.Contains("review") || 
             lowerMessage.Contains("nhận xét") || lowerMessage.Contains("sao")) && !string.IsNullOrEmpty(realData))
        {
            response.AppendLine("Thông tin đánh giá:");
            response.AppendLine(realData);
            response.AppendLine("\nBạn có thể xem tất cả đánh giá trên trang 'Đánh giá' của website.");
            return response.ToString();
        }

        // Fallback to normal sample response
        return GetSampleResponse(userMessage);
    }
}


