# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/Directory.Packages.props ./
COPY src/Directory.Build.props ./
COPY src/TaskManagmentSystem.sln ./

# Copy all project files
COPY src/Common/Modules.Common.Domain/Modules.Common.Domain.csproj ./Common/Modules.Common.Domain/
COPY src/Common/Modules.Common.Application/Modules.Common.Application.csproj ./Common/Modules.Common.Application/
COPY src/Common/Modules.Common.Infrastructure/Modules.Common.Infrastructure.csproj ./Common/Modules.Common.Infrastructure/
COPY src/Common/Modules.Common.API/Modules.Common.API.csproj ./Common/Modules.Common.API/

COPY src/Users/Modules.Users.Domain/Modules.Users.Domain.csproj ./Users/Modules.Users.Domain/
COPY src/Users/Modules.Users.Infrastructure/Modules.Users.Infrastructure.csproj ./Users/Modules.Users.Infrastructure/
COPY src/Users/Modules.Users.Features/Modules.Users.Features.csproj ./Users/Modules.Users.Features/

COPY src/Tasks/Modules.Tasks.Domain/Modules.Tasks.Domain.csproj ./Tasks/Modules.Tasks.Domain/
COPY src/Tasks/Modules.Tasks.Infrastructure/Modules.Tasks.Infrastructure.csproj ./Tasks/Modules.Tasks.Infrastructure/
COPY src/Tasks/Modules.Tasks.Features/Modules.Tasks.Features.csproj ./Tasks/Modules.Tasks.Features/

COPY src/TaskManagmentSystem.Host/TaskManagmentSystem.Host.csproj ./TaskManagmentSystem.Host/

# Restore dependencies
RUN dotnet restore TaskManagmentSystem.Host/TaskManagmentSystem.Host.csproj

# Copy everything else
COPY src/ ./

# Build
WORKDIR /src/TaskManagmentSystem.Host
RUN dotnet build TaskManagmentSystem.Host.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish TaskManagmentSystem.Host.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Entry point
ENTRYPOINT ["dotnet", "TaskManagmentSystem.Host.dll"]
