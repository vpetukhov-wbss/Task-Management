using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modules.Common.Infrastructure.Database;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateModuleDatabasesAsync(this IServiceScope scope,
        CancellationToken cancellationToken = default)
    {
        var migrators = scope.ServiceProvider.GetRequiredService<IEnumerable<IModuleDatabaseMigrator>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IModuleDatabaseMigrator>>();

        foreach (var migrator in migrators)
        {
            try
            {
                await migrator.MigrateAsync(scope, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to migrate database for {MigratorType}. Application will continue but database operations may fail.",
                    migrator.GetType().Name);
            }
        }
    }
}
