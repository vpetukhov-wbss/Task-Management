using Modules.Common.Domain.Events;

namespace Modules.Tasks.Domain.Tasks.Events;

/// <summary>
/// Event raised when a new task is created
/// </summary>
public sealed record TaskCreatedEvent(
    TaskId TaskId,
    string Title,
    string? Description,
    TaskPriority Priority,
    string UserId,
    DateTimeOffset CreatedAt) : IEvent;

