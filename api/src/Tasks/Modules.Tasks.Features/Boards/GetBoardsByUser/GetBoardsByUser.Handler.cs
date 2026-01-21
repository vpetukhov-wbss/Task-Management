using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Features.Boards.Shared;

namespace Modules.Tasks.Features.Boards.GetBoardsByUser;

internal interface IGetBoardsByUserHandler : IHandler
{
	Task<Result<IReadOnlyList<BoardResponse>>> HandleAsync(string userId, CancellationToken cancellationToken);
}

internal sealed class GetBoardsByUserHandler(
	IBoardRepository boardRepository,
	ILogger<GetBoardsByUserHandler> logger)
	: IGetBoardsByUserHandler
{
	public async Task<Result<IReadOnlyList<BoardResponse>>> HandleAsync(
		string userId,
		CancellationToken cancellationToken)
	{
		var boards = await boardRepository.GetByUserIdAsync(userId, cancellationToken);

		var response = boards.Select(b => b.ToResponse()).ToList();

		logger.LogInformation("Retrieved {Count} boards for user {UserId}", response.Count, userId);
		return response;
	}
}

