using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Boards.Shared.Routes;

namespace Modules.Tasks.Features.Boards.GetBoardsByUser;

public class GetBoardsByUserEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet(RouteConsts.GetByUser, Handle)
			.RequireAuthorization()
			.WithTags("Boards");
	}

	private static async Task<IResult> Handle(
		string userId,
		IGetBoardsByUserHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(userId, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}

