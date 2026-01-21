namespace Modules.Tasks.Domain.Tasks;

/// <summary>
/// Represents the status of a task
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is incomplete
    /// </summary>
    Incomplete = 0,

    /// <summary>
    /// Task is complete
    /// </summary>
    Complete = 1,

    /// <summary>
    /// Task is in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Task is on hold
    /// </summary>
    OnHold = 3
}

