using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Boards.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Boards.DeleteBoard;

public class DeleteBoardEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapDelete(RouteConsts.Delete, Handle)
			.RequireAuthorization(PolicyConstants.CanManageBoard) // Admin and Manager
			.WithTags("Boards");
	}

	private static async Task<IResult> Handle(
		Guid boardId,
		IDeleteBoardHandler handler,
		CancellationToken cancellationToken)
	{
		var response = await handler.HandleAsync(boardId, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.NoContent();
	}
}

