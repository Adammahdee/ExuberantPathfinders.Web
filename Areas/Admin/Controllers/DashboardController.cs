using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using ExuberantPathfinders.Web.Areas.Admin.ViewModels;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalApplications = await _context.Applications.CountAsync(),
                PendingApplications = await _context.Applications
                    .Where(a => a.Status == ApplicationStatus.UnderReview)
                    .CountAsync(),
                TotalDonations = await _context.Donations
                    .Where(d => d.Status == DonationStatus.Completed)
                    .SumAsync(d => d.Amount),
                TotalUsers = await _context.Users.CountAsync(),
                ActiveCampaigns = await _context.Campaigns
                    .Where(c => c.IsActive)
                    .CountAsync()
            };

            return View(stats);
        }

        [Authorize(Roles = "Admin,ProgramOfficer")]
        public async Task<IActionResult> Applications()
        {
            var applications = await _context.Applications
                .Include(a => a.Program)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Reports()
        {
            var monthlyData = await _context.Donations
                .Where(d => d.Status == DonationStatus.Completed && d.CompletedAt != null)
                .Select(d => new { Donation = d, Month = d.CompletedAt!.Value.Month })
                .GroupBy(x => x.Month)
                .Select(g => new MonthlyDonationReportViewModel
                {
                    Month = g.Key,
                    TotalAmount = g.Sum(x => x.Donation.Amount),
                    DonationCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return View(monthlyData);
        }
    }
}
