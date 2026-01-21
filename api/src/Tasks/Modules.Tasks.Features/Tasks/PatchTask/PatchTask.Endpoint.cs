using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Tasks.Features.Tasks.Shared.Routes;
using Modules.Users.Infrastructure.Authorization;

namespace Modules.Tasks.Features.Tasks.PatchTask;

public sealed record PatchTaskRequest(
	Guid TaskId,
	string? Title,
	string? Description,
	int? Priority,
	int? Status);

public class PatchTaskEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPatch(RouteConsts.Patch, Handle)
			.RequireAuthorization(PolicyConstants.CanManageTask) // Admin and Manager
			.WithTags("Tasks");
	}

	private static async Task<IResult> Handle(
		Guid taskId,
		[FromBody] PatchTaskRequest request,
		IValidator<PatchTaskRequest> validator,
		IPatchTaskHandler handler,
		ILogger<PatchTaskEndpoint> logger,
		CancellationToken cancellationToken)
	{
		if (taskId != request.TaskId)
		{
			return Results.BadRequest("Task ID in URL does not match request body");
		}

		logger.LogInformation("Patching task {TaskId} - Title: {Title}, Description: {Description}, Priority: {Priority}, Status: {Status}",
			taskId, request.Title != null ? "updated" : "unchanged",
			request.Description != null ? "updated" : "unchanged",
			request.Priority.HasValue ? request.Priority.Value.ToString() : "unchanged",
			request.Status.HasValue ? request.Status.Value.ToString() : "unchanged");

		var validationResult = await validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			logger.LogWarning("Validation failed for task {TaskId}: {Errors}",
				taskId, string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var response = await handler.HandleAsync(request, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		logger.LogInformation("Task {TaskId} patched successfully", taskId);
		return Results.Ok(response.Value);
	}
}

