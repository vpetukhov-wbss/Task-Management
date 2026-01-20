namespace Modules.Users.Features.Roles.Shared;

public sealed record RoleResponse(
    string Id,
    string Name,
    int UserCount);

