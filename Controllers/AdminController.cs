using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
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
                TotalUsers = await _context.Users.CountAsync()
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

        [HttpPost]
        public async Task<IActionResult> ApproveApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound();

            var previousStatus = application.Status;
            application.Status = ApplicationStatus.Approved;
            application.LastModifiedAt = DateTime.UtcNow;

            var history = new ApplicationStatusHistory
            {
                ApplicationId = id,
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Approved,
                ChangedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                ChangedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Applications));
        }

        [HttpPost]
        public async Task<IActionResult> RejectApplication(int id, string reason)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound();

            var previousStatus = application.Status;
            application.Status = ApplicationStatus.Rejected;
            application.ReviewNotes = reason;
            application.LastModifiedAt = DateTime.UtcNow;

            var history = new ApplicationStatusHistory
            {
                ApplicationId = id,
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Rejected,
                ChangedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Applications));
        }
    }
}
