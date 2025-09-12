using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId);
        Task<IEnumerable<TaskDto>> GetTasksForManagerAsync(string managerId);
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string createdByUserId);
        Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, string currentUserId, IEnumerable<string> userRoles);
        Task<bool> DeleteTaskAsync(int id, string currentUserId, IEnumerable<string> userRoles);
        Task<IEnumerable<TaskDto>> GetFilteredTasksAsync(TaskFilterDto filter, string? userId = null, bool isAdmin = false);
        Task<bool> CanUserAccessTaskAsync(int taskId, string userId, IEnumerable<string> userRoles);
    }
}