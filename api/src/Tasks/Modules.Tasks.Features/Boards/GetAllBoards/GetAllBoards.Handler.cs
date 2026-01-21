using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Features.Boards.Shared;

namespace Modules.Tasks.Features.Boards.GetAllBoards;

internal interface IGetAllBoardsHandler : IHandler
{
	Task<Result<IReadOnlyList<BoardResponse>>> HandleAsync(CancellationToken cancellationToken);
}

internal sealed class GetAllBoardsHandler(
	IBoardRepository boardRepository,
	ILogger<GetAllBoardsHandler> logger)
	: IGetAllBoardsHandler
{
	public async Task<Result<IReadOnlyList<BoardResponse>>> HandleAsync(
		CancellationToken cancellationToken)
	{
		var boards = await boardRepository.GetAllAsync(cancellationToken);

		var response = boards.Select(b => b.ToResponse()).ToList();

		logger.LogInformation("Retrieved {Count} boards", response.Count);
		return response;
	}
}

