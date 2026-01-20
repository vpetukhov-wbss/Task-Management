using Microsoft.AspNetCore.Identity;
using Modules.Common.Domain.Results;

namespace Modules.Users.Domain.Errors;

public static class RoleErrors
{
    private const string ErrorPrefix = "Roles";

    public static Error NotFound(string roleId) =>
        Error.NotFound($"{ErrorPrefix}.{nameof(NotFound)}", $"Role with ID {roleId} not found");

    public static Error AlreadyExists(string roleName) =>
        Error.Conflict($"{ErrorPrefix}.{nameof(AlreadyExists)}", $"Role with name '{roleName}' already exists");

    public static Error CreateFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.Failure($"{ErrorPrefix}.{nameof(CreateFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));

    public static Error UpdateFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.Failure($"{ErrorPrefix}.{nameof(UpdateFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));

    public static Error DeleteFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.Failure($"{ErrorPrefix}.{nameof(DeleteFailed)}", string.Join(", ", identityErrors.Select(e => e.Description)));

    public static Error CannotDeleteRoleWithUsers(string roleName, int userCount) =>
        Error.Conflict($"{ErrorPrefix}.{nameof(CannotDeleteRoleWithUsers)}", 
            $"Cannot delete role '{roleName}' because it has {userCount} user(s) assigned to it");
}

