using System;
using System.ComponentModel.DataAnnotations;

namespace ExuberantPathfinders.Web.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string IssueType { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; }

        public string? ResolutionNotes { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}