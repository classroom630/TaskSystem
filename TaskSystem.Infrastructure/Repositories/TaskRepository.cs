using Microsoft.EntityFrameworkCore;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;
using TaskSystem.Infrastructure.Data;

namespace TaskSystem.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public override async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(int userId)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.AssignedToId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByCreatedUserAsync(int userId)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.CreatedById == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskItemStatus status)
    {
        return await _dbSet
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status == status)
            .ToListAsync();
    }
}