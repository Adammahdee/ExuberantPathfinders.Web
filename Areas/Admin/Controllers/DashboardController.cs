using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using ExuberantPathfinders.Web.Areas.Admin.ViewModels;
using System.Security.Claims;

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
            var model = new AdminDashboardViewModel
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
                    .CountAsync(),
                RecentApplications = await _context.Applications
                    .Include(a => a.Applicant)
                    .Include(a => a.Program)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
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

        [HttpPost]
        [Authorize(Roles = "Admin,ProgramOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status, string? reason)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                TempData["AdminError"] = "Application not found.";
                return RedirectToAction(nameof(Applications));
            }

            var previousStatus = application.Status;
            if (previousStatus == status)
            {
                TempData["AdminInfo"] = "Application status is already set to that value.";
                return RedirectToAction(nameof(Applications));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            application.Status = status;
            application.LastModifiedAt = DateTime.UtcNow;
            if (status == ApplicationStatus.Approved || status == ApplicationStatus.Rejected)
            {
                application.ReviewedAt = DateTime.UtcNow;
                application.ReviewedById = userId;
                application.ReviewNotes = string.IsNullOrWhiteSpace(reason) ? "Updated by admin." : reason.Trim();
            }

            _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = application.Id,
                PreviousStatus = previousStatus,
                NewStatus = status,
                ChangedById = userId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Updated by admin." : reason.Trim(),
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["AdminSuccess"] = "Application status updated.";
            return RedirectToAction(nameof(Applications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["AdminError"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId)
            {
                TempData["AdminError"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = !user.IsActive;
            user.LastModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["AdminSuccess"] = user.IsActive ? "User activated." : "User deactivated.";
            return RedirectToAction(nameof(Users));
        }
    }
}
