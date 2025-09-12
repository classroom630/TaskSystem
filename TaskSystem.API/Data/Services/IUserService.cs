using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> AssignRoleAsync(AssignRoleDto assignRoleDto);
        Task<IEnumerable<string>> GetUserRolesAsync(string id);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
    }
}