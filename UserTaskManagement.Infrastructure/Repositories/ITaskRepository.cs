using TaskEntity = UserTaskManagement.Domain.Entities.Task;

namespace UserTaskManagement.Infrastructure.Repositories
{
    public interface ITaskRepository : IBaseRepository<TaskEntity>
    {
        Task<IEnumerable<TaskEntity>> GetTasksByUserIdAsync(int userId);
        Task<IEnumerable<TaskEntity>> GetTasksWithUsersAsync();
        Task<int> GetCompletedTasksCountAsync(int? userId = null);
        Task<int> GetTasksCountByUserAsync(int userId);
        Task<TaskEntity?> GetTaskWithUserAsync(int id);
    }
}