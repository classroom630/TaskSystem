using System.ComponentModel.DataAnnotations;
using UserTaskManagement.Domain.Enums;

namespace UserTaskManagement.Application.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        public Role Role { get; set; } = Role.User;
    }
}