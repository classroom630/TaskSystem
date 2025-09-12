using Microsoft.EntityFrameworkCore;
using TaskSystem.API.Models;

namespace TaskSystem.API.Data
{
    public class TaskSystemContext : DbContext
    {
        public TaskSystemContext(DbContextOptions<TaskSystemContext> options) : base(options)
        {
        }
        
        public DbSet<TaskItem> Tasks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.Status).HasConversion<int>();
                entity.Property(t => t.Priority).HasConversion<int>();
                entity.Property(t => t.CreatedAt).IsRequired();
            });
            
            // Seed some sample data
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem 
                { 
                    Id = 1, 
                    Title = "Complete project setup", 
                    Description = "Set up the basic project structure and dependencies",
                    Status = TaskItemStatus.Completed,
                    Priority = TaskPriority.High,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    CompletedAt = DateTime.UtcNow.AddDays(-2)
                },
                new TaskItem 
                { 
                    Id = 2, 
                    Title = "Implement API endpoints", 
                    Description = "Create REST API endpoints for task management",
                    Status = TaskItemStatus.InProgress,
                    Priority = TaskPriority.High,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    DueDate = DateTime.UtcNow.AddDays(2)
                },
                new TaskItem 
                { 
                    Id = 3, 
                    Title = "Design user interface", 
                    Description = "Create a responsive web interface for managing tasks",
                    Status = TaskItemStatus.Pending,
                    Priority = TaskPriority.Medium,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    DueDate = DateTime.UtcNow.AddDays(5)
                }
            );
        }
    }
}