using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Initialize()
        {
            await _context.Database.MigrateAsync();
            await SeedRoles();
            await SeedUsers();
            await SeedThematicAreas();
            await SeedPrograms();
        }

        private async Task SeedRoles()
        {
            var roleNames = new[] { "Admin", "ProgramOfficer", "Donor", "Applicant" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task SeedUsers()
        {
            // Admin user
            if (await _userManager.FindByEmailAsync("admin@exuberant.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@exuberant.com",
                    Email = "admin@exuberant.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Program Officer user
            if (await _userManager.FindByEmailAsync("officer@exuberant.com") == null)
            {
                var officer = new ApplicationUser
                {
                    UserName = "officer@exuberant.com",
                    Email = "officer@exuberant.com",
                    FirstName = "Program",
                    LastName = "Officer",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(officer, "Officer@123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(officer, "ProgramOfficer");
                }
            }
        }

        private async Task SeedThematicAreas()
        {
            if (_context.ThematicAreas.Any())
                return;

            var areas = new[]
            {
                new ThematicArea { Name = "Education", Code = "EDU", Description = "Educational programs and initiatives" },
                new ThematicArea { Name = "Health", Code = "HEALTH", Description = "Healthcare and wellness programs" },
                new ThematicArea { Name = "Environment", Code = "ENV", Description = "Environmental conservation programs" },
                new ThematicArea { Name = "Community Development", Code = "COM", Description = "Community development initiatives" }
            };

            await _context.ThematicAreas.AddRangeAsync(areas);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPrograms()
        {
            if (_context.Programs.Any())
                return;

            var officer = await _userManager.FindByEmailAsync("officer@exuberant.com");
            var thematicArea = await _context.ThematicAreas.FirstOrDefaultAsync();

            if (officer != null && thematicArea != null)
            {
                var program = new GrantProgram
                {
                    Name = "Youth Scholarship Program",
                    Description = "Providing scholarships to underprivileged youth",
                    ThematicAreaId = thematicArea.Id,
                    ProgramOfficerId = officer.Id,
                    Budget = 100000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1)
                };

                await _context.Programs.AddAsync(program);
                await _context.SaveChangesAsync();
            }
        }
    }
}
