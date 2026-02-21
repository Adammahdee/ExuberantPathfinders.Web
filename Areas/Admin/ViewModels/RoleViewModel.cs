using System.ComponentModel.DataAnnotations;

namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public int UserCount { get; set; }
    }
}
