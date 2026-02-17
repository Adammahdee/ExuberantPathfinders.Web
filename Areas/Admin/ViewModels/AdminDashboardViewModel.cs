using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public decimal TotalDonations { get; set; }
        public int TotalUsers { get; set; }
        public int TotalGrants { get; set; }
        public int TotalFunds { get; set; }
        public int ActiveCampaigns { get; set; }
        public List<Application> RecentApplications { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<AuditLog> RecentAuditLogs { get; set; } = new();
    }
}
