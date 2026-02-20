using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ExuberantPathfinders.Web.Services;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ReportsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index(string searchString, string statusFilter, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            int pageSize = 10;
            var reports = _context.Reports.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                reports = reports.Where(r => r.Email.Contains(searchString) || r.IssueType.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Pending")
                {
                    reports = reports.Where(r => !r.IsResolved);
                }
                else if (statusFilter == "Resolved")
                {
                    reports = reports.Where(r => r.IsResolved);
                }
            }

            reports = reports.OrderByDescending(r => r.CreatedAt);
            return View(await PaginatedList<Report>.CreateAsync(reports.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Reports/GetReportStatsData
        [HttpGet]
        public async Task<IActionResult> GetReportStatsData()
        {
            // Get report counts grouped by date for the last 30 days
            var reportData = await _context.Reports
                .Where(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var labels = reportData.Select(r => r.Date.ToString("MMM dd"));
            var data = reportData.Select(r => r.Count);

            return Json(new { labels, data });
        }

        // GET: Admin/Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/Reports/Resolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id, string resolutionNotes, string emailSubject)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            try
            {
                await _emailService.SendEmailAsync(report.Email, emailSubject, resolutionNotes);

                report.IsResolved = true;
                report.ResolutionNotes = resolutionNotes;
                report.ResolvedAt = DateTime.UtcNow;

                _context.Update(report);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Report resolved and email sent successfully.";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Error sending email: {ex.Message}";
                TempData["ToastType"] = "danger";
            }

            return RedirectToAction(nameof(Details), new { id = report.Id });

        }

        // POST: Admin/Reports/Reopen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.IsResolved = false;
            _context.Update(report);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Report reopened successfully.";
            TempData["ToastType"] = "info";

            return RedirectToAction(nameof(Details), new { id = report.Id });
        }

        // GET: Admin/Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}