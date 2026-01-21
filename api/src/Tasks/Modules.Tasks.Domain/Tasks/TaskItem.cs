using Modules.Common.Domain;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks.Events;

namespace Modules.Tasks.Domain.Tasks;

/// <summary>
/// Represents a task in the task management system
/// </summary>
public sealed class TaskItem : IAuditableEntity
{
    private TaskItem()
    {
    }

    private TaskItem(
        TaskId id,
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        string userId,
        BoardId boardId)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        UserId = userId;
        BoardId = boardId;
    }

    public TaskId Id { get; init; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public TaskStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public string UserId { get; init; } = null!;

    public BoardId BoardId { get; init; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public static Result<TaskItem> Create(
        string title,
        string? description,
        TaskPriority priority,
        string userId,
        BoardId boardId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return TaskErrors.TitleRequired();
        }

        if (title.Length > 200)
        {
            return TaskErrors.TitleTooLong();
        }

        if (description?.Length > 1000)
        {
            return TaskErrors.DescriptionTooLong();
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return TaskErrors.UserIdRequired();
        }

        if (boardId == BoardId.Empty)
        {
            return TaskErrors.BoardIdRequired();
        }

        var task = new TaskItem(
            TaskId.New(),
            title,
            description,
            TaskStatus.Incomplete,
            priority,
            userId,
            boardId);

        return task;
    }

    public Result<TaskItem> Update(string title, string? description, TaskPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return TaskErrors.TitleRequired();
        }

        if (title.Length > 200)
        {
            return TaskErrors.TitleTooLong();
        }

        if (description?.Length > 1000)
        {
            return TaskErrors.DescriptionTooLong();
        }

        Title = title;
        Description = description;
        Priority = priority;

        return this;
    }

    public Result<TaskItem> UpdateStatus(TaskStatus status)
    {
        Status = status;
        return this;
    }
}

