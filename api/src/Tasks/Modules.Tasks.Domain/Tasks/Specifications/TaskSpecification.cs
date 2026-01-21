using System.Linq.Expressions;

namespace Modules.Tasks.Domain.Tasks.Specifications;

/// <summary>
/// Base specification for task filtering
/// </summary>
public abstract class TaskSpecification
{
    public abstract Expression<Func<TaskItem, bool>> ToExpression();

    public bool IsSatisfiedBy(TaskItem task)
    {
        var predicate = ToExpression().Compile();
        return predicate(task);
    }
}

/// <summary>
/// Specification for filtering tasks by user
/// </summary>
public sealed class TasksByUserSpecification : TaskSpecification
{
    private readonly string _userId;

    public TasksByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<TaskItem, bool>> ToExpression()
    {
        return task => task.UserId == _userId;
    }
}

/// <summary>
/// Specification for filtering tasks by status
/// </summary>
public sealed class TasksByStatusSpecification : TaskSpecification
{
    private readonly TaskStatus _status;

    public TasksByStatusSpecification(TaskStatus status)
    {
        _status = status;
    }

    public override Expression<Func<TaskItem, bool>> ToExpression()
    {
        return task => task.Status == _status;
    }
}

/// <summary>
/// Specification for filtering tasks by priority
/// </summary>
public sealed class TasksByPrioritySpecification : TaskSpecification
{
    private readonly TaskPriority _priority;

    public TasksByPrioritySpecification(TaskPriority priority)
    {
        _priority = priority;
    }

    public override Expression<Func<TaskItem, bool>> ToExpression()
    {
        return task => task.Priority == _priority;
    }
}

/// <summary>
/// Specification for combining multiple specifications with AND logic
/// </summary>
public sealed class AndSpecification : TaskSpecification
{
    private readonly TaskSpecification _left;
    private readonly TaskSpecification _right;

    public AndSpecification(TaskSpecification left, TaskSpecification right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<TaskItem, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(TaskItem));
        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter));

        return Expression.Lambda<Func<TaskItem, bool>>(combined, parameter);
    }
}

/// <summary>
/// Specification for combining multiple specifications with OR logic
/// </summary>
public sealed class OrSpecification : TaskSpecification
{
    private readonly TaskSpecification _left;
    private readonly TaskSpecification _right;

    public OrSpecification(TaskSpecification left, TaskSpecification right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<TaskItem, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(TaskItem));
        var combined = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter));

        return Expression.Lambda<Func<TaskItem, bool>>(combined, parameter);
    }
}

/// <summary>
/// Specification for negating another specification
/// </summary>
public sealed class NotSpecification : TaskSpecification
{
    private readonly TaskSpecification _specification;

    public NotSpecification(TaskSpecification specification)
    {
        _specification = specification;
    }

    public override Expression<Func<TaskItem, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(TaskItem));
        var negated = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<TaskItem, bool>>(negated, parameter);
    }
}

/// <summary>
/// Extension methods for composing specifications
/// </summary>
public static class TaskSpecificationExtensions
{
    public static TaskSpecification And(this TaskSpecification left, TaskSpecification right)
    {
        return new AndSpecification(left, right);
    }

    public static TaskSpecification Or(this TaskSpecification left, TaskSpecification right)
    {
        return new OrSpecification(left, right);
    }

    public static TaskSpecification Not(this TaskSpecification specification)
    {
        return new NotSpecification(specification);
    }
}

