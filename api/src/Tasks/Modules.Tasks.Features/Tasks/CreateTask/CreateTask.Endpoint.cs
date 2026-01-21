using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Tasks.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Tasks.CreateTask;

public sealed record CreateTaskRequest(
	string Title,
	string? Description,
	int Priority,
	string UserId,
	Guid BoardId);

public class CreateTaskEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost(RouteConsts.Create, Handle)
			.RequireAuthorization(PolicyConstants.CanCreateTask) // Admin and Manager
			.WithTags("Tasks");
	}

	private static async Task<IResult> Handle(
		[FromBody] CreateTaskRequest request,
		IValidator<CreateTaskRequest> validator,
		ICreateTaskHandler handler,
		ILogger<CreateTaskEndpoint> logger,
		CancellationToken cancellationToken)
	{
		logger.LogInformation("Received CreateTask request: Title={Title}, Priority={Priority}, UserId={UserId}, BoardId={BoardId}",
			request.Title, request.Priority, request.UserId, request.BoardId);

		var validationResult = await validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			logger.LogWarning("Validation failed: {Errors}", string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var response = await handler.HandleAsync(request, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.Created($"/api/tasks/{response.Value?.Id}", response.Value);
	}
}

