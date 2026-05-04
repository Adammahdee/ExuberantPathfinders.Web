using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExuberantPathfinders.Web.Areas.Admin.ViewModels;
using ExuberantPathfinders.Web.Constants;
using ExuberantPathfinders.Web.Services;
using System.Security.Claims;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // base
    public class RolesController : Controller
    {
        private readonly IAdminRoleService _adminRoleService;

        public RolesController(IAdminRoleService adminRoleService)
        {
            _adminRoleService = adminRoleService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found.");
        }

        private void SetToast(string message, string type)
        {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = type;
        }

        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<IActionResult> Index()
        {
            var viewModels = await _adminRoleService.GetRolesAsync();
            return View(viewModels);
        }

        [Authorize(Policy = Permissions.Roles.Create)]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Roles.Create)]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _adminRoleService.CreateRoleAsync(model.Name, GetCurrentUserId());

            if (result.Succeeded)
            {
                SetToast("Role created successfully.", "success");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [Authorize(Policy = Permissions.Roles.ManagePermissions)]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var model = await _adminRoleService.GetPermissionsAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Roles.ManagePermissions)]
        public async Task<IActionResult> ManagePermissions(ManageRolePermissionsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var selectedPermissions = model.Permissions
                .Where(permission => permission.Selected)
                .Select(permission => permission.Value)
                .ToList();

            var result = await _adminRoleService.UpdatePermissionsAsync(model.RoleId, selectedPermissions, GetCurrentUserId());

            if (result.Succeeded)
            {
                SetToast("Permissions updated successfully.", "success");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Roles.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var result = await _adminRoleService.DeleteRoleAsync(id, GetCurrentUserId());

            if (result.Succeeded)
                SetToast("Role deleted successfully.", "success");
            else
                SetToast(result.Errors.FirstOrDefault()?.Description ?? "Error deleting role.", "danger");

            return RedirectToAction(nameof(Index));
        }
    }
}
