using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Tasks.UpdateTask;

internal interface IUpdateTaskHandler : IHandler
{
	Task<Result<TaskResponse>> HandleAsync(UpdateTaskRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateTaskHandler(
	ITaskRepository taskRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateTaskHandler> logger)
	: IUpdateTaskHandler
{
	public async Task<Result<TaskResponse>> HandleAsync(
		UpdateTaskRequest request,
		CancellationToken cancellationToken)
	{
		var taskResult = await taskRepository.GetByIdAsync(new TaskId(request.TaskId), cancellationToken);

		if (taskResult.IsError)
		{
			logger.LogWarning("Task with ID {TaskId} not found", request.TaskId);
			return taskResult.Errors;
		}

		var updateResult = taskResult.Value!.Update(
			request.Title,
			request.Description,
			(TaskPriority)request.Priority);

		if (updateResult.IsError)
		{
			logger.LogWarning("Failed to update task {TaskId}: {Errors}", request.TaskId, updateResult.Errors);
			return updateResult.Errors;
		}

		// Entity is not tracked (AsNoTracking in GetByIdAsync), so we need to explicitly update it
		taskRepository.Update(updateResult.Value!);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Task {TaskId} updated successfully", request.TaskId);

		return updateResult.Value!.ToResponse();
	}
}

