using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Roles.Shared.Routes;

namespace Modules.Users.Features.Roles.UpdateRole;

public sealed record UpdateRoleRequest(string Name);

public class UpdateRoleEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(RouteConsts.UpdateRole, Handle)
            .RequireAuthorization(UserPolicyConsts.UpdatePolicy)
            .WithTags("Roles");
    }

    private static async Task<IResult> Handle(
        string roleId,
        [FromBody] UpdateRoleRequest request,
        IValidator<UpdateRoleRequest> validator,
        IUpdateRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(roleId, request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}

