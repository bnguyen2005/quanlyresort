using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/restaurant-orders")]
public class RestaurantOrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;
    private readonly ILogger<RestaurantOrdersController> _logger;
    private readonly INotificationManager _notificationManager;

    public RestaurantOrdersController(
        IUnitOfWork unitOfWork, 
        ILogger<RestaurantOrdersController> logger,
        INotificationManager notificationManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationManager = notificationManager;
    }

    /// <summary>
    /// Customer d?t món an
    /// POST /api/restaurant-orders
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // Customer có th? d?t không c?n login (walk-in)
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
                return BadRequest(new { message = "D? li?u không h?p l?", errors });
            }
            
            // Validate Items list
            if (request.Items == null || !request.Items.Any())
            {
                _logger.LogWarning("[CreateOrder] Validation failed: No items in order");
                return BadRequest(new { message = "Ðon hàng ph?i có ít nh?t 1 món" });
            }

            // Validate và l?c items có Quantity = 0 ho?c <= 0
            var invalidItems = request.Items.Where(item => item.Quantity <= 0).ToList();
            if (invalidItems.Any())
            {
                var invalidServiceIds = string.Join(", ", invalidItems.Select(i => i.ServiceId));
                _logger.LogWarning($"[CreateOrder] Validation failed: Items with quantity <= 0: {invalidServiceIds}");
                return BadRequest(new { message = $"Các món có ID {invalidServiceIds} có s? lu?ng b?ng 0 ho?c không h?p l?. Không th? d?t hàng v?i s? lu?ng = 0. Vui lòng ch?n s? lu?ng l?n hon 0." });
            }

            // L?y danh sách items h?p l? (Quantity > 0)
            var validItems = request.Items.Where(item => item.Quantity > 0).ToList();
            if (!validItems.Any())
            {
                _logger.LogWarning("[CreateOrder] Validation failed: All items have quantity <= 0");
                return BadRequest(new { message = "T?t c? món trong don hàng d?u có s? lu?ng b?ng 0 ho?c không h?p l?. Không th? d?t hàng. Vui lòng ch?n s? lu?ng l?n hon 0." });
            }

            // Validate CustomerId if provided
            if (request.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == request.CustomerId.Value);
                if (!customerExists)
                {
                    return BadRequest(new { message = "CustomerId không t?n t?i" });
                }
            }

            // Validate BookingId if provided
            if (request.BookingId.HasValue)
            {
                var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == request.BookingId.Value);
                if (!bookingExists)
                {
                    return BadRequest(new { message = "BookingId không t?n t?i" });
                }
            }

            // Validate PaymentMethod if provided
            var validPaymentMethods = new[] { "Cash", "Card", "QR", "RoomCharge", "BankTransfer" };
            if (!string.IsNullOrEmpty(request.PaymentMethod) && !validPaymentMethods.Contains(request.PaymentMethod))
            {
                return BadRequest(new { message = $"PaymentMethod không h?p l?. Ch? ch?p nh?n: {string.Join(", ", validPaymentMethods)}" });
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

            // Calculate total and add items (ch? x? lý các item h?p l?)
            decimal totalAmount = 0;
            foreach (var item in validItems)
            {
                // Double-check: Validate item (should not reach here if quantity <= 0, but extra safety)
                if (item.Quantity <= 0)
                {
                    _logger.LogError($"[CreateOrder] Critical: Item {item.ServiceId} has quantity <= 0 after filtering");
                    return BadRequest(new { message = $"S? lu?ng món ID {item.ServiceId} ph?i l?n hon 0. Không th? d?t hàng v?i s? lu?ng = 0." });
                }

                var service = await _context.Services.FindAsync(item.ServiceId);
                if (service == null)
                {
                    return BadRequest(new { message = $"Món an ID {item.ServiceId} không t?n t?i" });
                }
                
                if (service.ServiceType != "Restaurant")
                {
                    return BadRequest(new { message = $"D?ch v? ID {item.ServiceId} không ph?i là món an nhà hàng" });
                }
                
                if (!service.IsActive)
                {
                    return BadRequest(new { message = $"Món an ID {item.ServiceId} dã b? vô hi?u hóa" });
                }

                if (service.Price < 0)
                {
                    return BadRequest(new { message = $"Giá món an ID {item.ServiceId} không h?p l?" });
                }

                var unitPrice = service.Price;
                var subTotal = unitPrice * item.Quantity;

                if (subTotal < 0)
                {
                    return BadRequest(new { message = $"T?ng ti?n món ID {item.ServiceId} không h?p l?" });
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
                return BadRequest(new { message = "T?ng ti?n don hàng không h?p l?" });
            }

            order.TotalAmount = totalAmount;

            _logger.LogInformation($"[CreateOrder] Order calculated. TotalAmount: {totalAmount}, Items: {order.OrderItems.Count}");
            
            _context.RestaurantOrders.Add(order);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"[CreateOrder] ? Order saved to database. OrderId: {order.OrderId}, OrderNumber: {order.OrderNumber}");

            // Load order with items and service info
            var createdOrder = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (createdOrder == null)
            {
                _logger.LogError($"[CreateOrder] ?? Created order not found after save! OrderId: {order.OrderId}");
            }
            else
            {
                _logger.LogInformation($"[CreateOrder] ? Order loaded successfully. OrderId: {createdOrder.OrderId}, Items: {createdOrder.OrderItems?.Count ?? 0}");
            }

            _logger.LogInformation($"[CreateOrder] ? Restaurant order created: {order.OrderNumber} by CustomerId: {request.CustomerId}");

            // Send order confirmation notification
            if (request.CustomerId.HasValue && createdOrder != null)
            {
                _ = Task.Run(async () =>
                {
                    await _notificationManager.SendOrderConfirmationAsync(
                        request.CustomerId.Value,
                        order.OrderNumber,
                        order.TotalAmount
                    );
                });
            }

            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant order");
            return StatusCode(500, new { message = "L?i khi t?o don d?t món", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y danh sách don d?t món c?a customer
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
            return StatusCode(500, new { message = "L?i khi t?i don d?t món", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y t?t c? don d?t món (admin)
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
            return StatusCode(500, new { message = "L?i khi t?i danh sách don", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y chi ti?t don d?t món
    /// GET /api/restaurant-orders/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow customers to view their orders (will check manually)
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            _logger.LogInformation($"[GetOrderById] ?? Request to get order {id}");
            
            var order = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .Include(o => o.Customer)
                .Include(o => o.Booking)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                _logger.LogWarning($"[GetOrderById] ? Order {id} not found");
                return NotFound(new { message = "Ðon d?t món không t?n t?i" });
            }

            // Check authorization: customer can only view their own orders, admin/staff can view all
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            
            _logger.LogInformation($"[GetOrderById] ?? User role: {userRole}, CustomerId claim: {customerIdClaim}, Order CustomerId: {order.CustomerId}");

            // If authenticated as customer, check if this is their order
            if (userRole == "Customer" && !string.IsNullOrEmpty(customerIdClaim))
            {
                if (int.TryParse(customerIdClaim, out int customerId) && order.CustomerId != customerId)
                {
                    _logger.LogWarning($"[GetOrderById] ?? Forbidden: Customer {customerId} trying to access order {id} (belongs to {order.CustomerId})");
                    return Forbid();
                }
            }
            // If not authenticated but order has customerId, allow if order was created by walk-in (customerId is null)
            // Or if order has customerId but user is not logged in, still allow (could be shared link)

            _logger.LogInformation($"[GetOrderById] ? Returning order {id} - Status: '{order.Status}', PaymentStatus: '{order.PaymentStatus}', OrderNumber: '{order.OrderNumber}', CustomerId: {order.CustomerId}");
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[GetOrderById] ? Exception getting order {id}");
            return StatusCode(500, new { message = "L?i khi t?i chi ti?t don", error = ex.Message });
        }
    }

    /// <summary>
    /// C?p nh?t tr?ng thái don d?t món (admin)
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
                return NotFound(new { message = "Ðon d?t món không t?n t?i" });
            }

            // Validate status
            var validStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivered", "Cancelled" };
            if (string.IsNullOrEmpty(request.Status) || !validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = $"Status không h?p l?. Ch? ch?p nh?n: {string.Join(", ", validStatuses)}" });
            }

            // Business rule: Cannot change status if order is already cancelled
            if (order.Status == "Cancelled" && request.Status != "Cancelled")
            {
                return BadRequest(new { message = "Không th? thay d?i tr?ng thái c?a don hàng dã b? h?y" });
            }

            // Business rule: Cannot cancel if already delivered
            if (order.Status == "Delivered" && request.Status == "Cancelled")
            {
                return BadRequest(new { message = "Không th? h?y don hàng dã du?c giao" });
            }

            var oldStatus = order.Status;
            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Order {order.OrderNumber} status updated: {oldStatus} -> {request.Status}");

            return Ok(new { message = "C?p nh?t tr?ng thái thành công", order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return StatusCode(500, new { message = "L?i khi c?p nh?t tr?ng thái", error = ex.Message });
        }
    }

    /// <summary>
    /// C?p nh?t tr?ng thái thanh toán don d?t món (admin)
    /// PATCH /api/restaurant-orders/{id}/payment-status
    /// </summary>
    [HttpPatch("{id}/payment-status")]
    [Authorize(Roles = "Admin,Manager,FrontDesk,Cashier")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
    {
        try
        {
            var order = await _context.RestaurantOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Ðon d?t món không t?n t?i" });
            }

            var validStatuses = new[] { "Unpaid", "Paid", "Refunded", "AwaitingConfirmation" };
            var validMethods = new[] { "Cash", "Card", "QR", "RoomCharge", "BankTransfer" };

            if (string.IsNullOrEmpty(request.PaymentStatus) || !validStatuses.Contains(request.PaymentStatus))
            {
                return BadRequest(new { message = $"PaymentStatus không h?p l?. Ch? ch?p nh?n: {string.Join(", ", validStatuses)}" });
            }

            string? paymentMethodToUse = request.PaymentMethod ?? order.PaymentMethod;

            if (request.PaymentStatus == "Paid")
            {
                if (string.IsNullOrEmpty(paymentMethodToUse))
                {
                    return BadRequest(new { message = "Vui lòng ch?n phuong th?c thanh toán khi dánh d?u don dã thanh toán." });
                }

                if (!validMethods.Contains(paymentMethodToUse))
                {
                    return BadRequest(new { message = $"PaymentMethod không h?p l?. Ch? ch?p nh?n: {string.Join(", ", validMethods)}" });
                }
            }
            else if (request.PaymentStatus == "AwaitingConfirmation")
            {
                // AwaitingConfirmation ch? áp d?ng cho ti?n m?t
                paymentMethodToUse = "Cash";
            }
            else
            {
                // V?i Unpaid/Refunded, có th? b? tr?ng phuong th?c
                paymentMethodToUse = null;
            }

            var oldStatus = order.PaymentStatus;
            order.PaymentStatus = request.PaymentStatus;
            order.PaymentMethod = paymentMethodToUse;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[UpdatePaymentStatus] Order {order.OrderNumber} payment status updated: {oldStatus} -> {request.PaymentStatus}, Method: {paymentMethodToUse}");

            return Ok(new
            {
                message = "C?p nh?t tr?ng thái thanh toán thành công",
                order
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status");
            return StatusCode(500, new { message = "L?i khi c?p nh?t tr?ng thái thanh toán", error = ex.Message });
        }
    }

    /// <summary>
    /// Thanh toán don d?t món
    /// POST /api/restaurant-orders/{id}/pay
    /// </summary>
    [HttpPost("{id}/pay")]
    [Authorize]
    public async Task<IActionResult> PayOrder(int id, [FromBody] PayOrderRequest? request)
    {
        try
        {
            // Handle null request
            if (request == null)
            {
                _logger.LogWarning($"[PayOrder] Request body is null for order {id}");
                request = new PayOrderRequest { PaymentMethod = "Cash" }; // Default to Cash
            }
            
            var order = await _context.RestaurantOrders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning($"[PayOrder] Order {id} not found");
                return NotFound(new { message = "Ðon d?t món không t?n t?i" });
            }

            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
            
            _logger.LogInformation($"[PayOrder] Order {id}, User role: {userRole}, CustomerId claim: {customerIdClaim}, Order CustomerId: {order.CustomerId}, PaymentMethod: {request.PaymentMethod}");

            // Authorization check:
            // - Customer ch? có th? thanh toán don hàng c?a chính h? (CustomerId kh?p)
            // - Admin/Manager/FrontDesk có th? thanh toán b?t k? don hàng nào
            // - Walk-in orders (CustomerId = null) không th? thanh toán online (c?n thanh toán t?i nhà hàng)
            if (userRole == "Customer")
            {
                // Customer không th? thanh toán don hàng c?a ngu?i khác
                if (string.IsNullOrEmpty(customerIdClaim))
                {
                    _logger.LogWarning($"[PayOrder] Customer without CustomerId claim trying to pay order {id}");
                    return StatusCode(403, new { message = "Không tìm th?y thông tin khách hàng trong token" });
                }
                
                // Walk-in orders (CustomerId = null) không th? thanh toán online
                if (order.CustomerId == null)
                {
                    _logger.LogWarning($"[PayOrder] Customer {customerIdClaim} trying to pay walk-in order {id}");
                    return BadRequest(new { message = "Ðon hàng này là don t?i qu?y, vui lòng thanh toán tr?c ti?p t?i nhà hàng" });
                }
                
                // Ki?m tra CustomerId kh?p
                if (int.TryParse(customerIdClaim, out int customerId) && order.CustomerId != customerId)
                {
                    _logger.LogWarning($"[PayOrder] Customer {customerId} trying to pay order {id} belonging to customer {order.CustomerId}");
                    return StatusCode(403, new { message = "B?n ch? có th? thanh toán don hàng c?a chính b?n" });
                }
                
                // CustomerId null ho?c không parse du?c
                if (!int.TryParse(customerIdClaim, out _))
                {
                    _logger.LogWarning($"[PayOrder] Invalid CustomerId claim: {customerIdClaim}");
                    return StatusCode(403, new { message = "Token không h?p l?" });
                }
            }
            // Admin/Manager/FrontDesk có th? thanh toán b?t k? don hàng nào (không c?n check thêm)

            if (order.PaymentStatus == "Paid")
            {
                return BadRequest(new { message = "Ðon hàng dã du?c thanh toán" });
            }

            // Validate PaymentMethod - use default if empty
            var validPaymentMethods = new[] { "Cash", "Card", "QR", "RoomCharge", "BankTransfer" };
            if (string.IsNullOrEmpty(request.PaymentMethod))
            {
                _logger.LogWarning($"[PayOrder] PaymentMethod is empty, defaulting to Cash for order {id}");
                request.PaymentMethod = "Cash";
            }
            
            if (!validPaymentMethods.Contains(request.PaymentMethod))
            {
                _logger.LogWarning($"[PayOrder] Invalid PaymentMethod: {request.PaymentMethod} for order {id}");
                return BadRequest(new { message = $"PaymentMethod không h?p l?. Ch? ch?p nh?n: {string.Join(", ", validPaymentMethods)}" });
            }

            // Business rule: Cannot pay if order is cancelled
            if (order.Status == "Cancelled")
            {
                return BadRequest(new { message = "Không th? thanh toán don hàng dã b? h?y" });
            }

            // N?u là Customer và PaymentMethod là Cash, ch? luu yêu c?u (ch? admin xác nh?n)
            if (userRole == "Customer" && request.PaymentMethod == "Cash")
            {
                // Luu thông tin yêu c?u thanh toán ti?n m?t vào SpecialRequests
                var specialRequests = order.SpecialRequests;
                Dictionary<string, object>? requestsDict = null;
                
                try
                {
                    if (!string.IsNullOrEmpty(specialRequests))
                    {
                        requestsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(specialRequests);
                    }
                }
                catch { }
                
                if (requestsDict == null)
                {
                    requestsDict = new Dictionary<string, object>();
                }
                
                requestsDict["cashPaymentRequested"] = true;
                requestsDict["cashPaymentRequestedAt"] = DateTime.UtcNow.ToString("O");
                requestsDict["cashPaymentRequestedBy"] = userEmail;
                
                order.SpecialRequests = System.Text.Json.JsonSerializer.Serialize(requestsDict);
                order.PaymentMethod = request.PaymentMethod;
                order.PaymentStatus = "AwaitingConfirmation"; // Ch? admin xác nh?n
                order.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation($"Order {order.OrderNumber} cash payment requested by customer, awaiting admin confirmation");
                
                return Ok(new { 
                    message = "Yêu c?u thanh toán ti?n m?t dã du?c g?i. Vui lòng ch? admin xác nh?n.", 
                    order,
                    awaitingConfirmation = true
                });
            }
            
            // Admin/Manager/FrontDesk ho?c PaymentMethod khác Cash: x? lý thanh toán ngay
            order.PaymentMethod = request.PaymentMethod;
            order.PaymentStatus = "Paid";
            order.UpdatedAt = DateTime.UtcNow;

            // If status is Pending, update to Confirmed
            if (order.Status == "Pending")
            {
                order.Status = "Confirmed";
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, $"[PayOrder] Database error when saving order {id}. OrderNumber: {order.OrderNumber}, PaymentStatus: {order.PaymentStatus}, Status: {order.Status}");
                return StatusCode(500, new { message = "L?i khi luu thông tin thanh toán", error = dbEx.Message });
            }

            _logger.LogInformation($"Order {order.OrderNumber} paid via {request.PaymentMethod}");

            // Send payment confirmation notification (only if payment is actually completed)
            if (order.PaymentStatus == "Paid" && order.CustomerId.HasValue)
            {
                _ = Task.Run(async () =>
                {
                    await _notificationManager.SendPaymentConfirmationAsync(
                        order.CustomerId.Value,
                        order.OrderNumber,
                        order.TotalAmount,
                        request.PaymentMethod
                    );
                });
            }

            return Ok(new { message = "Thanh toán thành công", order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying order");
            return StatusCode(500, new { message = "L?i khi thanh toán", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Admin xác nh?n thanh toán ti?n m?t cho restaurant order
    /// POST /api/restaurant-orders/{id}/approve-cash-payment
    /// </summary>
    [HttpPost("{id}/approve-cash-payment")]
    [Authorize(Roles = "Admin,FrontDesk,Cashier")]
    public async Task<IActionResult> ApproveCashPayment(int id)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            
            _logger.LogInformation($"[ApproveCashPayment] ?? Admin {userEmail} (Role: {userRole}) approving cash payment for order {id}");
            
            var order = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Service)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            
            if (order == null)
            {
                _logger.LogWarning($"[ApproveCashPayment] ? Order {id} not found");
                return NotFound(new { message = "Không tìm th?y don hàng" });
            }
            
            _logger.LogInformation($"[ApproveCashPayment] ?? Order {id} current status: Status='{order.Status}', PaymentStatus='{order.PaymentStatus}', OrderNumber='{order.OrderNumber}', CustomerId={order.CustomerId}");
            
            if (order.PaymentStatus == "Paid")
            {
                _logger.LogWarning($"[ApproveCashPayment] ?? Order {id} already paid");
                return BadRequest(new { message = "Ðon hàng dã du?c thanh toán" });
            }
            
            if (order.PaymentStatus != "AwaitingConfirmation")
            {
                _logger.LogWarning($"[ApproveCashPayment] ?? Order {id} PaymentStatus is '{order.PaymentStatus}', expected 'AwaitingConfirmation'");
                return BadRequest(new { message = "Ðon hàng này không có yêu c?u thanh toán ti?n m?t dang ch? xác nh?n" });
            }
            
            _logger.LogInformation($"[ApproveCashPayment] ?? Processing payment for order {id}...");
            
            // Xác nh?n thanh toán
            order.PaymentStatus = "Paid";
            order.UpdatedAt = DateTime.UtcNow;
            
            // C?p nh?t SpecialRequests d? ghi nh?n admin dã approve
            var specialRequests = order.SpecialRequests;
            Dictionary<string, object>? requestsDict = null;
            
            try
            {
                if (!string.IsNullOrEmpty(specialRequests))
                {
                    requestsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(specialRequests);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[ApproveCashPayment] ?? Error parsing SpecialRequests: {ex.Message}");
            }
            
            if (requestsDict == null)
            {
                requestsDict = new Dictionary<string, object>();
            }
            
            requestsDict["cashPaymentApproved"] = true;
            requestsDict["cashPaymentApprovedAt"] = DateTime.UtcNow.ToString("O");
            requestsDict["cashPaymentApprovedBy"] = userEmail;
            
            order.SpecialRequests = System.Text.Json.JsonSerializer.Serialize(requestsDict);
            
            // If status is Pending, update to Confirmed
            if (order.Status == "Pending")
            {
                order.Status = "Confirmed";
                _logger.LogInformation($"[ApproveCashPayment] ? Updated order status from Pending to Confirmed");
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"[ApproveCashPayment] ??? SUCCESS: Order {id} (OrderNumber: {order.OrderNumber}) approved! Final Status='{order.Status}', PaymentStatus='{order.PaymentStatus}'");
            
            // Send payment confirmation notification after admin approval
            if (order.PaymentStatus == "Paid" && order.CustomerId.HasValue)
            {
                _ = Task.Run(async () =>
                {
                    await _notificationManager.SendPaymentConfirmationAsync(
                        order.CustomerId.Value,
                        order.OrderNumber,
                        order.TotalAmount,
                        "Cash"
                    );
                });
            }
            
            return Ok(new { message = "Xác nh?n thanh toán ti?n m?t thành công", order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ApproveCashPayment] ? Exception approving cash payment for order {id}");
            return StatusCode(500, new { message = "L?i khi xác nh?n thanh toán", error = ex.Message });
        }
    }

    /// <summary>
    /// H?y don d?t món (Customer)
    /// POST /api/restaurant-orders/{id}/cancel
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CancelMyOrder(int id)
    {
        try
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
            {
                return Unauthorized(new { message = "Vui lòng dang nh?p d? h?y don" });
            }

            var order = await _context.RestaurantOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Ðon d?t món không t?n t?i" });
            }

            if (order.CustomerId != customerId)
            {
                return Forbid();
            }

            if (order.Status != "Pending")
            {
                return BadRequest(new { message = "Ch? có th? h?y don hàng dang ? tr?ng thái Ch? x? lý" });
            }

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"[CancelMyOrder] Customer {customerId} cancelled order {id}");

            return Ok(new { message = "H?y don hàng thành công", order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[CancelMyOrder] Error cancelling order {id}");
            return StatusCode(500, new { message = "L?i khi h?y don hàng", error = ex.Message });
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

public class UpdatePaymentStatusRequest
{
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}

public class PayOrderRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
}

