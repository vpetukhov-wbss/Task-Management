using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;

namespace Modules.Tasks.Features.Tasks.GetTaskById;

internal interface IGetTaskByIdHandler : IHandler
{
	Task<Result<TaskResponse>> HandleAsync(Guid taskId, CancellationToken cancellationToken);
}

internal sealed class GetTaskByIdHandler(
	ITaskRepository taskRepository,
	ILogger<GetTaskByIdHandler> logger)
	: IGetTaskByIdHandler
{
	public async Task<Result<TaskResponse>> HandleAsync(
		Guid taskId,
		CancellationToken cancellationToken)
	{
		var taskResult = await taskRepository.GetByIdAsync(new TaskId(taskId), cancellationToken);

		if (taskResult.IsError)
		{
			logger.LogInformation("Task with ID {TaskId} not found", taskId);
			return taskResult.Errors;
		}

		logger.LogInformation("Retrieved task with ID: {TaskId}", taskId);
		return taskResult.Value!.ToResponse();
	}
}

