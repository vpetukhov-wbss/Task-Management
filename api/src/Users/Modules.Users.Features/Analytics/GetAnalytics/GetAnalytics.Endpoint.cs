using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Users.Domain.Policies;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Modules.Users.Features.Analytics.GetAnalytics;

public class GetAnalyticsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/analytics", Handle)
            .RequireAuthorization(UserPolicyConsts.ReadPolicy)
            .WithTags("Analytics");
    }

    private static async Task<IResult> Handle(
        [FromServices] GetAnalyticsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(cancellationToken);

        if (result.IsError)
        {
            return result.Errors.ToProblem();
        }

        return Results.Ok(result.Value);
    }
}

