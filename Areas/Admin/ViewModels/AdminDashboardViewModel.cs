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
        public ImpactAnalyticsViewModel ImpactAnalytics { get; set; } = new();
        public List<Application> RecentApplications { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<AuditLog> RecentAuditLogs { get; set; } = new();
    }

    public class ImpactAnalyticsViewModel
    {
        public int ApprovedApplications { get; set; }
        public decimal ApprovalRate { get; set; }
        public int FundedBeneficiaries { get; set; }
        public decimal ApprovedRequestVolume { get; set; }
        public decimal CampaignGoalAttainmentRate { get; set; }
        public decimal AverageDonationAmount { get; set; }
        public List<MonthlyImpactPointViewModel> MonthlyImpact { get; set; } = new();
    }

    public class MonthlyImpactPointViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int SubmittedApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public decimal DonationAmount { get; set; }
    }
}
