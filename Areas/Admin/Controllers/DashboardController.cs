using ExuberantPathfinders.Web.Areas.Admin.ViewModels;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using ExuberantPathfinders.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExuberantPathfinders.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalApplications = await _context.Applications.CountAsync(),
                PendingApplications = await _context.Applications.Where(a => a.Status == ApplicationStatus.UnderReview).CountAsync(),
                TotalDonations = await _context.Donations.Where(d => d.Status == DonationStatus.Completed).SumAsync(d => d.Amount),
                TotalUsers = await _context.Users.CountAsync(),
                TotalGrants = await _context.Programs.CountAsync(),
                TotalFunds = await _context.Campaigns.CountAsync(),
                ActiveCampaigns = await _context.Campaigns.Where(c => c.IsActive).CountAsync(),
                RecentApplications = await _context.Applications
                    .Include(a => a.Applicant)
                    .Include(a => a.Program)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(6)
                    .ToListAsync(),
                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(6)
                    .ToListAsync(),
                RecentAuditLogs = await _context.AuditLogs
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(8)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Applications(string? search, ApplicationStatus? status, int page = 1, int pageSize = 10)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize, 5, 50, 10);

            var query = _context.Applications
                .Include(a => a.Program)
                .Include(a => a.Applicant)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(a =>
                    (a.SubmissionReference != null && a.SubmissionReference.Contains(value)) ||
                    a.Title.Contains(value) ||
                    (a.Applicant != null && (
                        a.Applicant.FirstName.Contains(value) ||
                        a.Applicant.LastName.Contains(value) ||
                        (a.Applicant.Email != null && a.Applicant.Email.Contains(value))
                    )) ||
                    (a.Program != null && a.Program.Name.Contains(value))
                );
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = CalculateTotalPages(totalCount, pageSize);
            page = ClampPage(page, totalPages);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new ApplicantsListViewModel
            {
                Items = items,
                Search = search?.Trim() ?? string.Empty,
                Status = status,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return View(model);
        }

        public async Task<IActionResult> Grants()
        {
            var officerIds = await _userManager.GetUsersInRoleAsync("ProgramOfficer");
            var model = new GrantManagementViewModel
            {
                Grants = await _context.Programs
                    .Include(g => g.ThematicArea)
                    .Include(g => g.ProgramOfficer)
                    .Include(g => g.Applications)
                    .Include(g => g.Campaigns)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync(),
                ThematicAreas = await _context.ThematicAreas
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .ToListAsync(),
                ProgramOfficers = officerIds.ToList(),
                FormError = TempData["GrantFormError"]?.ToString() ?? string.Empty,
                FormSuccess = TempData["GrantFormSuccess"]?.ToString() ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGrant(string name, string? description, int thematicAreaId, decimal budget, DateTime startDate, DateTime? endDate, string programOfficerId)
        {
            var validationError = await ValidateGrantInput(name, description, thematicAreaId, budget, startDate, endDate, programOfficerId);
            if (validationError != null)
            {
                TempData["GrantFormError"] = validationError;
                return RedirectToAction(nameof(Grants));
            }

            var grant = new GrantProgram
            {
                Name = name.Trim(),
                Description = (description ?? string.Empty).Trim(),
                ThematicAreaId = thematicAreaId,
                Budget = budget,
                StartDate = startDate,
                EndDate = endDate,
                ProgramOfficerId = programOfficerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Programs.Add(grant);
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Create, "GrantProgram", grant.Id, null, new
            {
                grant.Name,
                grant.Budget,
                grant.StartDate,
                grant.EndDate
            });

            TempData["GrantFormSuccess"] = "Grant program created.";
            return RedirectToAction(nameof(Grants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGrant(int id, string name, string? description, int thematicAreaId, decimal budget, DateTime startDate, DateTime? endDate, string programOfficerId)
        {
            var grant = await _context.Programs.FindAsync(id);
            if (grant == null)
            {
                TempData["GrantFormError"] = "Grant not found.";
                return RedirectToAction(nameof(Grants));
            }

            var validationError = await ValidateGrantInput(name, description, thematicAreaId, budget, startDate, endDate, programOfficerId);
            if (validationError != null)
            {
                TempData["GrantFormError"] = validationError;
                return RedirectToAction(nameof(Grants));
            }

            var old = new
            {
                grant.Name,
                grant.Description,
                grant.ThematicAreaId,
                grant.Budget,
                grant.StartDate,
                grant.EndDate,
                grant.ProgramOfficerId
            };

            grant.Name = name.Trim();
            grant.Description = (description ?? string.Empty).Trim();
            grant.ThematicAreaId = thematicAreaId;
            grant.Budget = budget;
            grant.StartDate = startDate;
            grant.EndDate = endDate;
            grant.ProgramOfficerId = programOfficerId;

            await _context.SaveChangesAsync();
            await LogAdminActionAsync(AuditAction.Update, "GrantProgram", grant.Id, old, new
            {
                grant.Name,
                grant.Description,
                grant.ThematicAreaId,
                grant.Budget,
                grant.StartDate,
                grant.EndDate,
                grant.ProgramOfficerId
            });

            TempData["GrantFormSuccess"] = "Grant updated successfully.";
            return RedirectToAction(nameof(Grants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGrant(int id)
        {
            var grant = await _context.Programs
                .Include(g => g.Applications)
                .Include(g => g.Campaigns)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grant == null)
            {
                TempData["GrantFormError"] = "Grant not found.";
                return RedirectToAction(nameof(Grants));
            }

            if (grant.Applications.Any() || grant.Campaigns.Any())
            {
                TempData["GrantFormError"] = "Grant cannot be deleted while linked to applications or funds. Deactivate it instead.";
                return RedirectToAction(nameof(Grants));
            }

            var old = new { grant.Name, grant.Budget, grant.IsActive };
            _context.Programs.Remove(grant);
            await _context.SaveChangesAsync();
            await LogAdminActionAsync(AuditAction.Delete, "GrantProgram", grant.Id, old, null);

            TempData["GrantFormSuccess"] = "Grant deleted.";
            return RedirectToAction(nameof(Grants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleGrantStatus(int id)
        {
            var grant = await _context.Programs.FindAsync(id);
            if (grant == null)
            {
                TempData["GrantFormError"] = "Grant not found.";
                return RedirectToAction(nameof(Grants));
            }

            var old = new { grant.IsActive };
            grant.IsActive = !grant.IsActive;
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Update, "GrantProgram", grant.Id, old, new { grant.IsActive });
            TempData["GrantFormSuccess"] = grant.IsActive ? "Grant activated." : "Grant deactivated.";
            return RedirectToAction(nameof(Grants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGrantBudget(int id, decimal budget)
        {
            var grant = await _context.Programs.FindAsync(id);
            if (grant == null)
            {
                TempData["GrantFormError"] = "Grant not found.";
                return RedirectToAction(nameof(Grants));
            }

            if (budget <= 0 || budget > 100000000000m)
            {
                TempData["GrantFormError"] = "Budget must be between 0.01 and 100,000,000,000.";
                return RedirectToAction(nameof(Grants));
            }

            var old = new { grant.Budget };
            grant.Budget = budget;
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Update, "GrantProgram", grant.Id, old, new { grant.Budget });
            TempData["GrantFormSuccess"] = "Grant budget updated.";
            return RedirectToAction(nameof(Grants));
        }

        public async Task<IActionResult> Funds()
        {
            var model = new FundManagementViewModel
            {
                Campaigns = await _context.Campaigns
                    .Include(c => c.Program)
                    .Include(c => c.Donations)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync(),
                Programs = await _context.Programs
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync(),
                FormError = TempData["FundFormError"]?.ToString() ?? string.Empty,
                FormSuccess = TempData["FundFormSuccess"]?.ToString() ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFund(string name, string? description, int programId, decimal targetAmount, DateTime startDate, DateTime endDate)
        {
            var validationError = await ValidateFundInput(name, description, programId, targetAmount, startDate, endDate);
            if (validationError != null)
            {
                TempData["FundFormError"] = validationError;
                return RedirectToAction(nameof(Funds));
            }

            var campaign = new Campaign
            {
                Name = name.Trim(),
                Description = (description ?? string.Empty).Trim(),
                ProgramId = programId,
                TargetAmount = targetAmount,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Create, "Campaign", campaign.Id, null, new
            {
                campaign.Name,
                campaign.TargetAmount,
                campaign.StartDate,
                campaign.EndDate
            });

            TempData["FundFormSuccess"] = "Fund campaign created.";
            return RedirectToAction(nameof(Funds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFund(int id, string name, string? description, int programId, decimal targetAmount, DateTime startDate, DateTime endDate)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                TempData["FundFormError"] = "Campaign not found.";
                return RedirectToAction(nameof(Funds));
            }

            var validationError = await ValidateFundInput(name, description, programId, targetAmount, startDate, endDate);
            if (validationError != null)
            {
                TempData["FundFormError"] = validationError;
                return RedirectToAction(nameof(Funds));
            }

            var old = new
            {
                campaign.Name,
                campaign.Description,
                campaign.ProgramId,
                campaign.TargetAmount,
                campaign.StartDate,
                campaign.EndDate
            };

            campaign.Name = name.Trim();
            campaign.Description = (description ?? string.Empty).Trim();
            campaign.ProgramId = programId;
            campaign.TargetAmount = targetAmount;
            campaign.StartDate = startDate;
            campaign.EndDate = endDate;
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Update, "Campaign", campaign.Id, old, new
            {
                campaign.Name,
                campaign.Description,
                campaign.ProgramId,
                campaign.TargetAmount,
                campaign.StartDate,
                campaign.EndDate
            });

            TempData["FundFormSuccess"] = "Campaign updated successfully.";
            return RedirectToAction(nameof(Funds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFund(int id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Donations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                TempData["FundFormError"] = "Campaign not found.";
                return RedirectToAction(nameof(Funds));
            }

            if (campaign.Donations.Any())
            {
                TempData["FundFormError"] = "Campaign cannot be deleted because it already has donation records. Deactivate it instead.";
                return RedirectToAction(nameof(Funds));
            }

            var old = new { campaign.Name, campaign.TargetAmount, campaign.IsActive };
            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
            await LogAdminActionAsync(AuditAction.Delete, "Campaign", campaign.Id, old, null);

            TempData["FundFormSuccess"] = "Campaign deleted.";
            return RedirectToAction(nameof(Funds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFundStatus(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                TempData["FundFormError"] = "Campaign not found.";
                return RedirectToAction(nameof(Funds));
            }

            var old = new { campaign.IsActive };
            campaign.IsActive = !campaign.IsActive;
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Update, "Campaign", campaign.Id, old, new { campaign.IsActive });
            TempData["FundFormSuccess"] = campaign.IsActive ? "Campaign activated." : "Campaign deactivated.";
            return RedirectToAction(nameof(Funds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFundTarget(int id, decimal targetAmount)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                TempData["FundFormError"] = "Campaign not found.";
                return RedirectToAction(nameof(Funds));
            }

            if (targetAmount <= 0 || targetAmount > 100000000000m)
            {
                TempData["FundFormError"] = "Target amount must be between 0.01 and 100,000,000,000.";
                return RedirectToAction(nameof(Funds));
            }

            var old = new { campaign.TargetAmount };
            campaign.TargetAmount = targetAmount;
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Update, "Campaign", campaign.Id, old, new { campaign.TargetAmount });
            TempData["FundFormSuccess"] = "Campaign target updated.";
            return RedirectToAction(nameof(Funds));
        }

        public async Task<IActionResult> Users(string? search, bool? isActive, int page = 1, int pageSize = 10)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize, 5, 50, 10);

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(u =>
                    u.FirstName.Contains(value) ||
                    u.LastName.Contains(value) ||
                    (u.Email != null && u.Email.Contains(value)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = CalculateTotalPages(totalCount, pageSize);
            page = ClampPage(page, totalPages);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new UsersListViewModel
            {
                Items = items,
                Search = search?.Trim() ?? string.Empty,
                IsActive = isActive,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return View(model);
        }

        public async Task<IActionResult> Logs(string? search, AuditAction? auditAction, string? entityType, int page = 1, int pageSize = 20)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize, 10, 100, 20);

            var query = _context.AuditLogs
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(l =>
                    l.EntityType.Contains(value) ||
                    l.NewValues.Contains(value) ||
                    l.OldValues.Contains(value) ||
                    (l.User != null && l.User.Email != null && l.User.Email.Contains(value)));
            }

            if (auditAction.HasValue)
            {
                query = query.Where(l => l.Action == auditAction.Value);
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                var entityFilter = entityType.Trim();
                query = query.Where(l => l.EntityType.Contains(entityFilter));
            }

            var totalCount = await query.CountAsync();
            var totalPages = CalculateTotalPages(totalCount, pageSize);
            page = ClampPage(page, totalPages);

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new LogsListViewModel
            {
                Items = items,
                Search = search?.Trim() ?? string.Empty,
                AuditAction = auditAction,
                EntityType = entityType?.Trim() ?? string.Empty,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return View(model);
        }

        public async Task<IActionResult> Reports()
        {
            var monthlyData = await _context.Donations
                .Where(d => d.Status == DonationStatus.Completed && d.CompletedAt != null)
                .Select(d => new { Donation = d, Month = d.CompletedAt!.Value.Month })
                .GroupBy(x => x.Month)
                .Select(g => new MonthlyDonationReportViewModel
                {
                    Month = g.Key,
                    TotalAmount = g.Sum(x => x.Donation.Amount),
                    DonationCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return View(monthlyData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status, string? reason)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                TempData["AdminError"] = "Application not found.";
                return RedirectToAction(nameof(Applications));
            }

            var previousStatus = application.Status;
            if (previousStatus == status)
            {
                TempData["AdminInfo"] = "Application status is already set to that value.";
                return RedirectToAction(nameof(Applications));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            application.Status = status;
            application.LastModifiedAt = DateTime.UtcNow;
            if (status == ApplicationStatus.Approved || status == ApplicationStatus.Rejected)
            {
                application.ReviewedAt = DateTime.UtcNow;
                application.ReviewedById = userId;
                application.ReviewNotes = string.IsNullOrWhiteSpace(reason) ? "Updated by admin." : reason.Trim();
            }

            _context.ApplicationStatusHistories.Add(new ApplicationStatusHistory
            {
                ApplicationId = application.Id,
                PreviousStatus = previousStatus,
                NewStatus = status,
                ChangedById = userId,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Updated by admin." : reason.Trim(),
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await LogAdminActionAsync(AuditAction.Update, "Application", application.Id, new { previousStatus }, new { status, reason });
            TempData["AdminSuccess"] = "Application status updated.";
            return RedirectToAction(nameof(Applications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["AdminError"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId)
            {
                TempData["AdminError"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            var old = new { user.IsActive };
            user.IsActive = !user.IsActive;
            user.LastModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await LogAdminActionAsync(AuditAction.Update, "ApplicationUser", 0, old, new { user.IsActive, user.Id });
            TempData["AdminSuccess"] = user.IsActive ? "User activated." : "User deactivated.";
            return RedirectToAction(nameof(Users));
        }

        private async Task<string?> ValidateGrantInput(string name, string? description, int thematicAreaId, decimal budget, DateTime startDate, DateTime? endDate, string programOfficerId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Grant name is required.";

            var trimmedName = name.Trim();
            if (trimmedName.Length < 3 || trimmedName.Length > 120)
                return "Grant name must be between 3 and 120 characters.";

            var desc = (description ?? string.Empty).Trim();
            if (desc.Length > 500)
                return "Description cannot exceed 500 characters.";

            if (budget <= 0 || budget > 100000000000m)
                return "Budget must be between 0.01 and 100,000,000,000.";

            if (startDate == default)
                return "A valid start date is required.";

            if (endDate.HasValue && endDate.Value.Date < startDate.Date)
                return "End date cannot be earlier than start date.";

            var thematicExists = await _context.ThematicAreas.AnyAsync(t => t.Id == thematicAreaId && t.IsActive);
            if (!thematicExists)
                return "Selected thematic area is invalid or inactive.";

            var officer = await _userManager.FindByIdAsync(programOfficerId);
            if (officer == null)
                return "Selected program officer does not exist.";

            var isOfficer = await _userManager.IsInRoleAsync(officer, "ProgramOfficer");
            if (!isOfficer)
                return "Selected user is not assigned to ProgramOfficer role.";

            return null;
        }

        private async Task<string?> ValidateFundInput(string name, string? description, int programId, decimal targetAmount, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Campaign name is required.";

            var trimmedName = name.Trim();
            if (trimmedName.Length < 3 || trimmedName.Length > 120)
                return "Campaign name must be between 3 and 120 characters.";

            var desc = (description ?? string.Empty).Trim();
            if (desc.Length > 500)
                return "Description cannot exceed 500 characters.";

            if (targetAmount <= 0 || targetAmount > 100000000000m)
                return "Target amount must be between 0.01 and 100,000,000,000.";

            if (startDate == default || endDate == default)
                return "Start and end dates are required.";

            if (endDate.Date < startDate.Date)
                return "End date cannot be earlier than start date.";

            var programExists = await _context.Programs.AnyAsync(p => p.Id == programId);
            if (!programExists)
                return "Selected program is invalid.";

            return null;
        }

        private static int NormalizePage(int page) => page < 1 ? 1 : page;

        private static int NormalizePageSize(int pageSize, int min, int max, int fallback)
        {
            if (pageSize < min || pageSize > max)
                return fallback;

            return pageSize;
        }

        private static int CalculateTotalPages(int totalCount, int pageSize)
        {
            return totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        private static int ClampPage(int page, int totalPages)
        {
            if (page > totalPages)
                return totalPages;

            return page;
        }

        private async Task LogAdminActionAsync(AuditAction action, string entityType, int entityId, object? oldValues = null, object? newValues = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogActionAsync(userId, action, entityType, entityId, oldValues, newValues, ipAddress);
        }
    }
}
