using Microsoft.Extensions.Logging;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;

namespace TaskSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ITaskRepository taskRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToUserDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        if (!UserRoles.AllRoles.Contains(request.Role))
        {
            throw new InvalidOperationException("Invalid role specified");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower(),
            UserName = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            // Role = request.Role, // TODO: Use Identity roles
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User created: {Email} with role {Role}", user.Email, request.Role);

        return MapToUserDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if email is already taken by another user
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
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
            // user.Role = request.Role; // TODO: Use Identity roles
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User updated: {Email}", user.Email);

        return MapToUserDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
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

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User deleted: {Email}", user.Email);

        return true;
    }

    public async Task<bool> AssignRoleAsync(int userId, string role)
    {
        if (!UserRoles.AllRoles.Contains(role))
        {
            throw new InvalidOperationException("Invalid role specified");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // user.Role = role; // TODO: Use Identity roles
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Role assigned: {Email} -> {Role}", user.Email, role);

        return true;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Role = "User", // TODO: Get from Identity roles
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            FullName = user.FullName
        };
    }
}