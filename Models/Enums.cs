namespace ExuberantPathfinders.Web.Models
{
    public enum ApplicationStatus
    {
        Draft = 0,
        Submitted = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        OnHold = 5
    }

    public enum DonationStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }

    public enum PaymentGateway
    {
        Paystack = 0,
        Manual = 1
    }

    public enum AuditAction
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        Approve = 3,
        Reject = 4
    }
}
