using TaskSystem.Core.Interfaces;
using TaskSystem.Infrastructure.Data;

namespace TaskSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IUserRepository? _users;
    private ITaskRepository? _tasks;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users
    {
        get
        {
            return _users ??= new UserRepository(_context);
        }
    }

    public ITaskRepository Tasks
    {
        get
        {
            return _tasks ??= new TaskRepository(_context);
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}