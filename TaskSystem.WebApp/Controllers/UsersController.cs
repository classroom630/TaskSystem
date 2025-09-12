using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.WebApp.Services;
using TaskSystem.WebApp.ViewModels;

namespace TaskSystem.WebApp.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IApiService apiService, ILogger<UsersController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    /// <summary>
    /// List all users (Admin only)
    /// </summary>
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var users = await _apiService.GetUsersAsync(token);
            var viewModel = new UserListViewModel
            {
                Users = users ?? new List<UserDto>()
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users list");
            TempData["ErrorMessage"] = "Unable to load users list";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    /// <summary>
    /// View user details
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var user = await _apiService.GetUserAsync(id, token);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
            TempData["ErrorMessage"] = "Unable to load user details";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Show create user form (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult Create()
    {
        var viewModel = new CreateUserViewModel();
        return View(viewModel);
    }

    /// <summary>
    /// Create new user (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var request = new CreateUserRequest
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                Password = viewModel.Password,
                Role = viewModel.Role
            };

            var user = await _apiService.CreateUserAsync(request, token);
            if (user != null)
            {
                TempData["SuccessMessage"] = "User created successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to create user. Please try again.");
                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ModelState.AddModelError("", "An error occurred while creating the user");
            return View(viewModel);
        }
    }

    /// <summary>
    /// Show edit user form (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var user = await _apiService.GetUserAsync(id, token);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit, ID: {UserId}", id);
            TempData["ErrorMessage"] = "Unable to load user for editing";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Update user (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var request = new UpdateUserRequest
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                Role = viewModel.Role,
                IsActive = viewModel.IsActive
            };

            var user = await _apiService.UpdateUserAsync(id, request, token);
            if (user != null)
            {
                TempData["SuccessMessage"] = "User updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to update user. Please try again.");
                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            ModelState.AddModelError("", "An error occurred while updating the user");
            return View(viewModel);
        }
    }

    /// <summary>
    /// Show delete user confirmation (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var user = await _apiService.GetUserAsync(id, token);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for deletion, ID: {UserId}", id);
            TempData["ErrorMessage"] = "Unable to load user for deletion";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = UserRoles.Admin)]
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
            var result = await _apiService.DeleteUserAsync(id, token);
            if (result)
            {
                TempData["SuccessMessage"] = "User deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the user";
        }

        return RedirectToAction(nameof(Index));
    }
}