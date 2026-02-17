using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class ApplicantsListViewModel
    {
        public List<Application> Items { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public ApplicationStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class UsersListViewModel
    {
        public List<ApplicationUser> Items { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class LogsListViewModel
    {
        public List<AuditLog> Items { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public AuditAction? AuditAction { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
