# Sqeez

Sqeez is an open-source, self-hosted educational quiz platform for schools and learning institutions. It provides role-based administration, subject and class management, quiz authoring, student quiz attempts, local media storage, XP rewards, and badge achievements.

The current codebase contains a working ASP.NET Core backend, React frontend, PostgreSQL persistence, generated TypeScript API client, tests, Docker images, and GitHub Actions CI/CD.

## Features

- Role-based users: Student, Teacher, and Admin.
- User authentication with JWT access tokens, refresh-token sessions, and HTTP-only cookies.
- Email verification and password reset support.
- School classes, subjects, and enrollments.
- Admin tools for users, classes, subjects, badges, imports, and system settings.
- Teacher tools for assigned subjects, quiz management, quiz builder, attempts, and manual grading.
- Student quiz player with question transitions, answer recap, media display, and results.
- Quiz questions with text, media, choice answers, strict multiple choice, free-text answers, time limits, difficulty, and optional penalties.
- XP rewards based on improved quiz performance.
- Rule-based badge awarding.
- Local public and private file storage for avatars, badges, and quiz media.
- CSV master import for classes, subjects, and students.
- Frontend localization with English and Czech locale files.

## Tech Stack

### Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL with Npgsql
- BCrypt password hashing
- MailKit email integration
- CsvHelper import processing
- Scalar/OpenAPI API reference

### Frontend

- React 19
- TypeScript
- Vite
- TanStack Router
- TanStack Query
- Orval-generated API hooks
- Axios
- Zustand
- Tailwind CSS
- i18next
- Vitest and Testing Library

### Infrastructure

- Docker
- Docker Compose
- Nginx frontend/reverse proxy container with runtime templating
- GitHub Container Registry
- GitHub Actions CI/CD

## Repository Structure

```text
.
+-- analysis/                         Historical analysis and UML artifacts
+-- src/
|   +-- backend/
|   |   +-- Sqeez.Api/                ASP.NET Core API, EF model, services, tests
|   +-- frontend/
|   |   +-- sqeez/                    React/Vite frontend
|   +-- docker-compose.yml            Production-oriented compose file
+-- scripts/
|   +-- setup-vps.sh                  Fresh VPS bootstrap helper
+-- PROJECT_DESCRIPTION.md            Detailed implementation description
+-- RUNNING.md                        Local and Docker running guide
+-- LICENSE.md                        MIT license
+-- README.md
```

## Documentation

- [Project description](PROJECT_DESCRIPTION.md) explains the implemented system, roles, architecture, core workflows, and limitations.
- [Running guide](RUNNING.md) explains how to configure and run the project locally and with Docker.
- `analysis/` contains earlier analysis artifacts. Some of those files are older than the implementation and should be treated as historical context.

## Quick Start

For full setup details, see [RUNNING.md](RUNNING.md).

Backend:

```powershell
cd src/backend/Sqeez.Api
dotnet restore
dotnet ef database update
dotnet run --launch-profile https
```

Frontend:

```powershell
cd src/frontend/sqeez
yarn install
yarn dev
```

Default local URLs:

- Frontend: `http://localhost:3000`
- Backend HTTP: `http://localhost:5000`
- Backend HTTPS: `https://localhost:5001`
- OpenAPI document: `https://localhost:5001/openapi/v1.json`
- Scalar API reference: `https://localhost:5001/scalar/v1`

Use the HTTPS backend profile for normal browser authentication testing because auth cookies are configured as `Secure`.

## Testing

Backend:

```powershell
cd src/backend/Sqeez.Api
dotnet test Sqeez.Api.Tests/Sqeez.Api.Tests.csproj
```

Frontend:

```powershell
cd src/frontend/sqeez
yarn test --run
```

Frontend coverage:

```powershell
cd src/frontend/sqeez
yarn test:coverage
```

## API Client Generation

The frontend API client is generated from `src/frontend/sqeez/src/api/api.yaml` using Orval.

```powershell
cd src/frontend/sqeez
yarn api:gen
```

## License

Sqeez is released under the MIT License. See [LICENSE.md](LICENSE.md).
