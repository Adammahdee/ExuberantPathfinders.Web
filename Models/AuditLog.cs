namespace ExuberantPathfinders.Web.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty; // JSON serialized
        public string NewValues { get; set; } = string.Empty; // JSON serialized
        public string IPAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }
    }
}
