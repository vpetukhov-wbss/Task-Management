# Task Management System - Production Deployment Guide

## 📋 Table of Contents
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Monitoring](#monitoring)
- [Troubleshooting](#troubleshooting)
- [Production Checklist](#production-checklist)

## 🔧 Prerequisites

### Required Software
- **Docker**: Version 20.10 or higher
- **Docker Compose**: Version 2.0 or higher
- **Git**: For cloning the repository

### System Requirements
- **CPU**: Minimum 4 cores (8 cores recommended for production)
- **RAM**: Minimum 8GB (16GB recommended for production)
- **Disk**: Minimum 20GB free space
- **OS**: Linux, macOS, or Windows with WSL2

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone <repository-url>
cd TaskManagmentSystem/deploy
```

### 2. Configure Environment Variables
```bash
# Copy the example environment file
cp .env.example .env

# Edit the .env file with your values
nano .env  # or use your preferred editor

# For Development: Set ASPNETCORE_ENVIRONMENT=Development
# For Production: Set ASPNETCORE_ENVIRONMENT=Production and adjust resource limits
```

### 3. Start the Application
```bash
# Using PowerShell (Windows)
./start.ps1

# Using Bash (Linux/macOS)
docker-compose up -d --build
```

### 4. Verify Deployment
```bash
# Check all services are running
docker-compose ps

# View logs
docker-compose logs -f
```

### 5. Access the Application
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **API Health**: http://localhost:5000/health
- **Seq Logs**: http://localhost:8081
- **Jaeger Tracing**: http://localhost:16686

## ⚙️ Configuration

### Single `.env` File for All Environments

The application uses a **single `.env` file** that controls both development and production settings. Simply change the values in `.env` based on your environment.

#### Key Environment Variables

**Environment Mode**
```env
# Development or Production
ASPNETCORE_ENVIRONMENT=Development  # Change to 'Production' for production
```

**Database Configuration**
```env
MSSQL_SA_PASSWORD=YourStrong!Passw0rd  # MUST CHANGE for production!
MSSQL_PID=Developer                     # Use 'Standard' or 'Enterprise' for production
DB_NAME=TaskManagementSystem
```

**API Configuration**
```env
API_PORT=5000
JWT_SECRET_KEY=<minimum-32-character-secret>  # MUST CHANGE for production!
JWT_ISSUER=TaskManagementSystem
JWT_AUDIENCE=TaskManagementSystem
CORS_ALLOWED_ORIGINS=http://localhost:3000  # Update for production domain
```

**UI Configuration**
```env
UI_PORT=3000
REACT_APP_API_URL=http://localhost:5000/api  # Update for production domain
```

**Resource Limits** (adjust based on environment)
```env
# Development defaults: 2 CPUs, 4GB RAM for DB
# Production example: 4 CPUs, 8GB RAM for DB

DB_CPU_LIMIT=2              # Set to 4 for production
DB_MEMORY_LIMIT=4g          # Set to 8g for production
API_REPLICAS=1              # Set to 2+ for production high availability
```

**Observability**
```env
SEQ_PORT=8081               # Structured logging UI
JAEGER_UI_PORT=16686        # Distributed tracing UI
JAEGER_SPAN_STORAGE_TYPE=memory  # Use 'badger' for production persistence
```

See `.env.example` for all available configuration options.

## 📦 Deployment

### Development Deployment
```bash
# 1. Edit .env and set:
#    ASPNETCORE_ENVIRONMENT=Development
#    MSSQL_PID=Developer
#    API_REPLICAS=1

# 2. Start all services
docker-compose up -d

# 3. View logs
docker-compose logs -f api
```

### Production Deployment
```bash
# 1. Edit .env and set:
#    ASPNETCORE_ENVIRONMENT=Production
#    MSSQL_PID=Standard (or Enterprise)
#    DB_CPU_LIMIT=4
#    DB_MEMORY_LIMIT=8g
#    API_REPLICAS=2
#    JAEGER_SPAN_STORAGE_TYPE=badger
#    Update JWT_SECRET_KEY, CORS_ALLOWED_ORIGINS, etc.

# 2. Build with no cache
docker-compose build --no-cache

# 3. Start services
docker-compose up -d

# 4. Verify health
docker-compose ps
curl http://localhost:5000/health
```

### Update Deployment
```bash
# Pull latest changes
git pull

# Rebuild and restart
docker-compose up -d --build



## 🔍 Troubleshooting

### Common Issues

#### 1. SQL Server Won't Start
**Symptoms**: Container exits immediately or health check fails

**Solutions**:
```bash
# Check password complexity
# Password must be at least 8 characters with uppercase, lowercase, numbers, and symbols

# Check logs
docker-compose logs mssql

# Verify EULA acceptance
# Ensure ACCEPT_EULA=Y in docker-compose.yml

# Check disk space
df -h
```

#### 2. API Can't Connect to Database
**Symptoms**: API logs show connection errors

**Solutions**:
```bash
# Wait for SQL Server to be fully ready (30-60 seconds)
docker-compose logs mssql | grep "SQL Server is now ready"

# Verify connection string
docker-compose exec api env | grep ConnectionStrings

# Test database connectivity
docker-compose exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT 1" -C
```

#### 3. UI Can't Connect to API
**Symptoms**: Frontend shows connection errors

**Solutions**:
```bash
# Verify API is running
curl http://localhost:5000/health

# Check CORS configuration
docker-compose exec api env | grep CORS

# Verify API URL in UI build
docker-compose logs ui | grep REACT_APP_API_URL
```

#### 4. Port Already in Use
**Symptoms**: "port is already allocated" error

**Solutions**:
```bash
# Find process using the port
netstat -ano | findstr :5000  # Windows
lsof -i :5000                 # Linux/macOS

# Change port in .env file
API_PORT=5001

# Restart services
docker-compose down
docker-compose up -d
```

## ✅ Production Checklist

### Security
- [ ] Change SQL Server SA password to a strong, unique password
- [ ] Generate a secure JWT secret key (minimum 32 characters)
- [ ] Update CORS allowed origins to production domains only
- [ ] Enable HTTPS with valid SSL certificates
- [ ] Remove or secure Seq and Jaeger endpoints
- [ ] Review and update security headers in nginx.conf
- [ ] Disable development/debug features
- [ ] Implement rate limiting
- [ ] Set up firewall rules
- [ ] Enable SQL Server encryption

### Configuration
- [ ] Update API URL in UI build arguments
- [ ] Configure proper connection strings
- [ ] Set appropriate resource limits
- [ ] Configure log retention policies
- [ ] Set up environment-specific configurations
- [ ] Review and update CORS settings
- [ ] Configure health check endpoints
- [ ] Set up proper time zones

### Infrastructure
- [ ] Set up automated backups for SQL Server
- [ ] Configure volume backup strategy
- [ ] Implement monitoring and alerting
- [ ] Set up log aggregation
- [ ] Configure auto-restart policies
- [ ] Plan for horizontal scaling
- [ ] Set up load balancer (if needed)
- [ ] Configure CDN for static assets

### Monitoring & Observability
- [ ] Configure Seq retention and alerts
- [ ] Set up Jaeger sampling strategy
- [ ] Implement application performance monitoring
- [ ] Configure uptime monitoring
- [ ] Set up error tracking
- [ ] Create dashboards for key metrics
- [ ] Configure alert notifications
- [ ] Document runbooks for common issues

## 🔄 Maintenance

### Backup Database

```bash
# Create backup
docker-compose exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "BACKUP DATABASE TaskManagementSystem TO DISK='/var/opt/mssql/backup/TaskManagementSystem.bak'" -C

# Copy backup to host
docker cp taskmanagement-mssql:/var/opt/mssql/backup/TaskManagementSystem.bak ./backups/
```

### Restore Database

```bash
# Copy backup to container
docker cp ./backups/TaskManagementSystem.bak taskmanagement-mssql:/var/opt/mssql/backup/

# Restore
docker-compose exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "RESTORE DATABASE TaskManagementSystem FROM DISK='/var/opt/mssql/backup/TaskManagementSystem.bak' WITH REPLACE" -C
```

### Clean Up

```bash
# Stop all services
docker-compose down

# Remove volumes (WARNING: This deletes all data!)
docker-compose down -v

# Remove unused images
docker image prune -a -f
```

## 📄 License

[Your License Here]

