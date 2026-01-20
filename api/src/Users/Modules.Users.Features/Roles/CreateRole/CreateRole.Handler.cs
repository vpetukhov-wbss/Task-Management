using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Features.Roles.Shared;

namespace Modules.Users.Features.Roles.CreateRole;

internal interface ICreateRoleHandler : IHandler
{
    Task<Result<RoleResponse>> HandleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateRoleHandler(
    RoleManager<Domain.Users.Role> roleManager,
    ILogger<CreateRoleHandler> logger)
    : ICreateRoleHandler
{
    public async Task<Result<RoleResponse>> HandleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        // Check if role already exists
        var existingRole = await roleManager.FindByNameAsync(request.Name);
        if (existingRole is not null)
        {
            logger.LogInformation("Role with name {RoleName} already exists", request.Name);
            return RoleErrors.AlreadyExists(request.Name);
        }

        var role = new Domain.Users.Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            NormalizedName = request.Name.ToUpperInvariant()
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create role: {@Errors}", result.Errors);
            return RoleErrors.CreateFailed(result.Errors);
        }

        logger.LogInformation("Created role with ID: {RoleId}", role.Id);

        return new RoleResponse(role.Id, role.Name!, 0);
    }
}

