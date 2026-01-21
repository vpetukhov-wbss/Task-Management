using Modules.Tasks.Domain.Boards;

namespace Modules.Tasks.Features.Boards.Shared;

internal static class BoardMappingExtensions
{
	public static BoardResponse ToResponse(this Board board)
	{
		return new BoardResponse(
			board.Id.Value,
			board.Title,
			board.UserId,
			board.CreatedAtUtc,
			board.UpdatedAtUtc);
	}
}

