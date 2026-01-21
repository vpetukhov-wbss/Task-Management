using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Boards.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Boards.UpdateBoard;

public sealed record UpdateBoardRequest(
	Guid BoardId,
	string Title);

public class UpdateBoardEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPut(RouteConsts.Update, Handle)
			.RequireAuthorization(PolicyConstants.CanManageBoard) // Admin and Manager
			.WithTags("Boards");
	}

	private static async Task<IResult> Handle(
		Guid boardId,
		[FromBody] UpdateBoardRequest request,
		IValidator<UpdateBoardRequest> validator,
		IUpdateBoardHandler handler,
		CancellationToken cancellationToken)
	{
		if (boardId != request.BoardId)
		{
			return Results.BadRequest("Board ID in URL does not match request body");
		}

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

		return Results.Ok(response.Value);
	}
}

