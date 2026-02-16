namespace ExuberantPathfinders.Web.ViewModels
{
    public class DonationViewModel
    {
        public int CampaignId { get; set; }
        public decimal Amount { get; set; }
        public string? DonorEmail { get; set; }
        public string? DonorName { get; set; }
    }
}
