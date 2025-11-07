namespace QuanLyResort.Middleware;

/// <summary>
/// Extension methods để đăng ký middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Thêm JWT Authorization Middleware vào pipeline
    /// </summary>
    public static IApplicationBuilder UseJwtAuthorizationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtAuthorizationMiddleware>();
    }
}

