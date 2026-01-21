# Task Management System - Stop Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Stopping Task Management System" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

docker-compose down

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Services stopped successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "To remove all data, run: docker-compose down -v" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "✗ Failed to stop services" -ForegroundColor Red
    exit 1
}

