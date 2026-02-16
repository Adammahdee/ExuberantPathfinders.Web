namespace ExuberantPathfinders.Web.Models
{
    public class Application
    {
        public int Id { get; set; }
        public string ApplicantId { get; set; } = string.Empty;
        public int ProgramId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal RequestedAmount { get; set; }
        public string? SubmissionReference { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public string? ReviewedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }

        public ApplicationUser? Applicant { get; set; }
        public GrantProgram? Program { get; set; }
        public ApplicationUser? ReviewedBy { get; set; }
        public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
    }
}
