using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Draft => "Draft",
                ApplicationStatus.Submitted => "Submitted",
                ApplicationStatus.UnderReview => "Under Review",
                ApplicationStatus.Approved => "Approved",
                ApplicationStatus.Rejected => "Rejected",
                ApplicationStatus.OnHold => "On Hold",
                _ => "Unknown"
            };
        }

        public static string GetDisplayName(this DonationStatus status)
        {
            return status switch
            {
                DonationStatus.Pending => "Pending",
                DonationStatus.Processing => "Processing",
                DonationStatus.Completed => "Completed",
                DonationStatus.Failed => "Failed",
                DonationStatus.Refunded => "Refunded",
                _ => "Unknown"
            };
        }

        public static string GetDisplayName(this PaymentGateway gateway)
        {
            return gateway switch
            {
                PaymentGateway.Paystack => "Paystack",
                PaymentGateway.Manual => "Manual Transfer",
                _ => "Unknown"
            };
        }

        public static string GetDisplayName(this AuditAction action)
        {
            return action switch
            {
                AuditAction.Create => "Created",
                AuditAction.Update => "Updated",
                AuditAction.Delete => "Deleted",
                AuditAction.Approve => "Approved",
                AuditAction.Reject => "Rejected",
                _ => "Unknown"
            };
        }

        public static string GetBadgeClass(this ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Draft => "badge bg-secondary",
                ApplicationStatus.Submitted => "badge bg-info",
                ApplicationStatus.UnderReview => "badge bg-warning",
                ApplicationStatus.Approved => "badge bg-success",
                ApplicationStatus.Rejected => "badge bg-danger",
                ApplicationStatus.OnHold => "badge bg-warning",
                _ => "badge bg-light"
            };
        }

        public static string GetBadgeClass(this DonationStatus status)
        {
            return status switch
            {
                DonationStatus.Pending => "badge bg-warning",
                DonationStatus.Processing => "badge bg-info",
                DonationStatus.Completed => "badge bg-success",
                DonationStatus.Failed => "badge bg-danger",
                DonationStatus.Refunded => "badge bg-secondary",
                _ => "badge bg-light"
            };
        }
    }
}
