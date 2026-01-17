# Task Management System

A modern, full-stack task management application built with .NET 9, React, and SQL Server.

## 🚀 Quick Start (Windows)

### Prerequisites
- **Docker Desktop** installed and running
- **Windows 10/11** with PowerShell

### Start the Application
```bash
start.bat
```
This will:
- Check Docker is running
- Create `.env` file if missing
- Build and start all services
- Show access URLs
- Keep the window open until you press Enter

### Stop the Application
```bash
stop.bat
```
This will:
- Stop all running containers
- Remove containers (data is preserved)
- Keep the window open until you press Enter

### Uninstall Everything
```bash
uninstall.bat
```
⚠️ **WARNING**: This will delete ALL data including database, logs, and traces!

This will:
- Ask for confirmation (type `YES`)
- Stop all containers
- Remove all volumes (deletes data)
- Remove all images
- Clean up Docker resources
- Keep the window open until you press Enter

## 🌐 Access Points

After starting with `start.bat`, access:

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | React UI |
| **API** | http://localhost:5000 | .NET Backend |
| **Health Check** | http://localhost:5000/health | API Status |
| **Swagger** | http://localhost:5000/swagger | API Docs (dev only) |
| **Seq** | http://localhost:8081 | Structured Logs |
| **Jaeger** | http://localhost:16686 | Distributed Tracing |

## ⚙️ Configuration

All configuration is in `deploy/.env` file.

### For Development (default)
```env
ASPNETCORE_ENVIRONMENT=Development
MSSQL_PID=Developer
API_REPLICAS=1
```

### For Production
Edit `deploy/.env` and change:
```env
ASPNETCORE_ENVIRONMENT=Production
MSSQL_PID=Standard
DB_CPU_LIMIT=4
DB_MEMORY_LIMIT=8g
API_REPLICAS=2
MSSQL_SA_PASSWORD=<strong-password>
JWT_SECRET_KEY=<32-char-random-key>
CORS_ALLOWED_ORIGINS=https://yourdomain.com
```

See `deploy/.env.example` for all options.


## 🏗️ Architecture

### Backend (.NET 9)
- **Clean Architecture** with Vertical Slice
- **Modular Monolith** design
- **Entity Framework Core** with SQL Server
- **JWT Authentication**
- **Health Checks**
- **OpenTelemetry** tracing
- **Serilog** structured logging

### Frontend (React)
- **React 19** with TypeScript
- **Redux Toolkit** for state management
- **React Router** for navigation
- **Axios** for API calls
- **Trello-like** board interface

### Infrastructure
- **Docker Compose** for orchestration
- **SQL Server 2022** database
- **Seq** for log aggregation
- **Jaeger** for distributed tracing
- **Nginx** for frontend serving

## 🛠️ Development

### View Logs
```bash
cd deploy
docker-compose logs -f
```

### View Specific Service Logs
```bash
cd deploy
docker-compose logs -f api
docker-compose logs -f ui
docker-compose logs -f mssql
```

### Rebuild Services
```bash
cd deploy
docker-compose up -d --build
```

### Check Service Status
```bash
cd deploy
docker-compose ps
```

## 🔧 Troubleshooting

### Services won't start
1. Make sure Docker Desktop is running
2. Check logs: `cd deploy && docker-compose logs`
3. Try rebuilding: `cd deploy && docker-compose up -d --build`

### Port conflicts
Edit `deploy/.env` and change ports:
```env
API_PORT=5001
UI_PORT=3001
```

### Database connection issues
Wait 30-60 seconds for SQL Server to fully start, then check:
```bash
cd deploy
docker-compose logs mssql | findstr "ready"
```

## 📄 License

[Your License Here]

## 🤝 Contributing

[Your Contributing Guidelines Here]

