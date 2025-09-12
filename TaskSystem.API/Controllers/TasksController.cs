using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.API.Data.Services;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tasks (Admin sees all, Manager sees team tasks, User sees own tasks)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] TaskFilterDto? filter = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (filter == null)
            {
                filter = new TaskFilterDto();
            }

            IEnumerable<TaskDto> tasks;

            if (userRoles.Contains("Admin"))
            {
                tasks = await _taskService.GetFilteredTasksAsync(filter, isAdmin: true);
            }
            else if (userRoles.Contains("Manager"))
            {
                tasks = await _taskService.GetTasksForManagerAsync(currentUserId!);
            }
            else
            {
                tasks = await _taskService.GetFilteredTasksAsync(filter, currentUserId);
            }

            return Ok(tasks);
        }

        /// <summary>
        /// Get task by ID (with permission check)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Check if user can access this task
            var canAccess = await _taskService.CanUserAccessTaskAsync(id, currentUserId!, userRoles);
            if (!canAccess)
            {
                return Forbid();
            }

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        /// <summary>
        /// Create new task (User or Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "User,Manager,Admin")]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _taskService.CreateTaskAsync(createTaskDto, currentUserId!);
            
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        /// <summary>
        /// Update task (with permission check)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var task = await _taskService.UpdateTaskAsync(id, updateTaskDto, currentUserId!, userRoles);
            return Ok(task);
        }

        /// <summary>
        /// Delete task (with permission check)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var result = await _taskService.DeleteTaskAsync(id, currentUserId!, userRoles);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Get tasks assigned to current user
        /// </summary>
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tasks = await _taskService.GetTasksByUserIdAsync(currentUserId!);
            return Ok(tasks);
        }
    }
}