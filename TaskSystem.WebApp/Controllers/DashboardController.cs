using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.Core.Entities;
using TaskSystem.WebApp.Services;
using TaskSystem.WebApp.ViewModels;

namespace TaskSystem.WebApp.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IApiService apiService, ILogger<DashboardController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;

        try
        {
            // Get tasks based on user role
            var tasks = await _apiService.GetTasksAsync(token) ?? new List<TaskSystem.Core.DTOs.TaskDto>();
            
            // Get users for admin role
            if (userRole == UserRoles.Admin)
            {
                var users = await _apiService.GetUsersAsync(token) ?? new List<TaskSystem.Core.DTOs.UserDto>();
                ViewBag.UserCount = users.Count();
                ViewBag.RecentUsers = users.OrderByDescending(u => u.CreatedAt).Take(5);
            }

            ViewBag.TaskCount = tasks.Count();
            ViewBag.RecentTasks = tasks.OrderByDescending(t => t.CreatedAt).Take(5);
            ViewBag.PendingTasks = tasks.Count(t => t.Status == TaskItemStatus.Pending);
            ViewBag.InProgressTasks = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
            ViewBag.CompletedTasks = tasks.Count(t => t.Status == TaskItemStatus.Completed);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            ViewBag.Error = "Unable to load dashboard data";
            return View();
        }
    }
}