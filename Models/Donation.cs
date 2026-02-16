namespace ExuberantPathfinders.Web.Models
{
    public class Donation
    {
        public int Id { get; set; }
        public string DonorId { get; set; } = string.Empty;
        public int CampaignId { get; set; }
        public decimal Amount { get; set; }
        public DonationStatus Status { get; set; } = DonationStatus.Pending;
        public PaymentGateway Gateway { get; set; } = PaymentGateway.Paystack;
        
        // Paystack Integration
        public string? PaystackReference { get; set; }
        public string? PaystackAuthorizationUrl { get; set; }
        public string? PaystackAccessCode { get; set; }
        public string? TransactionId { get; set; }
        
        // Verification
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public string? VerificationNotes { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ApplicationUser? Donor { get; set; }
        public Campaign? Campaign { get; set; }
    }
}
