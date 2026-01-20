using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Roles.Shared.Routes;

namespace Modules.Users.Features.Roles.DeleteRole;

public class DeleteRoleEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(RouteConsts.DeleteRole, Handle)
            .RequireAuthorization(UserPolicyConsts.DeletePolicy)
            .WithTags("Roles");
    }

    private static async Task<IResult> Handle(
        string roleId,
        IDeleteRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(roleId, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}

