using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;

namespace Modules.Tasks.Features.Tasks.GetTasksByPriority;

internal interface IGetTasksByPriorityHandler : IHandler
{
	Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(string userId, int priority, CancellationToken cancellationToken);
}

internal sealed class GetTasksByPriorityHandler(
	ITaskRepository taskRepository,
	ILogger<GetTasksByPriorityHandler> logger)
	: IGetTasksByPriorityHandler
{
	public async Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(
		string userId,
		int priority,
		CancellationToken cancellationToken)
	{
		if (!Enum.IsDefined(typeof(TaskPriority), priority))
		{
			logger.LogWarning("Invalid priority value: {Priority}", priority);
			return Error.Validation("Tasks.InvalidPriority", $"Invalid priority value: {priority}");
		}

		var tasks = await taskRepository.GetByUserIdAndPriorityAsync(
			userId,
			(TaskPriority)priority,
			cancellationToken);

		var response = tasks.Select(t => t.ToResponse()).ToList();

		logger.LogInformation("Retrieved {Count} tasks with priority {Priority} for user {UserId}",
			response.Count, (TaskPriority)priority, userId);

		return response;
	}
}

