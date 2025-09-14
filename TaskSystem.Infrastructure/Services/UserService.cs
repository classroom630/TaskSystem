using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;

namespace TaskSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager, 
        RoleManager<ApplicationRole> roleManager,
        ITaskRepository taskRepository, 
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _taskRepository = taskRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }
        
        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        return user == null ? null : await MapToUserDtoAsync(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        if (!UserRoles.AllRoles.Contains(request.Role))
        {
            throw new InvalidOperationException("Invalid role specified");
        }

        var user = _mapper.Map<User>(request);
        user.UserName = request.Email.ToLower();
        user.Email = request.Email.ToLower();
        user.IsActive = true;

        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, request.Role);

        _logger.LogInformation("User created: {Email} with role {Role}", user.Email, request.Role);

        return await MapToUserDtoAsync(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if email is already taken by another user
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already taken by another user");
            }
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email.ToLower();
        user.UserName = request.Email.ToLower();

        if (!string.IsNullOrEmpty(request.Role))
        {
            if (!UserRoles.AllRoles.Contains(request.Role))
            {
                throw new InvalidOperationException("Invalid role specified");
            }
            
            // Remove current roles and add new role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("User updated: {Email}", user.Email);

        return await MapToUserDtoAsync(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return false;
        }

        // Check if user has any tasks assigned or created
        var assignedTasks = await _taskRepository.GetByAssignedUserAsync(id);
        var createdTasks = await _taskRepository.GetByCreatedUserAsync(id);

        if (assignedTasks.Any() || createdTasks.Any())
        {
            throw new InvalidOperationException("Cannot delete user with existing tasks. Please reassign or delete tasks first.");
        }

        var result = await _userManager.DeleteAsync(user);
        
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("User deleted: {Email}", user.Email);

        return true;
    }

    public async Task<bool> AssignRoleAsync(int userId, string role)
    {
        if (!UserRoles.AllRoles.Contains(role))
        {
            throw new InvalidOperationException("Invalid role specified");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        // Remove current roles and add new role
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        _logger.LogInformation("Role assigned: {Email} -> {Role}", user.Email, role);

        return true;
    }

    private async Task<UserDto> MapToUserDtoAsync(User user)
    {
        var userDto = _mapper.Map<UserDto>(user);
        var roles = await _userManager.GetRolesAsync(user);
        userDto.Role = roles.FirstOrDefault() ?? string.Empty;
        return userDto;
    }
}