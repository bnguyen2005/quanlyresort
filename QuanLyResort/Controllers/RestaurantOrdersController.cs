using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/restaurant-orders")]
public class RestaurantOrdersController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly ILogger<RestaurantOrdersController> _logger;

    public RestaurantOrdersController(ResortDbContext context, ILogger<RestaurantOrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Customer đặt món ăn
    /// POST /api/restaurant-orders
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // Customer có thể đặt không cần login (walk-in)
    public async Task<IActionResult> CreateOrder([FromBody] CreateRestaurantOrderRequest request)
    {
        try
        {
            _logger.LogInformation($"[CreateOrder] Request received. CustomerId: {request.CustomerId}, Items count: {request.Items?.Count ?? 0}");
            
            // Validate ModelState (check validation attributes)
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();
                _logger.LogWarning($"[CreateOrder] ModelState validation failed: {string.Join("; ", errors)}");
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }
            
            // Validate Items list
            if (request.Items == null || !request.Items.Any())
            {
                _logger.LogWarning("[CreateOrder] Validation failed: No items in order");
                return BadRequest(new { message = "Đơn hàng phải có ít nhất 1 món" });
            }

            // Validate và lọc items có Quantity = 0 hoặc <= 0
            var invalidItems = request.Items.Where(item => item.Quantity <= 0).ToList();
            if (invalidItems.Any())
            {
                var invalidServiceIds = string.Join(", ", invalidItems.Select(i => i.ServiceId));
                _logger.LogWarning($"[CreateOrder] Validation failed: Items with quantity <= 0: {invalidServiceIds}");
                return BadRequest(new { message = $"Các món có ID {invalidServiceIds} có số lượng bằng 0 hoặc không hợp lệ. Không thể đặt hàng với số lượng = 0. Vui lòng chọn số lượng lớn hơn 0." });
            }

            // Lấy danh sách items hợp lệ (Quantity > 0)
            var validItems = request.Items.Where(item => item.Quantity > 0).ToList();
            if (!validItems.Any())
            {
                _logger.LogWarning("[CreateOrder] Validation failed: All items have quantity <= 0");
                return BadRequest(new { message = "Tất cả món trong đơn hàng đều có số lượng bằng 0 hoặc không hợp lệ. Không thể đặt hàng. Vui lòng chọn số lượng lớn hơn 0." });
            }

            // Validate CustomerId if provided
            if (request.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == request.CustomerId.Value);
                if (!customerExists)
                {
                    return BadRequest(new { message = "CustomerId không tồn tại" });
                }
            }

            // Validate BookingId if provided
            if (request.BookingId.HasValue)
            {
                var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == request.BookingId.Value);
                if (!bookingExists)
                {
                    return BadRequest(new { message = "BookingId không tồn tại" });
                }
            }

            // Validate PaymentMethod if provided
            var validPaymentMethods = new[] { "Cash", "Card", "QR", "RoomCharge", "BankTransfer" };
            if (!string.IsNullOrEmpty(request.PaymentMethod) && !validPaymentMethods.Contains(request.PaymentMethod))
            {
                return BadRequest(new { message = $"PaymentMethod không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validPaymentMethods)}" });
            }

            // Generate order number
            var lastOrder = await _context.RestaurantOrders
                .OrderByDescending(o => o.OrderId)
                .FirstOrDefaultAsync();

            var orderNumber = lastOrder != null
                ? int.Parse(lastOrder.OrderNumber.Replace("ORD", "")) + 1
                : 1;
            
            _logger.LogInformation($"[CreateOrder] Generated order number: ORD{orderNumber:D7}");

            // Create order
            var order = new RestaurantOrder
            {
                OrderNumber = $"ORD{orderNumber:D7}",
                CustomerId = request.CustomerId,
                BookingId = request.BookingId,
                DeliveryAddress = request.DeliveryAddress,
                RequestedDeliveryTime = request.RequestedDeliveryTime,
                SpecialRequests = request.SpecialRequests,
                PaymentMethod = request.PaymentMethod ?? "Cash",
                PaymentStatus = "Unpaid",
                Status = "Pending",
                CreatedBy = request.CustomerId.HasValue ? "Customer" : "Walk-in Guest",
                CreatedAt = DateTime.UtcNow
            };

            // Calculate total and add items (chỉ xử lý các item hợp lệ)
            decimal totalAmount = 0;
            foreach (var item in validItems)
            {
                // Double-check: Validate item (should not reach here if quantity <= 0, but extra safety)
                if (item.Quantity <= 0)
                {
                    _logger.LogError($"[CreateOrder] Critical: Item {item.ServiceId} has quantity <= 0 after filtering");
                    return BadRequest(new { message = $"Số lượng món ID {item.ServiceId} phải lớn hơn 0. Không thể đặt hàng với số lượng = 0." });
                }

                var service = await _context.Services.FindAsync(item.ServiceId);
                if (service == null)
                {
                    return BadRequest(new { message = $"Món ăn ID {item.ServiceId} không tồn tại" });
                }
                
                if (service.ServiceType != "Restaurant")
                {
                    return BadRequest(new { message = $"Dịch vụ ID {item.ServiceId} không phải là món ăn nhà hàng" });
                }
                
                if (!service.IsActive)
                {
                    return BadRequest(new { message = $"Món ăn ID {item.ServiceId} đã bị vô hiệu hóa" });
                }

                if (service.Price < 0)
                {
                    return BadRequest(new { message = $"Giá món ăn ID {item.ServiceId} không hợp lệ" });
                }

                var unitPrice = service.Price;
                var subTotal = unitPrice * item.Quantity;

                if (subTotal < 0)
                {
                    return BadRequest(new { message = $"Tổng tiền món ID {item.ServiceId} không hợp lệ" });
                }

                order.OrderItems.Add(new RestaurantOrderItem
                {
                    ServiceId = item.ServiceId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    SubTotal = subTotal,
                    SpecialNote = item.SpecialNote?.Length > 200 ? item.SpecialNote.Substring(0, 200) : item.SpecialNote
                });

                totalAmount += subTotal;
            }

            // Validate total amount
            if (totalAmount < 0)
            {
                return BadRequest(new { message = "Tổng tiền đơn hàng không hợp lệ" });
            }

            order.TotalAmount = totalAmount;

            _logger.LogInformation($"[CreateOrder] Order calculated. TotalAmount: {totalAmount}, Items: {order.OrderItems.Count}");
            
            _context.RestaurantOrders.Add(order);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"[CreateOrder] ✅ Order saved to database. OrderId: {order.OrderId}, OrderNumber: {order.OrderNumber}");

            // Load order with items and service info
            var createdOrder = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (createdOrder == null)
            {
                _logger.LogError($"[CreateOrder] ⚠️ Created order not found after save! OrderId: {order.OrderId}");
            }
            else
            {
                _logger.LogInformation($"[CreateOrder] ✅ Order loaded successfully. OrderId: {createdOrder.OrderId}, Items: {createdOrder.OrderItems?.Count ?? 0}");
            }

            _logger.LogInformation($"[CreateOrder] ✅ Restaurant order created: {order.OrderNumber} by CustomerId: {request.CustomerId}");

            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant order");
            return StatusCode(500, new { message = "Lỗi khi tạo đơn đặt món", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách đơn đặt món của customer
    /// GET /api/restaurant-orders/my
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Customer,Admin,FrontDesk,Manager")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
            {
                return Unauthorized(new { message = "CustomerId not found in token" });
            }

            var orders = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders");
            return StatusCode(500, new { message = "Lỗi khi tải đơn đặt món", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy tất cả đơn đặt món (admin)
    /// GET /api/restaurant-orders
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<IActionResult> GetAllOrders([FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation($"[GetAllOrders] Request received. Status: {status}, FromDate: {fromDate}, ToDate: {toDate}");
            
            var query = _context.RestaurantOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= toDate.Value);
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            
            _logger.LogInformation($"[GetAllOrders] Found {orders.Count} orders");
            
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return StatusCode(500, new { message = "Lỗi khi tải danh sách đơn", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy chi tiết đơn đặt món
    /// GET /api/restaurant-orders/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow customers to view their orders (will check manually)
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            _logger.LogInformation($"[GetOrderById] Request for OrderId: {id}");
            
            var order = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .Include(o => o.Customer)
                .Include(o => o.Booking)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                _logger.LogWarning($"[GetOrderById] Order {id} not found");
                return NotFound(new { message = "Đơn đặt món không tồn tại" });
            }

            // Check authorization: customer can only view their own orders, admin/staff can view all
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            
            _logger.LogInformation($"[GetOrderById] User role: {userRole}, CustomerId claim: {customerIdClaim}, Order CustomerId: {order.CustomerId}");

            // If authenticated as customer, check if this is their order
            if (userRole == "Customer" && !string.IsNullOrEmpty(customerIdClaim))
            {
                if (int.TryParse(customerIdClaim, out int customerId) && order.CustomerId != customerId)
                {
                    _logger.LogWarning($"[GetOrderById] Customer {customerId} tried to access order {id} belonging to customer {order.CustomerId}");
                    return Forbid();
                }
            }
            // If not authenticated but order has customerId, allow if order was created by walk-in (customerId is null)
            // Or if order has customerId but user is not logged in, still allow (could be shared link)

            _logger.LogInformation($"[GetOrderById] ✅ Order {id} retrieved successfully");
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by id");
            return StatusCode(500, new { message = "Lỗi khi tải chi tiết đơn", error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật trạng thái đơn đặt món (admin)
    /// PATCH /api/restaurant-orders/{id}/status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _context.RestaurantOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Đơn đặt món không tồn tại" });
            }

            // Validate status
            var validStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivered", "Cancelled" };
            if (string.IsNullOrEmpty(request.Status) || !validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = $"Status không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validStatuses)}" });
            }

            // Business rule: Cannot change status if order is already cancelled
            if (order.Status == "Cancelled" && request.Status != "Cancelled")
            {
                return BadRequest(new { message = "Không thể thay đổi trạng thái của đơn hàng đã bị hủy" });
            }

            // Business rule: Cannot cancel if already delivered
            if (order.Status == "Delivered" && request.Status == "Cancelled")
            {
                return BadRequest(new { message = "Không thể hủy đơn hàng đã được giao" });
            }

            var oldStatus = order.Status;
            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {order.OrderNumber} status updated: {oldStatus} -> {request.Status}");

            return Ok(new { message = "Cập nhật trạng thái thành công", order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái", error = ex.Message });
        }
    }

    /// <summary>
    /// Thanh toán đơn đặt món
    /// POST /api/restaurant-orders/{id}/pay
    /// </summary>
    [HttpPost("{id}/pay")]
    [Authorize]
    public async Task<IActionResult> PayOrder(int id, [FromBody] PayOrderRequest request)
    {
        try
        {
            var order = await _context.RestaurantOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Đơn đặt món không tồn tại" });
            }

            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            
            _logger.LogInformation($"[PayOrder] User role: {userRole}, CustomerId claim: {customerIdClaim}, Order CustomerId: {order.CustomerId}");

            // Authorization check:
            // - Customer chỉ có thể thanh toán đơn hàng của chính họ (CustomerId khớp)
            // - Admin/Manager/FrontDesk có thể thanh toán bất kỳ đơn hàng nào
            // - Walk-in orders (CustomerId = null) không thể thanh toán online (cần thanh toán tại nhà hàng)
            if (userRole == "Customer")
            {
                // Customer không thể thanh toán đơn hàng của người khác
                if (string.IsNullOrEmpty(customerIdClaim))
                {
                    _logger.LogWarning($"[PayOrder] Customer without CustomerId claim trying to pay order {id}");
                    return StatusCode(403, new { message = "Không tìm thấy thông tin khách hàng trong token" });
                }
                
                // Walk-in orders (CustomerId = null) không thể thanh toán online
                if (order.CustomerId == null)
                {
                    _logger.LogWarning($"[PayOrder] Customer {customerIdClaim} trying to pay walk-in order {id}");
                    return BadRequest(new { message = "Đơn hàng này là đơn tại quầy, vui lòng thanh toán trực tiếp tại nhà hàng" });
                }
                
                // Kiểm tra CustomerId khớp
                if (int.TryParse(customerIdClaim, out int customerId) && order.CustomerId != customerId)
                {
                    _logger.LogWarning($"[PayOrder] Customer {customerId} trying to pay order {id} belonging to customer {order.CustomerId}");
                    return StatusCode(403, new { message = "Bạn chỉ có thể thanh toán đơn hàng của chính bạn" });
                }
                
                // CustomerId null hoặc không parse được
                if (!int.TryParse(customerIdClaim, out _))
                {
                    _logger.LogWarning($"[PayOrder] Invalid CustomerId claim: {customerIdClaim}");
                    return StatusCode(403, new { message = "Token không hợp lệ" });
                }
            }
            // Admin/Manager/FrontDesk có thể thanh toán bất kỳ đơn hàng nào (không cần check thêm)

            if (order.PaymentStatus == "Paid")
            {
                return BadRequest(new { message = "Đơn hàng đã được thanh toán" });
            }

            // Validate PaymentMethod
            var validPaymentMethods = new[] { "Cash", "Card", "QR", "RoomCharge", "BankTransfer" };
            if (string.IsNullOrEmpty(request.PaymentMethod) || !validPaymentMethods.Contains(request.PaymentMethod))
            {
                return BadRequest(new { message = $"PaymentMethod không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validPaymentMethods)}" });
            }

            // Business rule: Cannot pay if order is cancelled
            if (order.Status == "Cancelled")
            {
                return BadRequest(new { message = "Không thể thanh toán đơn hàng đã bị hủy" });
            }

            order.PaymentMethod = request.PaymentMethod;
            order.PaymentStatus = "Paid";
            order.UpdatedAt = DateTime.UtcNow;

            // If status is Pending, update to Confirmed
            if (order.Status == "Pending")
            {
                order.Status = "Confirmed";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {order.OrderNumber} paid via {request.PaymentMethod}");

            return Ok(new { message = "Thanh toán thành công", order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying order");
            return StatusCode(500, new { message = "Lỗi khi thanh toán", error = ex.Message });
        }
    }
}

// DTOs
public class CreateRestaurantOrderRequest
{
    public int? CustomerId { get; set; }
    public int? BookingId { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime? RequestedDeliveryTime { get; set; }
    public string? SpecialRequests { get; set; }
    public string? PaymentMethod { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    [Required(ErrorMessage = "ServiceId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ServiceId must be greater than 0")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0. Cannot order with quantity = 0.")]
    public int Quantity { get; set; }

    [StringLength(200, ErrorMessage = "SpecialNote cannot exceed 200 characters")]
    public string? SpecialNote { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class PayOrderRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
}
