using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserTaskManagement.Application.DTOs.User;
using UserTaskManagement.Application.Interfaces;
using UserTaskManagement.Domain.Entities;
using UserTaskManagement.Domain.Enums;

namespace UserTaskManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;

        public UserService(
            UserManager<User> userManager,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserService> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async System.Threading.Tasks.Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<UserDto>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async System.Threading.Tasks.Task<UserDto?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                return user != null ? _mapper.Map<UserDto>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<UserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                return user != null ? _mapper.Map<UserDto>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                var existingUser = await _userRepository.ExistsAsync(createUserDto.Email);
                if (existingUser)
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                var user = _mapper.Map<User>(createUserDto);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User creation failed: {errors}");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, createUserDto.Role.ToString());
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign role {Role} to user {Email}", createUserDto.Role, createUserDto.Email);
                }

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(createUserDto.Email, createUserDto.FirstName, createUserDto.LastName);

                _logger.LogInformation("User {Email} created successfully", createUserDto.Email);

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", createUserDto.Email);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Check if email is being changed and if it's already taken
                if (user.Email != updateUserDto.Email)
                {
                    var emailExists = await _userRepository.ExistsAsync(updateUserDto.Email);
                    if (emailExists)
                    {
                        throw new InvalidOperationException("Email is already taken by another user");
                    }
                }

                // Update user properties
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.Email = updateUserDto.Email;
                user.UserName = updateUserDto.Email;
                user.NormalizedEmail = updateUserDto.Email.ToUpper();
                user.NormalizedUserName = updateUserDto.Email.ToUpper();
                user.Role = updateUserDto.Role;
                user.IsActive = updateUserDto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User update failed: {errors}");
                }

                // Update role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                var targetRole = updateUserDto.Role.ToString();
                
                if (!currentRoles.Contains(targetRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, targetRole);
                }

                _logger.LogInformation("User {UserId} updated successfully", id);

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} deleted successfully", id);
                    return true;
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to delete user {UserId}: {Errors}", id, errors);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<bool> AssignRoleAsync(int userId, Role role)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Remove all current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new role
                var result = await _userManager.AddToRoleAsync(user, role.ToString());
                
                if (result.Succeeded)
                {
                    // Update the role in our entity as well
                    user.Role = role;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);

                    _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
                    return true;
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to assign role {Role} to user {UserId}: {Errors}", role, userId, errors);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", role, userId);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<bool> ToggleUserStatusAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} status toggled to {Status}", id, user.IsActive ? "Active" : "Inactive");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<int> GetUsersCountAsync()
        {
            try
            {
                return await _userRepository.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users count");
                throw;
            }
        }
    }
}