using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Tasks;

public static class TaskErrors
{
    private const string ErrorPrefix = "Tasks";

    public static Error NotFound(Guid taskId) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(NotFound)}", $"Task with ID {taskId} not found");

    public static Error TitleRequired() =>
        Error.Validation($"{ErrorPrefix}.{nameof(TitleRequired)}", "Task title is required");

    public static Error TitleTooLong() =>
        Error.Validation($"{ErrorPrefix}.{nameof(TitleTooLong)}", "Task title cannot exceed 200 characters");

    public static Error DescriptionTooLong() =>
        Error.Validation($"{ErrorPrefix}.{nameof(DescriptionTooLong)}", "Task description cannot exceed 1000 characters");

    public static Error UserIdRequired() =>
        Error.Validation($"{ErrorPrefix}.{nameof(UserIdRequired)}", "User ID is required");

    public static Error BoardIdRequired() =>
        Error.Validation($"{ErrorPrefix}.{nameof(BoardIdRequired)}", "Board ID is required");

    public static Error UnauthorizedAccess() =>
        Error.Forbidden($"{ErrorPrefix}.{nameof(UnauthorizedAccess)}", "You do not have permission to access this task");
}

