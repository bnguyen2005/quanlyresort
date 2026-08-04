using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace QuanLyResort.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private const string DefaultLanguage = "vi";

    public LocalizationService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _translations = LoadTranslations();
    }

    public string GetString(string key, string? language = null)
    {
        language ??= GetCurrentLanguage();
        
        if (_translations.TryGetValue(language, out var langDict) && 
            langDict.TryGetValue(key, out var value))
        {
            return value;
        }

        // Fallback to default language
        if (language != DefaultLanguage && 
            _translations.TryGetValue(DefaultLanguage, out var defaultDict) && 
            defaultDict.TryGetValue(key, out var defaultValue))
        {
            return defaultValue;
        }

        // Return key if translation not found
        return key;
    }

    public string GetString(string key, object? parameters, string? language = null)
    {
        var template = GetString(key, language);
        
        if (parameters == null) return template;

        // Simple parameter replacement: {0}, {1}, etc. or {name}
        var result = template;
        var props = parameters.GetType().GetProperties();
        
        foreach (var prop in props)
        {
            var placeholder = $"{{{prop.Name}}}";
            var value = prop.GetValue(parameters)?.ToString() ?? "";
            result = result.Replace(placeholder, value);
        }

        // Also support indexed placeholders {0}, {1}
        if (props.Length == 0)
        {
            var values = parameters as object[] ?? new[] { parameters };
            for (int i = 0; i < values.Length; i++)
            {
                result = result.Replace($"{{{i}}}", values[i]?.ToString() ?? "");
            }
        }

        return result;
    }

    public string GetCurrentLanguage()
    {
        // Try to get from HTTP context (cookie, header, or query)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Check query string
            if (httpContext.Request.Query.TryGetValue("lang", out var queryLang))
            {
                return IsLanguageSupported(queryLang.ToString()) ? queryLang.ToString() : DefaultLanguage;
            }

            // Check cookie
            if (httpContext.Request.Cookies.TryGetValue("language", out var cookieLang))
            {
                return IsLanguageSupported(cookieLang) ? cookieLang : DefaultLanguage;
            }

            // Check Accept-Language header
            var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                var preferredLang = ParseAcceptLanguage(acceptLanguage);
                if (preferredLang != null && IsLanguageSupported(preferredLang))
                {
                    return preferredLang;
                }
            }
        }

        return DefaultLanguage;
    }

    public void SetLanguage(string language)
    {
        if (!IsLanguageSupported(language))
            language = DefaultLanguage;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Append("language", language, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });
        }
    }

    public bool IsLanguageSupported(string language)
    {
        return _translations.ContainsKey(language?.ToLower() ?? "");
    }

    public List<string> GetSupportedLanguages()
    {
        return _translations.Keys.ToList();
    }

    private Dictionary<string, Dictionary<string, string>> LoadTranslations()
    {
        var translations = new Dictionary<string, Dictionary<string, string>>();

        // Vietnamese (default)
        translations["vi"] = new Dictionary<string, string>
        {
            // Common
            { "common.save", "Lưu" },
            { "common.cancel", "Hủy" },
            { "common.delete", "Xóa" },
            { "common.edit", "Sửa" },
            { "common.search", "Tìm kiếm" },
            { "common.loading", "Đang tải..." },
            { "common.error", "Lỗi" },
            { "common.success", "Thành công" },
            
            // Auth
            { "auth.login", "Đăng nhập" },
            { "auth.logout", "Đăng xuất" },
            { "auth.register", "Đăng ký" },
            { "auth.email", "Email" },
            { "auth.password", "Mật khẩu" },
            { "auth.forgot_password", "Quên mật khẩu?" },
            
            // Booking
            { "booking.title", "Đặt phòng" },
            { "booking.check_in", "Ngày nhận phòng" },
            { "booking.check_out", "Ngày trả phòng" },
            { "booking.guests", "Số khách" },
            { "booking.total", "Tổng tiền" },
            { "booking.confirm", "Xác nhận đặt phòng" },
            
            // Payment
            { "payment.title", "Thanh toán" },
            { "payment.method", "Phương thức thanh toán" },
            { "payment.amount", "Số tiền" },
            { "payment.success", "Thanh toán thành công" },
            
            // Restaurant
            { "restaurant.menu", "Thực đơn" },
            { "restaurant.order", "Đặt món" },
            { "restaurant.cart", "Giỏ hàng" },
            { "restaurant.total", "Tổng tiền" },
            
            // Account
            { "account.profile", "Hồ sơ" },
            { "account.bookings", "Đặt phòng của tôi" },
            { "account.orders", "Đơn hàng" },
            { "account.loyalty_points", "Điểm thưởng" },
            
            // Admin
            { "admin.dashboard", "Bảng điều khiển" },
            { "admin.customers", "Khách hàng" },
            { "admin.bookings", "Đặt phòng" },
            { "admin.rooms", "Phòng" },
            { "admin.reports", "Báo cáo" }
        };

        // English
        translations["en"] = new Dictionary<string, string>
        {
            // Common
            { "common.save", "Save" },
            { "common.cancel", "Cancel" },
            { "common.delete", "Delete" },
            { "common.edit", "Edit" },
            { "common.search", "Search" },
            { "common.loading", "Loading..." },
            { "common.error", "Error" },
            { "common.success", "Success" },
            
            // Auth
            { "auth.login", "Login" },
            { "auth.logout", "Logout" },
            { "auth.register", "Register" },
            { "auth.email", "Email" },
            { "auth.password", "Password" },
            { "auth.forgot_password", "Forgot password?" },
            
            // Booking
            { "booking.title", "Booking" },
            { "booking.check_in", "Check-in Date" },
            { "booking.check_out", "Check-out Date" },
            { "booking.guests", "Guests" },
            { "booking.total", "Total" },
            { "booking.confirm", "Confirm Booking" },
            
            // Payment
            { "payment.title", "Payment" },
            { "payment.method", "Payment Method" },
            { "payment.amount", "Amount" },
            { "payment.success", "Payment Successful" },
            
            // Restaurant
            { "restaurant.menu", "Menu" },
            { "restaurant.order", "Order" },
            { "restaurant.cart", "Cart" },
            { "restaurant.total", "Total" },
            
            // Account
            { "account.profile", "Profile" },
            { "account.bookings", "My Bookings" },
            { "account.orders", "Orders" },
            { "account.loyalty_points", "Loyalty Points" },
            
            // Admin
            { "admin.dashboard", "Dashboard" },
            { "admin.customers", "Customers" },
            { "admin.bookings", "Bookings" },
            { "admin.rooms", "Rooms" },
            { "admin.reports", "Reports" }
        };

        return translations;
    }

    private string? ParseAcceptLanguage(string acceptLanguage)
    {
        // Parse "en-US,en;q=0.9,vi;q=0.8" to get preferred language
        var languages = acceptLanguage.Split(',')
            .Select(lang => lang.Split(';')[0].Trim().ToLower())
            .ToList();

        // Check for exact match
        foreach (var lang in languages)
        {
            if (IsLanguageSupported(lang))
                return lang;
            
            // Check for language code (en-US -> en)
            var langCode = lang.Split('-')[0];
            if (IsLanguageSupported(langCode))
                return langCode;
        }

        return null;
    }
}

