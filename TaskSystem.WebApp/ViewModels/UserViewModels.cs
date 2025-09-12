using System.ComponentModel.DataAnnotations;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;

namespace TaskSystem.WebApp.ViewModels;

public class UserListViewModel
{
    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
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
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = UserRoles.User;

    public List<string> AvailableRoles { get; set; } = UserRoles.AllRoles.ToList();
}

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string? Role { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public List<string> AvailableRoles { get; set; } = UserRoles.AllRoles.ToList();
}