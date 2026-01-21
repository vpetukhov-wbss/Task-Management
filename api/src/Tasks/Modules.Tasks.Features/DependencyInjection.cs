using FluentValidation;
using Microsoft.Extensions.Configuration;
using Modules.Common.Application.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TasksModuleRegistration
{
	public static IServiceCollection AddTasksModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddTasksModuleApi()
			.AddTasksInfrastructure(configuration);
	}

	private static IServiceCollection AddTasksModuleApi(this IServiceCollection services)
	{
		services.RegisterApiEndpointsFromAssemblyContaining(typeof(TasksModuleRegistration));

		services.RegisterHandlersFromAssemblyContaining(typeof(TasksModuleRegistration));

		services.AddValidatorsFromAssembly(typeof(TasksModuleRegistration).Assembly);

		// Register analytics handler
		services.AddScoped<Modules.Tasks.Features.Analytics.GetTaskAnalytics.GetTaskAnalyticsHandler>();

		return services;
	}
}

