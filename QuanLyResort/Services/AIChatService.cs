using QuanLyResort.Repositories;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using System.Globalization;

namespace QuanLyResort.Services;

/// <summary>
/// Service để tương tác với AI Chat API (Groq, OpenAI, Cohere, HuggingFace hoặc Sample mode)
/// Tự động tích hợp dữ liệu thời gian thực từ Database của Resort vào ngữ cảnh trò chuyện.
/// </summary>
public class AIChatService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIChatService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IBookingService? _bookingService;
    private readonly IRoomService? _roomService;
    private readonly IUnitOfWork? _unitOfWork;
    private ResortDbContext? _context => _unitOfWork?.Context;
    private readonly string? _apiKey;
    private readonly string _apiUrl;
    private readonly string _model;
    private readonly string _provider; // "openai", "groq", "huggingface", "cohere", "sample"

    public bool HasDbConnection => _context != null;

    public AIChatService(
        IConfiguration configuration,
        ILogger<AIChatService> logger,
        HttpClient httpClient,
        IUnitOfWork? unitOfWork = null,
        IBookingService? bookingService = null,
        IRoomService? roomService = null)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _unitOfWork = unitOfWork;
        _bookingService = bookingService;
        _roomService = roomService;

        // Clear any existing BaseAddress để tránh conflict với absolute URLs
        if (_httpClient.BaseAddress != null)
        {
            _logger.LogWarning("[AI Chat] HttpClient has BaseAddress: {BaseAddress}, clearing it", _httpClient.BaseAddress);
            _httpClient.BaseAddress = null;
        }

        // Clear default headers để tránh conflict
        _httpClient.DefaultRequestHeaders.Clear();

        var aiConfig = _configuration.GetSection("AIChat");
        _apiKey = aiConfig["ApiKey"];
        _provider = aiConfig["Provider"]?.ToLowerInvariant() ?? "sample";
        _model = aiConfig["Model"] ?? "llama-3.1-8b-instant";

        // Cấu hình URL endpoint tùy theo Provider
        if (string.IsNullOrEmpty(_apiKey) || _provider == "sample")
        {
            _apiUrl = "";
            _logger.LogInformation("[AI Chat] Chế độ phản hồi mẫu với dữ liệu thật từ DB (Provider: sample hoặc chưa có API Key)");
        }
        else if (_provider == "groq")
        {
            _apiUrl = aiConfig["ApiUrl"] ?? "https://api.groq.com/openai/v1/chat/completions";
            _model = aiConfig["Model"] ?? "llama-3.1-8b-instant";
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
            _model = aiConfig["Model"] ?? "gpt-3.5-turbo";
        }

        if (!string.IsNullOrEmpty(_apiKey) && _provider != "sample")
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ResortDeluxe-AIChat/1.0");

            if (_provider == "huggingface")
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            _logger.LogInformation("[AI Chat] API Key đã sẵn sàng (Provider: {Provider}, Model: {Model})", _provider, _model);
        }
        else
        {
            _logger.LogInformation("[AI Chat] Khởi chạy ở chế độ Local Sample với dữ liệu thật từ Database");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Gửi câu hỏi đến AI và nhận câu trả lời được tăng cường dữ liệu từ Database (RAG)
    /// </summary>
    public async Task<string> SendMessageAsync(string userMessage, string? conversationContext = null, int? customerId = null)
    {
        try
        {
            // 1. Luôn truy vấn dữ liệu thời gian thực từ Database dựa theo nội dung câu hỏi
            var realData = await FetchRealDataAsync(userMessage, customerId);

            // 2. Nếu không có API key hoặc ở chế độ sample, trả về câu trả lời thông minh dựa trên dữ liệu thật
            if (string.IsNullOrEmpty(_apiKey) || _provider == "sample")
            {
                _logger.LogInformation("[AI Chat] Trả về phản hồi thông minh định dạng từ dữ liệu thực tế Database");
                return GetSampleResponseWithRealData(userMessage, realData);
            }

            // 3. Chuẩn bị System Prompt với dữ liệu thật từ Database
            var systemPrompt = $@"Bạn là trợ lý AI chuyên nghiệp, nhiệt tình và thân thiện của Resort Deluxe (Khu nghỉ dưỡng cao cấp Resort Deluxe).
Nhiệm vụ của bạn là tư vấn, giải đáp thắc mắc và hỗ trợ khách hàng dựa trên dữ liệu thực tế của resort được cung cấp dưới đây.

DỮ LIỆU THỰC TẾ TỪ CƠ SỞ DỮ LIỆU CỦA RESORT:
{realData}

NGUYÊN TẮC TRẢ LỜI:
1. Luôn ưu tiên sử dụng thông tin và con số thực tế ở trên (giá phòng, loại phòng, số lượng phòng trống, thực đơn, dịch vụ, đánh giá) để trả lời chính xác.
2. Không bịa đặt giá cả hoặc tiện ích không có trong dữ liệu trên.
3. Trả lời bằng tiếng Việt lịch sự, chu đáo, định dạng đẹp mắt (dùng gạch đầu dòng, icon thân thiện).
4. Khi khách hàng muốn đặt phòng, hướng dẫn khách chọn phòng và bấm nút 'Đặt phòng ngay' trên website.";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (!string.IsNullOrEmpty(conversationContext))
            {
                messages.Add(new { role = "assistant", content = conversationContext });
            }

            messages.Add(new { role = "user", content = userMessage });

            object requestBody;
            if (_provider == "cohere")
            {
                requestBody = new
                {
                    message = userMessage,
                    model = _model,
                    temperature = 0.7,
                    max_tokens = 600
                };
            }
            else if (_provider == "huggingface")
            {
                requestBody = new
                {
                    inputs = userMessage,
                    parameters = new
                    {
                        max_new_tokens = 600,
                        temperature = 0.7
                    }
                };
            }
            else
            {
                requestBody = new
                {
                    model = _model,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 600
                };
            }

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestUri = Uri.TryCreate(_apiUrl, UriKind.Absolute, out var uri) ? uri : new Uri(_apiUrl, UriKind.Absolute);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[AI Chat] API Error - Status: {StatusCode}, Response: {Response}", response.StatusCode, responseContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return "Xin lỗi, API Key của AI hiện chưa hợp lệ hoặc đã hết hạn. Hệ thống đang chuyển sang chế độ dự phòng bằng dữ liệu thật:\n\n" + GetSampleResponseWithRealData(userMessage, realData);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return "Hệ thống AI đang quá tải. Dưới đây là thông tin thực tế từ hệ thống resort để hỗ trợ bạn:\n\n" + GetSampleResponseWithRealData(userMessage, realData);
                }

                // Dự phòng trả về dữ liệu thật khi external API lỗi
                return GetSampleResponseWithRealData(userMessage, realData);
            }

            string? aiResponse = null;
            if (_provider == "cohere")
            {
                var doc = JsonDocument.Parse(responseContent);
                aiResponse = doc.RootElement.GetProperty("text").GetString();
            }
            else if (_provider == "huggingface")
            {
                var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    aiResponse = doc.RootElement[0].GetProperty("generated_text").GetString();
                }
            }
            else
            {
                var doc = JsonDocument.Parse(responseContent);
                aiResponse = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }

            return aiResponse ?? GetSampleResponseWithRealData(userMessage, realData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] Ngoại lệ khi xử lý tin nhắn chat: {Message}", ex.Message);
            var fallbackData = await FetchRealDataAsync(userMessage, customerId);
            return GetSampleResponseWithRealData(userMessage, fallbackData);
        }
    }

    /// <summary>
    /// Loại bỏ dấu tiếng Việt để so khớp intent chính xác kể cả khi khách gõ không dấu hoặc viết tắt
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
    }

    /// <summary>
    /// Truy vấn dữ liệu thực tế từ Database dựa trên câu hỏi người dùng
    /// </summary>
    public async Task<string> FetchRealDataAsync(string userMessage, int? customerId = null)
    {
        var dataContext = new StringBuilder();
        var rawLower = (userMessage ?? "").ToLower();
        var norm = RemoveDiacritics(rawLower);

        try
        {
            if (_context == null)
            {
                _logger.LogWarning("[AI Chat] Không thể truy cập Database (UnitOfWork Context is null)");
                return "Hệ thống đang bảo trì dữ liệu cục bộ.";
            }

            // Nhận diện ý định câu hỏi (hỗ trợ cả tiếng Việt có dấu, không dấu và tiếng Anh)
            bool isRoomQuery = norm.Contains("phong") || norm.Contains("room") || 
                               norm.Contains("gia") || norm.Contains("price") || 
                               norm.Contains("trong") || norm.Contains("available") ||
                               norm.Contains("suite") || norm.Contains("deluxe") || 
                               norm.Contains("villa") || norm.Contains("standard");

            bool isFoodQuery = norm.Contains("nha hang") || norm.Contains("restaurant") || 
                               norm.Contains("menu") || norm.Contains("thuc don") || 
                               norm.Contains("mon an") || norm.Contains("do an") || 
                               norm.Contains("thuc an") || norm.Contains("an uong") || 
                               norm.Contains("buffet") || norm.Contains("nuoc") || 
                               norm.Contains("cocktail");

            bool isServiceQuery = norm.Contains("dich vu") || norm.Contains("service") || 
                                  norm.Contains("spa") || norm.Contains("massage") || 
                                  norm.Contains("gym") || norm.Contains("ho boi") || 
                                  norm.Contains("pool") || norm.Contains("tien ich") || 
                                  norm.Contains("tour") || norm.Contains("xe");

            bool isReviewQuery = norm.Contains("danh gia") || norm.Contains("review") || 
                                 norm.Contains("nhan xet") || norm.Contains("rating") || 
                                 norm.Contains("sao") || norm.Contains("star") || 
                                 norm.Contains("feedback") || norm.Contains("chat luong");

            bool isBookingQuery = norm.Contains("booking") || norm.Contains("dat phong") || 
                                  norm.Contains("don dat") || norm.Contains("reservation") || 
                                  norm.Contains("lich su") || norm.Contains("ma dat");

            bool isPolicyQuery = norm.Contains("chinh sach") || norm.Contains("policy") || 
                                 norm.Contains("quy dinh") || norm.Contains("check in") || 
                                 norm.Contains("check out") || norm.Contains("checkin") || 
                                 norm.Contains("checkout") || norm.Contains("nhan phong") || 
                                 norm.Contains("tra phong") || norm.Contains("huy") || 
                                 norm.Contains("cancel") || norm.Contains("doi phong") ||
                                 norm.Contains("hoan tien");

            bool isGeneralQuery = !isRoomQuery && !isFoodQuery && !isServiceQuery && 
                                  !isReviewQuery && !isBookingQuery && !isPolicyQuery;

            // 1. DỮ LIỆU CÁC HẠNG PHÒNG & GIÁ NIÊM YẾT (Load khi hỏi phòng hoặc hỏi chung)
            if (isRoomQuery || isGeneralQuery)
            {
                try
                {
                    var roomTypes = await _context.RoomTypes
                        .Where(rt => rt.IsActive)
                        .OrderBy(rt => rt.BasePrice)
                        .ToListAsync();

                    if (roomTypes.Any())
                    {
                        dataContext.AppendLine("=== CÁC HẠNG PHÒNG VÀ GIÁ NIÊM YẾT TẠI RESORT ===");
                        foreach (var rt in roomTypes)
                        {
                            var priceStr = rt.BasePrice > 0 ? $"{rt.BasePrice:N0} VND/đêm" : "Liên hệ";
                            dataContext.AppendLine($"• Hạng {rt.TypeName} (Mã: {rt.TypeCode}):");
                            dataContext.AppendLine($"  - Giá tiêu chuẩn: {priceStr}");
                            dataContext.AppendLine($"  - Sức chứa: {rt.StandardOccupancy} người lớn (Tối đa {rt.MaxOccupancy} người)");
                            if (rt.RoomSize > 0) dataContext.AppendLine($"  - Diện tích: {rt.RoomSize} m²");
                            if (!string.IsNullOrEmpty(rt.BedType)) dataContext.AppendLine($"  - Loại giường: {rt.BedType}");
                            if (!string.IsNullOrEmpty(rt.Amenities)) dataContext.AppendLine($"  - Tiện nghi: {rt.Amenities}");
                            if (!string.IsNullOrEmpty(rt.Description)) dataContext.AppendLine($"  - Mô tả: {rt.Description}");
                        }
                    }

                    // Danh sách phòng thực tế đang còn trống
                    var availableRooms = await _context.Rooms
                        .Where(r => r.IsAvailable)
                        .OrderBy(r => r.RoomNumber)
                        .Take(12)
                        .ToListAsync();

                    if (availableRooms.Any())
                    {
                        dataContext.AppendLine($"\n=== PHÒNG THỰC TẾ ĐANG CÒN TRỐNG ({availableRooms.Count} phòng) ===");
                        foreach (var r in availableRooms)
                        {
                            var price = r.PricePerNight > 0 ? $"{r.PricePerNight:N0} VND/đêm" : "Theo giá hạng";
                            dataContext.AppendLine($"• Phòng {r.RoomNumber} - Hạng {r.RoomType} - Tầng {r.Floor} - Giá: {price}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] Lỗi lấy danh sách phòng từ DB");
                }
            }

            // 2. DỮ LIỆU THỰC ĐƠN NHÀ HÀNG & ẨM THỰC
            if (isFoodQuery || isGeneralQuery)
            {
                try
                {
                    var menuItems = await _context.Services
                        .Where(s => s.ServiceType == "Restaurant" && s.IsActive)
                        .OrderBy(s => s.ServiceName)
                        .Take(15)
                        .ToListAsync();

                    if (menuItems.Any())
                    {
                        dataContext.AppendLine("\n=== THỰC ĐƠN NHÀ HÀNG RESORT ===");
                        foreach (var item in menuItems)
                        {
                            var price = item.Price > 0 ? $"{item.Price:N0} VND" : "Miễn phí / Theo thời giá";
                            var unit = !string.IsNullOrEmpty(item.Unit) ? $"/{item.Unit}" : "";
                            var desc = !string.IsNullOrEmpty(item.Description) ? $" ({item.Description})" : "";
                            dataContext.AppendLine($"• {item.ServiceName}: {price}{unit}{desc}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] Lỗi lấy menu nhà hàng từ DB");
                }
            }

            // 3. DỊCH VỤ & TIỆN ÍCH RESORT (Spa, Hồ bơi, Gym, Tour, v.v.)
            if (isServiceQuery || isGeneralQuery)
            {
                try
                {
                    var otherServices = await _context.Services
                        .Where(s => s.ServiceType != "Restaurant" && s.IsActive)
                        .OrderBy(s => s.ServiceType)
                        .ThenBy(s => s.ServiceName)
                        .Take(15)
                        .ToListAsync();

                    if (otherServices.Any())
                    {
                        dataContext.AppendLine("\n=== CÁC DỊCH VỤ & TIỆN ÍCH NỔI BẬT ===");
                        foreach (var s in otherServices)
                        {
                            var price = s.Price > 0 ? $"{s.Price:N0} VND" : "Tiện ích có sẵn";
                            var unit = !string.IsNullOrEmpty(s.Unit) ? $"/{s.Unit}" : "";
                            dataContext.AppendLine($"• [{s.ServiceType}] {s.ServiceName}: {price}{unit}");
                            if (!string.IsNullOrEmpty(s.Description)) dataContext.AppendLine($"  Chi tiết: {s.Description}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] Lỗi lấy dịch vụ từ DB");
                }
            }

            // 4. ĐÁNH GIÁ & XẾP HẠNG THỰC TẾ TỪ KHÁCH HÀNG
            if (isReviewQuery || isGeneralQuery)
            {
                try
                {
                    var reviewsQuery = _context.Reviews.Where(r => r.IsVisible && r.IsApproved);
                    var totalReviews = await reviewsQuery.CountAsync();

                    if (totalReviews > 0)
                    {
                        var avgRating = await reviewsQuery.AverageAsync(r => r.Rating);
                        var recentReviews = await reviewsQuery
                            .Include(r => r.Customer)
                            .Include(r => r.Room)
                            .OrderByDescending(r => r.CreatedAt)
                            .Take(5)
                            .Select(r => new
                            {
                                r.Rating,
                                r.Comment,
                                CustomerName = r.Customer != null ? r.Customer.FullName : "Khách lưu trú",
                                RoomNumber = r.Room != null ? r.Room.RoomNumber : null
                            })
                            .ToListAsync();

                        dataContext.AppendLine($"\n=== ĐÁNH GIÁ CỦA KHÁCH HÀNG (Điểm trung bình: {avgRating:F1}/5.0 sao - {totalReviews} lượt đánh giá) ===");
                        foreach (var rev in recentReviews)
                        {
                            var roomInfo = !string.IsNullOrEmpty(rev.RoomNumber) ? $" (Phòng {rev.RoomNumber})" : "";
                            dataContext.AppendLine($"• {rev.Rating}★ - {rev.CustomerName}{roomInfo}: \"{rev.Comment}\"");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] Lỗi lấy đánh giá từ DB");
                }
            }

            // 5. THÔNG TIN BOOKING CỦA KHÁCH HÀNG HIỆN TẠI (Nếu đã đăng nhập)
            if (isBookingQuery && customerId.HasValue)
            {
                try
                {
                    var customerBookings = await _context.Bookings
                        .Include(b => b.Room)
                        .Where(b => b.CustomerId == customerId.Value)
                        .OrderByDescending(b => b.CreatedAt)
                        .Take(5)
                        .ToListAsync();

                    if (customerBookings.Any())
                    {
                        dataContext.AppendLine($"\n=== LỊCH SỬ ĐẶT PHÒNG CỦA QUÝ KHÁCH ({customerBookings.Count} đơn gần nhất) ===");
                        foreach (var b in customerBookings)
                        {
                            var totalStr = b.EstimatedTotalAmount > 0 ? $"{b.EstimatedTotalAmount:N0} VND" : "Chưa có";
                            var roomStr = b.Room != null ? $"Phòng {b.Room.RoomNumber} ({b.Room.RoomType})" : (b.RequestedRoomType ?? "Chưa chỉ định");
                            dataContext.AppendLine($"• Mã đơn: {b.BookingCode} | Trạng thái: {b.Status} | Thời gian: {b.CheckInDate:dd/MM/yyyy} - {b.CheckOutDate:dd/MM/yyyy} | Phòng: {roomStr} | Tổng tiền: {totalStr}");
                        }
                    }
                    else
                    {
                        dataContext.AppendLine("\n=== ĐƠN ĐẶT PHÒNG CỦA QUÝ KHÁCH ===");
                        dataContext.AppendLine("Quý khách hiện chưa có đơn đặt phòng nào trên hệ thống.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] Lỗi lấy booking của khách hàng từ DB");
                }
            }

            // 6. CHÍNH SÁCH VÀ CÂU HỎI THƯỜNG GẶP (FAQs)
            if (isPolicyQuery || isGeneralQuery)
            {
                try
                {
                    var faqs = await _context.FAQs
                        .Where(f => f.IsActive)
                        .OrderBy(f => f.DisplayOrder)
                        .Take(6)
                        .ToListAsync();

                    if (faqs.Any())
                    {
                        dataContext.AppendLine("\n=== CHÍNH SÁCH VÀ CÂU HỎI THƯỜNG GẶP ===");
                        foreach (var faq in faqs)
                        {
                            dataContext.AppendLine($"• Hỏi: {faq.Question}");
                            dataContext.AppendLine($"  Đáp: {faq.Answer}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AI Chat] Lỗi lấy FAQs từ DB");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Chat] Lỗi tổng quát trong FetchRealDataAsync");
        }

        var result = dataContext.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            _logger.LogInformation("[AI Chat] ✅ Đã nạp thành công {Length} ký tự dữ liệu thực từ Database", result.Length);
        }

        return result;
    }

    /// <summary>
    /// Trả về câu trả lời thông minh dựa trên dữ liệu thực tế từ Database khi chạy chế độ Sample/Offline
    /// </summary>
    private string GetSampleResponseWithRealData(string userMessage, string realData)
    {
        var rawLower = (userMessage ?? "").ToLower();
        var norm = RemoveDiacritics(rawLower);
        var response = new StringBuilder();

        bool isRoomQuery = norm.Contains("phong") || norm.Contains("room") || 
                           norm.Contains("gia") || norm.Contains("price") || 
                           norm.Contains("trong") || norm.Contains("available") ||
                           norm.Contains("suite") || norm.Contains("deluxe") || 
                           norm.Contains("villa") || norm.Contains("standard");

        bool isFoodQuery = norm.Contains("nha hang") || norm.Contains("restaurant") || 
                           norm.Contains("menu") || norm.Contains("thuc don") || 
                           norm.Contains("mon an") || norm.Contains("do an") || 
                           norm.Contains("thuc an") || norm.Contains("an uong") || 
                           norm.Contains("buffet");

        bool isServiceQuery = norm.Contains("dich vu") || norm.Contains("service") || 
                              norm.Contains("spa") || norm.Contains("massage") || 
                              norm.Contains("gym") || norm.Contains("ho boi") || 
                              norm.Contains("pool") || norm.Contains("tien ich");

        bool isReviewQuery = norm.Contains("danh gia") || norm.Contains("review") || 
                             norm.Contains("nhan xet") || norm.Contains("rating") || 
                             norm.Contains("sao") || norm.Contains("star");

        bool isBookingQuery = norm.Contains("booking") || norm.Contains("dat phong") || 
                              norm.Contains("don dat") || norm.Contains("reservation");

        bool isPolicyQuery = norm.Contains("chinh sach") || norm.Contains("policy") || 
                             norm.Contains("quy dinh") || norm.Contains("check in") || 
                             norm.Contains("check out") || norm.Contains("checkin") || 
                             norm.Contains("checkout") || norm.Contains("huy");

        if (isRoomQuery)
        {
            response.AppendLine("🏨 **THÔNG TIN PHÒNG VÀ GIÁ TỪ HỆ THỐNG RESORT DELUXE:**\n");
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine(realData);
                response.AppendLine("\n💡 *Quý khách có thể xem ảnh chi tiết và đặt phòng trực tiếp tại mục 'Phòng' trên thanh điều hướng của website.*");
            }
            else
            {
                response.AppendLine("Hiện tại hệ thống đang cập nhật danh sách phòng. Quý khách vui lòng truy cập trang 'Phòng' hoặc liên hệ hotline để được hỗ trợ.");
            }
            return response.ToString();
        }

        if (isFoodQuery)
        {
            response.AppendLine("🍽️ **THỰC ĐƠN & NHÀ HÀNG RESORT DELUXE:**\n");
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine(realData);
                response.AppendLine("\n💡 *Quý khách có thể gọi món trực tiếp qua trang 'Nhà hàng' hoặc liên hệ lễ tân để phục vụ tại phòng.*");
            }
            return response.ToString();
        }

        if (isServiceQuery)
        {
            response.AppendLine("✨ **DỊCH VỤ & TIỆN ÍCH CAO CẤP TẠI RESORT DELUXE:**\n");
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine(realData);
                response.AppendLine("\n💡 *Quý khách có thể liên hệ quầy lễ tân hoặc đặt dịch vụ trực tiếp trên website.*");
            }
            return response.ToString();
        }

        if (isReviewQuery)
        {
            response.AppendLine("⭐ **ĐÁNH GIÁ THỰC TẾ TỪ KHÁCH LƯU TRÚ:**\n");
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine(realData);
                response.AppendLine("\n💡 *Xem toàn bộ nhận xét tại trang 'Đánh giá' trên hệ thống.*");
            }
            return response.ToString();
        }

        if (isBookingQuery)
        {
            response.AppendLine("📋 **THÔNG TIN ĐẶT PHÒNG:**\n");
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine(realData);
                response.AppendLine("\n💡 *Quý khách có thể quản lý chi tiết tại mục 'Đặt phòng của tôi'.*");
            }
            else
            {
                response.AppendLine("Để đặt phòng mới, quý khách vui lòng chọn phòng tại trang 'Phòng', chọn ngày check-in/check-out và xác nhận đặt phòng.");
            }
            return response.ToString();
        }

        if (isPolicyQuery)
        {
            response.AppendLine("📜 **CHÍNH SÁCH RESORT DELUXE:**\n");
            if (!string.IsNullOrEmpty(realData))
            {
                response.AppendLine(realData);
            }
            else
            {
                response.AppendLine("• Giờ nhận phòng (Check-in): từ 14:00.");
                response.AppendLine("• Giờ trả phòng (Check-out): trước 12:00.");
                response.AppendLine("• Hủy phòng: Miễn phí trước 24 giờ nhận phòng.");
            }
            return response.ToString();
        }

        // Trường hợp chào hỏi hoặc câu hỏi tổng quan: Hiển thị lời chào kèm thông tin tóm tắt thực tế từ DB
        response.AppendLine("Dạ xin chào Quý khách! Em là Trợ lý AI của **Resort Deluxe** 🏖️");
        response.AppendLine("Dưới đây là thông tin mới nhất từ hệ thống cơ sở dữ liệu của resort:\n");

        if (!string.IsNullOrEmpty(realData))
        {
            response.AppendLine(realData);
        }
        else
        {
            response.AppendLine("• Resort cung cấp đầy đủ các hạng phòng Standard, Deluxe, Suite và Villa cao cấp.");
            response.AppendLine("• Tiện ích: Hồ bơi vô cực, Spa trị liệu, Nhà hàng ẩm thực Á - Âu, Phòng Gym 24/7.");
        }

        response.AppendLine("\nQuý khách có thể hỏi em bất kỳ thông tin nào về **giá phòng, thực đơn nhà hàng, dịch vụ spa, đánh giá hoặc chính sách đặt phòng** ạ!");
        return response.ToString();
    }
}
