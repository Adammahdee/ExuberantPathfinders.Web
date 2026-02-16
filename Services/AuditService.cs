using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using System.Text.Json;

namespace ExuberantPathfinders.Web.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, AuditAction action, string entityType, int entityId, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string userId, AuditAction action, string entityType, int entityId,
            object? oldValues = null, object? newValues = null, string? ipAddress = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : string.Empty,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : string.Empty,
                IPAddress = ipAddress ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
