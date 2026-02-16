namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class MonthlyDonationReportViewModel
    {
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public int DonationCount { get; set; }

        public string MonthName
        {
            get => System.Globalization.CultureInfo.CurrentCulture
                .DateTimeFormat.GetMonthName(Month);
        }
    }
}
