using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskSystem.API.Data;
using TaskSystem.API.DTOs;
using TaskSystem.API.Models;

namespace TaskSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskSystemContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskSystemContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var taskDtos = tasks.Select(MapToDto);
            return Ok(taskDtos);
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return MapToDto(task);
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(CreateTaskItemDto createDto)
        {
            var task = new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Priority = createDto.Priority,
                DueDate = createDto.DueDate,
                Status = TaskItemStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = MapToDto(task);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskItemDto updateDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Title = updateDto.Title;
            task.Description = updateDto.Description;
            task.Status = updateDto.Status;
            task.Priority = updateDto.Priority;
            task.DueDate = updateDto.DueDate;

            // Set completion time if task is being marked as completed
            if (updateDto.Status == TaskItemStatus.Completed && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else if (updateDto.Status != TaskItemStatus.Completed)
            {
                task.CompletedAt = null;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/tasks/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasksByStatus(TaskItemStatus status)
        {
            var tasks = await _context.Tasks
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var taskDtos = tasks.Select(MapToDto);
            return Ok(taskDtos);
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }

        private static TaskItemDto MapToDto(TaskItem task)
        {
            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt
            };
        }
    }
}