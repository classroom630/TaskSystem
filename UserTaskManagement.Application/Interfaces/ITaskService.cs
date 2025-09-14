using UserTaskManagement.Application.DTOs.Task;
using UserTaskManagement.Domain.Enums;

namespace UserTaskManagement.Application.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksAsync(int? userId = null, Role? userRole = null);
        Task<TaskDto?> GetTaskByIdAsync(int id, int requestingUserId, Role requestingUserRole);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int requestingUserId, Role requestingUserRole);
        Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int requestingUserId, Role requestingUserRole);
        Task<bool> DeleteTaskAsync(int id, int requestingUserId, Role requestingUserRole);
        Task<int> GetTasksCountAsync(int? userId = null);
        Task<int> GetCompletedTasksCountAsync(int? userId = null);
        Task<bool> ToggleTaskCompletionAsync(int id, int requestingUserId, Role requestingUserRole);
    }
}