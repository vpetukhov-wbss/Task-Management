using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Features.Boards.Shared;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Boards.CreateBoard;

internal interface ICreateBoardHandler : IHandler
{
	Task<Result<BoardResponse>> HandleAsync(CreateBoardRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateBoardHandler(
	IBoardRepository boardRepository,
	IUnitOfWork unitOfWork,
	ILogger<CreateBoardHandler> logger)
	: ICreateBoardHandler
{
	public async Task<Result<BoardResponse>> HandleAsync(
		CreateBoardRequest request,
		CancellationToken cancellationToken)
	{
		var boardResult = Board.Create(
			request.Title,
			request.UserId);

		if (boardResult.IsError)
		{
			logger.LogWarning("Failed to create board: {Errors}", boardResult.Errors);
			return boardResult.Errors;
		}

		await boardRepository.AddAsync(boardResult.Value!, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Board created successfully with ID: {BoardId}", boardResult.Value!.Id);
		return boardResult.Value.ToResponse();
	}
}

