# Qop

> **An open-source, self-hosted educational assessment platform.**

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Self Hosted](https://img.shields.io/badge/deployment-self--hosted-success) ![Status](https://img.shields.io/badge/status-development-orange)

## About Qop

**Qop** is a robust web application designed for educational institutions that require full data ownership and a distraction-free environment for student assessment. Unlike commercial cloud-based alternatives, Qop is built to be easily self-hosted, ensuring that student data, quiz content, and media assets remain within the school's local infrastructure.

The platform combines the structural hierarchy of a Learning Management System (LMS) with the immediate engagement of a modern gamified quiz application.

## Key Features

### Institutional Management

- **Granular Hierarchy:** A role-based system supporting Admins, Class Leaders, Subject Leaders, and Students.
- **Class Grouping:** Students are organized into Classes (e.g., "Grade 5B"), each with a dedicated leader.
- **Subject Integration:** Subjects (e.g., "Biology") are mapped to classes, allowing specific teachers to manage specific curricula.

### The Assessment Engine

- **Rich Media Support:** Questions and answers can be defined using Text or Images.
- **Flexible Logic:** Support for 2-6 answer options with single or multi-choice validation.
- **Asset Management:** Localized handling of heavy media files via self-hosted storage.

### Gamification & Progression

- **Badges:** Achievement system for streaks, perfect scores, and speed.
- **Leaderboards:** Two-tier ranking system: Global (School), Subject-level and Class-level.

### Analytics Dashboards

- **Principal View:** Global school performance and user management.
- **Class Leader View:** Holistic view of a class group's attendance and overall engagement.
- **Subject Leader View:** detailed breakdown of quiz pass rates and question difficulty per subject.

---

## User Roles & Workflow

Qop utilizes an user inheritance model (`Student` ⊂ `Teacher` ⊂ `Admin`).

### 1. Admin (Principal / Tech Admin)

The system architect.

- Creates Students, Teachers, and Subjects.
- Assigns **Class Leaders** to groups.
- Assigns **Subject Leaders** to specific subjects within a group.

### 2. Teacher (Leader)

A dual-context role:

- **As Class Leader:** Monitors the overall well-being and statistics of their specific class group (e.g., "Homeroom 101").
- **As Subject Leader:** Manages quizzes, content, and specific academic statistics for a subject (e.g., "Math for Homeroom 101").

### 3. Student

The end-user.

- Participates in quizzes.
- Tracks personal progress through the subject curriculum.
- Competes on leaderboards and earns badges.

---

## 🛠 Technology Stack

Qop is designed for performance and easy containerization.

- **Backend:** C# / ASP.NET Core
- **Frontend:** React.js
- **Database:** PostgreSQL
- **Caching/Realtime:** Redis
- **Infrastructure:** Docker & Docker Compose

---

## Deployment (Self-Hosting)

- **TODO**
