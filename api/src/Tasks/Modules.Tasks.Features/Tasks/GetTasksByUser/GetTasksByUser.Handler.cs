using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;

namespace Modules.Tasks.Features.Tasks.GetTasksByUser;

internal interface IGetTasksByUserHandler : IHandler
{
	Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(string userId, CancellationToken cancellationToken);
}

internal sealed class GetTasksByUserHandler(
	ITaskRepository taskRepository,
	ILogger<GetTasksByUserHandler> logger)
	: IGetTasksByUserHandler
{
	public async Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(
		string userId,
		CancellationToken cancellationToken)
	{
		var tasks = await taskRepository.GetByUserIdAsync(userId, cancellationToken);

		var response = tasks.Select(t => t.ToResponse()).ToList();

		logger.LogInformation("Retrieved {Count} tasks for user {UserId}", response.Count, userId);

		return response;
	}
}

