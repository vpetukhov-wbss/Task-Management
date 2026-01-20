namespace Modules.Users.Infrastructure.Authorization;

/// <summary>
/// Constants for authorization policies
/// </summary>
public static class PolicyConstants
{
    /// <summary>
    /// Policy for creating boards - Only Admin
    /// </summary>
    public const string CanCreateBoard = "CanCreateBoard";

    /// <summary>
    /// Policy for managing boards - Admin and Manager
    /// </summary>
    public const string CanManageBoard = "CanManageBoard";

    /// <summary>
    /// Policy for creating tasks - Admin and Manager
    /// </summary>
    public const string CanCreateTask = "CanCreateTask";

    /// <summary>
    /// Policy for managing tasks - Admin and Manager
    /// </summary>
    public const string CanManageTask = "CanManageTask";

    /// <summary>
    /// Policy for read-only access - All authenticated users
    /// </summary>
    public const string CanViewContent = "CanViewContent";
}

