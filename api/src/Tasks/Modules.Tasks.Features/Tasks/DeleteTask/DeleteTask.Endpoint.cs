using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Tasks.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Tasks.DeleteTask;

public class DeleteTaskEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapDelete(RouteConsts.Delete, Handle)
			.RequireAuthorization(PolicyConstants.CanManageTask) // Admin and Manager
			.WithTags("Tasks");
	}

	private static async Task<IResult> Handle(
		Guid taskId,
		IDeleteTaskHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(taskId, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.NoContent();
	}
}

