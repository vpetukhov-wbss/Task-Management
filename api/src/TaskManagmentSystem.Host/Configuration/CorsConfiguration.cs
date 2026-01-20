namespace TaskManagementSystem.Host.Configuration;

public sealed class CorsConfiguration
{
	public string PolicyName { get; init; } = "AllowFrontend";
	public string[] AllowedOrigins { get; init; } = [];
	public bool AllowAnyMethod { get; init; } = true;
	public bool AllowAnyHeader { get; init; } = true;
	public bool AllowCredentials { get; init; } = true;
}

