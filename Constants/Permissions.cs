using System.Collections.Generic;

namespace ExuberantPathfinders.Web.Constants
{
    public static class Permissions
    {
        public static class Reports
        {
            public const string View = "Permissions.Reports.View";
            public const string Create = "Permissions.Reports.Create";
            public const string Edit = "Permissions.Reports.Edit";
            public const string Delete = "Permissions.Reports.Delete";
        }

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
            public const string ManagePermissions = "Permissions.Roles.ManagePermissions";
        }

        public static List<string> GetAllPermissions()
        {
            return new List<string>
            {
                Reports.View,
                Reports.Create,
                Reports.Edit,
                Reports.Delete,
                Users.View,
                Users.Create,
                Users.Edit,
                Users.Delete,
                Roles.View,
                Roles.Create,
                Roles.Edit,
                Roles.Delete,
                Roles.ManagePermissions
            };
        }
    }
}
