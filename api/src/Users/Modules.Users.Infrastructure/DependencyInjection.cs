using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;
using Modules.Users.Domain.Authentication;
using Modules.Users.Domain.Users;
using Modules.Users.Infrastructure.Authorization;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Policies;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDatabase(configuration);

		services.AddScoped<IClientAuthorizationService, ClientAuthorizationService>();

		services.AddSingleton<IPolicyFactory, UsersPolicyFactory>();

		services.AddAuthorizationPolicies();

		return services;
	}

	private static void AddAuthorizationPolicies(this IServiceCollection services)
	{
		services.AddAuthorization(options =>
		{
			// Only Admin can create boards
			options.AddPolicy(PolicyConstants.CanCreateBoard, policy =>
				policy.RequireRole(RoleConstants.Admin));

			// Admin and Manager can manage boards (update, delete)
			options.AddPolicy(PolicyConstants.CanManageBoard, policy =>
				policy.RequireRole(RoleConstants.Admin, RoleConstants.Manager));

			// Admin and Manager can create tasks
			options.AddPolicy(PolicyConstants.CanCreateTask, policy =>
				policy.RequireRole(RoleConstants.Admin, RoleConstants.Manager));

			// Admin and Manager can manage tasks (update, delete, patch)
			options.AddPolicy(PolicyConstants.CanManageTask, policy =>
				policy.RequireRole(RoleConstants.Admin, RoleConstants.Manager));

			// All authenticated users can view content
			options.AddPolicy(PolicyConstants.CanViewContent, policy =>
				policy.RequireAuthenticatedUser());
		});
	}

	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("SqlServer");

		services.AddDbContext<UsersDbContext>((provider, options) =>
		{
			var interceptor = provider.GetRequiredService<AuditableInterceptor>();

			options
				.UseSqlServer(connectionString, sqlServerOptions =>
				{
					sqlServerOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
				})
				.AddInterceptors(interceptor);
		});

		services.AddScoped<IModuleDatabaseMigrator, UsersDatabaseMigrator>();

		services.AddSingleton<AuditableInterceptor>();

		services
			.AddIdentityCore<User>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequiredLength = 8;
			})
			.AddRoles<Role>()
			.AddEntityFrameworkStores<UsersDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();
	}
}
