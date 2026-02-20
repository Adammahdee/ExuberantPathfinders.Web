using MailKit.Net.Smtp;
using MimeKit;namespace ExuberantPathfinders.Web.Services
{
    public interface IEmailService
    {
        Task SendApplicationSubmittedEmailAsync(string email, string applicantName, string applicationRef);
        Task SendApplicationApprovedEmailAsync(string email, string applicantName);
        Task SendApplicationRejectedEmailAsync(string email, string applicantName, string reason);
        Task SendDonationReceiptEmailAsync(string email, string donorName, decimal amount, string transactionId);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
     public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendApplicationSubmittedEmailAsync(string email, string applicantName, string applicationRef) { await SendEmailAsync(email, "Application Submitted", $"Dear {applicantName}, Your application {applicationRef} has been submitted successfully!"); }

        public async Task SendApplicationApprovedEmailAsync(string email, string applicantName) { await SendEmailAsync(email, "Application Approved", $"Dear {applicantName}, Congratulations! Your application has been approved!"); }

        public async Task SendApplicationRejectedEmailAsync(string email, string applicantName, string reason) { await SendEmailAsync(email, "Application Rejected", $"Dear {applicantName}, We regret to inform you that your application has been rejected due to: {reason}"); }
        public async Task SendDonationReceiptEmailAsync(string email, string donorName, decimal amount, string transactionId)
        {
            await SendEmailAsync(email, "Donation Receipt", $"Dear {donorName}, Thank you for your generous donation of {amount:C}. Your transaction ID is {transactionId}.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings.GetValue<string>("Host");
            var port = smtpSettings.GetValue<int>("Port");
            var username = smtpSettings.GetValue<string>("Username");
            var password = smtpSettings.GetValue<string>("Password");
            var senderName = smtpSettings.GetValue<string>("SenderName");
            var senderEmail = smtpSettings.GetValue<string>("SenderEmail") ?? string.Empty;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.Connect(host, port, MailKit.Security.SecureSocketOptions.StartTls);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(username, password);

                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log it)
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
    }
}
