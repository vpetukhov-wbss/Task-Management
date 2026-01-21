namespace Modules.Tasks.Features.Boards.Shared;

public sealed record BoardResponse(
	Guid Id,
	string Title,
	string UserId,
	DateTimeOffset CreatedAtUtc,
	DateTimeOffset? UpdatedAtUtc);

