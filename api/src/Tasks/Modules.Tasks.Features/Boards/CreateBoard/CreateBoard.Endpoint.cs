using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Boards.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Boards.CreateBoard;

public sealed record CreateBoardRequest(
	string Title,
	string UserId);

public class CreateBoardEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost(RouteConsts.Create, Handle)
			.RequireAuthorization(PolicyConstants.CanCreateBoard) // Only Admin
			.WithTags("Boards");
	}

	private static async Task<IResult> Handle(
		[FromBody] CreateBoardRequest request,
		IValidator<CreateBoardRequest> validator,
		ICreateBoardHandler handler,
		CancellationToken cancellationToken)
	{
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

		return Results.Created($"/api/boards/{response.Value?.Id}", response.Value);
	}
}

