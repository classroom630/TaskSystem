using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;

namespace TaskSystem.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    Task<IEnumerable<TaskDto>> GetTasksByUserAsync(int userId);
    Task<IEnumerable<TaskDto>> GetTasksForUserAsync(int currentUserId, string userRole);
    Task<TaskDto?> GetTaskByIdAsync(int id);
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int createdById);
    Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskRequest request, int currentUserId, string userRole);
    Task<bool> DeleteTaskAsync(int id, int currentUserId, string userRole);
}