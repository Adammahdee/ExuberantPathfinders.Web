using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class GrantManagementViewModel
    {
        public List<GrantProgram> Grants { get; set; } = new();
        public List<ThematicArea> ThematicAreas { get; set; } = new();
        public List<ApplicationUser> ProgramOfficers { get; set; } = new();
        public string FormError { get; set; } = string.Empty;
        public string FormSuccess { get; set; } = string.Empty;
    }
}
