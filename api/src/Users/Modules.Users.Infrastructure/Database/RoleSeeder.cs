using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Users.Domain.Policies;
using Modules.Users.Domain.Users;

namespace Modules.Users.Infrastructure.Database;

/// <summary>
/// Seeds default roles and demo users into the database
/// </summary>
public sealed class RoleSeeder
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RoleSeeder> _logger;

    public RoleSeeder(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        ILogger<RoleSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedRolesAsync()
    {
        _logger.LogInformation("Starting role seeding...");

        foreach (var roleName in RoleConstants.AllRoles)
        {
            Role? role;
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                role = new Role
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    continue;
                }
            }
            else
            {
                _logger.LogInformation("Role {RoleName} already exists", roleName);
                role = await _roleManager.FindByNameAsync(roleName);
            }

            // Add claims to roles (only if they don't already have them)
            if (role != null)
            {
                await AddRoleClaimsAsync(role, roleName);
            }
        }

        _logger.LogInformation("🌱 Role seeding completed");

        // Seed demo users
        await SeedDemoUsersAsync();
    }

    private async Task AddRoleClaimsAsync(Role role, string roleName)
    {
        var existingClaims = await _roleManager.GetClaimsAsync(role);

        var claimsToAdd = roleName switch
        {
            RoleConstants.Admin => new[]
            {
                new Claim(UserPolicyConsts.ReadPolicy, "true"),
                new Claim(UserPolicyConsts.CreatePolicy, "true"),
                new Claim(UserPolicyConsts.UpdatePolicy, "true"),
                new Claim(UserPolicyConsts.DeletePolicy, "true")
            },
            RoleConstants.Manager => new[]
            {
                new Claim(UserPolicyConsts.ReadPolicy, "true"),
                new Claim(UserPolicyConsts.UpdatePolicy, "true")
            },
            RoleConstants.User => new[]
            {
                new Claim(UserPolicyConsts.ReadPolicy, "true")
            },
            _ => Array.Empty<Claim>()
        };

        var claimsToAddFiltered = claimsToAdd
            .Where(claim => !existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value));

        foreach (var claim in claimsToAddFiltered)
        {
            var result = await _roleManager.AddClaimAsync(role, claim);
            if (result.Succeeded)
            {
                _logger.LogInformation("Added claim {ClaimType} to role {RoleName}",
                    claim.Type, roleName);
            }
            else
            {
                _logger.LogError("Failed to add claim {ClaimType} to role {RoleName}: {Errors}",
                    claim.Type, roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedDemoUsersAsync()
    {
        _logger.LogInformation("Starting demo user seeding...");

        var demoUsers = new[]
        {
            new { Email = "admin@test.com", Password = "Test1234!", Role = RoleConstants.Admin },
            new { Email = "manager@test.com", Password = "Test1234!", Role = RoleConstants.Manager },
            new { Email = "user@test.com", Password = "Test1234!", Role = RoleConstants.User }
        };

        foreach (var demoUser in demoUsers)
        {
            var existingUser = await _userManager.FindByEmailAsync(demoUser.Email);
            if (existingUser == null)
            {
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = demoUser.Email,
                    UserName = demoUser.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, demoUser.Password);
                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, demoUser.Role);
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation("Created demo user: {Email} with role {Role}",
                            demoUser.Email, demoUser.Role);
                    }
                    else
                    {
                        _logger.LogError("Failed to assign role {Role} to user {Email}: {Errors}",
                            demoUser.Role, demoUser.Email,
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogError("Failed to create demo user {Email}: {Errors}",
                        demoUser.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogInformation("Demo user {Email} already exists", demoUser.Email);
            }
        }

        _logger.LogInformation("Demo user seeding completed");
    }
}

