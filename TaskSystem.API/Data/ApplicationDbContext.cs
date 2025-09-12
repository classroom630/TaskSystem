using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskSystem.API.Models.Domain;

namespace TaskSystem.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            });
            
            // Configure TaskItem
            builder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.Status).HasConversion<string>();
                entity.Property(t => t.Priority).HasConversion<string>();
                
                // Configure relationships
                entity.HasOne(t => t.AssignedToUser)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(t => t.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(t => t.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired();
                
                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Seed roles
            SeedRoles(builder);
        }
        
        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole 
                { 
                    Id = "1", 
                    Name = "Admin", 
                    NormalizedName = "ADMIN" 
                },
                new IdentityRole 
                { 
                    Id = "2", 
                    Name = "Manager", 
                    NormalizedName = "MANAGER" 
                },
                new IdentityRole 
                { 
                    Id = "3", 
                    Name = "User", 
                    NormalizedName = "USER" 
                }
            );
        }
    }
}