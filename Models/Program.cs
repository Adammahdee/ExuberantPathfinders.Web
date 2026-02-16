namespace ExuberantPathfinders.Web.Models
{
    public class GrantProgram
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ThematicAreaId { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProgramOfficerId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ThematicArea? ThematicArea { get; set; }
        public ApplicationUser? ProgramOfficer { get; set; }
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}
