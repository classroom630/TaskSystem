using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Repositories
{
    public interface ITaskRepository : IGenericRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(string userId);
        Task<IEnumerable<TaskItem>> GetTasksForManagerAsync(string managerId);
        Task<IEnumerable<TaskItem>> GetAllTasksWithUsersAsync();
        Task<TaskItem?> GetTaskWithUserDetailsAsync(int taskId);
        Task<IEnumerable<TaskItem>> GetFilteredTasksAsync(TaskFilterDto filter, string? userId = null, bool isAdmin = false);
    }
}