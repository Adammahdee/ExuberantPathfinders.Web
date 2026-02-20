using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 10;
            var reports = _context.Reports.OrderByDescending(r => r.CreatedAt);
            return View(await PaginatedList<Report>.CreateAsync(reports.AsNoTracking(), pageNumber ?? 1, pageSize));
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
        public async Task<IActionResult> Resolve(int id, string resolutionNotes)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.IsResolved = true;
            report.ResolutionNotes = resolutionNotes;
            report.ResolvedAt = DateTime.UtcNow;

            _context.Update(report);
            await _context.SaveChangesAsync();

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