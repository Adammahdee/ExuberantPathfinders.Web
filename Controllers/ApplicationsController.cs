using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using System.Security.Claims;

namespace ExuberantPathfinders.Web.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var applications = await _context.Applications
                .Where(a => a.ApplicantId == userId)
                .Include(a => a.Program)
                .Include(a => a.StatusHistory)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        public async Task<IActionResult> Create()
        {
            var programs = await _context.Programs
                .Where(p => p.IsActive)
                .ToListAsync();

            return View(programs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Application application, bool acceptPathfinderAgreement)
        {
            if (!acceptPathfinderAgreement)
            {
                TempData["ApplicationError"] = "Please agree to the Pathfinder Agreement before saving your application.";
                return RedirectToAction(nameof(Create));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            application.ApplicantId = userId;
            application.Status = ApplicationStatus.Draft;
            application.SubmissionReference = $"APP-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            application.CreatedAt = DateTime.UtcNow;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == userId);

            if (application == null)
                return NotFound();

            if (application.Status != ApplicationStatus.Draft)
            {
                TempData["ApplicationError"] = "Only draft applications can be submitted.";
                return RedirectToAction(nameof(Index));
            }

            application.Status = ApplicationStatus.Submitted;
            application.SubmittedAt = DateTime.UtcNow;
            application.LastModifiedAt = DateTime.UtcNow;

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            TempData["ApplicationSuccess"] = "Application submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var application = await _context.Applications
                .Include(a => a.StatusHistory)
                .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == userId);

            if (application == null)
                return NotFound();

            if (application.Status != ApplicationStatus.Draft)
            {
                TempData["ApplicationError"] = "Only draft applications can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            if (application.StatusHistory.Any())
            {
                _context.ApplicationStatusHistories.RemoveRange(application.StatusHistory);
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            TempData["ApplicationSuccess"] = "Draft application deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
