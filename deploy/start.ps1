# Task Management System - Docker Deployment Script
# This script builds and starts all services

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - Docker Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Building and starting services..." -ForegroundColor Yellow
Write-Host ""

# Build and start services
docker-compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Services started successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access the applications at:" -ForegroundColor Cyan
    Write-Host "  Frontend UI:      http://localhost:3000" -ForegroundColor White
    Write-Host "  Backend API:      http://localhost:5000" -ForegroundColor White
    Write-Host "  Seq Logs:         http://localhost:8081" -ForegroundColor White
    Write-Host "  Jaeger Tracing:   http://localhost:16686" -ForegroundColor White
    Write-Host "  SQL Server:       localhost:1433" -ForegroundColor White
    Write-Host ""
    Write-Host "Credentials:" -ForegroundColor Cyan
    Write-Host "  SQL Server SA:    YourStrong!Passw0rd" -ForegroundColor White
    Write-Host ""
    Write-Host "View logs with: docker-compose logs -f" -ForegroundColor Yellow
    Write-Host "Stop services with: docker-compose down" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "✗ Failed to start services" -ForegroundColor Red
    Write-Host "Check the logs with: docker-compose logs" -ForegroundColor Yellow
    exit 1
}

