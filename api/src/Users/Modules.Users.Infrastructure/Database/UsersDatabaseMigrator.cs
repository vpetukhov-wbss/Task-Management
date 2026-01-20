using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Common.Infrastructure.Database;
using Modules.Users.Domain.Users;

namespace Modules.Users.Infrastructure.Database;

public class UsersDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UsersDatabaseMigrator>>();

        try
        {
            // Check if database can be connected
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                logger.LogWarning("Cannot connect to database. Skipping Users module migration.");
                return;
            }

            // Check if there are pending migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations for Users module", pendingMigrations.Count());
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Users module migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Users module database is up to date");
            }

            // Seed roles and demo users after migration
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleSeederLogger = scope.ServiceProvider.GetRequiredService<ILogger<RoleSeeder>>();
            var roleSeeder = new RoleSeeder(roleManager, userManager, roleSeederLogger);
            await roleSeeder.SeedRolesAsync();
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "SQL error during Users module migration. Database may not be accessible.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during Users module migration");
        }
    }
}
