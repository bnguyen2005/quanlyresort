namespace QuanLyResort.Services;

public interface IAuditService
{
    Task LogAsync(string entityName, int entityId, string action, string? performedBy = null, 
        string? oldValues = null, string? newValues = null, string? description = null);
}

