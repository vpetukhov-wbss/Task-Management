using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Modules.Common.API.ErrorHandling;
using Serilog;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

[SuppressMessage("Design", "MA0051:Method is too long")]
public static class DependencyInjection
{
    public static IServiceCollection AddCoreWebApiInfrastructure(this IServiceCollection services)
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
	            options.SwaggerDoc("v1", new OpenApiInfo
	            {
		            Title = "Task Management System API",
		            Version = "v1",
		            Description = "API for managing users and tasks"
	            });

	            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	            {
		            In = ParameterLocation.Header,
		            Description = "JWT Authorization header. Enter: {token} (without Bearer)",
		            Name = "Authorization",
		            Type = SecuritySchemeType.Http,
		            BearerFormat = "JWT",
		            Scheme = "Bearer"
	            });

	            options.AddSecurityRequirement(new OpenApiSecurityRequirement
	            {
		            {
			            new OpenApiSecurityScheme
			            {
				            Reference = new OpenApiReference
				            {
					            Type=ReferenceType.SecurityScheme,
					            Id="Bearer"
				            }
			            },
			            Array.Empty<string>()
		            }
	            });

	            options.TagActionsBy(api =>
	            {
		            if (api.ActionDescriptor.EndpointMetadata
			            .OfType<Microsoft.AspNetCore.Http.Metadata.ITagsMetadata>()
			            .FirstOrDefault() is { } tagsMetadata && tagsMetadata.Tags.Any())
		            {
			            return tagsMetadata.Tags.ToList();
		            }
		            return new[] { api.GroupName ?? "Default" };
	            });
	            options.DocInclusionPredicate((_, _) => true);
            });

        services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails();

        services.Configure<JsonOptions>(opt =>
        {
            opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            opt.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        return services;
    }

    public static void AddCoreHostLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration));
    }
}
