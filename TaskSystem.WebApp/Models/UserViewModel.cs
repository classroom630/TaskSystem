using System.ComponentModel.DataAnnotations;

namespace TaskSystem.WebApp.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Last Login")]
        public DateTime? LastLoginAt { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
        
        public List<string> Roles { get; set; } = new();
        
        [Display(Name = "Roles")]
        public string RolesDisplay => string.Join(", ", Roles);
    }
    
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";
    }
    
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}