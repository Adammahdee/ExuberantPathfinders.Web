using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ExuberantPathfinders.Web.ViewComponents
{
    public class ReportStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ReportStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var stats = await _context.Reports
                .GroupBy(r => r.IsResolved)
                .Select(g => new { IsResolved = g.Key, Count = g.Count() })
                .ToListAsync();

            var pending = stats.FirstOrDefault(s => !s.IsResolved)?.Count ?? 0;
            var resolved = stats.FirstOrDefault(s => s.IsResolved)?.Count ?? 0;

            return View(new { Pending = pending, Resolved = resolved });
        }
    }
}