using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Boards;

public static class BoardErrors
{
    private const string ErrorPrefix = "Boards";

    public static Error TitleRequired() =>
        Error.Validation($"{ErrorPrefix}.{nameof(TitleRequired)}", "Board title is required");

    public static Error TitleTooLong() =>
        Error.Validation($"{ErrorPrefix}.{nameof(TitleTooLong)}", "Board title cannot exceed 200 characters");

    public static Error UserIdRequired() =>
        Error.Validation($"{ErrorPrefix}.{nameof(UserIdRequired)}", "User ID is required");

    public static Error NotFound() =>
        Error.NotFound($"{ErrorPrefix}.{nameof(NotFound)}", "Board not found");

    public static Error UnauthorizedAccess() =>
        Error.Forbidden($"{ErrorPrefix}.{nameof(UnauthorizedAccess)}", "You do not have permission to access this board");
}

