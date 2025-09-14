using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserTaskManagement.Domain.Entities;
using UserTaskManagement.Infrastructure.Data.Configurations;

namespace UserTaskManagement.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.Task> Tasks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new TaskConfiguration());
            builder.ApplyConfiguration(new RefreshTokenConfiguration());

            // Seed roles
            builder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "User", NormalizedName = "USER" },
                new IdentityRole<int> { Id = 2, Name = "Manager", NormalizedName = "MANAGER" },
                new IdentityRole<int> { Id = 3, Name = "Admin", NormalizedName = "ADMIN" }
            );

            // Create default admin user
            var adminUser = new User
            {
                Id = 1,
                FirstName = "System",
                LastName = "Administrator",
                UserName = "admin@tasksystem.com",
                NormalizedUserName = "ADMIN@TASKSYSTEM.COM",
                Email = "admin@tasksystem.com",
                NormalizedEmail = "ADMIN@TASKSYSTEM.COM",
                EmailConfirmed = true,
                Role = Domain.Enums.Role.Admin,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            // Hash password
            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin123!");

            builder.Entity<User>().HasData(adminUser);

            // Assign admin role to admin user
            builder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = 1, RoleId = 3 }
            );
        }
    }
}