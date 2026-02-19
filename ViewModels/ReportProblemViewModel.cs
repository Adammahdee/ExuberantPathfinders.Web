using System.ComponentModel.DataAnnotations;

namespace ExuberantPathfinders.Web.ViewModels
{
    public class ReportProblemViewModel
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string IssueType { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Message { get; set; } = string.Empty;
    }
}
