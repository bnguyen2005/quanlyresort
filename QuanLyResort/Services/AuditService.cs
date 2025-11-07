using Microsoft.AspNetCore.Http;
using QuanLyResort.Models;
using QuanLyResort.Repositories;
using System.Security.Claims;

namespace QuanLyResort.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityName, int entityId, string action, string? performedBy = null, 
        string? oldValues = null, string? newValues = null, string? description = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // Tự động lấy IP Address
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
        
        // Tự động lấy User Agent
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
        
        // Tự động lấy username từ claims nếu không truyền vào
        if (string.IsNullOrEmpty(performedBy) && httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            performedBy = httpContext.User.FindFirst(ClaimTypes.Name)?.Value 
                       ?? httpContext.User.FindFirst("Username")?.Value
                       ?? "System";
        }

        var auditLog = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            PerformedBy = performedBy ?? "System",
            OldValues = oldValues,
            NewValues = newValues,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }
}

