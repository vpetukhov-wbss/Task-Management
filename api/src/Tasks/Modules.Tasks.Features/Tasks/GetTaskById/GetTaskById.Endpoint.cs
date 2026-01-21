using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Tasks.Shared.Routes;

namespace Modules.Tasks.Features.Tasks.GetTaskById;

public class GetTaskByIdEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet(RouteConsts.GetById, Handle)
			.RequireAuthorization()
			.WithTags("Tasks");
	}

	private static async Task<IResult> Handle(
		Guid taskId,
		IGetTaskByIdHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(taskId, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}

