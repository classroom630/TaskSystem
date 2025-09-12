using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;
        
        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            ILogger<UserService> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }
        
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();
            
            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(userDto);
            }
            
            return userDtos;
        }
        
        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            
            return userDto;
        }
        
        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }
            
            if (!await _roleManager.RoleExistsAsync(createUserDto.Role))
            {
                throw new InvalidOperationException($"Role '{createUserDto.Role}' does not exist");
            }
            
            var user = _mapper.Map<ApplicationUser>(createUserDto);
            user.Id = Guid.NewGuid().ToString();
            
            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }
            
            await _userManager.AddToRoleAsync(user, createUserDto.Role);
            
            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName);
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = new List<string> { createUserDto.Role };
            
            _logger.LogInformation("User {Email} created successfully by admin", createUserDto.Email);
            
            return userDto;
        }
        
        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found");
            }
            
            _mapper.Map(updateUserDto, user);
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            
            _logger.LogInformation("User {Id} updated successfully", id);
            
            return userDto;
        }
        
        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;
            
            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Id} deleted successfully", id);
                return true;
            }
            
            return false;
        }
        
        public async Task<bool> AssignRoleAsync(AssignRoleDto assignRoleDto)
        {
            var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
            if (user == null) return false;
            
            if (!await _roleManager.RoleExistsAsync(assignRoleDto.Role))
            {
                throw new InvalidOperationException($"Role '{assignRoleDto.Role}' does not exist");
            }
            
            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            // Add new role
            var result = await _userManager.AddToRoleAsync(user, assignRoleDto.Role);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {Role} assigned to user {UserId}", assignRoleDto.Role, assignRoleDto.UserId);
                return true;
            }
            
            return false;
        }
        
        public async Task<IEnumerable<string>> GetUserRolesAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return new List<string>();
            
            return await _userManager.GetRolesAsync(user);
        }
        
        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            var userDtos = new List<UserDto>();
            
            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = new List<string> { role };
                userDtos.Add(userDto);
            }
            
            return userDtos;
        }
    }
}