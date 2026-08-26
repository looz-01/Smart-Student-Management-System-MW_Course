# Smart Student Management System

Student Management Web API built with ASP.NET Core (.NET 10) that allows educational institutions to manage students, teachers, courses, enrollments, grades, and attendance through a secure RESTful API.

## Solution Structure

| Project | Description |
|---|---|
| `StudentManagementSystem.API` | Web API (controllers, services, models, EF Core, JWT auth) |
| `StudentManagementSystem.DTOs` | Shared DTOs / request & response contracts |
| `StudentManagementSystem.API.Tests` | xUnit unit & integration tests |

## Tech Stack

- ASP.NET Core 10 (Minimal hosting, OpenAPI + Scalar)
- Entity Framework Core 10 + SQL Server
- ASP.NET Core Identity (roles: Admin / Teacher / Student)
- JWT Bearer authentication with refresh-token rotation
- AutoMapper 16
- xUnit + EF Core Sqlite (in-memory) for tests

## Getting Started

1. Create the database and apply migrations:

   ```bash
   dotnet ef database update --project StudentManagementSystem.API
   ```

2. Run the API:

   ```bash
   dotnet run --project StudentManagementSystem.API
   ```

3. Open the interactive API docs (development only): `https://localhost:7243/scalar` (or `http://localhost:5245/scalar`).

4. The default admin account is seeded on startup (configured under `SeedAdmin` in `appsettings.json`):

   ```
   Email:    admin@school.com
   Password: Admin@123456
   ```

   > Change the password in production (or better: override via environment variables / user-secrets).

## Configuration

| Setting | Purpose |
|---|---|
| `ConnectionStrings:SystemConn` | SQL Server connection string |
| `Jwt:Key` | JWT signing key (must be ≥ 32 chars). **Dev-only default — override in production.** |
| `Jwt:Issuer` / `Jwt:Audience` | Validated on every token |
| `Jwt:DurationInMinutes` | Access-token lifetime (default 60) |
| `Jwt:RefreshTokenDurationInDays` | Refresh-token lifetime (default 7) |
| `SeedAdmin:Email` / `SeedAdmin:Password` | Auto-seeded admin account |

Recommended way to override secrets locally:

```bash
dotnet user-secrets set "Jwt:Key" "<your-secret-key>" --project StudentManagementSystem.API
dotnet user-secrets set "SeedAdmin:Password" "<your-password>" --project StudentManagementSystem.API
```

## API Overview

### Auth (`/api/Auth`)
- `POST register` — create Student/Teacher account (Admin role is not selectable)
- `POST login` — returns access + refresh tokens
- `POST refresh` — rotates refresh tokens (old token is revoked on use)

### Admin (`/api/Admin`) — `Admin` role
- `GET dashboard` — entity counts
- `GET users?searchTerm=&role=&page=` — paged user list
- `PUT change-role` — change a user's role (self-demotion is blocked, missing profile is auto-created)

### Students (`/api/Students`)
- `GET`, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`, `POST {id}/photo` — Admin/Teacher (delete/photo: Admin)
- `GET me` — current Student's own profile

### Teachers (`/api/Teachers`) — `Admin`
- CRUD + `GET me` (Teacher)

### Courses (`/api/Courses`)
- Admin/Teacher read; Admin create/update/delete

### Enrollments (`/api/Enrollments`)
- Admin/Teacher create & list; Admin delete

### Grades (`/api/Grades`)
- Admin/Teacher manage; Teachers are restricted to courses they teach
- `GET my` — current Student's own grades
- Score validated 0–100; student must be enrolled in the course

### Attendance (`/api/Attendances`)
- Admin/Teacher manage; Teachers restricted to their courses
- `GET my` — current Student's own attendance
- One record per student per course per day

## Security Notes

- Passwords: min 8 chars, must contain a digit; account lockout after 5 failed attempts (5 min)
- Auth endpoints are rate-limited (10 req/min) against brute force
- Refresh tokens are stored hashed (SHA-256), rotated on use, and expire after 7 days
- JWT validates issuer, audience, signing key, and lifetime
- Image uploads: extension + MIME validation, 2 MB limit, path-traversal-safe deletion
- Global exception handler returns consistent JSON errors (no stack traces outside Development)
- `Student.UserId` / `Teacher.UserId` / `RefreshToken.Token` are unique-indexed

## Tests

```bash
dotnet test
```

46 tests covering auth flows (register/login/refresh rotation), role-based business rules (grade & attendance ownership), validation, pagination, and response contracts.