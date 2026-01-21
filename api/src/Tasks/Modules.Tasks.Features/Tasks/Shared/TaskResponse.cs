namespace Modules.Tasks.Features.Tasks.Shared;

public sealed record TaskResponse(
	Guid Id,
	string Title,
	string? Description,
	int Status,
	string StatusName,
	int Priority,
	string PriorityName,
	string UserId,
	Guid BoardId,
	DateTimeOffset CreatedAtUtc,
	DateTimeOffset? UpdatedAtUtc);

