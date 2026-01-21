using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Tasks.DeleteTask;

internal interface IDeleteTaskHandler : IHandler
{
	Task<Result<Success>> HandleAsync(Guid taskId, CancellationToken cancellationToken);
}

internal sealed class DeleteTaskHandler(
	ITaskRepository taskRepository,
	IUnitOfWork unitOfWork,
	ILogger<DeleteTaskHandler> logger)
	: IDeleteTaskHandler
{
	public async Task<Result<Success>> HandleAsync(
		Guid taskId,
		CancellationToken cancellationToken)
	{
		var taskResult = await taskRepository.GetByIdAsync(new TaskId(taskId), cancellationToken);

		if (taskResult.IsError)
		{
			logger.LogWarning("Task with ID {TaskId} not found", taskId);
			return taskResult.Errors;
		}

		// Entity is already tracked by EF Core from GetByIdAsync, so we can delete it directly
		taskRepository.Delete(taskResult.Value!);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Task {TaskId} deleted successfully", taskId);

		return Result.Success;
	}
}

