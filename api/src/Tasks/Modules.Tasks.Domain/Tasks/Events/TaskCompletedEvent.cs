using Modules.Common.Domain.Events;

namespace Modules.Tasks.Domain.Tasks.Events;

/// <summary>
/// Event raised when a task is marked as complete
/// </summary>
public sealed record TaskCompletedEvent(
    TaskId TaskId,
    string UserId,
    DateTimeOffset CompletedAt) : IEvent;

