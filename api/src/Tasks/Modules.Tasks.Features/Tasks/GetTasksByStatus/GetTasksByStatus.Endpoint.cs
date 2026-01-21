using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Tasks.Shared.Routes;

namespace Modules.Tasks.Features.Tasks.GetTasksByStatus;

public class GetTasksByStatusEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet(RouteConsts.GetByStatus, Handle)
			.RequireAuthorization()
			.WithTags("Tasks");
	}

	private static async Task<IResult> Handle(
		string userId,
		int status,
		IGetTasksByStatusHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(userId, status, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}

