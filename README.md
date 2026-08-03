# Car Rental Management System

This workspace contains a production-oriented monorepo scaffold for a commercial car rental platform built with:

- React 19 + Material UI + Redux Toolkit + React Router
- ASP.NET Core 8 Web API
- SQL Server + Entity Framework Core
- Clean Architecture
- JWT-ready, Swagger, Serilog, Redis, Hangfire, Docker, and deployment guidance

## Solution layout

- `backend/` — enterprise API and persistence layer
- `frontend/` — responsive React dashboard and booking shell
- `docker-compose.yml` — local service orchestration for API, SQL Server, and Redis

## Quick start

### Backend

```powershell
dotnet run --project backend/src/CarRent.Api/CarRent.Api.csproj
```

### Frontend

```powershell
npm install
npm run dev
```

## Verification

The current scaffold was verified with:

- `dotnet test` on the solution
- `npm run build` on the frontend project

