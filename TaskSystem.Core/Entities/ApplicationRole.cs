using Microsoft.AspNetCore.Identity;

namespace TaskSystem.Core.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] AllRoles = { Admin, Manager, User };
}