# System Description of Qop

- **Version:** 1.1
- **Status:** Analysis Phase
- **Scope:** Core Logic, Data Structure, and Functional Requirements

---

## 1. Project Abstract

**Qop** is an open-source, self-hosted web application designed to support the educational processes. The platform combines the structural hierarchy of a Learning Management System (LMS) with the immediate engagement of a modern gamified quiz application.

It is designed for institutions requiring **full data ownership** (self-hosted), ensuring that student data, quiz content, and media assets remain within the local infrastructure rather than relying on commercial cloud providers.

## 2. Actors & Role Hierarchy

The system utilizes a cumulative permission model where roles inherit capabilities: `Student` $\subseteq$ `Teacher` $\subseteq$ `Admin`.

### 2.1. Administrator (Principal / Tech Admin)

The system architect and central authority responsible for institutional configuration.

- **User Management:** Create, edit, and archive Users (Students, Teachers).
- **Curriculum Structure:** Create Subjects and appoint Teachers
- **Class Management:** Create Class Groups (e.g., "Grade 5B") and assign **Class Leaders**.
- **Subject Assignment:** Map Subjects to Classes and assign **Subject Leaders** for those specific instances.
- **Global Analytics:** View school performance metrics.

### 2.2. Teacher (Leader)

A dual-context role that adapts based on the teacher's assignment:

#### Context A: The Class Leader (Homeroom)

- **Scope:** The specific Class Group (e.g., "Homeroom 101").
- **Responsibilities:**
  - Monitor overall well-being and attendance of the group.
  - View holistic engagement statistics (e.g., "Student X has not participated in any subject this week").

#### Context B: The Subject Leader (Instructor)

- **Scope:** The specific Subject Instance (e.g., "Math for Homeroom 101").
- **Responsibilities:**
  - **Content Management:** Create and manage quizzes for this subject.
  - **Academic Analytics:** View detailed pass rates, question difficulty heatmaps, and individual subject mastery.

### 2.3. Student (Learner)

The end-user focused on consumption and progression.

- **Scope:** Personal profile and enrolled Subject Instances.
- **Capabilities:**
  - Participate in quizzes.
  - Track personal progress.
  - Compete on Leaderboards, earn Badges or Achievements.

---

## 3. Functional Requirements

### 3.1. Institutional Management

- **Granular Access Control:** Enables strict separation of concerns between Admins, Class and Subject Teachers and Students.
- **Subject Integration:** Ability to map a subject (e.g., "Math") to multiple classes with different leaders for each instance.
- **Bulk import:** The system shall allow the Admin to upload a CSV/Excel file to batch-create Student accounts and assign them to Class Groups.

### 3.2. The Quiz Core

- **Rich Media Support:** Questions and Answers must support **Text** and **Images**.
- **Time Limits:** Quizzes may optionally include a countdown timer per attempt or per question.
- **Retry policy:** Student can retake the quiz based on the defined number of retakes (defaultly infinite)
- **Options:** Support for 2 to 6 answer options with one to many correct answers.
- **Teacher:** Must be able to take quizzes, but his results must be excluded from academic metrics but shown in subject leaderboard.
- **Validation:** Support for Single-Choice and Multi-Choice answer validation.
- **Asset Management:** Efficient handling of local media files via Docker volumes.

### 3.3. Gamification & Progression

To drive engagement, the system prioritizes "Game" mechanics over "Grade" mechanics.

- **Immediate feedback:** Student is informed about quiz progress after every question.
- **XP (Experience Points):** Awarded for correct answers to drive leveling. The points are awarded only from the single best attempt.
- **Badges:** Achievement system tracking levels, perfect scores, and speed.
- **Leaderboards:** A three-tier ranking system:
  1.  **Global:** School-wide ranking.
  2.  **Class-level:** Ranking within the specific Class Group.
  3.  **Subject-level:** Ranking within a specific Subject Instance.

### 3.4. Analytics Dashboards

- **Principal View:** Global school performance.
- **Class Leader View:** Aggregate group attendance and engagement.
- **Subject Leader View:** Specific quiz pass rates and question difficulty analysis.

---

## 4. User Interface (UI/UX) Requirements

### 4.1. Mobile-First Design

The student interface must be optimized for mobile devices (smartphones/tablets).

- Touch-friendly targets (large buttons).
- Vertical scrolling layouts.

### 4.2. Visual Themes (Dark Mode)

The application must support a comprehensive **Dark Theme**.

- **Requirement:** The UI must automatically detect system preferences or allow a manual toggle between Light and Dark modes.
- **Accessibility:** High contrast ratios must be maintained in Dark Mode to ensure readability of quiz questions and text.

---

## 5. Non-Functional Requirements

### 5.1. Performance & Scalability

- **Concurrency:** The backend must handle a "Classroom Burst" scenario, where 30-50 users submit answers within the same 1-second window without API latency exceeding 200ms.
- **Database Optimization:** All read-heavy operations (Leaderboards) must utilize Redis caching to prevent PostgreSQL bottlenecks during peak usage.
- **Asset Delivery:** Media files (Images) must be served via optimized static file handlers with proper browser caching headers to minimize bandwidth usage on school networks.

### 5.2. Usability & Accessibility

- **Mobile Responsiveness:** The application must be fully functional on devices with screen widths as small as 320px.
- **Offline Tolerance:** The React frontend must gracefully handle momentary network interruptions without losing the user's current quiz input.

### 5.3. Security & Privacy

- **Data Sovereignty:** The system must not transmit user data to any external third-party services. All assets must be served locally.
- **Authentication:** Passwords must be hashed using industry standards (Argon2).
- **Authorization:** API endpoints must enforce strict Role-Based Access Control at the Middleware level. A Student token must never be able to trigger an Admin or Teacher endpoint.

### 5.4. Reliability & Maintenance

- **Deployment:** The entire system (Backend, Frontend, DB, Cache) must be deployable via a single `docker-compose up` command.
- **Error Handling:** The system must implement global exception handling. API errors should return standardized JSON error codes rather than stack traces.
- **Storage Limits:** The system should allow Admins to configure maximum file upload sizes (e.g., 5MB per image) to prevent disk exhaustion on the host server.

## 6. Technical Stack

- **Backend:** C# / ASP.NET Core (Performance & Type Safety).
- **Frontend:** React.js (Responsive SPA).
- **Database:** PostgreSQL (Relational Data).
- **Caching/Realtime:** Redis (Leaderboards & Live State).
- **Infrastructure:** Docker & Docker Compose (Single-command deployment).

---

## 7. Open Issues

- **Grading Logic and :** Currently grading is **Auto-Graded Only**. Manual review for free-text answers is not in the MVP scope as the free-text is out of scope for now.
- **Multiplayer:** The current scope is **Singleplayer**. Real-time multiplayer "lobbies" can be added in the future.
