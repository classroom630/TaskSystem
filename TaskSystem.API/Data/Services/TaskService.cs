using AutoMapper;
using Microsoft.AspNetCore.Identity;
using TaskSystem.API.Data.Repositories;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;
        private readonly IEmailService _emailService;
        
        public TaskService(
            ITaskRepository taskRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<TaskService> logger,
            IEmailService emailService)
        {
            _taskRepository = taskRepository;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }
        
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllTasksWithUsersAsync();
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
        
        public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId)
        {
            var tasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
        
        public async Task<IEnumerable<TaskDto>> GetTasksForManagerAsync(string managerId)
        {
            var tasks = await _taskRepository.GetTasksForManagerAsync(managerId);
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
        
        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetTaskWithUserDetailsAsync(id);
            if (task == null) return null;
            
            return _mapper.Map<TaskDto>(task);
        }
        
        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, string createdByUserId)
        {
            // Verify assigned user exists
            var assignedUser = await _userManager.FindByIdAsync(createTaskDto.AssignedToUserId);
            if (assignedUser == null)
            {
                throw new KeyNotFoundException($"User with ID '{createTaskDto.AssignedToUserId}' not found");
            }
            
            var task = _mapper.Map<TaskItem>(createTaskDto);
            task.CreatedByUserId = createdByUserId;
            
            var createdTask = await _taskRepository.AddAsync(task);
            
            // Send notification email
            await _emailService.SendTaskAssignedEmailAsync(
                assignedUser.Email!, 
                assignedUser.FullName, 
                createTaskDto.Title);
            
            _logger.LogInformation("Task {TaskId} created and assigned to user {UserId}", createdTask.Id, createTaskDto.AssignedToUserId);
            
            return _mapper.Map<TaskDto>(await _taskRepository.GetTaskWithUserDetailsAsync(createdTask.Id));
        }
        
        public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, string currentUserId, IEnumerable<string> userRoles)
        {
            var task = await _taskRepository.GetTaskWithUserDetailsAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"Task with ID '{id}' not found");
            }
            
            // Check permissions
            if (!await CanUserAccessTaskAsync(id, currentUserId, userRoles))
            {
                throw new UnauthorizedAccessException("You don't have permission to update this task");
            }
            
            // If assigning to a different user, verify the user exists
            if (!string.IsNullOrEmpty(updateTaskDto.AssignedToUserId) && 
                updateTaskDto.AssignedToUserId != task.AssignedToUserId)
            {
                var assignedUser = await _userManager.FindByIdAsync(updateTaskDto.AssignedToUserId);
                if (assignedUser == null)
                {
                    throw new KeyNotFoundException($"User with ID '{updateTaskDto.AssignedToUserId}' not found");
                }
                
                // Send notification email for reassignment
                await _emailService.SendTaskAssignedEmailAsync(
                    assignedUser.Email!, 
                    assignedUser.FullName, 
                    updateTaskDto.Title);
            }
            
            _mapper.Map(updateTaskDto, task);
            task.UpdatedAt = DateTime.UtcNow;
            
            var updatedTask = await _taskRepository.UpdateAsync(task);
            
            _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, currentUserId);
            
            return _mapper.Map<TaskDto>(await _taskRepository.GetTaskWithUserDetailsAsync(updatedTask.Id));
        }
        
        public async Task<bool> DeleteTaskAsync(int id, string currentUserId, IEnumerable<string> userRoles)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return false;
            
            // Check permissions
            if (!await CanUserAccessTaskAsync(id, currentUserId, userRoles))
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this task");
            }
            
            await _taskRepository.DeleteAsync(task);
            
            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, currentUserId);
            
            return true;
        }
        
        public async Task<IEnumerable<TaskDto>> GetFilteredTasksAsync(TaskFilterDto filter, string? userId = null, bool isAdmin = false)
        {
            var tasks = await _taskRepository.GetFilteredTasksAsync(filter, userId, isAdmin);
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
        
        public async Task<bool> CanUserAccessTaskAsync(int taskId, string userId, IEnumerable<string> userRoles)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return false;
            
            var roles = userRoles.ToList();
            
            // Admin can access all tasks
            if (roles.Contains("Admin")) return true;
            
            // Manager can access all tasks (for now - in real scenario, they'd access their team's tasks)
            if (roles.Contains("Manager")) return true;
            
            // User can only access their own tasks
            return task.AssignedToUserId == userId || task.CreatedByUserId == userId;
        }
    }
}