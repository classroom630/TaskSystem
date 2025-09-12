using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.WebApp.Services;
using TaskSystem.WebApp.ViewModels;

namespace TaskSystem.WebApp.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IApiService apiService, ILogger<TasksController> logger)
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

        try
        {
            var tasks = await _apiService.GetTasksAsync(token) ?? new List<TaskDto>();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var viewModel = new TaskListViewModel
            {
                Tasks = tasks,
                CurrentUserRole = userRole
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tasks");
            TempData["Error"] = "Unable to load tasks";
            return View(new TaskListViewModel());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var task = await _apiService.GetTaskAsync(id, token);
            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var viewModel = new TaskDetailsViewModel
            {
                Task = task,
                CanEdit = CanUserModifyTask(task, currentUserId, userRole),
                CanDelete = CanUserModifyTask(task, currentUserId, userRole)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task {TaskId}", id);
            TempData["Error"] = "Unable to load task details";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Create()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = new CreateTaskViewModel();

        try
        {
            // Get available users for assignment
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            if (userRole == UserRoles.Admin || userRole == UserRoles.Manager)
            {
                var users = await _apiService.GetUsersAsync(token);
                viewModel.AvailableUsers = users?.Where(u => u.IsActive) ?? new List<UserDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users for task creation");
            // Continue without users list
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskViewModel model)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            // Reload available users
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
                if (userRole == UserRoles.Admin || userRole == UserRoles.Manager)
                {
                    var users = await _apiService.GetUsersAsync(token);
                    model.AvailableUsers = users?.Where(u => u.IsActive) ?? new List<UserDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for task creation");
            }

            return View(model);
        }

        try
        {
            var createRequest = new CreateTaskRequest
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                DueDate = model.DueDate,
                AssignedToId = model.AssignedToId
            };

            var createdTask = await _apiService.CreateTaskAsync(createRequest, token);
            if (createdTask != null)
            {
                TempData["Success"] = "Task created successfully!";
                return RedirectToAction(nameof(Details), new { id = createdTask.Id });
            }

            ModelState.AddModelError("", "Failed to create task");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            ModelState.AddModelError("", "An error occurred while creating the task");
        }

        // Reload available users on error
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            if (userRole == UserRoles.Admin || userRole == UserRoles.Manager)
            {
                var users = await _apiService.GetUsersAsync(token);
                model.AvailableUsers = users?.Where(u => u.IsActive) ?? new List<UserDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users for task creation");
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var task = await _apiService.GetTaskAsync(id, token);
            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (!CanUserModifyTask(task, currentUserId, userRole))
            {
                return Forbid();
            }

            var viewModel = new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                AssignedToId = task.AssignedToId
            };

            // Get available users for assignment
            if (userRole == UserRoles.Admin || userRole == UserRoles.Manager)
            {
                var users = await _apiService.GetUsersAsync(token);
                viewModel.AvailableUsers = users?.Where(u => u.IsActive) ?? new List<UserDto>();
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task {TaskId} for editing", id);
            TempData["Error"] = "Unable to load task for editing";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditTaskViewModel model)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            // Reload available users
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
                if (userRole == UserRoles.Admin || userRole == UserRoles.Manager)
                {
                    var users = await _apiService.GetUsersAsync(token);
                    model.AvailableUsers = users?.Where(u => u.IsActive) ?? new List<UserDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for task editing");
            }

            return View(model);
        }

        try
        {
            var updateRequest = new UpdateTaskRequest
            {
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                Priority = model.Priority,
                DueDate = model.DueDate,
                AssignedToId = model.AssignedToId
            };

            var updatedTask = await _apiService.UpdateTaskAsync(id, updateRequest, token);
            if (updatedTask != null)
            {
                TempData["Success"] = "Task updated successfully!";
                return RedirectToAction(nameof(Details), new { id = updatedTask.Id });
            }

            ModelState.AddModelError("", "Failed to update task");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            ModelState.AddModelError("", "An error occurred while updating the task");
        }

        // Reload available users on error
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            if (userRole == UserRoles.Admin || userRole == UserRoles.Manager)
            {
                var users = await _apiService.GetUsersAsync(token);
                model.AvailableUsers = users?.Where(u => u.IsActive) ?? new List<UserDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users for task editing");
        }

        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var task = await _apiService.GetTaskAsync(id, token);
            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (!CanUserModifyTask(task, currentUserId, userRole))
            {
                return Forbid();
            }

            var viewModel = new TaskDetailsViewModel
            {
                Task = task,
                CanEdit = true,
                CanDelete = true
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task {TaskId} for deletion", id);
            TempData["Error"] = "Unable to load task for deletion";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var success = await _apiService.DeleteTaskAsync(id, token);
            if (success)
            {
                TempData["Success"] = "Task deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete task";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            TempData["Error"] = "An error occurred while deleting the task";
        }

        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
    }

    private static bool CanUserModifyTask(TaskDto task, int currentUserId, string userRole)
    {
        return userRole switch
        {
            UserRoles.Admin => true,
            UserRoles.Manager => true,
            UserRoles.User => task.CreatedById == currentUserId || task.AssignedToId == currentUserId,
            _ => false
        };
    }
}