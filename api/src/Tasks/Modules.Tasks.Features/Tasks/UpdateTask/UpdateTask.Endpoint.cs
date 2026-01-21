using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Tasks.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Tasks.UpdateTask;

public sealed record UpdateTaskRequest(
	Guid TaskId,
	string Title,
	string? Description,
	int Priority);

public class UpdateTaskEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPut(RouteConsts.Update, Handle)
			.RequireAuthorization(PolicyConstants.CanManageTask) // Admin and Manager
			.WithTags("Tasks");
	}

	private static async Task<IResult> Handle(
		Guid taskId,
		[FromBody] UpdateTaskRequest request,
		IValidator<UpdateTaskRequest> validator,
		IUpdateTaskHandler handler,
		CancellationToken cancellationToken)
	{
		if (taskId != request.TaskId)
		{
			return Results.BadRequest("Task ID in URL does not match request body");
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

