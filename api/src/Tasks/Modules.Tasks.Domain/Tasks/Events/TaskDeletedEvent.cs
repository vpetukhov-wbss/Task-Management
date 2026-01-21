using Modules.Common.Domain.Events;

namespace Modules.Tasks.Domain.Tasks.Events;

/// <summary>
/// Event raised when a task is deleted
/// </summary>
public sealed record TaskDeletedEvent(
    TaskId TaskId,
    string UserId,
    DateTimeOffset DeletedAt) : IEvent;

