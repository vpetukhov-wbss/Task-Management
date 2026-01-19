using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Modules.Common.Application;
using Modules.Common.Domain.Events;
using Modules.Common.Infrastructure.Configuration;
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string[] activityModuleNames)
    {
        services.AddMemoryCache();

        services.AddHostOpenTelemetry(activityModuleNames);

        services.AddJwtAuthentication(configuration);
        services.AddClaimsAuthorization();

        services.AddEventPublishing();

        return services;
    }

    private static IServiceCollection AddEventPublishing(this IServiceCollection services)
    {
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddSingleton<AuditableInterceptor>();

        return services;
    }

    private static IServiceCollection AddHostOpenTelemetry(
        this IServiceCollection services,
        params string[] activityModuleNames)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("TaskManagementSystem"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddSource(activityModuleNames);

                tracing.AddOtlpExporter();
            });

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthConfiguration>()
            .Bind(configuration.GetSection(nameof(AuthConfiguration)));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["AuthConfiguration:Issuer"],
            ValidAudience = configuration["AuthConfiguration:Audience"],
#pragma warning disable S6781
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthConfiguration:Key"]  ?? throw new InvalidOperationException("JWT key is not configured")))
#pragma warning restore S6781
        };

        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;

                // Map custom "role" claim to ClaimTypes.Role for authorization
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var roleClaim = identity.FindFirst("role");
                            if (roleClaim != null)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static IServiceCollection AddClaimsAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationConfigureOptions>();
        services.AddAuthorization();

        return services;
    }
}
