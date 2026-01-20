namespace Modules.Users.Features.Users.Shared;

public sealed record UserResponse(
    string Id,
    string Email,
    string Role,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
