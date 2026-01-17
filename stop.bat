@echo off
REM ============================================
REM Task Management System - Stop Script
REM ============================================

set "SCRIPT_DIR=%~dp0"
set "DEPLOY_DIR=%SCRIPT_DIR%deploy"

powershell -NoExit -ExecutionPolicy Bypass -Command ^
"Write-Host '============================================' -ForegroundColor Cyan; ^
Write-Host 'Task Management System - Stopping Services' -ForegroundColor Cyan; ^
Write-Host '============================================' -ForegroundColor Cyan; ^
Write-Host ''; ^
$deployPath = '%DEPLOY_DIR%'; ^
if (-not (Test-Path $deployPath)) { ^
    Write-Host 'ERROR: deploy folder not found!' -ForegroundColor Red; ^
    Write-Host 'Please run this script from the project root directory.' -ForegroundColor Red; ^
    Write-Host ''; ^
    Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
    Read-Host; ^
    exit 1; ^
}; ^
Set-Location $deployPath; ^
Write-Host 'Current directory: ' -NoNewline; ^
Write-Host (Get-Location) -ForegroundColor Green; ^
Write-Host ''; ^
Write-Host 'Checking running services...' -ForegroundColor Cyan; ^
Write-Host ''; ^
docker-compose ps; ^
Write-Host ''; ^
Write-Host 'Stopping services...' -ForegroundColor Cyan; ^
Write-Host ''; ^
docker-compose down; ^
if ($LASTEXITCODE -eq 0) { ^
    Write-Host ''; ^
    Write-Host '============================================' -ForegroundColor Green; ^
    Write-Host 'Services stopped successfully!' -ForegroundColor Green; ^
    Write-Host '============================================' -ForegroundColor Green; ^
    Write-Host ''; ^
    Write-Host 'All containers have been stopped and removed.' -ForegroundColor Green; ^
    Write-Host 'Data volumes are preserved.' -ForegroundColor Yellow; ^
    Write-Host ''; ^
    Write-Host 'To start services again, run: ' -NoNewline; ^
    Write-Host 'start.bat' -ForegroundColor Yellow; ^
    Write-Host 'To remove all data, run: ' -NoNewline; ^
    Write-Host 'uninstall.bat' -ForegroundColor Yellow; ^
} else { ^
    Write-Host ''; ^
    Write-Host 'ERROR: Failed to stop services!' -ForegroundColor Red; ^
    Write-Host 'Check the error messages above for details.' -ForegroundColor Red; ^
}; ^
Write-Host ''; ^
Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
Read-Host"

