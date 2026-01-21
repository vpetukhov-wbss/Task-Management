using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Features.Boards.Shared;

namespace Modules.Tasks.Features.Boards.GetBoardById;

internal interface IGetBoardByIdHandler : IHandler
{
	Task<Result<BoardResponse>> HandleAsync(Guid boardId, CancellationToken cancellationToken);
}

internal sealed class GetBoardByIdHandler(
	IBoardRepository boardRepository,
	ILogger<GetBoardByIdHandler> logger)
	: IGetBoardByIdHandler
{
	public async Task<Result<BoardResponse>> HandleAsync(
		Guid boardId,
		CancellationToken cancellationToken)
	{
		var boardResult = await boardRepository.GetByIdAsync(new BoardId(boardId), cancellationToken);

		if (boardResult.IsError)
		{
			logger.LogInformation("Board with ID {BoardId} not found", boardId);
			return boardResult.Errors;
		}

		logger.LogInformation("Retrieved board with ID: {BoardId}", boardId);
		return boardResult.Value!.ToResponse();
	}
}

