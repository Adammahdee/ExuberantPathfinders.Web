using Microsoft.AspNetCore.Identity;

namespace ExuberantPathfinders.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
