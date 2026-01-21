using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Tasks.PatchTask;

internal interface IPatchTaskHandler : IHandler
{
	Task<Result<TaskResponse>> HandleAsync(PatchTaskRequest request, CancellationToken cancellationToken);
}

internal sealed class PatchTaskHandler(
	ITaskRepository taskRepository,
	IUnitOfWork unitOfWork,
	ILogger<PatchTaskHandler> logger)
	: IPatchTaskHandler
{
	public async Task<Result<TaskResponse>> HandleAsync(
		PatchTaskRequest request,
		CancellationToken cancellationToken)
	{
		var taskResult = await taskRepository.GetByIdAsync(new TaskId(request.TaskId), cancellationToken);

		if (taskResult.IsError)
		{
			logger.LogWarning("Task with ID {TaskId} not found", request.TaskId);
			return taskResult.Errors;
		}

		var task = taskResult.Value!;

		// Apply only the fields that were provided (not null)
		var title = request.Title ?? task.Title;
		var description = request.Description ?? task.Description;
		var priority = request.Priority.HasValue ? (TaskPriority)request.Priority.Value : task.Priority;

		var updateResult = task.Update(title, description, priority);

		if (updateResult.IsError)
		{
			logger.LogWarning("Failed to patch task {TaskId}: {Errors}", request.TaskId, updateResult.Errors);
			return updateResult.Errors;
		}

		// Update status if provided
		if (request.Status.HasValue)
		{
			logger.LogInformation("Updating task {TaskId} status from {OldStatus} to {NewStatus}",
				request.TaskId, task.Status, (Domain.Tasks.TaskStatus)request.Status.Value);

			var statusUpdateResult = updateResult.Value!.UpdateStatus((Domain.Tasks.TaskStatus)request.Status.Value);
			if (statusUpdateResult.IsError)
			{
				logger.LogWarning("Failed to update status for task {TaskId}: {Errors}", request.TaskId, statusUpdateResult.Errors);
				return statusUpdateResult.Errors;
			}

			logger.LogInformation("Status updated successfully for task {TaskId}: {NewStatus}",
				request.TaskId, updateResult.Value!.Status);
		}

		taskRepository.Update(updateResult.Value!);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Task {TaskId} patched successfully", request.TaskId);

		return updateResult.Value!.ToResponse();
	}
}

