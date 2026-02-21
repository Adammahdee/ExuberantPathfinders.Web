using System.ComponentModel.DataAnnotations;

namespace ExuberantPathfinders.Web.Models
{
    public class AppSetting
    {
        [Key]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Group { get; set; } = "General"; // e.g., "Email", "Maintenance", "Banner"
    }
}