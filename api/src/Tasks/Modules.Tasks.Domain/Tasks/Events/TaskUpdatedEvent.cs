using Modules.Common.Domain.Events;

namespace Modules.Tasks.Domain.Tasks.Events;

/// <summary>
/// Event raised when a task is updated
/// </summary>
public sealed record TaskUpdatedEvent(
    TaskId TaskId,
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTimeOffset UpdatedAt) : IEvent;

