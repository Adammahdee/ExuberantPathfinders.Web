namespace ExuberantPathfinders.Web.Models
{
    public class ApplicationStatusHistory
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationStatus PreviousStatus { get; set; }
        public ApplicationStatus NewStatus { get; set; }
        public string ChangedById { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public Application? Application { get; set; }
        public ApplicationUser? ChangedBy { get; set; }
    }
}
