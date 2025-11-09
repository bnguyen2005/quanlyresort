using System.Security.Claims;

namespace QuanLyResort.Middleware;

/// <summary>
/// Middleware kiểm tra JWT token và phân quyền truy cập
/// Xác định role và kiểm tra quyền trước khi cho phép truy cập endpoint
/// </summary>
public class JwtAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthorizationMiddleware> _logger;

    // Định nghĩa tất cả roles trong hệ thống
    private static readonly string[] ValidRoles = new[]
    {
        "Admin",
        "Manager", 
        "Business",
        "FrontDesk",
        "Cashier",
        "Accounting",
        "Inventory",
        "Customer"
    };

        // Mapping endpoints public không cần authentication
        private static readonly string[] PublicEndpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/customer-login",
            "/api/auth/register",
            "/api/auth/staff-login",
            "/api/coupons/validate", // Allow coupon validation without auth
            "/api/coupons/active", // Allow customers to view active coupons without auth
            "/api/simplepayment/webhook", // Allow webhook from PayOs/VietQR without auth
            "/api/payment/webhook", // Allow legacy webhook without auth
            "/api/payment/payos-webhook", // Allow PayOs webhook without auth
            "/api/payment/vietqr-webhook", // Allow VietQR webhook without auth
            "/api/payment/mbbank-webhook", // Allow MB Bank webhook without auth
            "/api/payment/bank-webhook", // Allow generic bank webhook without auth
            "/api/faqs", // Allow public access to FAQs
            "/api/supporttickets", // Allow public access to create support tickets (POST)
            "/swagger",
            "/swagger/",
            "/swagger/v1/swagger.json",
            "/health",
            "/customer",
            "/admin/assets",
            "/admin/vendor",
            "/admin/js",
            "/admin/css"
        };

    public JwtAuthorizationMiddleware(RequestDelegate next, ILogger<JwtAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get path without query string for matching
        var rawPath = context.Request.Path.Value ?? "";
        var path = rawPath.ToLower();
        var method = context.Request.Method.ToUpper();
        
        _logger.LogInformation("[Authorization] Checking path: {Path}, Method: {Method}", rawPath, method);

        // Cho phép webhook endpoints không cần token - CHECK TRƯỚC TẤT CẢ (TRƯỚC CẢ SWAGGER)
        if (method == "POST" && (
            path == "/api/simplepayment/webhook" ||
            path == "/api/payment/webhook" ||
            path == "/api/payment/payos-webhook" ||
            path == "/api/payment/vietqr-webhook" ||
            path == "/api/payment/mbbank-webhook" ||
            path == "/api/payment/bank-webhook"
        ))
        {
            _logger.LogInformation("[Authorization] ✅ Allowing webhook request: {Path}", rawPath);
            await _next(context);
            return;
        }

        // Cho phép Swagger endpoints không cần token - CHECK TRƯỚC TẤT CẢ
        if (path.StartsWith("/swagger"))
        {
            _logger.LogInformation("[Authorization] ✅ Allowing Swagger request: {Path}", rawPath);
            await _next(context);
            return;
        }

        // Cho phép GET /api/reviews không cần token (public để xem đánh giá) - CHECK TRƯỚC TẤT CẢ
        // Check cả path chính xác và các path con như /api/reviews/{id}, /api/reviews/can-review/{roomId}
        if (path.StartsWith("/api/reviews") && method == "GET")
        {
            _logger.LogInformation("[Authorization] ✅ Allowing public GET request to: {Path}", rawPath);
            await _next(context);
            return;
        }

        // Cho phép GET /api/coupons/validate và /api/coupons/active không cần token (public để validate và xem coupons)
        // Check TRƯỚC khi kiểm tra token - CRITICAL: Must be BEFORE any auth checks
        if (method == "GET" && path.Contains("/api/coupons/"))
        {
            if (path.Contains("/coupons/validate") || path.Contains("/coupons/active"))
            {
                _logger.LogInformation("[Authorization] ✅ Allowing public GET request to: {Path} (coupon endpoint)", rawPath);
                await _next(context);
                return;
            }
        }

        // Skip middleware cho public endpoints
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        // Cho phép GET /api/room-types và GET /api/room-types/{id} không cần token
        if (path.StartsWith("/api/room-types") && method == "GET")
        {
            await _next(context);
            return;
        }

        // Cho phép GET /api/rooms và GET /api/rooms/{id} không cần token (public để xem danh sách phòng)
        if ((path == "/api/rooms" || (path.StartsWith("/api/rooms/") && path.Split('/').Length == 4 && int.TryParse(path.Split('/')[3], out _))) && method == "GET")
        {
            await _next(context);
            return;
        }

        // Cho phép GET /api/rooms/floors không cần token (public endpoint)
        if (path == "/api/rooms/floors" && method == "GET")
        {
            await _next(context);
            return;
        }

        // Cho phép GET /api/services/restaurant/menu và /api/services/types không cần token (public endpoints)
        if (method == "GET" && path.StartsWith("/api/services/"))
        {
            if (path.Contains("/restaurant/menu") || path == "/api/services/types" || path.Contains("/services/types"))
            {
                await _next(context);
                return;
            }
        }

        // Cho phép POST /api/restaurant-orders không cần token (customer có thể đặt món walk-in)
        if (path == "/api/restaurant-orders" && method == "POST")
        {
            await _next(context);
            return;
        }

        // Cho phép GET /api/restaurant-orders/{id} không cần token (customer có thể xem order details)
        // Check if this is a GET request to /api/restaurant-orders/{id} (not /api/restaurant-orders/{id}/status or /pay)
        if (path.StartsWith("/api/restaurant-orders/") && method == "GET")
        {
            // Remove leading / and split (path is already lowercased)
            var cleanPath = path.TrimStart('/');
            var pathParts = cleanPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            _logger.LogInformation($"[Authorization] Checking restaurant-orders path: {path}, parts: [{string.Join(", ", pathParts)}], count: {pathParts.Length}");
            
            // Should be: ["api", "restaurant-orders", "123"] = 3 parts (all lowercase)
            // NOT: ["api", "restaurant-orders", "123", "status"] = 4 parts (this needs auth)
            // NOT: ["api", "restaurant-orders", "123", "pay"] = 4 parts (this needs auth)
            if (pathParts.Length == 3 && 
                pathParts[0] == "api" && 
                pathParts[1] == "restaurant-orders" && 
                int.TryParse(pathParts[2], out _))
            {
                _logger.LogInformation($"[Authorization] ✅ Allowing GET /api/restaurant-orders/{{id}} without auth: {path}");
                await _next(context);
                return;
            }
            else
            {
                _logger.LogInformation($"[Authorization] ❌ NOT allowing GET /api/restaurant-orders path (wrong format): {path}, parts count: {pathParts.Length}");
            }
        }

        // Chỉ kiểm tra các API endpoints (/api/*)
        // Nhưng skip nếu đã được cho phép ở trên (reviews, rooms, etc.)
        if (!path.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        // Log all API requests for debugging
        _logger.LogInformation($"[Authorization] API Request: {method} {path}");

        // Lấy thông tin user từ Claims (đã được xác thực bởi JWT middleware)
        var identity = context.User.Identity as ClaimsIdentity;
        
        if (identity == null || !identity.IsAuthenticated)
        {
            _logger.LogWarning("[Authorization] Unauthorized access attempt to: {Path}", path);
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Unauthorized. Please login to access this resource.",
                path = path
            });
            return;
        }

        // Lấy role từ claims
        var roleClaim = identity.FindFirst(ClaimTypes.Role)?.Value;
        var usernameClaim = identity.FindFirst(ClaimTypes.Name)?.Value;
        var userIdClaim = identity.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            _logger.LogWarning("[Authorization] No role claim found for user: {Username}", usernameClaim);
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Forbidden. Your account does not have a valid role.",
                path = path
            });
            return;
        }

        // Kiểm tra role hợp lệ
        if (!ValidRoles.Contains(roleClaim))
        {
            _logger.LogWarning("[Authorization] Invalid role '{Role}' for user: {Username} accessing: {Path}", 
                roleClaim, usernameClaim, path);
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsJsonAsync(new
            {
                message = $"Forbidden. Invalid role: {roleClaim}",
                path = path
            });
            return;
        }

        // Log successful authorization
        _logger.LogInformation("[Authorization] User: {Username} (ID: {UserId}, Role: {Role}) accessing: {Path} (raw: {RawPath})", 
            usernameClaim, userIdClaim, roleClaim, path, rawPath);

        // Kiểm tra quyền truy cập specific endpoint (optional - có thể mở rộng)
        if (!HasPermissionToAccess(path, roleClaim))
        {
            _logger.LogWarning("[Authorization] Access denied for role '{Role}' to: {Path} (raw: {RawPath})", roleClaim, path, rawPath);
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsJsonAsync(new
            {
                message = $"Forbidden. You don't have permission to access this resource. Required role permissions not met.",
                role = roleClaim,
                path = path,
                rawPath = rawPath
            });
            return;
        }
        
        _logger.LogInformation("[Authorization] ✅ Access granted for role '{Role}' to: {Path}", roleClaim, path);

        // Cho phép truy cập
        await _next(context);
    }

    /// <summary>
    /// Kiểm tra endpoint có phải là public không
    /// </summary>
    private bool IsPublicEndpoint(string path)
    {
        return PublicEndpoints.Any(endpoint => path.StartsWith(endpoint.ToLower()));
    }

    /// <summary>
    /// Kiểm tra quyền truy cập dựa trên role và endpoint
    /// Có thể mở rộng logic phân quyền chi tiết hơn ở đây
    /// </summary>
    private bool HasPermissionToAccess(string path, string role)
    {
        // Admin có quyền truy cập tất cả
        if (role == "Admin")
            return true;

        // Manager có quyền truy cập hầu hết, trừ một số endpoints nhạy cảm
        if (role == "Manager")
        {
            // Chặn Manager xóa user hoặc employee
            if (path.Contains("/usermanagement/") && path.EndsWith("/delete"))
                return false;
            if (path.Contains("/employeemanagement/") && path.EndsWith("/delete"))
                return false;
            
            return true;
        }

        // Business: Truy cập bookings, rooms, customers, reports
        if (role == "Business")
        {
            return path.Contains("/bookings") || 
                   path.Contains("/rooms") || 
                   path.Contains("/customers") ||
                   path.Contains("/customermanagement") ||
                   path.Contains("/reports");
        }

        // FrontDesk: Truy cập bookings, rooms, customers, restaurant-orders (không thể xóa)
        if (role == "FrontDesk")
        {
            if (path.Contains("/delete") || path.Contains("/usermanagement") || path.Contains("/employeemanagement"))
                return false;
                
            return path.Contains("/bookings") || 
                   path.Contains("/rooms") || 
                   path.Contains("/customers") ||
                   path.Contains("/customermanagement") ||
                   path.Contains("/restaurant-orders");
        }

        // Cashier: Truy cập invoices, bookings (chỉ đọc), charges
        if (role == "Cashier")
        {
            return path.Contains("/invoices") || 
                   path.Contains("/bookings") || 
                   path.Contains("/charges");
        }

        // Accounting: Truy cập invoices, reports, inventory
        if (role == "Accounting")
        {
            return path.Contains("/invoices") || 
                   path.Contains("/reports") || 
                   path.Contains("/inventory");
        }

        // Inventory: Truy cập inventory endpoints
        if (role == "Inventory")
        {
            return path.Contains("/inventory");
        }

        // Customer: Chỉ truy cập thông tin của chính họ
        if (role == "Customer")
        {
            // Cho phép khách xem rooms/services, tạo và xem bookings của chính họ,
            // xem/cập nhật thông tin cá nhân qua customermanagement
            // Cho phép truy cập restaurant-orders: tạo, xem đơn của mình, thanh toán đơn của mình
            // Cho phép truy cập reviews: xem reviews (GET), tạo review của mình (POST), xem review của mình
            // Cho phép truy cập payment: tạo payment session, xem payment status, nhận webhook
            // Cho phép truy cập simplepayment: tạo PayOs payment link, xem webhook status
            // (Controller sẽ kiểm tra authorization chi tiết hơn)
            bool hasPermission = path.StartsWith("/api/rooms") ||
                   path.StartsWith("/api/services") ||
                   path.StartsWith("/api/bookings") ||
                   path.StartsWith("/api/customermanagement") ||
                   path.StartsWith("/api/customers/profile") ||
                   path.StartsWith("/api/restaurant-orders") ||
                   path.StartsWith("/api/reviews") ||
                   path.StartsWith("/api/payment") ||
                   path.StartsWith("/api/simplepayment");
            
            // Log để debug
            if (!hasPermission)
            {
                _logger.LogWarning("[HasPermission] Customer role denied access to: {Path}", path);
            }
            else
            {
                _logger.LogInformation("[HasPermission] Customer role allowed access to: {Path}", path);
            }
            
            return hasPermission;
        }

        // Mặc định: không cho phép
        return false;
    }
}

