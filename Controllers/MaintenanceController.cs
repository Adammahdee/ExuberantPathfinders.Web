using Microsoft.AspNetCore.Mvc;
using ExuberantPathfinders.Web.Data;
using System.Threading.Tasks;

namespace ExuberantPathfinders.Web.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var setting = await _context.AppSettings.FindAsync("MaintenanceEndTime");
            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                ViewData["MaintenanceEndTime"] = setting.Value;
            }
            return View();
        }
    }
}