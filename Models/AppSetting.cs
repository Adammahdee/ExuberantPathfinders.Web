using System.ComponentModel.DataAnnotations;

namespace ExuberantPathfinders.Web.Models
{
    /// <summary>
    /// Represents a single application setting stored in the database.
    /// </summary>
    public class AppSetting
    {
        [Key]
        [MaxLength(128)]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(128)]
        public string Group { get; set; } = string.Empty;
    }
}