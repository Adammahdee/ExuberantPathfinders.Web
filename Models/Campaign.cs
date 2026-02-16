namespace ExuberantPathfinders.Web.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProgramId { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal AmountRaised { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public GrantProgram? Program { get; set; }
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
