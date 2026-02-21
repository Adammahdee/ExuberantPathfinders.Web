using System.Collections.Generic;

namespace ExuberantPathfinders.Web.Areas.Admin.ViewModels
{
    public class ManageRolePermissionsViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionViewModel> Permissions { get; set; } = new List<PermissionViewModel>();
    }

    public class PermissionViewModel
    {
        public string Value { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}