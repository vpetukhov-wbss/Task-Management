using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.Shared;

namespace Modules.Tasks.Features.Tasks.GetTasksByBoard;

internal interface IGetTasksByBoardHandler : IHandler
{
	Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(Guid boardId, CancellationToken cancellationToken);
}

internal sealed class GetTasksByBoardHandler(
	ITaskRepository taskRepository,
	IBoardRepository boardRepository,
	ILogger<GetTasksByBoardHandler> logger)
	: IGetTasksByBoardHandler
{
	public async Task<Result<IReadOnlyList<TaskResponse>>> HandleAsync(
		Guid boardId,
		CancellationToken cancellationToken)
	{
		// Verify board exists
		var boardExists = await boardRepository.ExistsAsync(new BoardId(boardId), cancellationToken);
		if (!boardExists)
		{
			logger.LogWarning("Board with ID {BoardId} not found", boardId);
			return BoardErrors.NotFound();
		}

		var tasks = await taskRepository.GetByBoardIdAsync(new BoardId(boardId), cancellationToken);

		var response = tasks.Select(t => t.ToResponse()).ToList();

		logger.LogInformation("Retrieved {Count} tasks for board {BoardId}", response.Count, boardId);

		return response;
	}
}

