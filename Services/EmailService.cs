namespace ExuberantPathfinders.Web.Services
{
    public interface IEmailService
    {
        Task SendApplicationSubmittedEmailAsync(string email, string applicantName, string applicationRef);
        Task SendApplicationApprovedEmailAsync(string email, string applicantName);
        Task SendApplicationRejectedEmailAsync(string email, string applicantName, string reason);
        Task SendDonationReceiptEmailAsync(string email, string donorName, decimal amount, string transactionId);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendApplicationSubmittedEmailAsync(string email, string applicantName, string applicationRef)
        {
            // Implementation for sending application submitted email
            await Task.Delay(100); // Placeholder
        }

        public async Task SendApplicationApprovedEmailAsync(string email, string applicantName)
        {
            // Implementation for sending application approved email
            await Task.Delay(100); // Placeholder
        }

        public async Task SendApplicationRejectedEmailAsync(string email, string applicantName, string reason)
        {
            // Implementation for sending application rejected email
            await Task.Delay(100); // Placeholder
        }

        public async Task SendDonationReceiptEmailAsync(string email, string donorName, decimal amount, string transactionId)
        {
            // Implementation for sending donation receipt email
            await Task.Delay(100); // Placeholder
        }
    }
}
