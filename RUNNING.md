# Running Sqeez

This guide explains how to run Sqeez locally for development and how the production-oriented Docker setup is intended to work.

## Requirements

Install:

- .NET 10 SDK
- Node.js 22
- Yarn
- Docker or another PostgreSQL 18-compatible local database
- EF Core CLI tool, if you want to run migrations from the terminal

Install the EF Core CLI if needed:

```powershell
dotnet tool install --global dotnet-ef
```

## Local Development

The recommended local setup is:

- PostgreSQL in Docker or local PostgreSQL.
- Backend from source using the HTTPS launch profile.
- Frontend from source using Vite.

### 1. Start PostgreSQL

Example with Docker:

```powershell
docker run --name sqeez-postgres `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=TodoSecurePassword `
  -e POSTGRES_DB=SqeezDb `
  -p 5432:5432 `
  -d postgres:18
```

If the container already exists, start it:

```powershell
docker start sqeez-postgres
```

### 2. Configure Backend Environment

Create `src/backend/Sqeez.Api/.env` from `src/backend/Sqeez.Api/.env.example`.

For local development, the important values are:

```dotenv
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=SqeezDb;Username=postgres;Password=TodoSecurePassword
TokenKey=This_Is_My_Super_Secret_Key_That_Must_Be_At_Least_64_Characters_Long_123456789!
FrontendUrl=http://localhost:3000

SUPER_USER_EMAIL=test@example.com
SUPER_USER_DEFAULT_PASSWORD=YourSuperSecretPassword123!
```

Email verification and password reset use SMTP settings from the same file. For development, use a sandbox SMTP service or set system config so email verification is not required.

### 3. Apply Database Migrations

```powershell
cd src/backend/Sqeez.Api
dotnet restore
dotnet ef database update
```

### 4. Optional Seed Data

The backend includes a seed mode. It only inserts data when no users exist yet.

```powershell
cd src/backend/Sqeez.Api
dotnet run -- seed
```

Seed data includes an admin, teachers, students, classes, subjects, enrollments, sample quizzes, sample media, and badges.

### 5. Run Backend

Use the HTTPS launch profile:

```powershell
cd src/backend/Sqeez.Api
dotnet run --launch-profile https
```

Backend URLs:

- `https://localhost:5001`
- `http://localhost:5000`

Use `https://localhost:5001` for browser auth testing. The auth cookies are configured as `Secure`, so they are not set correctly over plain HTTP.

Development API docs:

- OpenAPI: `https://localhost:5001/openapi/v1.json`
- Scalar: `https://localhost:5001/scalar/v1`

Your browser may ask you to trust the local ASP.NET development certificate. If needed, run:

```powershell
dotnet dev-certs https --trust
```

### 6. Configure Frontend Environment

Create `src/frontend/sqeez/.env` from `src/frontend/sqeez/.env.example`.

Recommended local values:

```dotenv
VITE_API_BASE_URL=https://localhost:5001
VITE_PORT=3000
```

### 7. Run Frontend

```powershell
cd src/frontend/sqeez
yarn install
yarn dev
```

Open:

```text
http://localhost:3000
```

## Login Notes

Public registration is controlled by system configuration. If public registration is disabled, create users through seed data, CSV import, or the admin interface.

The seeded super admin email and password come from:

```dotenv
SUPER_USER_EMAIL
SUPER_USER_DEFAULT_PASSWORD
```

The default seed password for other seeded users is currently defined in the backend seeder.

## Frontend API Client Generation

The frontend uses generated React Query hooks from the OpenAPI schema.

Generate the client:

```powershell
cd src/frontend/sqeez
yarn api:gen
```

The generator reads:

```text
src/frontend/sqeez/src/api/api.yaml
```

and writes generated files under:

```text
src/frontend/sqeez/src/api/generated
```

## Tests

Backend tests:

```powershell
cd src/backend/Sqeez.Api
dotnet test Sqeez.Api.Tests/Sqeez.Api.Tests.csproj
```

PostgreSQL integration tests require Docker and the `SQEEZ_RUN_POSTGRES_TESTS` environment variable:

```powershell
cd src/backend/Sqeez.Api
$env:SQEEZ_RUN_POSTGRES_TESTS="true"
dotnet test Sqeez.Api.Tests/Sqeez.Api.Tests.csproj --filter FullyQualifiedName~Postgres
```

Frontend tests:

```powershell
cd src/frontend/sqeez
yarn test --run
```

Frontend coverage:

```powershell
cd src/frontend/sqeez
yarn test:coverage
```

Frontend production build:

```powershell
cd src/frontend/sqeez
yarn build
```

Backend production build:

```powershell
cd src/backend/Sqeez.Api
dotnet publish -c Release
```

## Docker Compose

The compose file at `src/docker-compose.yml` is production-oriented. It expects prebuilt backend and frontend images from GitHub Container Registry:

- `ghcr.io/${GHCR_OWNER}/sqeez-backend:latest`
- `ghcr.io/${GHCR_OWNER}/sqeez-frontend:latest`

It runs:

- PostgreSQL.
- Backend API.
- Frontend Nginx container.

For a production server, the repository does not need to be cloned. The runtime directory only needs:

```text
+-- docker-compose.yml
+-- .env
```

The current CD workflow uses `/root/Sqeez` as that runtime directory. It copies the latest `docker-compose.yml` from the repository during deployment, while `.env` remains a server-local secret file.

For manual setup, copy `src/docker-compose.yml` to the server runtime directory and create `.env` next to it. Example:

```dotenv
GHCR_OWNER=your-github-owner

POSTGRES_USER=postgres
POSTGRES_PASSWORD=TodoSecurePassword
POSTGRES_DB=SqeezDb

ConnectionStrings__DefaultConnection=Host=sqeez-postgres;Port=5432;Database=SqeezDb;Username=postgres;Password=TodoSecurePassword
TokenKey=This_Is_My_Super_Secret_Key_That_Must_Be_At_Least_64_Characters_Long_123456789!
FrontendUrl=https://your-domain.example

SUPER_USER_EMAIL=admin@example.com
SUPER_USER_DEFAULT_PASSWORD=ChangeThisPassword123!

SmtpSettings__Server=smtp.example.com
SmtpSettings__Port=587
SmtpSettings__SenderName=Sqeez App
SmtpSettings__SenderEmail=noreply@example.com
SmtpSettings__Username=your_smtp_username
SmtpSettings__Password=your_smtp_password
```

Then run:

```powershell
cd /root/Sqeez
docker compose up -d
```

The included frontend Nginx config is currently domain-oriented and references `sqeez.org` and Let's Encrypt certificate paths. Adjust `src/frontend/sqeez/nginx.conf` or provide matching certificates before using it for another domain.

## Deployment Workflow

The GitHub Actions deployment workflow:

1. Runs after successful CI on `main`.
2. Builds backend and frontend Docker images.
3. Pushes images to GitHub Container Registry.
4. Downloads the generated EF migration script from CI.
5. Ensures `/root/Sqeez` exists on the server.
6. Copies `docker-compose.yml` and the temporary migration script to `/root/Sqeez`.
7. Reads database credentials from the server's `.env`.
8. Pulls the latest container images.
9. Starts PostgreSQL.
10. Applies migrations with `psql`.
11. Removes the temporary migration script.
12. Starts the full stack with Docker Compose.

Manual database seeding is available through the `Manual Database Seed` workflow.
