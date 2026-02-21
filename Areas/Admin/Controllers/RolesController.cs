using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExuberantPathfinders.Web.Areas.Admin.ViewModels;
using ExuberantPathfinders.Web.Services;
using System.Security.Claims;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IAdminRoleService _adminRoleService;

        public RolesController(IAdminRoleService adminRoleService)
        {
            _adminRoleService = adminRoleService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModels = await _adminRoleService.GetRolesAsync();
            return View(viewModels);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _adminRoleService.CreateRoleAsync(model.Name, adminId);

            if (result.Succeeded)
            {
                TempData["ToastMessage"] = "Role created successfully.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _adminRoleService.DeleteRoleAsync(id, adminId);

            if (result.Succeeded)
            {
                TempData["ToastMessage"] = "Role deleted successfully.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = result.Errors.FirstOrDefault()?.Description ?? "Error deleting role.";
                TempData["ToastType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManagePermissions(string id)
        {
            var model = await _adminRoleService.GetPermissionsAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(ManageRolePermissionsViewModel model)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Extract selected permissions
            var selectedPermissions = model.Permissions
                .Where(p => p.Selected)
                .Select(p => p.Value)
                .ToList();

            var result = await _adminRoleService.UpdatePermissionsAsync(model.RoleId, selectedPermissions, adminId);

            if (result.Succeeded)
            {
                TempData["ToastMessage"] = "Permissions updated successfully.";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}