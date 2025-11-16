# 🤖 Implement AI Chat Với Dữ Liệu Thật Từ Website

## 📊 Tình Trạng Hiện Tại

**AI Chat hiện tại:**
- ❌ Chưa có truy cập database
- ❌ Chỉ trả về responses mẫu hoặc từ AI API
- ❌ Không thể lấy dữ liệu thật như rooms, bookings, prices

## 🎯 Mục Tiêu

**Cho phép AI Chat:**
- ✅ Lấy danh sách phòng thật từ database
- ✅ Lấy giá phòng thật
- ✅ Lấy thông tin booking (nếu user đã đăng nhập)
- ✅ Trả lời câu hỏi dựa trên dữ liệu thật

## 🔧 Giải Pháp: Function Calling / Tool Use

### Cách 1: Function Calling (Khuyến Nghị)

**Sử dụng OpenAI Function Calling hoặc Groq Tool Use để AI có thể gọi các function:**

1. **AI nhận message từ user**
2. **AI quyết định cần gọi function nào** (ví dụ: `get_rooms`, `get_room_prices`)
3. **Backend gọi function và lấy dữ liệu thật**
4. **Backend gửi dữ liệu thật vào context cho AI**
5. **AI trả lời dựa trên dữ liệu thật**

### Cách 2: RAG (Retrieval Augmented Generation)

**Embed dữ liệu vào vector database và retrieve khi cần:**

1. **Embed rooms, prices vào vector database**
2. **Khi user hỏi, search vector database**
3. **Lấy relevant data và gửi vào AI context**
4. **AI trả lời dựa trên retrieved data**

### Cách 3: Pre-fetch Data (Đơn Giản Nhất)

**Lấy dữ liệu thật trước khi gửi đến AI:**

1. **Parse user message để detect intent** (hỏi về phòng, giá, booking)
2. **Gọi API/service để lấy dữ liệu thật**
3. **Format dữ liệu và thêm vào system prompt**
4. **Gửi đến AI với context đầy đủ**

## 💡 Implementation Plan

### Bước 1: Thêm Dependencies Vào AIChatService

**Inject các services cần thiết:**
- `IBookingService` - Lấy thông tin booking
- `IRoomService` - Lấy thông tin phòng
- `ResortDbContext` - Truy cập database trực tiếp (nếu cần)

### Bước 2: Detect Intent Từ User Message

**Parse user message để biết user muốn gì:**
- "Phòng nào còn trống?" → Cần lấy available rooms
- "Giá phòng là bao nhiêu?" → Cần lấy room prices
- "Tôi có booking nào không?" → Cần lấy user bookings
- "Phòng Deluxe có gì?" → Cần lấy room details

### Bước 3: Fetch Real Data

**Gọi service để lấy dữ liệu thật:**
```csharp
// Ví dụ: Lấy available rooms
var availableRooms = await _roomService.GetAvailableRoomsAsync();

// Ví dụ: Lấy room prices
var roomTypes = await _context.RoomTypes.ToListAsync();

// Ví dụ: Lấy user bookings (nếu đã đăng nhập)
var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId);
```

### Bước 4: Format Data Và Thêm Vào Context

**Format dữ liệu thành text và thêm vào system prompt:**
```csharp
var dataContext = $@"
Dữ liệu thật từ website:
- Phòng còn trống: {string.Join(", ", availableRooms.Select(r => r.RoomNumber))}
- Giá phòng: {string.Join("\n", roomTypes.Select(rt => $"{rt.TypeName}: {rt.BasePrice:N0} VND/đêm"))}
- Booking của bạn: {string.Join("\n", bookings.Select(b => $"Booking {b.BookingCode}: {b.Status}"))}
";

var systemPrompt = $@"
Bạn là trợ lý AI của Resort Deluxe.
Dữ liệu thật từ website:
{dataContext}

Hãy trả lời dựa trên dữ liệu thật này.
";
```

## 🔧 Code Implementation

### 1. Update AIChatService Constructor

```csharp
private readonly IBookingService? _bookingService;
private readonly IRoomService? _roomService;
private readonly ResortDbContext? _context;

public AIChatService(
    IConfiguration configuration,
    ILogger<AIChatService> logger,
    HttpClient httpClient,
    IBookingService? bookingService = null,
    IRoomService? roomService = null,
    ResortDbContext? context = null)
{
    // ... existing code ...
    _bookingService = bookingService;
    _roomService = roomService;
    _context = context;
}
```

### 2. Add Data Fetching Methods

```csharp
/// <summary>
/// Lấy dữ liệu thật từ database dựa trên user message
/// </summary>
private async Task<string> FetchRealDataAsync(string userMessage, int? customerId = null)
{
    var dataContext = new StringBuilder();
    var lowerMessage = userMessage.ToLower();

    // Detect intent và fetch data
    if (lowerMessage.Contains("phòng") || lowerMessage.Contains("room"))
    {
        // Lấy available rooms
        if (_roomService != null)
        {
            var rooms = await _roomService.GetAvailableRoomsAsync();
            dataContext.AppendLine($"Phòng còn trống: {rooms.Count} phòng");
            foreach (var room in rooms.Take(10))
            {
                dataContext.AppendLine($"- {room.RoomNumber} ({room.RoomType}): {room.PricePerNight:N0} VND/đêm");
            }
        }

        // Lấy room types và prices
        if (_context != null)
        {
            var roomTypes = await _context.RoomTypes
                .Where(rt => rt.IsActive)
                .ToListAsync();
            
            dataContext.AppendLine("\nLoại phòng và giá:");
            foreach (var rt in roomTypes)
            {
                dataContext.AppendLine($"- {rt.TypeName}: {rt.BasePrice:N0} VND/đêm");
            }
        }
    }

    if (lowerMessage.Contains("booking") || lowerMessage.Contains("đặt phòng"))
    {
        if (customerId.HasValue && _bookingService != null)
        {
            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId.Value);
            dataContext.AppendLine($"\nBooking của bạn: {bookings.Count} booking");
            foreach (var booking in bookings.Take(5))
            {
                dataContext.AppendLine($"- {booking.BookingCode}: {booking.Status}, {booking.EstimatedTotalAmount:N0} VND");
            }
        }
    }

    return dataContext.ToString();
}
```

### 3. Update SendMessageAsync

```csharp
public async Task<string> SendMessageAsync(string userMessage, string? conversationContext = null, int? customerId = null)
{
    try
    {
        // Fetch real data based on user message
        var realData = await FetchRealDataAsync(userMessage, customerId);

        // Tạo system prompt với dữ liệu thật
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

        // ... rest of existing code ...
    }
}
```

### 4. Update AIChatController

```csharp
[HttpPost("send")]
[AllowAnonymous]
public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
{
    try
    {
        // Get customer ID from JWT token if available
        int? customerId = null;
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (!string.IsNullOrEmpty(customerIdClaim) && int.TryParse(customerIdClaim, out var id))
        {
            customerId = id;
        }

        var response = await _aiChatService.SendMessageAsync(
            request.Message, 
            request.Context,
            customerId); // Pass customer ID
        
        return Ok(new
        {
            success = true,
            message = response,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        // ... error handling ...
    }
}
```

## 📋 Checklist Implementation

- [ ] **Inject services vào AIChatService:**
  - [ ] `IBookingService`
  - [ ] `IRoomService`
  - [ ] `ResortDbContext`

- [ ] **Implement FetchRealDataAsync:**
  - [ ] Detect intent từ user message
  - [ ] Fetch available rooms
  - [ ] Fetch room types và prices
  - [ ] Fetch user bookings (nếu đã đăng nhập)

- [ ] **Update SendMessageAsync:**
  - [ ] Gọi FetchRealDataAsync
  - [ ] Thêm real data vào system prompt
  - [ ] Pass customer ID nếu có

- [ ] **Update AIChatController:**
  - [ ] Extract customer ID từ JWT token
  - [ ] Pass customer ID vào SendMessageAsync

- [ ] **Test:**
  - [ ] Test với câu hỏi về phòng
  - [ ] Test với câu hỏi về giá
  - [ ] Test với câu hỏi về booking (đã đăng nhập)
  - [ ] Test với câu hỏi chung (không cần data)

## 🎯 Ví Dụ

### Trước (Không có dữ liệu thật):
**User:** "Phòng nào còn trống?"
**AI:** "Để đặt phòng, bạn có thể chọn phòng trên trang 'Phòng' của website..."

### Sau (Có dữ liệu thật):
**User:** "Phòng nào còn trống?"
**AI:** "Hiện tại có 5 phòng còn trống:
- Phòng 101 (Deluxe): 1,500,000 VND/đêm
- Phòng 102 (Standard): 800,000 VND/đêm
- Phòng 201 (Suite): 2,500,000 VND/đêm
..."

## 🔗 Links

- **AIChatService:** `QuanLyResort/Services/AIChatService.cs`
- **AIChatController:** `QuanLyResort/Controllers/AIChatController.cs`
- **RoomService:** `QuanLyResort/Services/RoomService.cs`
- **BookingService:** `QuanLyResort/Services/BookingService.cs`

## 💡 Lưu Ý

1. **Performance:** Chỉ fetch data khi cần (detect intent)
2. **Caching:** Có thể cache data trong vài phút để giảm database queries
3. **Privacy:** Chỉ lấy booking của user đó (không lấy booking của user khác)
4. **Error Handling:** Nếu fetch data lỗi, vẫn trả về response từ AI (fallback)

