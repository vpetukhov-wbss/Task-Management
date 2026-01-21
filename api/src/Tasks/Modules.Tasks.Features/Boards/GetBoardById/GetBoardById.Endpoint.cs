using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Boards.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Boards.GetBoardById;

public class GetBoardByIdEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapGet(RouteConsts.GetById, Handle)
			.RequireAuthorization(PolicyConstants.CanViewContent) // All authenticated users
			.WithTags("Boards");
	}

	private static async Task<IResult> Handle(
		Guid boardId,
		IGetBoardByIdHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(boardId, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Ok(response.Value);
	}
}

