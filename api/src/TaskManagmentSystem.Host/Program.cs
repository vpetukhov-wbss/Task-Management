using TaskManagementSystem.Host.Seeding;
using TaskManagementSystem.Host.Configuration;
using Modules.Common.API.Extensions;
using Modules.Common.Infrastructure.Database;

// Health check tags (static to avoid CA1861 warning)
var healthCheckDbTags = new[] { "db", "sql", "sqlserver" };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebHostDependencies();

builder.AddCoreHostLogging();

builder.Services.AddCoreWebApiInfrastructure();

// Add health checks
builder.Services.AddHealthChecks()
	.AddSqlServer(
		builder.Configuration.GetConnectionString("SqlServer") ??
		throw new InvalidOperationException("Connection string 'SqlServer' not found."),
		name: "sqlserver",
		tags: healthCheckDbTags);

var corsConfig = builder.Configuration
	.GetSection(nameof(CorsConfiguration))
	.Get<CorsConfiguration>() ?? new CorsConfiguration();

builder.Services.AddCors(options =>
{
	options.AddPolicy(corsConfig.PolicyName, policy =>
	{
		policy.WithOrigins(corsConfig.AllowedOrigins);

		if (corsConfig.AllowAnyMethod)
		{
			policy.AllowAnyMethod();
		}

		if (corsConfig.AllowAnyHeader)
		{
			policy.AllowAnyHeader();
		}

		if (corsConfig.AllowCredentials)
		{
			policy.AllowCredentials();
		}
	});
});

builder.Services.AddCoreInfrastructure(builder.Configuration,
[
]);

builder.Services
	.AddUsersModule(builder.Configuration)
	.AddTasksModule(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<SeedService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database migration...");
        await scope.MigrateModuleDatabasesAsync();
        logger.LogInformation("Database migration completed");

        logger.LogInformation("Starting user seeding...");
        var userSeedService = scope.ServiceProvider.GetRequiredService<UserSeedService>();
        await userSeedService.SeedUsersAsync();
        logger.LogInformation("User seeding completed");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization. The application will continue but database operations may fail.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsConfig.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseModuleMiddlewares();

app.MapApiEndpoints();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	ResponseWriter = async (context, report) =>
	{
		context.Response.ContentType = "application/json";
		var result = System.Text.Json.JsonSerializer.Serialize(new
		{
			status = report.Status.ToString(),
			checks = report.Entries.Select(e => new
			{
				name = e.Key,
				status = e.Value.Status.ToString(),
				description = e.Value.Description,
				duration = e.Value.Duration.TotalMilliseconds
			}),
			totalDuration = report.TotalDuration.TotalMilliseconds
		});
		await context.Response.WriteAsync(result);
	}
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	Predicate = _ => false
});

await app.RunAsync();
