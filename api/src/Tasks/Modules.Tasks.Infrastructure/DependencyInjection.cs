using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Notifications;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Domain.Tasks.Services;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;
using Modules.Tasks.Infrastructure.Notifications;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddRepositories();
        services.AddDomainServices();
        services.AddNotifications();

        return services;
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");

        services.AddDbContext<TasksDbContext>((provider, options) =>
        {
            var interceptor = provider.GetRequiredService<AuditableInterceptor>();

            options
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
                })
                .AddInterceptors(interceptor);
        });

        services.AddScoped<IModuleDatabaseMigrator, TasksDatabaseMigrator>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskDomainService, TaskDomainService>();
    }

    private static void AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
    }
}

