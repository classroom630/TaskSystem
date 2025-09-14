using UserTaskManagement.Domain.Entities;
using UserTaskManagement.Domain.Enums;

namespace UserTaskManagement.Application.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(Role role);
        Task<bool> ExistsAsync(string email);
        Task<User?> GetUserWithTasksAsync(int id);
    }
}