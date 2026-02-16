using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ExuberantPathfinders.Web.Services
{
    public interface IApplicationService
    {
        Task<List<Application>> GetUserApplicationsAsync(string userId);
        Task<Application?> GetApplicationByIdAsync(int id);
        Task CreateApplicationAsync(Application application);
        Task UpdateApplicationAsync(Application application);
        Task SubmitApplicationAsync(int id);
        Task ApproveApplicationAsync(int id, string reviewedById, string? notes = null);
        Task RejectApplicationAsync(int id, string reviewedById, string reason);
    }

    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Application>> GetUserApplicationsAsync(string userId)
        {
            return await _context.Applications
                .Where(a => a.ApplicantId == userId)
                .Include(a => a.Program)
                .Include(a => a.StatusHistory)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Application?> GetApplicationByIdAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.Program)
                .Include(a => a.StatusHistory)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task CreateApplicationAsync(Application application)
        {
            application.CreatedAt = DateTime.UtcNow;
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateApplicationAsync(Application application)
        {
            application.LastModifiedAt = DateTime.UtcNow;
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task SubmitApplicationAsync(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                throw new InvalidOperationException("Application not found");

            application.Status = ApplicationStatus.Submitted;
            application.SubmittedAt = DateTime.UtcNow;
            await UpdateApplicationAsync(application);
        }

        public async Task ApproveApplicationAsync(int id, string reviewedById, string? notes = null)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                throw new InvalidOperationException("Application not found");

            var previousStatus = application.Status;
            application.Status = ApplicationStatus.Approved;
            application.ReviewedById = reviewedById;
            application.ReviewNotes = notes;
            application.ReviewedAt = DateTime.UtcNow;

            var history = new ApplicationStatusHistory
            {
                ApplicationId = id,
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Approved,
                ChangedById = reviewedById,
                ChangedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await UpdateApplicationAsync(application);
        }

        public async Task RejectApplicationAsync(int id, string reviewedById, string reason)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                throw new InvalidOperationException("Application not found");

            var previousStatus = application.Status;
            application.Status = ApplicationStatus.Rejected;
            application.ReviewedById = reviewedById;
            application.ReviewNotes = reason;
            application.ReviewedAt = DateTime.UtcNow;

            var history = new ApplicationStatusHistory
            {
                ApplicationId = id,
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Rejected,
                ChangedById = reviewedById,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await UpdateApplicationAsync(application);
        }
    }
}
