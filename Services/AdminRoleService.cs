using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Areas.Admin.ViewModels;
using ExuberantPathfinders.Web.Models;
using ExuberantPathfinders.Web.Constants;

namespace ExuberantPathfinders.Web.Services
{
    public interface IAdminRoleService
    {
        Task<List<RoleViewModel>> GetRolesAsync();
        Task<IdentityResult> CreateRoleAsync(string roleName, string adminId);
        Task<IdentityResult> DeleteRoleAsync(string roleId, string adminId);
        Task<ManageRolePermissionsViewModel?> GetPermissionsAsync(string roleId);
        Task<IdentityResult> UpdatePermissionsAsync(string roleId, List<string> selectedPermissions, string adminId);
    }

    public class AdminRoleService : IAdminRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public AdminRoleService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _auditService = auditService;
        }

        public async Task<List<RoleViewModel>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var viewModels = new List<RoleViewModel>();

            foreach (var role in roles)
            {
                var count = (await _userManager.GetUsersInRoleAsync(role.Name)).Count;
                viewModels.Add(new RoleViewModel { Id = role.Id, Name = role.Name, UserCount = count });
            }

            return viewModels;
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName, string adminId)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role already exists." });
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                await _auditService.LogAsync(adminId, AuditAction.Create, "Role", roleName, "Created new role");
            }

            return result;
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId, string adminId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            }

            // Safety Checks
            if (role.Name == "Admin" || role.Name == "ProgramOfficer" || role.Name == "User")
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot delete system roles." });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                await _auditService.LogAsync(adminId, AuditAction.Delete, "Role", role.Id, $"Deleted role {role.Name}");
            }

            return result;
        }

        public async Task<ManageRolePermissionsViewModel?> GetPermissionsAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return null;

            var claims = await _roleManager.GetClaimsAsync(role);
            var allPermissions = Permissions.GetAllPermissions();

            var model = new ManageRolePermissionsViewModel
            {
                RoleId = roleId,
                RoleName = role.Name ?? string.Empty,
                Permissions = allPermissions.Select(p => new PermissionViewModel
                {
                    Value = p,
                    Selected = claims.Any(c => c.Type == "Permission" && c.Value == p)
                }).ToList()
            };
            return model;
        }

        public async Task<IdentityResult> UpdatePermissionsAsync(string roleId, List<string> selectedPermissions, string adminId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return IdentityResult.Failed(new IdentityError { Description = "Role not found" });

            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var currentPermissions = currentClaims.Where(c => c.Type == "Permission").ToList();

            foreach (var claim in currentPermissions)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            foreach (var permission in selectedPermissions)
            {
                await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }

            await _auditService.LogAsync(adminId, AuditAction.Update, "Role", role.Id, $"Updated permissions for {role.Name}");

            return IdentityResult.Success;
        }
    }
}