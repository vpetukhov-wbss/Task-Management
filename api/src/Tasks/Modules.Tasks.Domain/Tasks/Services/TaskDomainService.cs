using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Tasks.Services;

/// <summary>
/// Implementation of task domain service
/// </summary>
public sealed class TaskDomainService : ITaskDomainService
{
    public Result<bool> CanUserAccessTask(TaskItem task, string userId)
    {
        ArgumentNullException.ThrowIfNull(task);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return TaskErrors.UserIdRequired();
        }

        if (task.UserId != userId)
        {
            return TaskErrors.UnauthorizedAccess();
        }

        return true;
    }

    public Result<bool> CanUserModifyTask(TaskItem task, string userId)
    {
        return CanUserAccessTask(task, userId);
    }

    public Result<bool> CanUserDeleteTask(TaskItem task, string userId)
    {
        return CanUserAccessTask(task, userId);
    }
}

