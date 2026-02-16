using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using System.Security.Claims;

namespace ExuberantPathfinders.Web.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var campaigns = await _context.Campaigns
                .Where(c => c.IsActive)
                .Include(c => c.Program)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(campaigns);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Program)
                .Include(c => c.Donations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
                return NotFound();

            return View(campaign);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDonation(int campaignId, decimal amount)
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var donation = new Donation
            {
                DonorId = userId,
                CampaignId = campaignId,
                Amount = amount,
                Status = DonationStatus.Pending,
                Gateway = PaymentGateway.Paystack,
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // TODO: Integrate with Paystack
            return RedirectToAction(nameof(Details), new { id = campaignId });
        }

        [HttpGet]
        public async Task<IActionResult> MyDonations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var donations = await _context.Donations
                .Where(d => d.DonorId == userId)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(donations);
        }
    }
}
