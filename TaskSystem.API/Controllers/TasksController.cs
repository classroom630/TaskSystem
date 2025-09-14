using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserTaskManagement.Application.DTOs.Task;
using UserTaskManagement.Application.Interfaces;
using UserTaskManagement.Domain.Enums;

namespace TaskSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Get tasks (Admin & Manager see all, User sees own)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var tasks = await _taskService.GetTasksAsync(
                    currentUserRole == Role.User ? currentUserId : null, 
                    currentUserRole);

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var task = await _taskService.GetTaskByIdAsync(id, currentUserId, currentUserRole);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                return Ok(task);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You don't have permission to view this task");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create new task
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var task = await _taskService.CreateTaskAsync(createTaskDto, currentUserId, currentUserRole);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update task
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var task = await _taskService.UpdateTaskAsync(id, updateTaskDto, currentUserId, currentUserRole);
                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete task
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var result = await _taskService.DeleteTaskAsync(id, currentUserId, currentUserRole);
                if (!result)
                {
                    return NotFound(new { message = "Task not found" });
                }

                return Ok(new { message = "Task deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Toggle task completion status
        /// </summary>
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleTaskCompletion(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var result = await _taskService.ToggleTaskCompletionAsync(id, currentUserId, currentUserRole);
                if (!result)
                {
                    return NotFound(new { message = "Task not found" });
                }

                return Ok(new { message = "Task completion status toggled successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling task completion {TaskId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get my tasks
        /// </summary>
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tasks = await _taskService.GetTasksAsync(currentUserId, Role.User);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user tasks");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get task statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult> GetTaskStats()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                var userId = currentUserRole == Role.User ? currentUserId : (int?)null;

                var totalTasks = await _taskService.GetTasksCountAsync(userId);
                var completedTasks = await _taskService.GetCompletedTasksCountAsync(userId);
                var pendingTasks = totalTasks - completedTasks;

                return Ok(new
                {
                    totalTasks,
                    completedTasks,
                    pendingTasks,
                    completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task statistics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private Role GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<Role>(roleClaim ?? "User");
        }
    }
}