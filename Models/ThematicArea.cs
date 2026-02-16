namespace ExuberantPathfinders.Web.Models
{
    public class ThematicArea
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // e.g., "EDU", "HEALTH"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<GrantProgram> Programs { get; set; } = new List<GrantProgram>();
    }
}
