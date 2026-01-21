namespace Modules.Tasks.Domain.Boards;

/// <summary>
/// Strongly-typed identifier for Board entity
/// </summary>
public readonly record struct BoardId(Guid Value)
{
    public static BoardId New() => new(Guid.NewGuid());
    
    public static BoardId Empty => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}

