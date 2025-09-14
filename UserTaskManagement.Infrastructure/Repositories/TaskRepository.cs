using Microsoft.EntityFrameworkCore;
using UserTaskManagement.Infrastructure.Data;
using TaskEntity = UserTaskManagement.Domain.Entities.Task;

namespace UserTaskManagement.Infrastructure.Repositories
{
    public class TaskRepository : BaseRepository<TaskEntity>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskEntity>> GetTasksByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.User)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskEntity>> GetTasksWithUsersAsync()
        {
            return await _dbSet
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetCompletedTasksCountAsync(int? userId = null)
        {
            var query = _dbSet.Where(t => t.IsCompleted);
            
            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);
            
            return await query.CountAsync();
        }

        public async Task<int> GetTasksCountByUserAsync(int userId)
        {
            return await _dbSet.CountAsync(t => t.UserId == userId);
        }

        public async Task<TaskEntity?> GetTaskWithUserAsync(int id)
        {
            return await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public override async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
}