# Project Roadmap & TODO

**Project:** Qop (Open Source School Quiz Platform)
**Tech Stack:** C# .NET 8/9, React, PostgreSQL, Redis, Docker

---

## Phase 0: Analysis & Design

_Goal: Define the system architecture and data models before writing code._

- [ ] **System Analysis**
  - [x] Create `analysis/01_system_specification.md`.
  - [x] Create `analysis/02_use_case_diagram.md`.
  - [ ] Create `analysis/03_analytical_class_diagram.md`.
  - [ ] Create `analysis/04_design_class_diagram.md`.
  - [ ] Create `analysis/05_api_contract.md`.
  - [ ] Create `analysis/06_ui_wireframes.md`.

---

## Phase 1: Infrastructure & Foundation

_Goal: Get the "Hello World" stack running with containers._

- [ ] **Repository Setup**
  - [x] Initialize Git repository.
  - [ ] Create standard folder structure (`/src`, `/analysis`, `/docker`).
  - [ ] Create `.gitignore` (Visual Studio + Node + Docker templates).
- [ ] **Docker Orchestration**
  - [ ] Write `docker-compose.yml` (Services: Backend, Frontend, Postgres, Redis).
  - [ ] Configure Docker Volumes for DB persistence (`pgdata`).
  - [ ] Configure Docker Volumes for Local Media Storage (`./qop_media:/app/wwwroot/media`).
- [ ] **Backend Initialization (.NET)**
  - [ ] Create Solution (`sln`) and Web API Project.
  - [ ] Specific `appsettings.json` for Docker environment variables.
  - [ ] Configure Swagger/OpenAPI for API documentation.
- [ ] **Frontend Initialization (React)**
  - [ ] Initialize React project with Vite.
  - [ ] Configure Tailwind CSS.
  - [ ] Setup folder structure (`/components`, `/pages`, `/hooks`, `/services`).

---

## Phase 2: Core Backend & Identity

_Goal: Users can log in, and the database schema exists._

- [ ] **Database Schema (EF Core)**
  - [ ] Define User Entities (`Student`, `Teacher`, `Admin`).
  - [ ] Define School Entities (`SchoolClass`, `Subject`, `SchoolClassSubject`, `Enrollement`).
  - [ ] Define Quiz Entities (`Quiz`, `QuizQuestion`, `QuizOption`, `QuizAttempt`, `QuizResponse`).
  - [ ] Setup Relationships.
  - [ ] Run Initial Migration & Seed Data (Default Admin User).
- [ ] **Authentication & Security**
  - [ ] Implement ASP.NET Identity.
  - [ ] Implement JWT Token generation (Access + Refresh Tokens).
  - [ ] Implement Password Hashing (Argon2 configuration).
  - [ ] Create Authorization Policies (`RequireAdmin`, `RequireClassLeader`, `RequireSubjectLeader`).
- [ ] **Admin Features (API)**
  - [ ] CRUD Endpoints for Users.
  - [ ] CRUD Endpoints for Subjects & Classes.
  - [ ] **Logic:** Endpoint to assign a Teacher as "Class Leader" vs "Subject Leader".

---

## Phase 3: The Quiz Engine (Backend)

_Goal: Teachers can create quizzes with media, and data structures support it._

- [ ] **Media Handling**
  - [ ] Create File validated upload service.
  - [ ] Logic to save files to the Docker Volume path.
- [ ] **Quiz Management API**
  - [ ] CRUD for Quizzes (Title, Description, Time Limit).
  - [ ] CRUD for Questions (Support Text + Image URL).
  - [ ] CRUD for Options (IsCorrect boolean).
  - [ ] **Validation:** Ensure at least 1 correct answer per question.
- [ ] **Quiz Attempt Logic**
  - [ ] EP:
    - [ ] Quiz start
    - [ ] Quiz update
    - [ ] Quiz end (success / cancellation)
  - [ ] **Scoring Engine:** Calculate score based on correct answers.

---

## Phase 4: Student Frontend

_Goal: A student can take a quiz on a 320px phone screen._

- [ ] **UI/UX Foundations**
  - [ ] Implement **Dark Mode** Context & Toggle.
  - [ ] Create mobile-first responsive layout.
  - [ ] Login Page
  - [ ] Logout Page
  - [ ] Error Page
- [ ] **Student Pages**
  - [ ] **Dashboard:** View enrolled Subjects.
  - [ ] **Subject:** View available subject quizes and standings
  - [ ] **Class:** View class standings
  - [ ] **Quiz Player:**
    - [ ] Question with touch-friendly option button
    - [ ] Question feedback
    - [ ] **Offline Handling:** Store answers if request fails.
  - [ ] **Results Screen:** View score, XP gained, and feedback.

---

## Phase 5: Teacher Frontend & Analytics

_Goal: Teachers can manage their classes and see stats._

- [ ] **Class Leader Dashboard**
  - [ ] View list of students in Class Group.
  - [ ] Metric: "Last Active" timestamp for each student.
  - [ ] Metric: Overall Class Engagement.
- [ ] **Subject Leader Dashboard**
  - [ ] **Quiz Builder UI:** Form to manage questions and upload images.
  - [ ] **Stats View:** Table showing Pass/Fail rates per quiz.
  - [ ] **Heatmap:** Visual indication of "Hardest Questions" (e.g., Q3 was failed by 80%).
- [ ] **Admin Dashboard**
  - [ ] User Management Page.
  - [ ] System Setting Page.
  - [ ] System Health Status.

---

## Phase 6: Gamification

_Goal: Make it fun and fast._

- [ ] **Redis Integration**
  - [ ] Setup `StackExchange.Redis` in .NET.
  - [ ] **Leaderboards:** Implement `SortedSet` logic for:
    - [ ] Global XP Ranking.
    - [ ] Class XP Ranking.
  - [ ] **Caching:** Cache Dashboard stats (TTL: 5 mins).
- [ ] **Gamification Logic**
  - [ ] **XP Engine:** Calculate XP based on Score + Time.
  - [ ] **Badges:** Background service or Trigger to award badges:
    - [ ] _Perfect Score Badge_.
  - [ ] **Leveling:** Formula to convert Total XP -> Level.

---

## Phase 7: Optimization & Polish (NFRs)

_Goal: Prepare for production usage._

- [ ] **Performance Testing**
  - [ ] Load Test: Simulate 50 concurrent quiz submissions.
  - [ ] Optimize Database Indexes (Index on `SubjectId`, `ClassId`, `StudentId`).
- [ ] **Error Handling**
  - [ ] Implement Global Exception Handler.
  - [ ] Create "Friendly" Error Pages in React (404, 500).
- [ ] **Documentation**
  - [ ] Update and finalize system documentation.
  - [ ] Finalize `README.md` with deployment instructions.
- [ ] **Final Deployment Check**
  - [ ] Verify `docker-compose up` works.
  - [ ] Verify Data persistence.
