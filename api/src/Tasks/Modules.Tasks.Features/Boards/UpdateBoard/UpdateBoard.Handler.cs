using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Features.Boards.Shared;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Boards.UpdateBoard;

internal interface IUpdateBoardHandler : IHandler
{
	Task<Result<BoardResponse>> HandleAsync(UpdateBoardRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateBoardHandler(
	IBoardRepository boardRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateBoardHandler> logger)
	: IUpdateBoardHandler
{
	public async Task<Result<BoardResponse>> HandleAsync(
		UpdateBoardRequest request,
		CancellationToken cancellationToken)
	{
		var boardResult = await boardRepository.GetByIdAsync(new BoardId(request.BoardId), cancellationToken);

		if (boardResult.IsError)
		{
			logger.LogWarning("Board with ID {BoardId} not found", request.BoardId);
			return boardResult.Errors;
		}

		var updateResult = boardResult.Value!.Update(request.Title);

		if (updateResult.IsError)
		{
			logger.LogWarning("Failed to update board {BoardId}: {Errors}", request.BoardId, updateResult.Errors);
			return updateResult.Errors;
		}

		boardRepository.Update(boardResult.Value);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Board {BoardId} updated successfully", request.BoardId);
		return boardResult.Value.ToResponse();
	}
}

