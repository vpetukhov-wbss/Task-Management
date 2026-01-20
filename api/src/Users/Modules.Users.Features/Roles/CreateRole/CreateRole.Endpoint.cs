using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Roles.Shared.Routes;

namespace Modules.Users.Features.Roles.CreateRole;

public sealed record CreateRoleRequest(string Name);

public class CreateRoleEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(RouteConsts.CreateRole, Handle)
            .RequireAuthorization(UserPolicyConsts.CreatePolicy)
            .WithTags("Roles");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateRoleRequest request,
        IValidator<CreateRoleRequest> validator,
        ICreateRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Created($"/api/roles/{response.Value?.Id}", response.Value);
    }
}

