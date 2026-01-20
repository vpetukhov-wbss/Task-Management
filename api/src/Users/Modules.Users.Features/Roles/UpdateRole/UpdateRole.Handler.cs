using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Features.Roles.Shared;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Roles.UpdateRole;

internal interface IUpdateRoleHandler : IHandler
{
    Task<Result<RoleResponse>> HandleAsync(string roleId, UpdateRoleRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateRoleHandler(
    RoleManager<Domain.Users.Role> roleManager,
    UsersDbContext context,
    ILogger<UpdateRoleHandler> logger)
    : IUpdateRoleHandler
{
    public async Task<Result<RoleResponse>> HandleAsync(
        string roleId,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role is null)
        {
            logger.LogInformation("Role with ID {RoleId} not found", roleId);
            return RoleErrors.NotFound(roleId);
        }

        role.Name = request.Name;
        role.NormalizedName = request.Name.ToUpperInvariant();

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to update role: {@Errors}", result.Errors);
            return RoleErrors.UpdateFailed(result.Errors);
        }

        logger.LogInformation("Updated role with ID: {RoleId}", roleId);

        var userCount = context.UserRoles.Count(ur => ur.RoleId == roleId);

        return new RoleResponse(role.Id, role.Name!, userCount);
    }
}

