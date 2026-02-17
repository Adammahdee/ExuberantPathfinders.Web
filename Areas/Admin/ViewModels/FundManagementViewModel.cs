using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class FundManagementViewModel
    {
        public List<Campaign> Campaigns { get; set; } = new();
        public List<GrantProgram> Programs { get; set; } = new();
        public string FormError { get; set; } = string.Empty;
        public string FormSuccess { get; set; } = string.Empty;
    }
}
