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

            // Seed sample grants/scholarships and campaigns
            await SeedSampleFundingDataAsync();
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

        private async Task SeedSampleFundingDataAsync()
        {
            var adminUser = await _userManager.FindByEmailAsync("admin@exuberantpathfinders.org");
            if (adminUser == null)
            {
                _logger.LogWarning("Skipping sample funding data seeding because admin user was not found.");
                return;
            }

            var thematicAreas = await EnsureSampleThematicAreasAsync();

            if (!await _context.Programs.AnyAsync())
            {
                var samplePrograms = new[]
                {
                    new GrantProgram
                    {
                        Name = "Community Innovation Micro-Grants",
                        Description = "Small grants for community-led pilots that improve local resilience and livelihoods.",
                        ThematicAreaId = thematicAreas["COMMUNITY"].Id,
                        Budget = 150000m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-30),
                        EndDate = DateTime.UtcNow.Date.AddMonths(6),
                        ProgramOfficerId = adminUser.Id,
                        IsActive = true
                    },
                    new GrantProgram
                    {
                        Name = "STEM Scholars Fellowship 2026",
                        Description = "Scholarship support for high-performing STEM students from underserved communities.",
                        ThematicAreaId = thematicAreas["EDU"].Id,
                        Budget = 300000m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-14),
                        EndDate = DateTime.UtcNow.Date.AddMonths(8),
                        ProgramOfficerId = adminUser.Id,
                        IsActive = true
                    },
                    new GrantProgram
                    {
                        Name = "Women in Leadership Scholarships",
                        Description = "Scholarship grants for women pursuing postgraduate leadership and policy studies.",
                        ThematicAreaId = thematicAreas["EDU"].Id,
                        Budget = 220000m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-7),
                        EndDate = DateTime.UtcNow.Date.AddMonths(9),
                        ProgramOfficerId = adminUser.Id,
                        IsActive = true
                    }
                };

                await _context.Programs.AddRangeAsync(samplePrograms);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Campaigns.AnyAsync())
            {
                var programs = await _context.Programs
                    .AsNoTracking()
                    .ToListAsync();

                var microGrantProgram = programs.FirstOrDefault(p => p.Name == "Community Innovation Micro-Grants");
                var stemScholarProgram = programs.FirstOrDefault(p => p.Name == "STEM Scholars Fellowship 2026");
                var womenScholarProgram = programs.FirstOrDefault(p => p.Name == "Women in Leadership Scholarships");

                var sampleCampaigns = new List<Campaign>();

                if (microGrantProgram != null)
                {
                    sampleCampaigns.Add(new Campaign
                    {
                        Name = "Back 100 Community Projects",
                        Description = "Fund catalytic micro-grants for neighborhood cooperatives and local innovators.",
                        ProgramId = microGrantProgram.Id,
                        TargetAmount = 120000m,
                        AmountRaised = 27500m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-21),
                        EndDate = DateTime.UtcNow.Date.AddMonths(4),
                        IsActive = true
                    });
                }

                if (stemScholarProgram != null)
                {
                    sampleCampaigns.Add(new Campaign
                    {
                        Name = "Sponsor a STEM Scholar",
                        Description = "Help cover tuition, devices, and mentorship for the next cohort of STEM scholars.",
                        ProgramId = stemScholarProgram.Id,
                        TargetAmount = 180000m,
                        AmountRaised = 64350m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-10),
                        EndDate = DateTime.UtcNow.Date.AddMonths(5),
                        IsActive = true
                    });
                }

                if (womenScholarProgram != null)
                {
                    sampleCampaigns.Add(new Campaign
                    {
                        Name = "Advance Women Leaders Fund",
                        Description = "Support scholarships for women entering leadership and public-impact programs.",
                        ProgramId = womenScholarProgram.Id,
                        TargetAmount = 140000m,
                        AmountRaised = 21900m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-5),
                        EndDate = DateTime.UtcNow.Date.AddMonths(6),
                        IsActive = true
                    });
                }

                if (sampleCampaigns.Count > 0)
                {
                    await _context.Campaigns.AddRangeAsync(sampleCampaigns);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task<Dictionary<string, ThematicArea>> EnsureSampleThematicAreasAsync()
        {
            var requiredAreas = new[]
            {
                new ThematicArea { Code = "EDU", Name = "Education", Description = "Programs focused on scholarships, learning, and school advancement.", IsActive = true },
                new ThematicArea { Code = "COMMUNITY", Name = "Community Development", Description = "Programs that empower communities through innovation and local infrastructure.", IsActive = true },
                new ThematicArea { Code = "YOUTH", Name = "Youth Empowerment", Description = "Programs that support youth leadership, employability, and civic participation.", IsActive = true }
            };

            var existingAreas = await _context.ThematicAreas.ToListAsync();
            var byCode = existingAreas
                .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var newlyAdded = new List<ThematicArea>();
            foreach (var area in requiredAreas)
            {
                if (!byCode.ContainsKey(area.Code))
                {
                    newlyAdded.Add(area);
                }
            }

            if (newlyAdded.Count > 0)
            {
                await _context.ThematicAreas.AddRangeAsync(newlyAdded);
                await _context.SaveChangesAsync();

                existingAreas = await _context.ThematicAreas.ToListAsync();
                byCode = existingAreas
                    .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            }

            return byCode;
        }
    }
}
