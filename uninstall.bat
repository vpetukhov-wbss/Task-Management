@echo off
REM ============================================
REM Task Management System - Uninstall Script
REM ============================================

set "SCRIPT_DIR=%~dp0"
set "DEPLOY_DIR=%SCRIPT_DIR%deploy"

powershell -NoExit -ExecutionPolicy Bypass -Command ^
"Write-Host '============================================' -ForegroundColor Red; ^
Write-Host 'Task Management System - UNINSTALL' -ForegroundColor Red; ^
Write-Host '============================================' -ForegroundColor Red; ^
Write-Host ''; ^
Write-Host 'WARNING: This will remove ALL data including:' -ForegroundColor Yellow; ^
Write-Host '  - All containers' -ForegroundColor Yellow; ^
Write-Host '  - All volumes (database data, logs, traces)' -ForegroundColor Yellow; ^
Write-Host '  - All networks' -ForegroundColor Yellow; ^
Write-Host '  - All images' -ForegroundColor Yellow; ^
Write-Host ''; ^
Write-Host 'THIS ACTION CANNOT BE UNDONE!' -ForegroundColor Red; ^
Write-Host ''; ^
$confirmation = Read-Host 'Are you sure you want to continue? Type YES to confirm'; ^
if ($confirmation -ne 'YES') { ^
    Write-Host ''; ^
    Write-Host 'Uninstall cancelled.' -ForegroundColor Green; ^
    Write-Host ''; ^
    Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
    Read-Host; ^
    exit 0; ^
}; ^
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
Write-Host 'Step 1: Stopping all services...' -ForegroundColor Cyan; ^
Write-Host ''; ^
docker-compose down; ^
Write-Host ''; ^
Write-Host 'Step 2: Removing all volumes (data will be deleted)...' -ForegroundColor Cyan; ^
Write-Host ''; ^
docker-compose down -v; ^
Write-Host ''; ^
Write-Host 'Step 3: Removing Docker images...' -ForegroundColor Cyan; ^
Write-Host ''; ^
$images = docker images --filter=reference='*taskmanagement*' -q; ^
if ($images) { ^
    docker rmi -f $images; ^
    Write-Host 'Removed TaskManagement images.' -ForegroundColor Green; ^
} else { ^
    Write-Host 'No TaskManagement images found.' -ForegroundColor Yellow; ^
}; ^
Write-Host ''; ^
Write-Host 'Step 4: Cleaning up unused Docker resources...' -ForegroundColor Cyan; ^
Write-Host ''; ^
docker system prune -f; ^
Write-Host ''; ^
if ($LASTEXITCODE -eq 0) { ^
    Write-Host '============================================' -ForegroundColor Green; ^
    Write-Host 'Uninstall completed successfully!' -ForegroundColor Green; ^
    Write-Host '============================================' -ForegroundColor Green; ^
    Write-Host ''; ^
    Write-Host 'All services, data, and images have been removed.' -ForegroundColor Green; ^
    Write-Host ''; ^
    Write-Host 'To reinstall, run: ' -NoNewline; ^
    Write-Host 'start.bat' -ForegroundColor Yellow; ^
} else { ^
    Write-Host ''; ^
    Write-Host 'WARNING: Some errors occurred during uninstall.' -ForegroundColor Yellow; ^
    Write-Host 'Check the messages above for details.' -ForegroundColor Yellow; ^
}; ^
Write-Host ''; ^
Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
Read-Host"

