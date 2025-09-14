using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskEntity = UserTaskManagement.Domain.Entities.Task;

namespace UserTaskManagement.Infrastructure.Data.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
    {
        public void Configure(EntityTypeBuilder<TaskEntity> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.UserId)
                .IsRequired();

            // Relationships
            builder.HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.IsCompleted);
            builder.HasIndex(t => t.DueDate);
        }
    }
}