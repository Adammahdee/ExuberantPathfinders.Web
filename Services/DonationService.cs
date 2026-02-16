using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ExuberantPathfinders.Web.Services
{
    public interface IDonationService
    {
        Task<Donation?> GetDonationByIdAsync(int id);
        Task<List<Donation>> GetCampaignDonationsAsync(int campaignId);
        Task CreateDonationAsync(Donation donation);
        Task UpdateDonationAsync(Donation donation);
        Task CompleteDonationAsync(int id, string transactionId);
        Task RefundDonationAsync(int id);
    }

    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;

        public DonationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Donation?> GetDonationByIdAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.Campaign)
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Donation>> GetCampaignDonationsAsync(int campaignId)
        {
            return await _context.Donations
                .Where(d => d.CampaignId == campaignId && d.Status == DonationStatus.Completed)
                .Include(d => d.Donor)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateDonationAsync(Donation donation)
        {
            donation.CreatedAt = DateTime.UtcNow;
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDonationAsync(Donation donation)
        {
            _context.Donations.Update(donation);
            await _context.SaveChangesAsync();
        }

        public async Task CompleteDonationAsync(int id, string transactionId)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
                throw new InvalidOperationException("Donation not found");

            donation.Status = DonationStatus.Completed;
            donation.TransactionId = transactionId;
            donation.CompletedAt = DateTime.UtcNow;
            donation.IsVerified = true;
            donation.VerifiedAt = DateTime.UtcNow;

            // Update campaign amount raised
            var campaign = await _context.Campaigns.FindAsync(donation.CampaignId);
            if (campaign != null)
            {
                campaign.AmountRaised += donation.Amount;
            }

            await UpdateDonationAsync(donation);
        }

        public async Task RefundDonationAsync(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
                throw new InvalidOperationException("Donation not found");

            donation.Status = DonationStatus.Refunded;

            // Update campaign amount raised
            var campaign = await _context.Campaigns.FindAsync(donation.CampaignId);
            if (campaign != null)
            {
                campaign.AmountRaised -= donation.Amount;
            }

            await UpdateDonationAsync(donation);
        }
    }
}
