using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ExuberantPathfinders.Web.Constants;
using ExuberantPathfinders.Web.Models;
using Microsoft.Extensions.Logging;

namespace ExuberantPathfinders.Web.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DbInitializer> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task Initialize()
        {
            // Apply any pending migrations automatically
            await _context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync();

            // Seed Admin User
            await SeedAdminUserAsync();

            // Seed AppSettings
            await SeedAppSettingsAsync();
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "ProgramOfficer", "User" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogInformation("Seeding role: {Role}", role);
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin Permissions
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole != null)
            {
                var claims = await _roleManager.GetClaimsAsync(adminRole);
                var allPermissions = Permissions.GetAllPermissions();
                foreach (var permission in allPermissions)
                {
                    if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                    {
                        await _roleManager.AddClaimAsync(adminRole, new Claim("Permission", permission));
                    }
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            var adminEmail = "admin@exuberantpathfinders.org";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                _logger.LogInformation("Admin user not found. Creating default admin user.");
                
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user created successfully.");
                }
                else
                {
                    _logger.LogError("Error creating admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogInformation("Admin user already exists.");
            }
        }

        private async Task SeedAppSettingsAsync()
        {
            if (!await _context.AppSettings.AnyAsync())
            {
                var settings = new[]
                {
                    new AppSetting { Key = "MaintenanceMode", Value = "false", Description = "Enable to show maintenance page to non-admin users.", Group = "System" },
                    new AppSetting { Key = "MaintenanceEndTime", Value = DateTime.UtcNow.AddHours(2).ToString("o"), Description = "Estimated completion time (ISO 8601 format).", Group = "System" },
                    new AppSetting { Key = "AllowRegistration", Value = "true", Description = "Allow new users to register.", Group = "System" },
                    new AppSetting { Key = "ContactEmail", Value = "info@exuberantpathfinders.org", Description = "Main contact email address.", Group = "General" },
                    new AppSetting { Key = "SupportPhone", Value = "+234-09078511868", Description = "Support phone number.", Group = "General" },
                    new AppSetting { Key = "HomepageBanner", Value = "true", Description = "Show promotional banner on homepage.", Group = "Banner" },
                    new AppSetting { Key = "BannerMessage", Value = "Applications for 2026 Grants are now open!", Description = "Text to display in the homepage banner.", Group = "Banner" }
                };

                await _context.AppSettings.AddRangeAsync(settings);
                await _context.SaveChangesAsync();
            }
        }
    }
}