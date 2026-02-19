using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExuberantPathfinders.Web.Data;
using ExuberantPathfinders.Web.Models;
using ExuberantPathfinders.Web.ViewModels;

namespace ExuberantPathfinders.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Terms()
    {
        return View();
    }

    public IActionResult CommunityGuidelines()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ReportProblem()
    {
        return View(new ReportProblemViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportProblem(ReportProblemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var subject = model.IssueType switch
        {
            "TechnicalGlitch" => "Technical Glitch: The app isn't fueling my project correctly.",
            "GrantInquiry" => "Grant Inquiry: I have a question about my strategic funding.",
            "CommunityReport" => "Community Report: I encountered behavior that isn't 'Exuberant'.",
            _ => "Other: I need a navigator for a different issue."
        };

        var message = new ContactMessage
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Subject = subject,
            Message = model.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync();

        TempData["SupportSuccess"] = "Message Received. Our support team is acting as your navigator. We'll review this and get back to you shortly so you can get back to building the future.";
        return RedirectToAction(nameof(ReportProblem));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
