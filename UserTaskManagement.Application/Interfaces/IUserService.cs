using UserTaskManagement.Application.DTOs.User;
using UserTaskManagement.Domain.Enums;

namespace UserTaskManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AssignRoleAsync(int userId, Role role);
        Task<bool> ToggleUserStatusAsync(int id);
        Task<int> GetUsersCountAsync();
    }
}