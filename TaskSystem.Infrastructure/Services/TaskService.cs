using Microsoft.Extensions.Logging;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;

namespace TaskSystem.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;

    public TaskService(IUnitOfWork unitOfWork, ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync();
        return tasks.Select(MapToTaskDto);
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByUserAsync(int userId)
    {
        var assignedTasks = await _unitOfWork.Tasks.GetByAssignedUserAsync(userId);
        var createdTasks = await _unitOfWork.Tasks.GetByCreatedUserAsync(userId);
        
        var allTasks = assignedTasks.Union(createdTasks).Distinct();
        return allTasks.Select(MapToTaskDto);
    }

    public async Task<IEnumerable<TaskDto>> GetTasksForUserAsync(int currentUserId, string userRole)
    {
        IEnumerable<TaskItem> tasks;

        switch (userRole)
        {
            case UserRoles.Admin:
                tasks = await _unitOfWork.Tasks.GetAllAsync();
                break;
            
            case UserRoles.Manager:
                // Manager can see all tasks for now - could be limited to team tasks in future
                tasks = await _unitOfWork.Tasks.GetAllAsync();
                break;
            
            case UserRoles.User:
            default:
                // Users can only see tasks assigned to them or created by them
                var assignedTasks = await _unitOfWork.Tasks.GetByAssignedUserAsync(currentUserId);
                var createdTasks = await _unitOfWork.Tasks.GetByCreatedUserAsync(currentUserId);
                tasks = assignedTasks.Union(createdTasks).Distinct();
                break;
        }

        return tasks.Select(MapToTaskDto);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        return task == null ? null : MapToTaskDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int createdById)
    {
        // Validate assigned user exists if specified
        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _unitOfWork.Users.GetByIdAsync(request.AssignedToId.Value);
            if (assignedUser == null || !assignedUser.IsActive)
            {
                throw new InvalidOperationException("Assigned user does not exist or is inactive");
            }
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TaskItemStatus.Pending,
            DueDate = request.DueDate,
            CreatedById = createdById,
            AssignedToId = request.AssignedToId
        };

        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Task created: {Title} by user {UserId}", task.Title, createdById);

        // Fetch the task again to get navigation properties
        var createdTask = await _unitOfWork.Tasks.GetByIdAsync(task.Id);
        return MapToTaskDto(createdTask!);
    }

    public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskRequest request, int currentUserId, string userRole)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        // Check permissions
        if (!CanUserModifyTask(task, currentUserId, userRole))
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this task");
        }

        // Validate assigned user exists if specified
        if (request.AssignedToId.HasValue)
        {
            var assignedUser = await _unitOfWork.Users.GetByIdAsync(request.AssignedToId.Value);
            if (assignedUser == null || !assignedUser.IsActive)
            {
                throw new InvalidOperationException("Assigned user does not exist or is inactive");
            }
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Task updated: {TaskId} by user {UserId}", id, currentUserId);

        // Fetch the task again to get updated navigation properties
        var updatedTask = await _unitOfWork.Tasks.GetByIdAsync(task.Id);
        return MapToTaskDto(updatedTask!);
    }

    public async Task<bool> DeleteTaskAsync(int id, int currentUserId, string userRole)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null)
        {
            return false;
        }

        // Check permissions
        if (!CanUserModifyTask(task, currentUserId, userRole))
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this task");
        }

        await _unitOfWork.Tasks.DeleteAsync(task);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Task deleted: {TaskId} by user {UserId}", id, currentUserId);

        return true;
    }

    private static bool CanUserModifyTask(TaskItem task, int currentUserId, string userRole)
    {
        return userRole switch
        {
            UserRoles.Admin => true,
            UserRoles.Manager => true,
            UserRoles.User => task.CreatedById == currentUserId || task.AssignedToId == currentUserId,
            _ => false
        };
    }

    private static TaskDto MapToTaskDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CreatedById = task.CreatedById,
            CreatedByName = task.CreatedBy?.FullName ?? "Unknown",
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.FullName
        };
    }
}