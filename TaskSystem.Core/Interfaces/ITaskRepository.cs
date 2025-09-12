using TaskSystem.Core.Entities;

namespace TaskSystem.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(int userId);
    Task<IEnumerable<TaskItem>> GetByCreatedUserAsync(int userId);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskItemStatus status);
}