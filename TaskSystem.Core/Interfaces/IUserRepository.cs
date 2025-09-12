using System.Linq.Expressions;
using TaskSystem.Core.Entities;

namespace TaskSystem.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetFirstAsync(Expression<Func<User, bool>> predicate);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate);
    Task<User> AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(User entity);
    Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<User, bool>> predicate);
    Task<int> SaveChangesAsync();
    
    // User-specific methods
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
}