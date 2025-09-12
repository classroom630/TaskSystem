using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;

namespace TaskSystem.API.Controllers;

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
    /// Get tasks based on user role and permissions
    /// Admin & Manager: see all tasks
    /// User: see only their own tasks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
    {
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        var tasks = await _taskService.GetTasksForUserAsync(currentUserId, userRole);
        return Ok(tasks);
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        // Check if user has permission to view this task
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        if (userRole == UserRoles.User && 
            task.CreatedById != currentUserId && 
            task.AssignedToId != currentUserId)
        {
            return Forbid();
        }

        return Ok(task);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{UserRoles.User},{UserRoles.Manager},{UserRoles.Admin}")]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        var task = await _taskService.CreateTaskAsync(request, currentUserId);
        
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    /// <summary>
    /// Update task
    /// User: can update own tasks or assigned tasks
    /// Manager: can update all tasks
    /// Admin: can update all tasks
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        var task = await _taskService.UpdateTaskAsync(id, request, currentUserId, userRole);
        return Ok(task);
    }

    /// <summary>
    /// Delete task
    /// User: can delete own tasks or assigned tasks
    /// Manager: can delete all tasks  
    /// Admin: can delete all tasks
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        var deleted = await _taskService.DeleteTaskAsync(id, currentUserId, userRole);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Get tasks for a specific user (Admin and Manager only)
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Admin}")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByUser(int userId)
    {
        var tasks = await _taskService.GetTasksByUserAsync(userId);
        return Ok(tasks);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)!.Value;
    }
}