namespace ExuberantPathfinders.Web.ViewModels
{
    public class ApplicationSubmitViewModel
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal RequestedAmount { get; set; }
    }
}
