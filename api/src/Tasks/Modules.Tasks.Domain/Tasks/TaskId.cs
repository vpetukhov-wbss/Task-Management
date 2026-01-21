namespace Modules.Tasks.Domain.Tasks;

/// <summary>
/// Strongly-typed identifier for Task entity
/// </summary>
public readonly record struct TaskId(Guid Value)
{
    public static TaskId New() => new(Guid.NewGuid());
    
    public static TaskId Empty => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}

