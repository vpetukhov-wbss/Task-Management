using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Users;
using Modules.Users.Features.Users.Shared;

namespace Modules.Users.Features.Users.RegisterUser;

internal interface IRegisterUserHandler : IHandler
{
    Task<Result<UserResponse>> HandleAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}

internal sealed class RegisterUserHandler(
    UserManager<User> userManager,
    ILogger<RegisterUserHandler> logger)
    : IRegisterUserHandler
{
    public async Task<Result<UserResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            logger.LogInformation("Failed to register user: {@Errors}", result.Errors);
            return UserErrors.RegistrationFailed(result.Errors);
        }

        // Assign role - default to "User" if not specified
        var roleToAssign = !string.IsNullOrWhiteSpace(request.Role)
            ? request.Role
            : RoleConstants.User;

        var roleResult = await userManager.AddToRoleAsync(user, roleToAssign);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Failed to assign role {Role} to user {UserId}: {@Errors}",
                roleToAssign, user.Id, roleResult.Errors);
        }
        else
        {
            logger.LogInformation("Assigned role {Role} to user {UserId}", roleToAssign, user.Id);
        }

        logger.LogInformation("Created user with ID: {UserId}", user.Id);

        return new UserResponse(user.Id, user.Email!, roleToAssign, user.CreatedAtUtc, user.UpdatedAtUtc);
    }
}
