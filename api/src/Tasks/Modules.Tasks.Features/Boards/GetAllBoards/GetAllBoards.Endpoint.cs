using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Boards.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Boards.GetAllBoards;

public class GetAllBoardsEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet(RouteConsts.GetAll, Handle)
			.RequireAuthorization(PolicyConstants.CanViewContent) // All authenticated users
			.WithTags("Boards");
	}

	private static async Task<IResult> Handle(
		IGetAllBoardsHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}

