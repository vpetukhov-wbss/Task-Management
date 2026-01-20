using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Roles.DeleteRole;

internal interface IDeleteRoleHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string roleId, CancellationToken cancellationToken);
}

internal sealed class DeleteRoleHandler(
    RoleManager<Domain.Users.Role> roleManager,
    UsersDbContext context,
    ILogger<DeleteRoleHandler> logger)
    : IDeleteRoleHandler
{
    public async Task<Result<Success>> HandleAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            logger.LogInformation("Role with ID {RoleId} not found", roleId);
            return RoleErrors.NotFound(roleId);
        }

        // Check if role has users
        var userCount = context.UserRoles.Count(ur => ur.RoleId == roleId);
        if (userCount > 0)
        {
            logger.LogWarning("Cannot delete role {RoleId} because it has {UserCount} users", roleId, userCount);
            return RoleErrors.CannotDeleteRoleWithUsers(role.Name!, userCount);
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to delete role: {@Errors}", result.Errors);
            return RoleErrors.DeleteFailed(result.Errors);
        }

        logger.LogInformation("Deleted role with ID: {RoleId}", roleId);
        return Result.Success;
    }
}

