namespace Modules.Users.Domain.Users;

/// <summary>
/// Constants for application roles
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Admin role - Full access, can create boards
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Manager role - Can create tasks, arrange boards, but cannot create boards
    /// </summary>
    public const string Manager = "Manager";

    /// <summary>
    /// User role - Read-only access
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// All available roles
    /// </summary>
    public static readonly string[] AllRoles = { Admin, Manager, User };
}

