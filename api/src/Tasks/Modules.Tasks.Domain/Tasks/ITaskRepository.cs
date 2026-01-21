using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Tasks;

/// <summary>
/// Repository interface for Task entity operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets a task by its ID
    /// </summary>
    Task<Result<TaskItem>> GetByIdAsync(TaskId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks for a specific user
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by status for a specific user
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetByUserIdAndStatusAsync(
        string userId,
        TaskStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by priority for a specific user
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetByUserIdAndPriorityAsync(
        string userId,
        TaskPriority priority,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks for a specific board
    /// </summary>
    Task<IReadOnlyList<TaskItem>> GetByBoardIdAsync(
        Boards.BoardId boardId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new task
    /// </summary>
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    void Update(TaskItem task);

    /// <summary>
    /// Deletes a task
    /// </summary>
    void Delete(TaskItem task);

    /// <summary>
    /// Checks if a task exists
    /// </summary>
    Task<bool> ExistsAsync(TaskId id, CancellationToken cancellationToken = default);
}

