using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;
using TaskStatus = Modules.Tasks.Domain.Tasks.TaskStatus;

namespace Modules.Tasks.Features.Tasks.GetTasksByStatus;

internal interface IGetTasksByStatusHandler : IHandler
{
	Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(string userId, int status, CancellationToken cancellationToken);
}

internal sealed class GetTasksByStatusHandler(
	ITaskRepository taskRepository,
	ILogger<GetTasksByStatusHandler> logger)
	: IGetTasksByStatusHandler
{
	public async Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(
		string userId,
		int status,
		CancellationToken cancellationToken)
	{
		if (!Enum.IsDefined(typeof(TaskStatus), status))
		{
			logger.LogWarning("Invalid status value: {Status}", status);
			return Error.Validation("Tasks.InvalidStatus", $"Invalid status value: {status}");
		}

		var tasks = await taskRepository.GetByUserIdAndStatusAsync(
			userId,
			(TaskStatus)status,
			cancellationToken);

		var response = tasks.Select(t => t.ToResponse()).ToList();

		logger.LogInformation("Retrieved {Count} tasks with status {Status} for user {UserId}",
			response.Count, (TaskStatus)status, userId);

		return response;
	}
}

