using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        public IActionResult Index(string? search, bool? isActive, int page = 1, int pageSize = 10)
        {
            return RedirectToAction(
                actionName: nameof(DashboardController.Users),
                controllerName: "Dashboard",
                routeValues: new { search, isActive, page, pageSize }
            );
        }
    }
}
