using Microsoft.EntityFrameworkCore;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Repositories
{
    public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
    {
        private readonly ApplicationDbContext _context;
        
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(string userId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Where(t => t.AssignedToUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<TaskItem>> GetTasksForManagerAsync(string managerId)
        {
            // In a real scenario, you would have a team/department structure
            // For now, we'll return all tasks for managers to manage
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<TaskItem>> GetAllTasksWithUsersAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<TaskItem?> GetTaskWithUserDetailsAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }
        
        public async Task<IEnumerable<TaskItem>> GetFilteredTasksAsync(TaskFilterDto filter, string? userId = null, bool isAdmin = false)
        {
            var query = _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .AsQueryable();
                
            // Apply user-based filtering if not admin
            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(t => t.AssignedToUserId == userId);
            }
            
            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);
                
            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);
                
            if (!string.IsNullOrEmpty(filter.AssignedToUserId))
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);
                
            if (filter.DueDateFrom.HasValue)
                query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);
                
            if (filter.DueDateTo.HasValue)
                query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);
            
            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }
    }
}