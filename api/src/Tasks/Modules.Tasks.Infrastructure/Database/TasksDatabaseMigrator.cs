using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Common.Infrastructure.Database;

namespace Modules.Tasks.Infrastructure.Database;

public sealed class TasksDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TasksDatabaseMigrator>>();

        try
        {
            // Check if database can be connected
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                logger.LogWarning("Cannot connect to database. Skipping Tasks module migration.");
                return;
            }

            // Check if there are pending migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations for Tasks module", pendingMigrations.Count());
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Tasks module migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Tasks module database is up to date");
            }
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "SQL error during Tasks module migration. Database may not be accessible.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during Tasks module migration");
        }
    }
}

