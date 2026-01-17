@echo off
REM ============================================
REM Task Management System - Start Script
REM ============================================

set "SCRIPT_DIR=%~dp0"
set "DEPLOY_DIR=%SCRIPT_DIR%deploy"

powershell -NoExit -ExecutionPolicy Bypass -Command ^
"Write-Host '============================================' -ForegroundColor Cyan; ^
Write-Host 'Task Management System - Starting Services' -ForegroundColor Cyan; ^
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
if (-not (Test-Path '.env')) { ^
    Write-Host 'WARNING: .env file not found!' -ForegroundColor Yellow; ^
    if (Test-Path '.env.example') { ^
        Write-Host 'Copying .env.example to .env...' -ForegroundColor Yellow; ^
        Copy-Item '.env.example' '.env'; ^
        Write-Host 'Created .env file. Please review and update the values.' -ForegroundColor Green; ^
        Write-Host ''; ^
    } else { ^
        Write-Host 'ERROR: .env.example not found!' -ForegroundColor Red; ^
        Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
        Read-Host; ^
        exit 1; ^
    } ^
}; ^
Write-Host 'Checking Docker...' -ForegroundColor Cyan; ^
try { ^
    $dockerVersion = docker --version 2>&1; ^
    Write-Host 'Docker: ' -NoNewline; ^
    Write-Host $dockerVersion -ForegroundColor Green; ^
} catch { ^
    Write-Host 'ERROR: Docker is not installed or not running!' -ForegroundColor Red; ^
    Write-Host 'Please install Docker Desktop and make sure it is running.' -ForegroundColor Red; ^
    Write-Host ''; ^
    Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
    Read-Host; ^
    exit 1; ^
}; ^
Write-Host ''; ^
Write-Host 'Starting services with docker-compose...' -ForegroundColor Cyan; ^
Write-Host ''; ^
docker-compose up -d --build; ^
if ($LASTEXITCODE -eq 0) { ^
    Write-Host ''; ^
    Write-Host '============================================' -ForegroundColor Green; ^
    Write-Host 'Services started successfully!' -ForegroundColor Green; ^
    Write-Host '============================================' -ForegroundColor Green; ^
    Write-Host ''; ^
    Write-Host 'Access Points:' -ForegroundColor Cyan; ^
    Write-Host '  Frontend:      ' -NoNewline; Write-Host 'http://localhost:3000' -ForegroundColor Yellow; ^
    Write-Host '  API:           ' -NoNewline; Write-Host 'http://localhost:5000' -ForegroundColor Yellow; ^
    Write-Host '  Health Check:  ' -NoNewline; Write-Host 'http://localhost:5000/health' -ForegroundColor Yellow; ^
    Write-Host '  Seq Logs:      ' -NoNewline; Write-Host 'http://localhost:8081' -ForegroundColor Yellow; ^
    Write-Host '  Jaeger Trace:  ' -NoNewline; Write-Host 'http://localhost:16686' -ForegroundColor Yellow; ^
    Write-Host ''; ^
    Write-Host 'Checking service status...' -ForegroundColor Cyan; ^
    Write-Host ''; ^
    docker-compose ps; ^
    Write-Host ''; ^
    Write-Host 'To view logs, run: ' -NoNewline; ^
    Write-Host 'docker-compose logs -f' -ForegroundColor Yellow; ^
    Write-Host 'To stop services, run: ' -NoNewline; ^
    Write-Host 'stop.bat' -ForegroundColor Yellow; ^
} else { ^
    Write-Host ''; ^
    Write-Host 'ERROR: Failed to start services!' -ForegroundColor Red; ^
    Write-Host 'Check the error messages above for details.' -ForegroundColor Red; ^
}; ^
Write-Host ''; ^
Write-Host 'Press Enter to exit...' -ForegroundColor Yellow; ^
Read-Host"

