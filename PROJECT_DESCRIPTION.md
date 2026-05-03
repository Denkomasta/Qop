# Sqeez Project Description

## Overview

Sqeez is an open-source, self-hosted web application for educational quizzes. It is designed for schools and other learning institutions that want to manage students, classes, subjects, quiz content, media assets, quiz attempts, and achievement rewards on their own infrastructure.

The project combines a small learning-management structure with a gamified quiz experience. Administrators manage the school setup, teachers manage assigned subjects and quizzes, and students complete quizzes, earn XP, and collect badges.

The current implementation consists of an ASP.NET Core backend, a React frontend, a PostgreSQL database, local file storage, and Docker-based deployment.

## Goals

Sqeez focuses on:

- Self-hosting and data ownership for educational institutions.
- Role-based access for students, teachers, and administrators.
- Structured school data: classes, subjects, enrollments, and assigned teachers.
- Quiz authoring with text, media, single-choice, multiple-choice, and free-text questions.
- Student quiz playback with immediate feedback where possible.
- XP and badge rewards based on quiz performance.
- Practical administration tools for users, classes, subjects, badges, imports, and system settings.

## User Roles

Sqeez uses three main user roles. In the backend model, `Teacher` extends `Student`, and `Admin` extends `Teacher`; Entity Framework stores them in a single `Users` table using table-per-hierarchy inheritance.

### Student

Students are the main learners in the system. They can:

- Sign in and manage their profile.
- View their enrolled subjects.
- View quizzes available through those subjects.
- Start and complete quiz attempts.
- View their own attempts and results.
- Earn XP and badges.
- View leaderboards, badges, class pages, and subject pages exposed by the frontend.

### Teacher

Teachers have student capabilities plus teaching tools. They can:

- Manage quizzes for subjects assigned to them.
- Create and edit quiz questions and options.
- Upload and use media assets in quiz content.
- View attempts for their quizzes.
- Grade free-text responses.
- Delete quiz attempts for their own quizzes.
- Access class-management tools when assigned as a class leader.

### Administrator

Administrators have the broadest access. They can:

- Manage users and roles.
- Manage school classes and subject records.
- Assign students to classes and enroll students in subjects.
- Manage global badge definitions and badge rules.
- Upload CSV master import files.
- Change system-level configuration.
- Access administrative views in the frontend.

## Main Functional Areas

### Authentication And Sessions

The backend implements email/password authentication with BCrypt password hashes. Access tokens are JWTs with a 15-minute lifetime. Refresh tokens are stored in the database as `UserSession` records.

The API sends tokens through HTTP-only cookies:

- `sqeez_access_token`
- `sqeez_refresh_token`

The frontend Axios client sends credentials with requests and automatically calls `/api/auth/refresh` after an unauthorized response from non-auth endpoints. The backend also tracks user activity through a `LastSeenMiddleware`, throttling database writes with in-memory caching.

Registration can be opened or closed through system configuration. The initial super-user email is configured by environment variable.

### Academic Structure

The academic model contains:

- `SchoolClass`: a class group with academic year and section.
- `Subject`: a subject assigned optionally to a class and teacher.
- `Enrollment`: a student's membership in a subject.

Students can belong to one school class. Subjects can be linked to a teacher and a school class. Enrollments connect students to subjects and are used to authorize quiz attempts.

### Quiz Authoring

Teachers and administrators can create quizzes under subjects. A quiz contains:

- Title and description.
- Maximum retry count.
- Creation date.
- Optional publish and closing dates.
- Questions.
- Attempts.

Questions support:

- Optional title text.
- Optional media asset.
- Difficulty value.
- Optional penalty behavior.
- Time limit.
- Single-choice or strict multiple-choice behavior.
- Options.

Options support:

- Text.
- Optional media asset.
- Correctness flag.
- Free-text mode.

The same quiz model supports automatically graded choice questions and manually corrected free-text questions.

### Quiz Attempts

Students can start an attempt only when:

- They are enrolled in the quiz subject.
- The quiz exists in that subject.
- The quiz has been published.
- The quiz has not passed its closing date.
- The retry limit has not been exceeded.

During an attempt:

1. The backend creates a `QuizAttempt`.
2. The frontend requests the next pending question.
3. The student submits an answer for the current question.
4. The backend stores a `QuizQuestionResponse`.
5. Choice questions are scored immediately.
6. Free-text questions receive a null score and require teacher correction.
7. The frontend shows question-level feedback and continues.
8. The attempt is completed when there are no more questions.

Completed attempts award XP based on the difference between the current attempt score and the student's previous best completed score for the same quiz. This prevents repeated attempts from repeatedly awarding the same points.

If any response requires manual grading, the attempt enters `PendingCorrection`. After a teacher grades all pending responses, the attempt becomes completed and reward processing runs.

### Gamification

Sqeez includes XP and badge rewards.

Students store their current XP directly on the user record. Badges are defined globally by administrators and can include one or more rules. A badge rule has:

- Metric.
- Operator.
- Target value.

Implemented badge metrics include score percentage and total score. The service also defines metrics for perfect answer count and total attempts, although the current rule evaluator primarily uses score percentage and total score.

When a completed attempt is processed, the badge service checks all badges the student has not earned yet. If all rules for a badge are satisfied, Sqeez awards the badge and adds its XP bonus to the student.

### Media Assets

Sqeez stores files locally rather than relying on external storage.

There are two storage areas:

- Public `wwwroot` files for avatars and badge icons.
- Private `SecureStorage` files for quiz media.

The upload service validates:

- File presence.
- File size based on system configuration.
- Allowed extension.
- File signature using `File.TypeChecker`.
- Path traversal safety when resolving or deleting files.

Supported upload extensions include common image, video, audio, and PDF formats. Avatar and badge uploads are restricted to image formats.

### CSV Import

Administrators can upload a CSV master file through the import endpoint. The import flow can:

- Validate rows.
- Ensure class records exist.
- Create new subjects.
- Create student accounts.
- Assign students to classes.

The expected columns are:

- `Class Name`
- `Academic Year`
- `Subject Name`
- `Subject Code`
- `First Name`
- `Last Name`
- `Email`
- `Password`

Invalid rows are reported in the import result. Existing records are skipped where applicable.

### System Configuration

Sqeez stores global settings in a `SystemConfig` table. Current configuration includes:

- School name.
- Logo URL.
- Support email.
- Default language.
- Current academic year.
- Public registration toggle.
- Email verification requirement.
- Upload size limits.
- Maximum active sessions per user.

The frontend reads this configuration to adapt navigation and user-facing behavior, such as whether public registration is available.

## Backend Architecture

The backend is located in `src/backend/Sqeez.Api`.

It uses:

- ASP.NET Core controllers.
- DTOs for request and response contracts.
- Service classes for business logic.
- Entity Framework Core for persistence.
- PostgreSQL provider through Npgsql.
- BCrypt for password hashing.
- MailKit for email sending.
- CsvHelper for CSV import.
- Scalar/OpenAPI for API reference in development.

The project follows a controller-service-data pattern:

- Controllers receive HTTP requests, extract current user context, and map service results to HTTP responses.
- Services contain business rules and database operations.
- `SqeezDbContext` defines entity sets and relationship mapping.
- DTOs define the API surface used by the frontend and generated TypeScript client.

Important backend areas:

- `Controllers/`
- `Services/`
- `Models/`
- `DTOs/`
- `Data/`
- `Migrations/`
- `Middlewares/`

## Frontend Architecture

The frontend is located in `src/frontend/sqeez`.

It uses:

- React.
- TypeScript.
- Vite.
- TanStack Router for file-based routing.
- TanStack Query for API fetching and caching.
- Orval-generated API hooks from the OpenAPI schema.
- Axios with a custom mutator for cookie credentials and refresh handling.
- Zustand for auth and quiz state.
- i18next for localization.
- Tailwind CSS and reusable UI components.

Important frontend areas:

- `src/routes/`: application routes and pages.
- `src/components/`: shared layout, quiz, settings, icon, and UI components.
- `src/api/`: OpenAPI schema, custom Axios client, and generated API hooks.
- `src/store/`: Zustand stores.
- `src/hooks/`: application hooks such as quiz engine and system config.
- `public/locales/`: translation files.

The authenticated route group checks `/api/auth/me` before allowing access to protected pages. The root layout loads the current user and system configuration, then builds navigation based on role.

## API Surface

The backend exposes REST endpoints grouped by controller:

- `/api/auth`
- `/api/users`
- `/api/classes`
- `/api/subjects`
- `/api/enrollments`
- `/api/quizzes`
- `/api/quiz-attempts`
- `/api/quizzes/{quizId}/statistics`
- `/api/badges`
- `/api/media-assets`
- `/api/import`
- `/api/system-config`

The frontend client is generated with Orval from `src/frontend/sqeez/src/api/api.yaml`.

## Testing

The backend test project is located at `src/backend/Sqeez.Api/Sqeez.Api.Tests`. It contains service tests, controller integration tests, and PostgreSQL integration tests using Testcontainers.

The frontend uses Vitest and Testing Library. Tests cover reusable UI components, quiz components, stores, hooks, and helper functions.

Continuous integration runs backend restore/build/tests, frontend install/lint/tests/build, PostgreSQL integration tests, and EF migration script generation.

## Deployment

Deployment is designed around Docker and GitHub Actions. The production server is intended to be a small runtime host, not a repository checkout.

The backend Docker image is built from `src/backend/Sqeez.Api/Dockerfile`.
The frontend Docker image is built from `src/frontend/sqeez/Dockerfile` and served by Nginx.

The compose file in `src/docker-compose.yml` runs:

- PostgreSQL.
- Backend API.
- Frontend/Nginx reverse proxy.

On the server, the runtime directory only needs `docker-compose.yml` and `.env`. The CD workflow builds and publishes images to GitHub Container Registry, copies the latest compose file and a temporary EF migration script to the server, applies migrations against the PostgreSQL container, removes the temporary script, and restarts the stack.

## Current Limitations And Notes

- Redis is mentioned in older analysis documents but is not part of the current active implementation.
- The current backend uses BCrypt, not Argon2.
- Free-text quiz answers require manual teacher grading.
- The quiz flow is single-player; live multiplayer/lobby behavior is not implemented.
- Some analysis files and the original roadmap are older than the implementation and should be treated as historical design context rather than current truth.
