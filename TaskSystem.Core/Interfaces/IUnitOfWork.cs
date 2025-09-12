namespace TaskSystem.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITaskRepository Tasks { get; }
    Task<int> SaveChangesAsync();
}