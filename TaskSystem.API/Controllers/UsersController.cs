using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;

namespace TaskSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Get user by ID (User can get their own profile, Admin can get any)
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        // Users can only access their own profile unless they're admin
        if (userRole != UserRoles.Admin && currentUserId != id)
        {
            return Forbid();
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Update user (User can update their own profile, Admin can update any)
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        // Users can only update their own profile
        if (userRole != UserRoles.Admin && currentUserId != id)
        {
            return Forbid();
        }

        // Regular users cannot change roles or active status
        if (userRole != UserRoles.Admin)
        {
            request.Role = null;
            request.IsActive = null;
        }

        var user = await _userService.UpdateUserAsync(id, request);
        return Ok(user);
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Assign role to user (Admin only)
    /// </summary>
    [HttpPost("{id:int}/assign-role")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult> AssignRole(int id, [FromBody] string role)
    {
        var assigned = await _userService.AssignRoleAsync(id, role);
        if (!assigned)
        {
            return NotFound();
        }

        return Ok(new { message = "Role assigned successfully" });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)!.Value;
    }
}