# Car Rental Management System

This repository contains a production-oriented baseline for a Car Rental Management System built with:

- React 19 frontend
- ASP.NET Core 8 Web API backend
- SQL Server + EF Core persistence
- Clean Architecture layering
- JWT, Swagger, Serilog, Redis, Hangfire, and Docker deployment support

## Solution Layout

- `backend/src/CarRent.Api` - ASP.NET Core Web API entrypoint
- `backend/src/CarRent.Application` - Use cases, services, DTOs, validators
- `backend/src/CarRent.Domain` - Core entities and business concepts
- `backend/src/CarRent.Infrastructure` - EF Core, caching, external services, persistence
- `backend/tests/CarRent.UnitTests` - Automated unit tests
- `frontend` - React 19 + Material UI application

## Notes

The initial scaffold is production-minded and ready for extension with authentication, booking logic, payment integration, alerts, and deployments.

## Database connection

The API always connects via Npgsql (PostgreSQL/Supabase), regardless of environment. Set the connection string via the `SUPABASE_DB_CONNECTION` environment variable (or `ConnectionStrings__DefaultConnection`) in Npgsql keyword format — not a `postgresql://` URI:

```
Host=<project>.supabase.co;Port=5432;Username=postgres;Password=<password>;Database=postgres;SSL Mode=Require;Trust Server Certificate=true
```

Apply `backend/sql/supabase/001_schema.sql` and `002_seed.sql` to the target database before first use. Never commit a real connection string to `appsettings*.json`.
