using AutoMapper;
using Microsoft.Extensions.Logging;
using UserTaskManagement.Application.DTOs.Task;
using UserTaskManagement.Application.Interfaces;
using UserTaskManagement.Domain.Enums;
using TaskEntity = UserTaskManagement.Domain.Entities.Task;

namespace UserTaskManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;
        private readonly IEmailService _emailService;

        public TaskService(
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<TaskService> logger,
            IEmailService emailService)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksAsync(int? userId = null, Role? userRole = null)
        {
            try
            {
                IEnumerable<TaskEntity> tasks;

                if (userRole == Role.Admin)
                {
                    // Admin sees all tasks
                    tasks = await _taskRepository.GetTasksWithUsersAsync();
                }
                else if (userRole == Role.Manager)
                {
                    // Manager sees all tasks (or could be filtered by team in future)
                    tasks = await _taskRepository.GetTasksWithUsersAsync();
                }
                else
                {
                    // User sees only their own tasks
                    if (userId.HasValue)
                    {
                        tasks = await _taskRepository.GetTasksByUserIdAsync(userId.Value);
                    }
                    else
                    {
                        tasks = new List<TaskEntity>();
                    }
                }

                return _mapper.Map<IEnumerable<TaskDto>>(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for user {UserId} with role {UserRole}", userId, userRole);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id, int requestingUserId, Role requestingUserRole)
        {
            try
            {
                var task = await _taskRepository.GetTaskWithUserAsync(id);
                if (task == null)
                {
                    return null;
                }

                // Check permissions
                if (!CanAccessTask(task, requestingUserId, requestingUserRole))
                {
                    throw new UnauthorizedAccessException("You don't have permission to view this task");
                }

                return _mapper.Map<TaskDto>(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int requestingUserId, Role requestingUserRole)
        {
            try
            {
                var task = _mapper.Map<TaskEntity>(createTaskDto);

                // Determine who the task should be assigned to
                if (createTaskDto.UserId.HasValue)
                {
                    // Admin or Manager assigning task to specific user
                    if (requestingUserRole == Role.User)
                    {
                        throw new UnauthorizedAccessException("Users cannot assign tasks to others");
                    }

                    // Verify the target user exists
                    var targetUser = await _userRepository.GetByIdAsync(createTaskDto.UserId.Value);
                    if (targetUser == null)
                    {
                        throw new InvalidOperationException("Target user not found");
                    }

                    task.UserId = createTaskDto.UserId.Value;

                    // Send notification email
                    await _emailService.SendTaskAssignedEmailAsync(targetUser.Email!, targetUser.FirstName, task.Title);
                }
                else
                {
                    // Task assigned to the requesting user
                    task.UserId = requestingUserId;
                }

                var createdTask = await _taskRepository.AddAsync(task);
                
                // Load user data for the response
                var taskWithUser = await _taskRepository.GetTaskWithUserAsync(createdTask.Id);
                
                _logger.LogInformation("Task {TaskId} created by user {UserId}", createdTask.Id, requestingUserId);

                return _mapper.Map<TaskDto>(taskWithUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for user {UserId}", requestingUserId);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int requestingUserId, Role requestingUserRole)
        {
            try
            {
                var task = await _taskRepository.GetTaskWithUserAsync(id);
                if (task == null)
                {
                    throw new InvalidOperationException("Task not found");
                }

                // Check permissions
                if (!CanModifyTask(task, requestingUserId, requestingUserRole))
                {
                    throw new UnauthorizedAccessException("You don't have permission to modify this task");
                }

                // Update task properties
                task.Title = updateTaskDto.Title;
                task.Description = updateTaskDto.Description;
                task.IsCompleted = updateTaskDto.IsCompleted;
                task.DueDate = updateTaskDto.DueDate;
                task.UpdatedAt = DateTime.UtcNow;

                var updatedTask = await _taskRepository.UpdateAsync(task);
                
                _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, requestingUserId);

                return _mapper.Map<TaskDto>(updatedTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id, int requestingUserId, Role requestingUserRole)
        {
            try
            {
                var task = await _taskRepository.GetTaskWithUserAsync(id);
                if (task == null)
                {
                    return false;
                }

                // Check permissions
                if (!CanModifyTask(task, requestingUserId, requestingUserRole))
                {
                    throw new UnauthorizedAccessException("You don't have permission to delete this task");
                }

                var result = await _taskRepository.DeleteAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, requestingUserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<int> GetTasksCountAsync(int? userId = null)
        {
            try
            {
                if (userId.HasValue)
                {
                    return await _taskRepository.GetTasksCountByUserAsync(userId.Value);
                }
                else
                {
                    return await _taskRepository.CountAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks count for user {UserId}", userId);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<int> GetCompletedTasksCountAsync(int? userId = null)
        {
            try
            {
                return await _taskRepository.GetCompletedTasksCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed tasks count for user {UserId}", userId);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<bool> ToggleTaskCompletionAsync(int id, int requestingUserId, Role requestingUserRole)
        {
            try
            {
                var task = await _taskRepository.GetTaskWithUserAsync(id);
                if (task == null)
                {
                    return false;
                }

                // Check permissions
                if (!CanModifyTask(task, requestingUserId, requestingUserRole))
                {
                    throw new UnauthorizedAccessException("You don't have permission to modify this task");
                }

                task.IsCompleted = !task.IsCompleted;
                task.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.UpdateAsync(task);
                
                _logger.LogInformation("Task {TaskId} completion toggled to {Status} by user {UserId}", 
                    id, task.IsCompleted, requestingUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling task completion for task {TaskId}", id);
                throw;
            }
        }

        private bool CanAccessTask(TaskEntity task, int requestingUserId, Role requestingUserRole)
        {
            return requestingUserRole == Role.Admin ||
                   requestingUserRole == Role.Manager ||
                   task.UserId == requestingUserId;
        }

        private bool CanModifyTask(TaskEntity task, int requestingUserId, Role requestingUserRole)
        {
            return requestingUserRole == Role.Admin ||
                   (requestingUserRole == Role.Manager) ||
                   (requestingUserRole == Role.User && task.UserId == requestingUserId);
        }
    }
}