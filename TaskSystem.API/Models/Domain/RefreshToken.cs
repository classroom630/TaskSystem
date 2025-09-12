using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.API.Models.Domain
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public DateTime ExpiryDate { get; set; }
        
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRevoked { get; set; } = false;
        
        public DateTime? RevokedAt { get; set; }
        
        // Foreign key to ApplicationUser
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
        
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}