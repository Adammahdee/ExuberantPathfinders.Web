using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using ExuberantPathfinders.Web.Services;
using ExuberantPathfinders.Web.Middleware;
using ExuberantPathfinders.Web.Constants;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add MySQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
    
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Add Services
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdminRoleService, AdminRoleService>();

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.GetAllPermissions())
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));
    }

    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

// Add MVC
builder.Services.AddControllersWithViews();

// Localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("en-GB")
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRequestLocalization();

// Maintenance middleware moved earlier in pipeline
app.UseMiddleware<MaintenanceMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var initializer = new DbInitializer(
            services.GetRequiredService<ApplicationDbContext>(),
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>(),
            services.GetRequiredService<ILogger<DbInitializer>>()
        );
        await initializer.Initialize();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Routes
app.MapAreaControllerRoute(
    name: "admin_route",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
