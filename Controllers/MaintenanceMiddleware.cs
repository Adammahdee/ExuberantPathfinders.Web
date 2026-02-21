using Microsoft.AspNetCore.Http;
using ExuberantPathfinders.Web.Data;
using System.Threading.Tasks;

namespace ExuberantPathfinders.Web.Middleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Allow access to static files, login, admin area, and the maintenance page itself
            if (path != null && (
                path.StartsWith("/admin") || 
                path.StartsWith("/account") || 
                path.StartsWith("/lib") || 
                path.StartsWith("/css") || 
                path.StartsWith("/js") || 
                path.StartsWith("/images") ||
                path.StartsWith("/maintenance")))
            {
                await _next(context);
                return;
            }

            // Check if maintenance mode is enabled
            var maintenanceSetting = await dbContext.AppSettings.FindAsync("MaintenanceMode");
            if (maintenanceSetting != null && maintenanceSetting.Value == "true")
            {
                context.Response.Redirect("/Maintenance");
                return;
            }

            await _next(context);
        }
    }
}