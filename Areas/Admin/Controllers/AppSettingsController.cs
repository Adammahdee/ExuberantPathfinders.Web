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
    public class AppSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AppSettings
        public async Task<IActionResult> Index()
        {
            // Group settings by their 'Group' property for display
            var settings = await _context.Set<AppSetting>()
                .OrderBy(s => s.Group)
                .ThenBy(s => s.Key)
                .ToListAsync();
            
            return View(settings);
        }

        // POST: Admin/AppSettings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string key, string value)
        {
            var setting = await _context.Set<AppSetting>().FindAsync(key);
            if (setting == null)
            {
                return NotFound();
            }

            setting.Value = value;
            _context.Update(setting);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Setting updated successfully.";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // Helper to toggle maintenance mode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMaintenance()
        {
            var setting = await _context.Set<AppSetting>().FindAsync("MaintenanceMode");
            if (setting != null) {
                 return await Update("MaintenanceMode", setting.Value == "true" ? "false" : "true");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}