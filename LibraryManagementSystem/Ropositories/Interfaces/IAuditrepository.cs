using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Ropositories.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task LogAsync(int? userId, string action, string entityName, int? entityId,
            string? oldValues = null, string? newValues = null, string? ipAddress = null);
        Task<List<AuditLog>> GetRecentLogsAsync(int count = 50);
    }
}
