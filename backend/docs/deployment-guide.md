# Deployment Guide

## Platform targets

- Docker
- IIS
- Nginx

## Prerequisites

1. Install SQL Server 2022 and configure a production-ready database.
2. Install Redis and configure a connection string.
3. Provision Azure Blob Storage or local storage for uploaded documents.
4. Configure payment providers, SMS, and email credentials via application settings.

## Steps

1. Build the API image with Docker.
2. Run `docker compose up -d` from the repository root.
3. Apply the SQL Server scripts in the `backend/sql` folder.
4. Set environment variables for JWT secrets, payment keys, SMTP settings, and Twilio credentials.
5. Publish the API to IIS or mount it behind Nginx for reverse proxying.

## Production checklist

- Enable HTTPS and TLS certificates.
- Restrict CORS to the production frontend origin.
- Turn on rate limiting, audit logging, and file upload validation.
- Enable Redis cache and Hangfire for scheduled processing.
