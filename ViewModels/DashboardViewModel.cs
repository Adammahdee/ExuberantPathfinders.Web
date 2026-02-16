using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public decimal TotalDonations { get; set; }
        public int TotalUsers { get; set; }
        public List<Application>? RecentApplications { get; set; } = new();
        public List<Donation>? RecentDonations { get; set; } = new();
    }
}
