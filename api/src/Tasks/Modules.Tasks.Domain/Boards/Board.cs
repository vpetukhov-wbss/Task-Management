using Modules.Common.Domain;
using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Boards;

/// <summary>
/// Represents a board in the task management system
/// </summary>
public sealed class Board : IAuditableEntity
{
    private Board()
    {
    }

    private Board(
        BoardId id,
        string title,
        string userId)
    {
        Id = id;
        Title = title;
        UserId = userId;
    }

    public BoardId Id { get; init; }

    public string Title { get; private set; } = null!;

    public string UserId { get; init; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public static Result<Board> Create(
        string title,
        string userId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return BoardErrors.TitleRequired();
        }

        if (title.Length > 200)
        {
            return BoardErrors.TitleTooLong();
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return BoardErrors.UserIdRequired();
        }

        var board = new Board(
            BoardId.New(),
            title,
            userId);

        return board;
    }

    public Result<Board> Update(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return BoardErrors.TitleRequired();
        }

        if (title.Length > 200)
        {
            return BoardErrors.TitleTooLong();
        }

        Title = title;
        return this;
    }
}

