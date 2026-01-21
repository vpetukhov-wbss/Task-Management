using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Modules.Tasks.Features.Analytics.GetTaskAnalytics;

public class GetTaskAnalyticsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/analytics/tasks", Handle)
            .RequireAuthorization()
            .WithTags("Analytics");
    }

    private static async Task<IResult> Handle(
        [FromServices] GetTaskAnalyticsHandler handler,
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

