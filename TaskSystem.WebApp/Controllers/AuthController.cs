using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.Core.DTOs;
using TaskSystem.WebApp.Services;
using TaskSystem.WebApp.ViewModels;

namespace TaskSystem.WebApp.Controllers;

public class AuthController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IApiService apiService, ILogger<AuthController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var loginRequest = new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            };

            var result = await _apiService.LoginAsync(loginRequest);

            if (result != null)
            {
                // Store JWT token in session
                HttpContext.Session.SetString("JwtToken", result.Token);
                HttpContext.Session.SetString("RefreshToken", result.RefreshToken);

                // Create claims for cookie authentication
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                    new(ClaimTypes.Name, result.User.FullName),
                    new(ClaimTypes.Email, result.User.Email),
                    new(ClaimTypes.Role, result.User.Role),
                    new("FirstName", result.User.FirstName),
                    new("LastName", result.User.LastName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = result.Expires
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                _logger.LogInformation("User {Email} logged in", model.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Email}", model.Email);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var registerRequest = new RegisterRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                Role = model.Role
            };

            var result = await _apiService.RegisterAsync(registerRequest);

            if (result != null)
            {
                _logger.LogInformation("User {Email} registered successfully by admin", model.Email);
                TempData["Success"] = "User registered successfully!";
                return RedirectToAction("Index", "Users");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", model.Email);
        }

        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Clear session
        HttpContext.Session.Clear();

        // Sign out
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("User logged out");

        return RedirectToAction("Index", "Home");
    }
}