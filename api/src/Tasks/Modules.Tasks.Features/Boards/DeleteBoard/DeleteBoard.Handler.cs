using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Boards.DeleteBoard;

internal interface IDeleteBoardHandler : IHandler
{
	Task<Result<Success>> HandleAsync(Guid boardId, CancellationToken cancellationToken);
}

internal sealed class DeleteBoardHandler(
	IBoardRepository boardRepository,
	IUnitOfWork unitOfWork,
	ILogger<DeleteBoardHandler> logger)
	: IDeleteBoardHandler
{
	public async Task<Result<Success>> HandleAsync(
		Guid boardId,
		CancellationToken cancellationToken)
	{
		var boardResult = await boardRepository.GetByIdAsync(new BoardId(boardId), cancellationToken);

		if (boardResult.IsError)
		{
			logger.LogWarning("Board with ID {BoardId} not found", boardId);
			return boardResult.Errors;
		}

		boardRepository.Delete(boardResult.Value!);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Board {BoardId} deleted successfully", boardId);
		return Result.Success;
	}
}

