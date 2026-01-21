using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Tasks.Services;

/// <summary>
/// Domain service for task-related business logic
/// </summary>
public interface ITaskDomainService
{
    /// <summary>
    /// Validates if a user can access a specific task
    /// </summary>
    Result<bool> CanUserAccessTask(TaskItem task, string userId);

    /// <summary>
    /// Validates if a user can modify a specific task
    /// </summary>
    Result<bool> CanUserModifyTask(TaskItem task, string userId);

    /// <summary>
    /// Validates if a user can delete a specific task
    /// </summary>
    Result<bool> CanUserDeleteTask(TaskItem task, string userId);
}

